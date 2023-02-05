/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

using System.Linq;

using UnityEditor;
using UnityEngine;

namespace MudBun
{
  [CustomEditor(typeof(MudRenderer), true)]
  [CanEditMultipleObjects]
  public class MudRendererEditor : MudRendererBaseEditor
  {
    protected override void LockMesh()
    {
      base.LockMesh();

      foreach (var renderer in targets.Select(x => (MudRenderer) x))
      {
        if (renderer == null)
          continue;

        if (renderer.MeshLocked)
          continue;

        DoLockMesh(renderer.transform, renderer.RecursiveLockMeshByEditor);
      }
    }

    private void DoLockMesh(Transform t, bool recursive, int depth = 0)
    {
      if (t == null)
        return;

      if (recursive)
      {
        for (int i = 0; i < t.childCount; ++i)
          DoLockMesh(t.GetChild(i), recursive, depth + 1);
      }

      var renderer = t.GetComponent<MudRenderer>();
      if (renderer == null)
        return;

      bool createNewObject = MeshGenerationCreateNewObject.boolValue;
      bool autoRigging = MeshGenerationAutoRigging.boolValue;
      bool generateCollider = MeshGenerationCreateCollider.boolValue;
      bool generateColliderMeshAsset = GenerateColliderMeshAssetByEditor.boolValue;
      bool generateMeshAsset = GenerateMeshAssetByEditor.boolValue;

      var prevMeshGenerationRenderableMeshMode = renderer.MeshGenerationRenderableMeshMode;
      if (createNewObject)
        renderer.MeshGenerationRenderableMeshMode = MudRendererBase.RenderableMeshMode.MeshRenderer;

      bool optimizeMeshForRendering = !GenerateMeshAssetByEditor.boolValue; // generated mesh assets are automatically optimized by the import pipeline
      renderer.LockMesh(autoRigging, false, null, MeshGenerationGenerateTextureUV.boolValue, MeshGenerationGenerateLightMapUV.boolValue, MeshGenerationWeldVertices.boolValue, optimizeMeshForRendering);

      if (createNewObject)
        renderer.MeshGenerationRenderableMeshMode = prevMeshGenerationRenderableMeshMode;

      // finish all access to serialized properties before they get disposed upon asset database refresh

      if (generateCollider)
      {
        var colliderMesh = renderer.AddCollider(renderer.gameObject, false, null, MeshGenerationForceConvexCollider.boolValue, MeshGenerationCreateRigidBody.boolValue);
        if (colliderMesh != null 
            && generateColliderMeshAsset)
        {
          renderer.ValidateAssetNames();

          string rootFolder = "Assets";
          string assetsFolder = "MudBun Generated Assets";
          string folderPath = $"{rootFolder}/{assetsFolder}";
          string assetName = renderer.GenerateColliderMeshAssetByEditorName;

          if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder(rootFolder, assetsFolder);

          string meshAssetPath = $"{folderPath}/{assetName}.mesh";
          AssetDatabase.CreateAsset(colliderMesh, meshAssetPath);
          AssetDatabase.Refresh();

          Debug.Log($"MudBun: Saved collider mesh asset - \"{folderPath}/{assetName}.mesh\"");
        }
      }

      if (generateMeshAsset)
      {
        renderer.ValidateAssetNames();

        string rootFolder = "Assets";
        string assetsFolder = "MudBun Generated Assets";
        string folderPath = $"{rootFolder}/{assetsFolder}";
        string assetName = renderer.GenerateMeshAssetByEditorName;

        if (!AssetDatabase.IsValidFolder(folderPath))
          AssetDatabase.CreateFolder(rootFolder, assetsFolder);

        Mesh mesh = null;
        Material mat = null;
        var meshFilter = renderer.GetComponent<MeshFilter>();
        var meshRenderer = renderer.GetComponent<MeshRenderer>();
        var skinnedMeshRenderer = renderer.GetComponent<SkinnedMeshRenderer>();
        if (meshRenderer != null)
        {
          if (meshFilter != null)
          {
            mesh = meshFilter.sharedMesh;
          }
          mat = meshRenderer.sharedMaterial;
        }
        else if (skinnedMeshRenderer != null)
        {
          mesh = skinnedMeshRenderer.sharedMesh;
          mat = skinnedMeshRenderer.sharedMaterial;
        }

        if (mesh != null)
        {
          string meshAssetPath = $"{folderPath}/{assetName}.mesh";
          AssetDatabase.CreateAsset(mesh, meshAssetPath);
          AssetDatabase.Refresh();

          Debug.Log($"MudBun: Saved mesh asset - \"{folderPath}/{assetName}.mesh\"");

          // somehow serialized properties get invalidated after asset database operations
          InitSerializedProperties();

          var savedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshAssetPath);
          if (savedMesh != null)
          {
            if (meshFilter != null)
              meshFilter.sharedMesh = savedMesh;
          }
        }

        if (mat != null)
        {
          if (meshRenderer != null)
            meshRenderer.sharedMaterial = mat;
          else if (skinnedMeshRenderer != null)
            skinnedMeshRenderer.sharedMaterial = mat;
        }
      }

      if (depth == 0 
          && createNewObject)
      {
        var clone = Instantiate(renderer.gameObject);
        clone.name = renderer.name + " (Locked Mesh Clone)";

        if (autoRigging)
        {
          var cloneRenderer = clone.GetComponent<MudRenderer>();
          cloneRenderer.RescanBrushesImmediate();
          cloneRenderer.DestroyAllBrushesImmediate();
        }
        else
        {
          DestroyAllChildren(clone.transform);
        }

        Undo.RegisterCreatedObjectUndo(clone, clone.name);
        DestroyImmediate(clone.GetComponent<MudRenderer>());
        Selection.activeObject = clone;

        renderer.UnlockMesh();
      }
    }

    protected override void GenerateSdf()
    {
      var renderer = (MudRendererBase)target;
      if (renderer == null)
        return;

      renderer.ValidateAssetNames();

      string rootFolder = "Assets";
      string assetsFolder = "MudBun Generated Assets";
      string folderPath = $"{rootFolder}/{assetsFolder}";
      string assetName = renderer.GenerateSdfByEditorName;

      if (!AssetDatabase.IsValidFolder(folderPath))
        AssetDatabase.CreateFolder(rootFolder, assetsFolder);

      Vector3Int size = GenerateSdfTextureSize.vector3IntValue;
      var sdf = new Texture3D(size.x, size.y, size.z, TextureFormat.RFloat, false);
      renderer.GenerateSdf(sdf, GenerateSdfCenter.vector3Value, GenerateSdfDimension.vector3Value);

      string sdfAssetPath = $"{folderPath}/{assetName}.asset";
      AssetDatabase.CreateAsset(sdf, sdfAssetPath);
      AssetDatabase.Refresh();

      renderer.MarkNeedsCompute();

      Debug.Log($"MudBun: Saved SDF texture - \"{folderPath}/{assetName}.asset\"");
    }
  }
}

