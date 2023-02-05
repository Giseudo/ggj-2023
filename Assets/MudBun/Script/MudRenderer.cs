/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace MudBun
{
  /// <summary>
  /// A renderer generates and renders dynamic meshes based on the brushes under its transform hierarchy. 
  /// <p/>
  /// It can generate SDF texture generations at dynamically at run-time, which can be combined with Unity's VFX graph for things like GPU particle collision detection.
  /// <p/>
  /// It also provides various CPU-based utilities that can used for gameplay purposes without needing GPU readbacks:
  /// <list type="bullet">
  ///   <item><description>SDF evaluation.</description></item>
  ///   <item><description>SDF normal (normalized gradient) evaluation.</description></item>
  ///   <item><description>Raycasts against mesh surface.</description></item>
  /// </list>
  /// </summary>
  [ExecuteInEditMode]
  public class MudRenderer : MudRendererBase
  {
    public delegate void MeshGenerated(Mesh mesh);
    public static event MeshGenerated OnMeshGenerated;
    public override void InvokeOnMeshGenerated(Mesh mesh)
    {
      OnMeshGenerated?.Invoke(mesh);
    }

    protected override void OnSharedMaterialChanged(UnityEngine.Object material)
    {
      foreach (var renderer in s_renderers)
      {
        if (renderer.SharedMaterial == material)
          renderer.MarkNeedsCompute();

        foreach (var b in renderer.Brushes)
        {
          var m = b.GetComponent<MudMaterial>();
          if (m != null && m.SharedMaterial != null && m.SharedMaterial == material)
            b.MarkDirty();
        }
      }
    }

    protected override void OnValidate()
    {
      base.OnValidate();

      #if UNITY_EDITOR
      EditorApplication.QueuePlayerLoopUpdate();
      #endif
    }

    protected override bool PreUpdateValidate()
    {
      if (!base.PreUpdateValidate())
        return false;

      #if UNITY_EDITOR
      if (RenderMode == RenderModeEnum.RayMarchedSurface 
          && RenderPipeline != ResourcesUtil.RenderPipelineEnum.URP)
      {
        Debug.LogWarning("The Ray-Marched Surface render mode is experimental and works in URP only.");
        return false;
      }
      #endif

      return true;
    }

    public override void NotifyHierarchyChange()
    {
      base.NotifyHierarchyChange();

      #if UNITY_EDITOR
      EditorApplication.QueuePlayerLoopUpdate();
      #endif
    }

    // TODO: WIP
    /*
    override public void RectifyNonUnitScaledParents()
    {
#if UNITY_EDITOR
      var goStack = new Stack<GameObject>();
      var goList = new List<GameObject>();

      // collect objects
      goStack.Push(gameObject);
      while (goStack.Count > 0)
      {
        var go = goStack.Pop();
        goList.Add(go);
        for (int i = 0; i < go.transform.childCount; ++i)
        {
          var childGo = go.transform.GetChild(i).gameObject;
          goStack.Push(childGo);
        }
      }

      // record transforms
      var positionMap = new Dictionary<GameObject, Vector3>();
      var rotationMap = new Dictionary<GameObject, Quaternion>();
      var scaleMap = new Dictionary<GameObject, Vector3>();
      foreach (var go in goList)
      {
        positionMap.Add(go, go.transform.position);
        rotationMap.Add(go, go.transform.rotation);
        scaleMap.Add(go, go.transform.localScale);
      }

      // rectify non-unit-scaled parents
      goStack.Push(gameObject);
      while (goStack.Count > 0)
      {
        var go = goStack.Pop();

        bool isBrush = (go.GetComponent<MudBrushBase>() != null);
        bool isUnitScaled = VectorUtil.MaxComp(go.transform.localScale) - VectorUtil.MinComp(go.transform.localScale) > MathUtil.Epsilon;
        bool shouldRectify = isBrush && !isUnitScaled && go.transform.childCount > 0;

        GameObject newParentGo = null;
        if (shouldRectify)
        {
          newParentGo = new GameObject(go.name + "(Rectified)");
          newParentGo.transform.position = go.transform.position;
          newParentGo.transform.rotation = go.transform.rotation;
          newParentGo.transform.SetParent(go.transform.parent, true);
          newParentGo.transform.SetSiblingIndex(go.transform.GetSiblingIndex() + 1);
        }

        for (int i = 0; i < go.transform.childCount; ++i)
        {
          var childGo = go.transform.GetChild(i).gameObject;
          goStack.Push(childGo);

          if (shouldRectify)
          {
            childGo.transform.SetParent(newParentGo.transform, true);
          }
        }
      }

      // restore transforms
      foreach (var go in goList)
      {
        go.transform.position = positionMap[go];
        go.transform.rotation = rotationMap[go];
        go.transform.localScale = scaleMap[go];
      }
#endif
    }
    */

    private T AddComponentHelper<T>(GameObject go) where T : Component
    {
      var comp = go.GetComponent<T>();
      if (comp == null)
      {
        #if UNITY_EDITOR
        comp = Undo.AddComponent<T>(go);
        #else
        comp = go.AddComponent<T>();
        #endif
      }
      else
      {
        #if UNITY_EDITOR
        Undo.RecordObject(comp, comp.name);
        #endif
      }

      if (m_addedComponents == null)
        m_addedComponents = new List<string>();

      var typeName = typeof(T).FullName;
      if (!m_addedComponents.Contains(typeName))
        m_addedComponents.Add(typeName);

      return comp;
    }

    private void RemoveComponentHelper<T>(GameObject go) where T : Component
    {
      // if not added, don't remove it
      var typeName = typeof(T).FullName;
      if (m_addedComponents == null 
          || !m_addedComponents.Contains(typeName))
        return;

      var comp = go.GetComponent<T>();
      if (comp != null)
      {
        #if UNITY_EDITOR
        Undo.DestroyObjectImmediate(comp);
        #else
        Destroy(comp);
        #endif
      }
    }

    public override Mesh AddCollider
    (
      GameObject go, 
      bool async, 
      Mesh mesh = null, 
      bool forceConvexCollider = false, 
      bool makeRigidBody = false
    )
    {
      var comp = AddComponentHelper<MeshCollider>(go);
      mesh = GenerateMesh(GeneratedMeshType.Collider, async, mesh);
      comp.sharedMesh = mesh;

      if (forceConvexCollider || makeRigidBody)
      {
        comp.convex = true;
      }

      if (makeRigidBody)
      {
        AddComponentHelper<Rigidbody>(go);
      }

      return mesh;
    }

    public override Mesh AddLockedStandardMesh
    (
      GameObject go, 
      bool autoRigging, 
      bool async, 
      Mesh mesh = null, 
      bool generateTextureUV = false, 
      bool generateLightMapUV = false, 
      bool weldVertices = false, 
      bool optimizeMeshForRendering = false
    )
    {
      #if UNITY_EDITOR
      Undo.RecordObject(this, name);
      #endif

      var transformStack = new Stack<Transform>();
      transformStack.Push(transform);
      while (transformStack.Count > 0)
      {
        var t = transformStack.Pop();
        if (t == null)
          continue;
        
        #if UNITY_EDITOR
        Undo.RecordObject(t, t.name);
        #endif

        for (int i = 0; i < t.childCount; ++i)
          transformStack.Push(t.GetChild(i));
      }

      m_doRigging = autoRigging;
      Transform [] aBone;
      mesh = GenerateMesh(GeneratedMeshType.Standard, go.transform, out aBone, async, mesh, generateTextureUV, generateLightMapUV, weldVertices, optimizeMeshForRendering);
      m_doRigging = false;

      Material material = 
        (m_lastLockedMeshMaterial == null)
          ? ResourcesUtil.DefaultLockedMeshMaterial 
          : m_lastLockedMeshMaterial;

      if (autoRigging)
      {
        var meshRenderer = AddComponentHelper<SkinnedMeshRenderer>(go);
        meshRenderer.sharedMesh = mesh;
        meshRenderer.sharedMaterial = material;
        meshRenderer.bones = aBone;
        meshRenderer.rootBone = go.transform;
      }
      else
      {
        var meshFilter = AddComponentHelper<MeshFilter>(go);
        var meshRenderer = AddComponentHelper<MeshRenderer>(go);
        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial = material;
      }

      m_lastLockedMeshMaterial = material;

      #if UNITY_EDITOR
      EditorApplication.QueuePlayerLoopUpdate();
      #endif

      return mesh;
    }

    private LockMeshIntermediateStateEnum m_lockMeshIntermediateState = LockMeshIntermediateStateEnum.Idle;
    protected override LockMeshIntermediateStateEnum LockMeshIntermediateState => m_lockMeshIntermediateState;

    [SerializeField] [HideInInspector] private List<string> m_addedComponents;

    public override void LockMesh
    (
      bool autoRigging, 
      bool async, 
      Mesh mesh = null,
      bool generateTextureUV = false,
      bool generateLightMapUV = false,
      bool weldVertices = false, 
      bool optimizeMeshForRendering = false
    )
    {
      m_lockMeshIntermediateState = LockMeshIntermediateStateEnum.PreLock;

      #if UNITY_EDITOR
      Undo.RecordObject(this, "Lock Mesh (" + name + ")");
      #endif

      base.LockMesh(autoRigging, async, mesh, generateTextureUV, generateLightMapUV, weldVertices, optimizeMeshForRendering);

      #if UNITY_EDITOR
      Undo.FlushUndoRecordObjects();
      #endif

      switch (MeshGenerationRenderableMeshMode)
      {
      case RenderableMeshMode.None:
        break;

      case RenderableMeshMode.Procedural:
        MarkNeedsCompute();
        break;

      case RenderableMeshMode.MeshRenderer:
        AddLockedStandardMesh(gameObject, autoRigging, async, mesh, generateTextureUV, generateLightMapUV, weldVertices, optimizeMeshForRendering);
        if (!async)
          DisposeLocalResources();
        break;
      }

      m_lockMeshIntermediateState = LockMeshIntermediateStateEnum.PostLock;
    }

    public override void UnlockMesh()
    {
      m_lockMeshIntermediateState = LockMeshIntermediateStateEnum.PreUnlock;

      #if UNITY_EDITOR
      Undo.RecordObject(this, "Unlock Mesh (" + name + ")");
      #endif

      base.UnlockMesh();

      #if UNITY_EDITOR
      Undo.FlushUndoRecordObjects();
      #endif

      RemoveComponentHelper<MeshCollider>(gameObject);
      RemoveComponentHelper<Rigidbody>(gameObject);
      RemoveComponentHelper<MeshFilter>(gameObject);
      RemoveComponentHelper<MeshRenderer>(gameObject);
      RemoveComponentHelper<SkinnedMeshRenderer>(gameObject);
      RemoveComponentHelper<MudLockedMeshRenderer>(gameObject);
      RemoveComponentHelper<MudStandardMeshRenderer>(gameObject);

      m_lockMeshIntermediateState = LockMeshIntermediateStateEnum.Idle;

      MeshGenerationLockOnStartByEditor = false;

      m_addedComponents = null;
    }

    protected override bool GenerateUV(Mesh mesh, bool generateTextureUV, bool generateLightMapUV)
    {
    #if UNITY_EDITOR
      if (generateTextureUV || generateLightMapUV)
      {
        Unwrapping.GenerateSecondaryUVSet(mesh);

        if (generateTextureUV)
          mesh.uv = mesh.uv2;

        if (!generateLightMapUV)
          mesh.uv2 = null;
      }
      return true;
    #else
      return false;
    #endif
    }

    //-------------------------------------------------------------------------

    private static NativeArray<Vector3> s_aSingleSampleSync;
    private static NativeArray<Sdf.Ray> s_aSingleRaySync;
    private static NativeArray<Sdf.Result> s_aSingleResultSync;
    private static NativeArray<Sdf.Contact> s_aSingleContactSync;

    private static void InitSyncJobData()
    {
      s_aSingleSampleSync = new NativeArray<Vector3>(1, Allocator.Persistent);
      s_aSingleRaySync = new NativeArray<Sdf.Ray>(1, Allocator.Persistent);
      s_aSingleResultSync = new NativeArray<Sdf.Result>(1, Allocator.Persistent);
      s_aSingleContactSync = new NativeArray<Sdf.Contact>(1, Allocator.Persistent);
    }

    private static void DisposeSyncJobData()
    {
      s_aSingleSampleSync.Dispose();
      s_aSingleRaySync.Dispose();
      s_aSingleResultSync.Dispose();
      s_aSingleContactSync.Dispose();
    }

    //-------------------------------------------------------------------------

    /// <summary>
    /// Generates an SDF texture into a RenderTexture object.
    /// </summary>
    /// <param name="sdf">The output texture.</param>
    /// <param name="origin">Point in renderer space mapped to the center of output texture. You can use the renderer's inverse transform to convert a point in world space into the renderer's local space.</param>
    /// <param name="dimension">Dimensions in renderer space mapped to the size output texture.</param>
    public override void GenerateSdf(RenderTexture sdf, Vector3 origin, Vector3 dimension)
    {
      base.GenerateSdf(sdf, origin, dimension);
    }

    /// <summary>
    /// Generates an SDF texture into a Texture3D object.
    /// </summary>
    /// <param name="sdf">The output texture.</param>
    /// <param name="origin">Point in renderer space mapped to the center of output texture. You can use the renderer's inverse transform to convert a point in world space into the renderer's local space.</param>
    /// <param name="dimension">Dimensions in renderer space mapped to the size output texture.</param>
    public override void GenerateSdf(Texture3D sdf, Vector3 origin, Vector3 dimension)
    {
      base.GenerateSdf(sdf, origin, dimension);
    }

    /// <summary>
    /// Synchronous CPU-based SDF evaluation that takes a single sample position and returns a single result. Function computes on the main thread and only returns when the entire computation is done.
    /// <p/>
    /// The result is inaccurate for scaled renderers, and is likely to be less accurate when non-union/blended brushes are involved due to SDF approximation (that still works for meshing algorithms and SDF raycasts). In these cases, use the more expensive <c>EvaluateSnapToSurface</c> and <c>EvaluateSnapToSurfaceAsync</c> and compute the distance between the sample point and contact point.
    /// </summary>
    /// <param name="p">The sample position.</param>
    /// <param name="maxSurfaceDistance">The range of evaluation from the sample position. Only brushes within this range are considered for evaluation. Use a conservative estimate of distance from the SDF's zero isosurface.</param>
    /// <param name="computeMaterials">Whether to compute the material at the sample point as well.</param>
    /// <returns>Result containing the SDF value, plus material if <c>computeMaterial</c> is true.</returns>
    public Sdf.Result EvaluateSdf(Vector3 p, float maxSurfaceDistance, bool computeMaterials)
    {
      UpdateComputeData();
      s_aSingleSampleSync[0] = transform.InverseTransformPoint(p);
      Sdf.EvaluateSdf(false, this, s_aSingleSampleSync, s_aSingleResultSync, m_aSdfBrush, m_numSdfBrushes, m_aSdfBrushMaterial, m_aabbTree.NodePods, m_aabbTree.Root, maxSurfaceDistance, computeMaterials, SurfaceShift);
      return s_aSingleResultSync[0];
    }

    /// <summary>
    /// Asynchronous CPU-based SDF evaluation. Function schedules a job that will compute on multiple threads.
    /// <p/>
    /// Results are inaccurate for scaled renderers, and are likely to be less accurate when non-union/blended brushes are involved due to SDF approximation (that still works for meshing algorithms and SDF raycasts). In these cases, use the more expensive <c>EvaluateSnapToSurface</c> and <c>EvaluateSnapToSurfaceAsync</c> and compute the distances between the samples point and contact points.
    /// </summary>
    /// <param name="samples">Array of sample position</param>
    /// <param name="results">Array of output results containing the SDF values, plus materials if <c>computeMaterial</c> is true.</param>
    /// <param name="maxSurfaceDistance">The range of evaluation from the sample positions. Only brushes within this range are considered for evaluation. Use a conservative estimate of distance from the SDF's zero isosurface.</param>
    /// <param name="computeMaterials">Whether to compute the materials at sample points as well.</param>
    /// <returns>A job handle that can be waited on for completion.</returns>
    public Sdf.EvalJobHandle EvaluateSdfAsync(NativeArray<Vector3> samples, NativeArray<Sdf.Result> results, float maxSurfaceDistance, bool computeMaterials)
    {
      UpdateComputeData();
      var hJob = Sdf.EvaluateSdf(true, this, samples, results, m_aSdfBrush, m_numSdfBrushes, m_aSdfBrushMaterial, m_aabbTree.NodePods, m_aabbTree.Root, maxSurfaceDistance, computeMaterials, SurfaceShift);
      m_jobQueue.Add(hJob);
      return hJob;
    }

    /// <summary>
    /// Synchronous CPU-based SDF normal (normalized gradient) evaluation that takes a single sample position and returns a single result. Function computes on the main thread and only returns when the entire computation is done.
    /// </summary>
    /// <param name="p">The sample position.</param>
    /// <param name="maxSurfaceDistance">The range of evaluation from the sample position. Only brushes within this range are considered for evaluation. Use a conservative estimate of distance from the SDF's zero isosurface.</param>
    /// <returns>Result containing the SDf normal.</returns>
    public Sdf.Result EvaluateNormal(Vector3 p, float maxSurfaceDistance)
    {
      UpdateComputeData();
      s_aSingleSampleSync[0] = transform.InverseTransformPoint(p);
      Sdf.EvaluateNormal(false, this, s_aSingleSampleSync, s_aSingleResultSync, m_aSdfBrush, m_numSdfBrushes, m_aSdfBrushMaterial, m_aabbTree.NodePods, m_aabbTree.Root, maxSurfaceDistance, SurfaceShift, 1e-3f);
      return s_aSingleResultSync[0];
    }

    /// <summary>
    /// Asynchronous CPU-based SDF normal (normalized gradient) evaluation. Function schedules a job that will compute on multiple threads.
    /// </summary>
    /// <param name="samples">Array of sample positions.</param>
    /// <param name="results">Array of output results containing the SDF normals.</param>
    /// <param name="computeMaterials">Whether to compute the materials at sample points as well.</param>
    /// <returns>A job handle that can be waited on for completion.</returns>
    public Sdf.EvalJobHandle EvaluateNormalAsync(NativeArray<Vector3> samples, NativeArray<Sdf.Result> results, float maxSurfaceDistance)
    {
      UpdateComputeData();
      var hJob = Sdf.EvaluateNormal(true, this, samples, results, m_aSdfBrush, m_numSdfBrushes, m_aSdfBrushMaterial, m_aabbTree.NodePods, m_aabbTree.Root, maxSurfaceDistance, SurfaceShift, 1e-3f);
      m_jobQueue.Add(hJob);
      return hJob;
    }

    /// <summary>
    /// Synchronous CPU-based SDF value and normal (normalized gradient) evaluation that takes a single sample position and returns a single result. Function computes on the main thread and only returns when the entire computation is done.
    /// <p/>
    /// The SDF result is inaccurate for scaled renderers, and is likely to be less accurate when non-union/blended brushes are involved due to SDF approximation (that still works for meshing algorithms and SDF raycasts). In these cases, use the more expensive <c>EvaluateSnapToSurface</c> and <c>EvaluateSnapToSurfaceAsync</c> and compute the distance between the sample point and contact point.
    /// </summary>
    /// <param name="p">The sample position</param>
    /// <param name="maxSurfaceDistance">The range of evaluation from the sample position. Only brushes within this range are considered for evaluation. Use a conservative estimate of distance from the SDF's zero isosurface.</param>
    /// <param name="computeMaterials">Whether to compute the materials at sample points as well.</param>
    /// <returns>Result containing the SDF value and normal, plus material if <c>computeMaterial</c> is true.</returns>
    public Sdf.Result EvaluateSdfAndNormal(Vector3 p, float maxSurfaceDistance, bool computeMaterials)
    {
      UpdateComputeData();
      s_aSingleSampleSync[0] = transform.InverseTransformPoint(p);
      Sdf.EvaluateSdfAndNormal(false, this, s_aSingleSampleSync, s_aSingleResultSync, m_aSdfBrush, m_numSdfBrushes, m_aSdfBrushMaterial, m_aabbTree.NodePods, m_aabbTree.Root, maxSurfaceDistance, computeMaterials, SurfaceShift, 1e-3f);
      return s_aSingleResultSync[0];
    }

    /// <summary>
    /// Asynchronous CPU-based SDF value and normal (normalized gradient) evaluation. Function schedules a job that will compute on multiple threads.
    /// <p/>
    /// SDF results are inaccurate for scaled renderers, and are likely to be less accurate when non-union/blended brushes are involved due to SDF approximation (that still works for meshing algorithms and SDF raycasts). In these cases, use the more expensive <c>EvaluateSnapToSurface</c> and <c>EvaluateSnapToSurfaceAsync</c> and compute the distances between the samples point and contact points.
    /// </summary>
    /// <param name="samples">Array of sample positions</param>
    /// <param name="results">Array of output results containing the SDF values and normals, plus materials if <c>computeMaterial</c> is true.</param>
    /// <param name="maxSurfaceDistance">The range of evaluation from the sample positions. Only brushes within this range are considered for evaluation. Use a conservative estimate of distance from the SDF's zero isosurface.</param>
    /// <param name="computeMaterials">Whether to compute the materials at sample points as well.</param>
    /// <returns>A job handle that can be waited on for completion.</returns>
    public Sdf.EvalJobHandle EvaluateSdfAndNormalAsync(NativeArray<Vector3> samples, NativeArray<Sdf.Result> results, float maxSurfaceDistance, bool computeMaterials)
    {
      UpdateComputeData();
      var hJob = Sdf.EvaluateSdfAndNormal(true, this, samples, results, m_aSdfBrush, m_numSdfBrushes, m_aSdfBrushMaterial, m_aabbTree.NodePods, m_aabbTree.Root, maxSurfaceDistance, computeMaterials, SurfaceShift, 1e-3f);
      m_jobQueue.Add(hJob);
      return hJob;
    }

    /// <summary>
    /// Synchronous CPU-based SDF raycast that takes a single ray and returns the raycast result. Function computes on the main thread and only returns when the entire computation is done.
    /// </summary>
    /// <param name="from">The starting point of the ray.</param>
    /// <param name="direction">The direction of the ray.</param>
    /// <param name="maxDistance">The maximum travel distance the ray.</param>
    /// <param name="computeMaterials">Whether to compute the material at the contact point (if hit) as well.</param>
    /// <param name="maxSteps">The maximum number of iterations to step the ray. If a hit hasn't been found after the maximum steps have been taken, then the raycast is a miss.</param>
    /// <param name="margin">The raycast is considered a hit if it ever reaches a point where the absolute SDF value is less than the margin. Smaller margin typically requires more steps to find a hit.</param>
    /// <param name="forceZeroBlendUnion">Whether to force all brushes to be treated as if they have zero blends with union operators. Useful for when needing to raycast against each brush's raw shape (e.g. for click-selection of mostly subtractive brushes).</param>
    /// <returns>Raycast result.</returns>
    public Sdf.Contact Raycast(Vector3 from, Vector3 direction, float maxDistance, bool computeMaterials, int maxSteps = 128, float margin = 1e-2f, bool forceZeroBlendUnion = false)
    {
      UpdateComputeData();
      Sdf.Ray ray;
      ray.From = from;
      ray.Direction = direction;
      ray.MaxDistance = maxDistance;
      s_aSingleRaySync[0] = ray;
      Sdf.EvaluateRaycast(false, this, s_aSingleRaySync, s_aSingleContactSync, margin, m_aSdfBrush, m_numSdfBrushes, m_aSdfBrushMaterial, m_aabbTree.NodePods, m_aabbTree.Root, computeMaterials, maxSteps, SurfaceShift, forceZeroBlendUnion);
      return s_aSingleContactSync[0];
    }

    /// <summary>
    /// Asynchronous CPU-based raycasts. Function schedules a job that will compute on multiple threads.
    /// </summary>
    /// <param name="casts">Array of rays.</param>
    /// <param name="results">Array of raycast results.</param>
    /// <param name="computeMaterials">Whether to compute the materials at contact points (if hit) as well.</param>
    /// <param name="maxSteps">The maximum number of iterations to step a ray. If a hit hasn't been found after the maximum steps have been taken, then the raycast is a miss.</param>
    /// <param name="margin">A raycast is considered a hit if it ever reaches a point where the absolute SDF value is less than the margin. Smaller margin typically requires more steps to find a hit.</param>
    /// <param name="forceZeroBlendUnion">Whether to force all brushes to be treated as if they have zero blends with union operators. Useful for when needing to raycast against each brush's raw shape (e.g. for click-selection of mostly subtractive brushes).</param>
    /// <returns>A job handle that can be waited on for completion.</returns>
    public Sdf.EvalJobHandle RaycastAsync(NativeArray<Sdf.Ray> casts, NativeArray<Sdf.Contact> results, bool computeMaterials, int maxSteps = 128, float margin = 1e-2f, bool forceZeroBlendUnion = false)
    {
      UpdateComputeData();
      var hJob = Sdf.EvaluateRaycast(true, this, casts, results, margin, m_aSdfBrush, m_numSdfBrushes, m_aSdfBrushMaterial, m_aabbTree.NodePods, m_aabbTree.Root, computeMaterials, maxSteps, SurfaceShift, forceZeroBlendUnion);
      m_jobQueue.Add(hJob);
      return hJob;
    }

    /// <summary>
    /// Synchronous CPU-based SDF raycast chain takes a single chain of sample points and returns a raycast result. Function computes on the main thread and only returns when the entire computation is done.
    /// </summary>
    /// <param name="chain">Array of points representing a series of consecutive rays that will be cast sequentially until a hit is found or if the array is exhausted.</param>
    /// <param name="computeMaterials">Whether to compute the materials at the contact point (if hit) as well.</param>
    /// <param name="maxSteps">The maximum number of iterations to step all the rays. If a hit hasn't been found after the maximum steps have been taken, then the raycast chain is a miss.</param>
    /// <param name="margin">A raycast chain is considered a hit if it ever reaches a point where the absolute SDF value is less than the margin. Smaller margin typically requires more steps to find a hit.</param>
    /// <returns>Raycast chain result.</returns>
    public Sdf.Contact RaycastChain(NativeArray<Vector3> chain, bool computeMaterials, int maxSteps = 512, float margin = 1e-2f)
    {
      UpdateComputeData();
      Sdf.EvaluateRaycastChain(false, this, chain, s_aSingleContactSync, margin, m_aSdfBrush, m_numSdfBrushes, m_aSdfBrushMaterial, m_aabbTree.NodePods, m_aabbTree.Root, computeMaterials, maxSteps, SurfaceShift);
      return s_aSingleContactSync[0];
    }

    /// <summary>
    /// Asynchronous CPU-based raycast chain. Function schedules a job that will compute on multiple threads.
    /// </summary>
    /// <param name="chain">Array of points representing a series of consecutive rays that will be cast sequentially until a hit is found or if the array is exhausted.</param>
    /// <param name="result">Array of a single raycast result for the entire raycast chain.</param>
    /// <param name="computeMaterials">Whether to compute the materials at the contact point (if hit) as well.</param>
    /// <param name="maxSteps">The maximum number of iterations to step all the rays. If a hit hasn't been found after the maximum steps have been taken, then the raycast chain is a miss.</param>
    /// <param name="margin">A raycast chain is considered a hit if it ever reaches a point where the absolute SDF value is less than the margin. Smaller margin typically requires more steps to find a hit.</param>
    /// <returns>A job handle that can be waited on for completion.</returns>
    public Sdf.EvalJobHandle RaycastChainAsync(NativeArray<Vector3> chain, NativeArray<Sdf.Contact> result, bool computeMaterials, int maxSteps = 128, float margin = 1e-2f)
    {
      UpdateComputeData();
      var hJob = Sdf.EvaluateRaycastChain(true, this, chain, result, margin, m_aSdfBrush, m_numSdfBrushes, m_aSdfBrushMaterial, m_aabbTree.NodePods, m_aabbTree.Root, computeMaterials, maxSteps, SurfaceShift);
      m_jobQueue.Add(hJob);
      return hJob;
    }

    /// <summary>
    /// Synchronous CPU-based SDF zero isosurface snapping that takes a single sample point and returns a potentially hit contact. Function computes on the main thread and only returns when the entire computation is done.
    /// <p/>
    /// Snapping is done by first evaluating the normal (normalized gradient) at the sample point, and then raycasting towards the SDF zero isosurface.
    /// </summary>
    /// <param name="p">The sample position.</param>
    /// <param name="maxSurfaceDistance">The range of evaluation from the sample position. Only brushes within this range are considered for evaluation. Use a conservative estimate of distance from the SDF's zero isosurface.</param>
    /// <param name="computeMaterials">Whether to compute the materials at the contact point (if snapping is successful) as well.</param>
    /// <param name="maxSteps">The maximum number of iterations to step the ray. If a hit hasn't been found after the maximum steps have been taken, then the raycast is a miss.</param>
    /// <param name="margin">The raycast is considered a hit if it ever reaches a point where the absolute SDF value is less than the margin. Smaller margin typically requires more steps to find a hit.</param>
    /// <returns>Surface snapping result.</returns>
    public Sdf.Contact SnapToSurface(Vector3 p, float maxSurfaceDistance, bool computeMaterials, int maxSteps = 128, float margin = 1e-2f)
    {
      UpdateComputeData();
      var samples = new NativeArray<Vector3>(1, Allocator.Temp);
      var results = new NativeArray<Sdf.Contact>(1, Allocator.Temp);
      samples[0] = p;
      var hJob = Sdf.EvaluateSnapToSurface(false, this, samples, results, margin, m_aSdfBrush, m_numSdfBrushes, m_aSdfBrushMaterial, m_aabbTree.NodePods, m_aabbTree.Root, computeMaterials, maxSurfaceDistance, maxSteps, SurfaceShift);
      hJob.Complete();
      var result = results[0];
      samples.Dispose();
      results.Dispose();
      return result;
    }

    /// <summary>
    /// Asynchronous CPU-based SDF zero isosurface snapping. Function schedules a job that will compute on multiple threads.
    /// <p/>
    /// Snapping is done by first evaluating normals (normalized gradients) at the sample points, and then raycasting towards the SDF zero isosurface.
    /// </summary>
    /// <param name="samples">Array of sample points.</param>
    /// <param name="results">Array of surface snapping results.</param>
    /// <param name="maxSurfaceDistance">The range of evaluation from the sample positions. Only brushes within this range are considered for evaluation. Use a conservative estimate of distance from the SDF's zero isosurface.</param>
    /// <param name="computeMaterials">Whether to compute the materials at the contact point (if snapping is successful) as well.</param>
    /// <param name="maxSteps">The maximum number of iterations to step a ray. If a hit hasn't been found after the maximum steps have been taken, then the raycast is a miss.</param>
    /// <param name="margin">A raycast is considered a hit if it ever reaches a point where the absolute SDF value is less than the margin. Smaller margin typically requires more steps to find a hit.</param>
    /// <returns>A job handle that can be waited on for completion.</returns>
    public Sdf.EvalJobHandle SnapToSurfaceAsync(NativeArray<Vector3> samples, NativeArray<Sdf.Contact> results, float maxSurfaceDistance, bool computeMaterials, int maxSteps = 128, float margin = 1e-2f)
    {
      UpdateComputeData();
      var hJob = Sdf.EvaluateSnapToSurface(true, this, samples, results, margin, m_aSdfBrush, m_numSdfBrushes, m_aSdfBrushMaterial, m_aabbTree.NodePods, m_aabbTree.Root, computeMaterials, maxSurfaceDistance, maxSteps, SurfaceShift);
      m_jobQueue.Add(hJob);
      return hJob;
    }

    /// <summary>
    /// Raycast against all renderers.
    /// </summary>
    /// <param name="from">The starting point of the ray.</param>
    /// <param name="direction">The direction of the ray.</param>
    /// <param name="maxDistance">The maximum distance traveldd by the ray.</param>
    /// <returns>Raycast result.</returns>
    public static Sdf.Contact Raycast(Vector3 from, Vector3 direction, float maxDistance)
    {
      Sdf.Contact ret = Sdf.Contact.New;

      foreach (var renderer in s_renderers)
      {
        var contact = ((MudRenderer) renderer).Raycast(from, direction, maxDistance, true);
        if (!contact.Hit)
          continue;

        if (ret.Hit && contact.GlobalT > ret.GlobalT)
          continue;

        ret = contact;
      }

      return ret;
    }

    internal static Sdf.Contact RaycastClickSelection(Vector3 from, Vector3 direction, float maxDistance)
    {
      Sdf.Contact ret = Sdf.Contact.New;

      foreach (var renderer in s_renderers)
      {
        switch (renderer.ClickSelection)
        {
          case ClickSelectionEnum.None:
          case ClickSelectionEnum.Gizmos:
            continue;
        }

        bool forceZeroBlendUnion = (renderer.ClickSelection == ClickSelectionEnum.RaycastForcedZeroBlendUnion);
        var contact = ((MudRenderer) renderer).Raycast(from, direction, maxDistance, true, 256, 1e-2f, forceZeroBlendUnion);
        if (!contact.Hit)
          continue;

        if (ret.Hit && contact.GlobalT > ret.GlobalT)
          continue;

        ret = contact;
      }

      return ret;
    }

    //-------------------------------------------------------------------------

    protected override void InitBeforeFirstRenderer()
    {
      base.InitBeforeFirstRenderer();
      InitSyncJobData();
      Sdf.InitAsyncJobData();

#if UNITY_EDITOR
      SelectionManager.Init();
#endif
    }

    protected override void CleanUpAfterLastRenderer()
    {
      base.CleanUpAfterLastRenderer();

      DisposeSyncJobData();
      Sdf.DisposeAsyncJobData();

#if UNITY_EDITOR
      SelectionManager.Dispose();
#endif
    }

    protected override void OnEnable()
    {
      base.OnEnable();

#if UNITY_EDITOR
      RegisterEditorEvents();
      SelectionManager.Init();
#endif
    }

    protected override void OnDisable()
    {
      base.OnDisable();

#if UNITY_EDITOR
      UnregisterEditorEvents();
      SelectionManager.NotifyRendererDisabled(this);
#endif
    }

#if UNITY_EDITOR
    protected override bool ValidateLocalResources()
    {
      bool res = base.ValidateLocalResources();
      if (!res)
        return false;

      Profiler.BeginSample("ValidateLocalResources (Renderer)");

      // clear all defaults
      if (RenderMaterialMesh == ResourcesUtilEditor.DefaultMeshMaterial)
        RenderMaterialMesh = null;
      if (RenderMaterialSplats == ResourcesUtilEditor.DefaultSplatMaterial)
        RenderMaterialSplats = null;
      if (RenderMaterialDecal == ResourcesUtilEditor.DefaultDecalMaterial)
        RenderMaterialDecal = null;
      if (RenderMaterialRayMarchedSurface == ResourcesUtilEditor.DefaultRayMarchedSurfaceMaterial)
        RenderMaterialRayMarchedSurface = null;

      // only assign default where/when necessary
      switch (RenderMode)
      {
        case RenderModeEnum.FlatMesh:
        case RenderModeEnum.SmoothMesh:
          if (RenderMaterialMesh == null)
            RenderMaterialMesh = ResourcesUtilEditor.DefaultMeshMaterial;
          break;

        case RenderModeEnum.CircleSplats:
        case RenderModeEnum.QuadSplats:
          if (RenderMaterialSplats == null)
            RenderMaterialSplats = ResourcesUtilEditor.DefaultSplatMaterial;
          break;

        case RenderModeEnum.Decal:
          if (RenderMaterialDecal == null)
            RenderMaterialDecal = ResourcesUtilEditor.DefaultDecalMaterial;
          break;

        case RenderModeEnum.RayMarchedSurface:
          if (RenderMaterialRayMarchedSurface == null)
            RenderMaterialRayMarchedSurface = ResourcesUtilEditor.DefaultRayMarchedSurfaceMaterial;
          break;
      }

      Profiler.EndSample();

      return true;
    }

    protected override bool ShouldHighlightBrushFromSelection(MudBrushBase brush)
    {
      if (Selection.Contains(brush.gameObject))
        return false;

      if (HoveredBrush == null)
        return false;

      if (HoveredBrush == brush)
        return true;

      if (!Selection.Contains(brush.Renderer.gameObject) 
          && HoveredBrush.Renderer == brush.Renderer 
          && !Selection.objects.Any(x  => (x as GameObject)?.GetComponent<MudBrushBase>()?.Renderer == brush.Renderer))
        return true;

      /*
      if (Selection.Contains(brush.Renderer.gameObject))
      {
        return (HoveredBrush == brush);
      }
      else
      {
        return (HoveredBrush.Renderer == brush.Renderer);
      }
      */

      return false;
    }

    internal static MudBrushBase HoveredBrush;

    private void OnHierarchyChanged()
    {
      if (MeshLocked)
        return;

      NotifyHierarchyChange();
    }

    private void OnEditorUpdate()
    {
      if (IsAnyMeshGenerationPending)
        EditorApplication.QueuePlayerLoopUpdate();
    }

    private void OnVisibilityChanged()
    {
      bool needsCompute = false;
      foreach (var b in Brushes)
      {
        bool isHidden = SceneVisibilityManager.instance.IsHidden(b.gameObject);
        if (isHidden != b.Hidden)
          needsCompute = true;

        b.Hidden = isHidden;
      }

      if (needsCompute)
      {
        ForceCompute();
        EditorApplication.QueuePlayerLoopUpdate();
      }
    }

    private void OnSceneSaved(UnityEngine.SceneManagement.Scene scene)
    {
      MarkNeedsCompute();
    }

    private void OnUndoPerformed()
    {
      MarkNeedsCompute();
    }

    private void OnBeforeAssemblyReload()
    {
      DisposeGlobalResources();
      DisposeLocalResources();
    }

    private void OnAfterAssemblyReload()
    {

    }

    private void RegisterEditorEvents()
    { 
      EditorApplication.hierarchyChanged += OnHierarchyChanged;
      EditorApplication.update += OnEditorUpdate;
      SceneVisibilityManager.visibilityChanged += OnVisibilityChanged;
      UnityEditor.SceneManagement.EditorSceneManager.sceneSaved += OnSceneSaved;
      Undo.undoRedoPerformed += OnUndoPerformed;
      AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
      AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
    }

    private void UnregisterEditorEvents()
    {
      EditorApplication.hierarchyChanged -= OnHierarchyChanged;
      EditorApplication.update -= OnEditorUpdate;
      SceneVisibilityManager.visibilityChanged -= OnVisibilityChanged;
      UnityEditor.SceneManagement.EditorSceneManager.sceneSaved -= OnSceneSaved;
      Undo.undoRedoPerformed -= OnUndoPerformed;
      AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
      AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
    }

    protected override bool IsEditorBusy()
    {
      if (EditorApplication.isCompiling)
        return true;

      if (EditorApplication.isUpdating)
        return true;

      return false;
    }

    public override void ReloadShaders()
    {
      base.ReloadShaders();

      EditorApplication.QueuePlayerLoopUpdate();
      SceneView.RepaintAll();
    }

    bool BrushesSelected
    {
      get
      {
        foreach (var b in Brushes)
          if (b.IsSelected())
            return true;

        return false;
      }
    }

    private void OnDrawGizmosSelected()
    {
      if (AlwaysDrawGizmos && BrushesSelected)
        return;

      DrawGizmos();
    }

    private void OnDrawGizmos()
    {
      if (!AlwaysDrawGizmos && !BrushesSelected)
        return;

      DrawGizmos();
    }

    private void DrawGizmos()
    {
      if (IsEditorBusy())
        return;

      if (MeshLocked)
        return;

      Color prevColor = Gizmos.color;
      
      Gizmos.matrix = transform.localToWorldMatrix;

      foreach (var b in Brushes)
      {
        Gizmos.color = GizmosUtil.OutlineDefault;
        b.DrawGizmosRs();
      }

      if (DrawRawBrushBounds)
      {
        Gizmos.color = Color.white;
        foreach (var b in Brushes)
        {
          Aabb bounds = b.BoundsRs;
          Gizmos.DrawWireCube(bounds.Center, bounds.Size);
        }
      }

      if (DrawComputeBrushBounds)
      {
        Gizmos.color = Color.yellow;
        m_aabbTree.ForEach(bounds => Gizmos.DrawWireCube(bounds.Center, bounds.Size));
      }

      if (DrawVoxelNodes)
      {
        Gizmos.color = Color.gray;
        var aNumAllocated = new int[m_numNodesAllocatedBuffer.count];
        m_numNodesAllocatedBuffer.GetData(aNumAllocated);
        int numTotalNodes = aNumAllocated[0];
        var aNode = new VoxelNode[numTotalNodes];
        m_nodePoolBuffer.GetData(aNode);
        var aNodeSize = NodeSizes;
        int iNode = 0;
        for (int depth = 0; depth <= VoxelNodeDepth; ++depth)
        {
          int numNodesInDepth = Mathf.Min(aNumAllocated[depth + 1], aNode.Length);

          if (DrawVoxelNodesDepth >= 0 && depth != DrawVoxelNodesDepth)
          {
            iNode += numNodesInDepth;
            continue;
          }

          float nodeSize = aNodeSize[depth];
          for (int i = 0; i < numNodesInDepth && iNode < aNode.Length; ++i, ++iNode)
          {
            Gizmos.DrawWireCube(aNode[iNode].Center, DrawVoxelNodesScale * nodeSize * Vector3.one);
          }
        }
      }

      if (UseCutoffVolume)
      {
        Vector3 centerRs =
          CutoffVolumeCenter != null 
            ? transform.InverseTransformPoint(CutoffVolumeCenter.position) 
            : Vector3.zero;
        GizmosUtil.DrawWireBox(centerRs, CutoffVolumeSize, Quaternion.identity);
      }

      Gizmos.matrix = Matrix4x4.identity;

      if (DrawRenderBounds)
      {
        Gizmos.color = Color.cyan;
        Aabb bounds = RenderBounds;
        Gizmos.DrawWireCube(bounds.Center, bounds.Size);
      }

      if (DrawGenerateSdfGizmos)
      {
        Gizmos.color = Color.white;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.DrawWireSphere(GenerateSdfCenter, 0.1f);
        Gizmos.DrawWireCube(GenerateSdfCenter, GenerateSdfDimension);
        Gizmos.matrix = Matrix4x4.identity;
      }

      Gizmos.color = prevColor;
    }
#endif
  }
}
