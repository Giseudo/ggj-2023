/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

using System;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace MudBun
{
  [CustomEditor(typeof(MudRendererBase))]
  [CanEditMultipleObjects]
  public class MudRendererBaseEditor : MudEditorBase
  {
    protected SerializedProperty MaxVoxelsK;
    protected SerializedProperty MaxChunks;
    protected SerializedProperty ShowGpuMemoryUsage;
    protected SerializedProperty AutoAdjustBudgetsToHighWaterMarks;
    protected SerializedProperty AutoAdjustBudgetsToHighWaterMarksMarginPercent;

    protected SerializedProperty VoxelDensity;
    protected SerializedProperty ComputeMode;
    protected SerializedProperty TimeSliceMode;
    protected SerializedProperty TimeSliceFrames;
    protected SerializedProperty TimeSliceFramesOffset;
    protected SerializedProperty TimeSlicePeriod;
    protected SerializedProperty TimeSliceTimeOffset;
    protected SerializedProperty HardwareMode;
    protected SerializedProperty RenderMode;
    protected SerializedProperty RayTracedVoxelMode;
    protected SerializedProperty RayTracedVoxelSizeMultiplier;
    protected SerializedProperty RayTracedVoxelSmoothCubeNormal;
    protected SerializedProperty RayTracedVoxelSphereFullness;
    protected SerializedProperty RayTracedVoxelPaddingMode;
    protected SerializedProperty RayTracedVoxelInternalPaddingDistance;
    protected SerializedProperty RayTracedVoxelSizeFadeDistance;
    protected SerializedProperty ShowAdvancedNormalOptions;
    protected SerializedProperty NormalQuantization;
    protected SerializedProperty Normal2dFade;
    protected SerializedProperty Normal2dStrength;
    protected SerializedProperty SmoothNormalBlurRelative;
    protected SerializedProperty SmoothNormalBlurAbsolute;
    protected SerializedProperty EnableAutoSmoothing;
    protected SerializedProperty AutoSmoothingMaxAngle;
    protected SerializedProperty EnableSmoothCorner;
    protected SerializedProperty SmoothCornerSubdivision;
    protected SerializedProperty SmoothCornerNormalBlur;
    protected SerializedProperty SmoothCornerFade;
    protected SerializedProperty ForceEvaluateAllBrushes;
    protected SerializedProperty Enable2dMode;
    protected SerializedProperty SurfaceShift;
    protected SerializedProperty MeshingMode;
    protected SerializedProperty ShowAdvancedGeometryOptions;
    protected SerializedProperty SurfaceNetsDualQuadsBlend;
    protected SerializedProperty SurfaceNetsBinarySearchIterations;
    protected SerializedProperty SurfaceNetsGradientDescentIterations;
    protected SerializedProperty SurfaceNetsGradientDescentFactor;
    protected SerializedProperty SurfaceNetsHighAccuracyMode;
    protected SerializedProperty DualContouringDualQuadsBlend;
    protected SerializedProperty DualContouringRelaxation;
    protected SerializedProperty DualContouringSolverIterations;
    protected SerializedProperty DualContouringBinarySearchIterations;
    protected SerializedProperty DualContouringGradientDescentIterations;
    protected SerializedProperty DualContouringGradientDescentFactor;
    protected SerializedProperty DualContouringHighAccuracyMode;
    protected SerializedProperty ShowAdvancedSplatOptions;
    protected SerializedProperty SplatSize;
    protected SerializedProperty SplatSizeJitter;
    protected SerializedProperty SplatNormalShift;
    protected SerializedProperty SplatNormalShiftJitter;
    protected SerializedProperty SplatColorJitter;
    protected SerializedProperty SplatPositionJitter;
    protected SerializedProperty SplatRotationJitter;
    protected SerializedProperty SplatOrientationJitter;
    protected SerializedProperty SplatOriginalNormalBlend;
    protected SerializedProperty SplatJitterNoisiness;
    protected SerializedProperty SplatCameraFacing;
    protected SerializedProperty SplatNormalsMatchCameraFacing;
    protected SerializedProperty SplatShadowsMatchCameraFacing;
    protected SerializedProperty SplatScreenSpaceFlattening;
    //protected SerializedProperty SplatSmoothNormalBlend;
    protected SerializedProperty CastShadows;
    protected SerializedProperty ReceiveShadows;
    
    protected SerializedProperty SharedMaterial;
    protected SerializedProperty MasterColor;
    protected SerializedProperty MasterEmission;
    protected SerializedProperty MasterMetallic;
    protected SerializedProperty MasterSmoothness;
    protected SerializedProperty RenderMaterialMesh;
    protected SerializedProperty RenderMaterialSplats;
    protected SerializedProperty RenderMaterialDecal;
    protected SerializedProperty RenderMaterialRayMarchedSurface;
    protected SerializedProperty RenderMaterialRayTracedVoxels;
    protected SerializedProperty RenderMaterialMode;

    protected SerializedProperty MaxRayMarchSteps;
    protected SerializedProperty RayMarchAccuracy;
    protected SerializedProperty RayMarchMaxRayDistance;
    
    /*
    protected SerializedProperty RayMarchStepSize;
    protected SerializedProperty NumLightMarchSteps;
    protected SerializedProperty RayMarchDistance;
    protected SerializedProperty RayMarchVolumeDensity;
    protected SerializedProperty RayMarchLight;
    protected SerializedProperty RayMarchVolumeAbsorption;
    protected SerializedProperty RayMarchLightAbsorption;
    protected SerializedProperty RayMarchDarknesThreshold;
    protected SerializedProperty RayMarchTransmittanceCurve;
    protected SerializedProperty UseRayMarchNoise;
    protected SerializedProperty RayMarchNoiseThreshold;
    protected SerializedProperty RayMarchNoiseEdgeFade;
    protected SerializedProperty RayMarchNoiseScrollSpeed;
    protected SerializedProperty RayMarchNoiseBaseOctaveSize;
    protected SerializedProperty RayMarchNoiseNumOctaves;
    protected SerializedProperty RayMarchNoiseOctaveOffsetFactor;
    */

    protected SerializedProperty MeshGenerationCreateNewObject;
    protected SerializedProperty MeshGenerationRenderableMeshMode;
    protected SerializedProperty MeshGenerationCreateCollider;
    protected SerializedProperty MeshGenerationForceConvexCollider;
    protected SerializedProperty MeshGenerationColliderSurfaceShift;
    protected SerializedProperty MeshGenerationCreateRigidBody;
    protected SerializedProperty GenerateColliderMeshAssetByEditor;
    protected SerializedProperty GenerateColliderMeshAssetByEditorName;
    protected SerializedProperty MeshGenerationColliderVoxelDensity;
    protected SerializedProperty MeshGenerationAutoRigging;
    protected SerializedProperty MeshGenerationGenerateTextureUV;
    protected SerializedProperty MeshGenerationGenerateLightMapUV;
    protected SerializedProperty MeshGenerationWeldVertices;
    protected SerializedProperty MeshGenerationLockOnStart;
    protected SerializedProperty GenerateMeshAssetByEditor;
    protected SerializedProperty GenerateMeshAssetByEditorName;
    protected SerializedProperty RecursiveLockMeshByEditor;
    protected SerializedProperty RememberLockedMeshMaterialByEditor;

    protected SerializedProperty DrawGenerateSdfGizmos;
    protected SerializedProperty GenerateSdfByEditorName;
    protected SerializedProperty GenerateSdfCenter;
    protected SerializedProperty GenerateSdfDimension;
    protected SerializedProperty GenerateSdfTextureSize;

    protected SerializedProperty UseCutoffVolume;
    protected SerializedProperty CutoffVolumeCenter;
    protected SerializedProperty CutoffVolumeSize;

    protected SerializedProperty ClickSelection;

    protected SerializedProperty AlwaysDrawGizmos;
    protected SerializedProperty DrawRawBrushBounds;
    protected SerializedProperty DrawComputeBrushBounds;
    protected SerializedProperty DrawRenderBounds;
    protected SerializedProperty DrawVoxelNodes;
    protected SerializedProperty DrawVoxelNodesDepth;
    protected SerializedProperty DrawVoxelNodesScale;


    protected void InitSerializedProperties()
    {
      MaxVoxelsK = serializedObject.FindProperty("MaxVoxelsK");
      MaxChunks = serializedObject.FindProperty("MaxChunks");
      ShowGpuMemoryUsage = serializedObject.FindProperty("ShowGpuMemoryUsage");
      AutoAdjustBudgetsToHighWaterMarks = serializedObject.FindProperty("AutoAdjustBudgetsToHighWaterMarks");
      AutoAdjustBudgetsToHighWaterMarksMarginPercent = serializedObject.FindProperty("AutoAdjustBudgetsToHighWaterMarksMarginPercent");

      VoxelDensity = serializedObject.FindProperty("VoxelDensity");
      ComputeMode = serializedObject.FindProperty("ComputeMode");
      TimeSliceMode = serializedObject.FindProperty("TimeSliceMode");
      TimeSliceFrames = serializedObject.FindProperty("TimeSliceFrames");
      TimeSliceFramesOffset = serializedObject.FindProperty("TimeSliceFramesOffset");
      TimeSlicePeriod = serializedObject.FindProperty("TimeSlicePeriod");
      TimeSliceTimeOffset = serializedObject.FindProperty("TimeSliceTimeOffset");
      HardwareMode = serializedObject.FindProperty("HardwareMode");
      RenderMode = serializedObject.FindProperty("RenderMode");
      RayTracedVoxelMode = serializedObject.FindProperty("RayTracedVoxelMode");
      RayTracedVoxelSizeMultiplier = serializedObject.FindProperty("RayTracedVoxelSizeMultiplier");
      RayTracedVoxelSmoothCubeNormal = serializedObject.FindProperty("RayTracedVoxelSmoothCubeNormal");
      RayTracedVoxelSphereFullness = serializedObject.FindProperty("RayTracedVoxelSphereFullness");
      RayTracedVoxelPaddingMode = serializedObject.FindProperty("RayTracedVoxelPaddingMode");
      RayTracedVoxelInternalPaddingDistance = serializedObject.FindProperty("RayTracedVoxelInternalPaddingDistance");
      RayTracedVoxelSizeFadeDistance = serializedObject.FindProperty("RayTracedVoxelSizeFadeDistance");
      ShowAdvancedNormalOptions = serializedObject.FindProperty("ShowAdvancedNormalOptions");
      NormalQuantization = serializedObject.FindProperty("NormalQuantization");
      Normal2dFade = serializedObject.FindProperty("Normal2dFade");
      Normal2dStrength = serializedObject.FindProperty("Normal2dStrength");
      SmoothNormalBlurRelative = serializedObject.FindProperty("SmoothNormalBlurRelative");
      SmoothNormalBlurAbsolute = serializedObject.FindProperty("SmoothNormalBlurAbsolute");
      EnableAutoSmoothing = serializedObject.FindProperty("EnableAutoSmoothing");
      AutoSmoothingMaxAngle = serializedObject.FindProperty("AutoSmoothingMaxAngle");
      EnableSmoothCorner = serializedObject.FindProperty("EnableSmoothCorner");
      SmoothCornerSubdivision = serializedObject.FindProperty("SmoothCornerSubdivision");
      SmoothCornerNormalBlur = serializedObject.FindProperty("SmoothCornerNormalBlur");
      SmoothCornerFade = serializedObject.FindProperty("SmoothCornerFade");
      ForceEvaluateAllBrushes = serializedObject.FindProperty("ForceEvaluateAllBrushes");
      Enable2dMode = serializedObject.FindProperty("Enable2dMode");
      SurfaceShift = serializedObject.FindProperty("SurfaceShift");
      MeshingMode = serializedObject.FindProperty("MeshingMode");
      ShowAdvancedGeometryOptions = serializedObject.FindProperty("ShowAdvancedGeometryOptions");
      SurfaceNetsDualQuadsBlend = serializedObject.FindProperty("SurfaceNetsDualQuadsBlend");
      SurfaceNetsBinarySearchIterations = serializedObject.FindProperty("SurfaceNetsBinarySearchIterations");
      SurfaceNetsGradientDescentIterations = serializedObject.FindProperty("SurfaceNetsGradientDescentIterations");
      SurfaceNetsHighAccuracyMode = serializedObject.FindProperty("SurfaceNetsHighAccuracyMode");
      SurfaceNetsGradientDescentFactor = serializedObject.FindProperty("SurfaceNetsGradientDescentFactor");
      DualContouringDualQuadsBlend = serializedObject.FindProperty("DualContouringDualQuadsBlend");
      DualContouringRelaxation = serializedObject.FindProperty("DualContouringRelaxation");
      DualContouringSolverIterations = serializedObject.FindProperty("DualContouringSolverIterations");
      DualContouringBinarySearchIterations = serializedObject.FindProperty("DualContouringBinarySearchIterations");
      DualContouringGradientDescentIterations = serializedObject.FindProperty("DualContouringGradientDescentIterations");
      DualContouringHighAccuracyMode = serializedObject.FindProperty("DualContouringHighAccuracyMode");
      DualContouringGradientDescentFactor = serializedObject.FindProperty("DualContouringGradientDescentFactor");
      ShowAdvancedSplatOptions = serializedObject.FindProperty("ShowAdvancedSplatOptions");
      SplatSize = serializedObject.FindProperty("SplatSize");
      SplatSizeJitter = serializedObject.FindProperty("SplatSizeJitter");
      SplatNormalShift = serializedObject.FindProperty("SplatNormalShift");
      SplatNormalShiftJitter = serializedObject.FindProperty("SplatNormalShiftJitter");
      SplatColorJitter = serializedObject.FindProperty("SplatColorJitter");
      SplatPositionJitter = serializedObject.FindProperty("SplatPositionJitter");
      SplatRotationJitter = serializedObject.FindProperty("SplatRotationJitter");
      SplatOrientationJitter = serializedObject.FindProperty("SplatOrientationJitter");
      SplatOriginalNormalBlend = serializedObject.FindProperty("SplatOriginalNormalBlend");
      SplatJitterNoisiness = serializedObject.FindProperty("SplatJitterNoisiness");
      SplatCameraFacing = serializedObject.FindProperty("SplatCameraFacing");
      SplatNormalsMatchCameraFacing = serializedObject.FindProperty("SplatNormalsMatchCameraFacing");
      SplatShadowsMatchCameraFacing = serializedObject.FindProperty("SplatShadowsMatchCameraFacing");
      SplatScreenSpaceFlattening = serializedObject.FindProperty("SplatScreenSpaceFlattening");
      //SplatSmoothNormalBlend = serializedObject.FindProperty("SplatSmoothNormalBlend");
      SplatCameraFacing = serializedObject.FindProperty("SplatCameraFacing");
      CastShadows = serializedObject.FindProperty("CastShadows");
      ReceiveShadows = serializedObject.FindProperty("ReceiveShadows");

      SharedMaterial = serializedObject.FindProperty("SharedMaterial");
      MasterColor = serializedObject.FindProperty("m_masterColor");
      MasterEmission = serializedObject.FindProperty("m_masterEmission");
      MasterMetallic = serializedObject.FindProperty("m_masterMetallic");
      MasterSmoothness = serializedObject.FindProperty("m_masterSmoothness");
      RenderMaterialMesh = serializedObject.FindProperty("RenderMaterialMesh");
      RenderMaterialSplats = serializedObject.FindProperty("RenderMaterialSplats");
      RenderMaterialDecal = serializedObject.FindProperty("RenderMaterialDecal");
      RenderMaterialRayMarchedSurface = serializedObject.FindProperty("RenderMaterialRayMarchedSurface");
      RenderMaterialRayTracedVoxels = serializedObject.FindProperty("RenderMaterialRayTracedVoxels");
      RenderMaterialMode = serializedObject.FindProperty("RenderMaterialMode");

      MaxRayMarchSteps = serializedObject.FindProperty("MaxRayMarchSteps");
      RayMarchAccuracy = serializedObject.FindProperty("RayMarchAccuracy");
      RayMarchMaxRayDistance = serializedObject.FindProperty("RayMarchMaxRayDistance");

      /*
      RayMarchStepSize = serializedObject.FindProperty("RayMarchStepSize");
      NumLightMarchSteps = serializedObject.FindProperty("NumLightMarchSteps");
      RayMarchDistance = serializedObject.FindProperty("RayMarchDistance");
      RayMarchVolumeDensity = serializedObject.FindProperty("RayMarchVolumeDensity");
      RayMarchLight = serializedObject.FindProperty("RayMarchLight");
      RayMarchVolumeAbsorption = serializedObject.FindProperty("RayMarchVolumeAbsorption");
      RayMarchLightAbsorption = serializedObject.FindProperty("RayMarchLightAbsorption");
      RayMarchDarknesThreshold = serializedObject.FindProperty("RayMarchDarknesThreshold");
      RayMarchTransmittanceCurve = serializedObject.FindProperty("RayMarchTransmittanceCurve");
      RayMarchNoiseEdgeFade = serializedObject.FindProperty("RayMarchNoiseEdgeFade");
      UseRayMarchNoise = serializedObject.FindProperty("UseRayMarchNoise");
      RayMarchNoiseThreshold = serializedObject.FindProperty("RayMarchNoiseThreshold");
      RayMarchNoiseScrollSpeed = serializedObject.FindProperty("RayMarchNoiseScrollSpeed");
      RayMarchNoiseBaseOctaveSize = serializedObject.FindProperty("RayMarchNoiseBaseOctaveSize");
      RayMarchNoiseNumOctaves = serializedObject.FindProperty("RayMarchNoiseNumOctaves");
      RayMarchNoiseOctaveOffsetFactor = serializedObject.FindProperty("RayMarchNoiseOctaveOffsetFactor");
      */

      MeshGenerationCreateNewObject = serializedObject.FindProperty("MeshGenerationCreateNewObject");
      MeshGenerationRenderableMeshMode = serializedObject.FindProperty("MeshGenerationRenderableMeshMode");
      MeshGenerationCreateCollider = serializedObject.FindProperty("MeshGenerationCreateCollider");
      MeshGenerationForceConvexCollider = serializedObject.FindProperty("MeshGenerationForceConvexCollider");
      MeshGenerationColliderSurfaceShift = serializedObject.FindProperty("MeshGenerationColliderSurfaceShift");
      MeshGenerationCreateRigidBody = serializedObject.FindProperty("MeshGenerationCreateRigidBody");
      GenerateColliderMeshAssetByEditor = serializedObject.FindProperty("GenerateColliderMeshAssetByEditor");
      GenerateColliderMeshAssetByEditorName = serializedObject.FindProperty("GenerateColliderMeshAssetByEditorName");
      MeshGenerationColliderVoxelDensity = serializedObject.FindProperty("MeshGenerationColliderVoxelDensity");
      MeshGenerationAutoRigging = serializedObject.FindProperty("MeshGenerationAutoRigging");
      MeshGenerationGenerateTextureUV = serializedObject.FindProperty("MeshGenerationGenerateTextureUV");
      MeshGenerationGenerateLightMapUV = serializedObject.FindProperty("MeshGenerationGenerateLightMapUV");
      MeshGenerationWeldVertices = serializedObject.FindProperty("MeshGenerationWeldVertices");
      MeshGenerationLockOnStart = serializedObject.FindProperty("MeshGenerationLockOnStart");
      GenerateMeshAssetByEditor = serializedObject.FindProperty("GenerateMeshAssetByEditor");
      GenerateMeshAssetByEditorName = serializedObject.FindProperty("GenerateMeshAssetByEditorName");
      RecursiveLockMeshByEditor = serializedObject.FindProperty("RecursiveLockMeshByEditor");
      RememberLockedMeshMaterialByEditor = serializedObject.FindProperty("RememberLockedMeshMaterialByEditor");

      DrawGenerateSdfGizmos = serializedObject.FindProperty("DrawGenerateSdfGizmos");
      GenerateSdfByEditorName = serializedObject.FindProperty("GenerateSdfByEditorName");
      GenerateSdfCenter = serializedObject.FindProperty("GenerateSdfCenter");
      GenerateSdfDimension = serializedObject.FindProperty("GenerateSdfDimension");
      GenerateSdfTextureSize = serializedObject.FindProperty("GenerateSdfTextureSize");

      UseCutoffVolume = serializedObject.FindProperty("UseCutoffVolume");
      CutoffVolumeCenter = serializedObject.FindProperty("CutoffVolumeCenter");
      CutoffVolumeSize = serializedObject.FindProperty("CutoffVolumeSize");

      ClickSelection = serializedObject.FindProperty("ClickSelection");

      AlwaysDrawGizmos = serializedObject.FindProperty("AlwaysDrawGizmos");
      DrawRawBrushBounds = serializedObject.FindProperty("DrawRawBrushBounds");
      DrawComputeBrushBounds = serializedObject.FindProperty("DrawComputeBrushBounds");
      DrawRenderBounds = serializedObject.FindProperty("DrawRenderBounds");
      DrawVoxelNodes = serializedObject.FindProperty("DrawVoxelNodes");
      DrawVoxelNodesDepth = serializedObject.FindProperty("DrawVoxelNodesDepth");
      DrawVoxelNodesScale = serializedObject.FindProperty("DrawVoxelNodesScale");
    }

    private void ShowMaterialOptions()
    {
      var renderer = (MudRendererBase) target;

      Header("Material");

      Property(SharedMaterial, 
        "Shared Material", 
            "External material used as the renderer's master material."
      );

      if (SharedMaterial.objectReferenceValue == null)
      {
        Property(MasterColor, 
          "Master Color", 
              "Master color multiplier."
        );

        Property(MasterEmission, 
          "Master Emission", 
              "Master emission multiplier. Alpha is not used."
        );

        Property(MasterMetallic, 
          "Master Metallic", 
              "Master metallic multiplier."
        );

        Property(MasterSmoothness, 
          "Master Smoothness", 
              "Master smoothness multiplier."
        );

        switch (renderer.RenderModeCategory)
        {
          case MudRendererBase.RenderModeCategoryEnum.Mesh:
            Property(RenderMaterialMesh, "Render Material");
            break;
            
          case MudRendererBase.RenderModeCategoryEnum.Splats:
            Property(RenderMaterialSplats, "Render Material");
            break;

          case MudRendererBase.RenderModeCategoryEnum.Decal:
            Property(RenderMaterialDecal, "Render Material");
            break;

          case MudRendererBase.RenderModeCategoryEnum.RayMarchedSurface:
            Property(RenderMaterialRayMarchedSurface, "Render Material");
            break;

          case MudRendererBase.RenderModeCategoryEnum.RayTracedVoxels:
            Property(RenderMaterialRayTracedVoxels, "Render Material");
            break;
        }

        Property(RenderMaterialMode, 
          "  Render Material Mode", 
              "A renderer enables & disables its render material's shader keywords to select the correct shader variant, " 
            + "which requires making a local copy of the render material. " 
            + "This means the renderer needs to refresh its copy if the referenced render material has changed. " 
            + "This is done automatically in the editor for convenience, but in builds the renderer uses the render material mode to decide how to refresh the copy.\n\n" 
            + "Static - The render material is not expected to change. It is only copied once. Manual refresh of the copy can be done by calling the MarkMaterialDirty method.\n\n"
            + "Dynamic - The render material is expected to be constantly changing, and it is copied once every frame. This may impact performance.");

        /*
        Indent();
          //Property(MaxRayMarchSteps, "Ray Steps");
          Property(RayMarchStepSize, "Ray Step Size");
          Property(NumLightMarchSteps, "Light Steps");
          //Property(RayMarchDistance, "Total Ray Distance");
          Property(RayMarchVolumeDensity, "Volume Density");
          Property(RayMarchLight, "Light", 
              "Type supported: directional, point.");
          Property(RayMarchVolumeAbsorption, "Volume Absorption");
          Property(RayMarchLightAbsorption, "Light Absorption");
          Property(RayMarchDarknesThreshold, "Darkness Threshold");
          Property(RayMarchTransmittanceCurve, "Transmittance Curve");

          Property(UseRayMarchNoise, "Use Noise");
          if (UseRayMarchNoise.boolValue)
          {
            Indent();
              Property(RayMarchNoiseThreshold, "Threshold");
              Property(RayMarchNoiseEdgeFade, "Edge Fade");
              Property(RayMarchNoiseScrollSpeed, "Scroll Speed");
              Property(RayMarchNoiseBaseOctaveSize, "Base Octave Size");
              Property(RayMarchNoiseNumOctaves, "Octaves");
              Property(RayMarchNoiseOctaveOffsetFactor, "Octave Offset Factor");
            Unindent();
          }
        Unindent();
        */
      }

      Space();
    }

    public virtual void OnEnable()
    {
      InitSerializedProperties();

      EditorApplication.update += Update;
    }

    private void OnDisable()
    {
      EditorApplication.update -= Update;
    }

    private static string IntCountString(long n, bool space = false, string suffix = "")
    {
      if (n < 1024)
        return n.ToString() + (space ? " " : "") + suffix;

      if (n < 1048576)
        return (n / 1024.0f).ToString("N1") + (space ? " " : "") + "K" + suffix;

      return (n / 1048576.0f).ToString("N1") + (space ? " " : "") + "M" + suffix;
    }

    public override void OnInspectorGUI()
    {
      serializedObject.Update();

      var renderer = (MudRendererBase) target;
      bool gpuMode = false;

      {
        Header("MudBun Version " + MudBun.Version);

        if (MudBun.IsFreeVersion)
        {
          Indent();
            Text("Trial Version Limitations:");
            Text("   - Watermark.");
            Text("   - Limited voxel density & triangle count for mesh utilities.");
            Text("   - No full source code.");
            Text("   - No commercial use.");
          Unindent();
        }

        Space();
      }

      if (!renderer.MeshLocked)
      {
        // editor
        {
          Header("Editor");

          Property(ClickSelection, "Click Selection");

          Space();
        } // end: editor

        // budgets
        {
          Header("Memory Budgets");

          Property(MaxVoxelsK, 
            "Max Voxels (K)", 
                "Maximum number of voxels times 1024.\n\n" 
              + "A voxel is a minimum unit where SDFs are evaluated.\n\n"
              + "Try increasing this value if voxels appear missing.\n\n" 
              + "The higher this value, the more GPU memory is used."
          );

          Property(MaxChunks, 
            "Max Voxel Chunks", 
                "Maximum number voxel chunks.\n\n" 
              + "A voxel chunk is a block of space that can contain multiple voxels." 
              + "The larger the space spanned by solids, the more voxel chunks are needed.\n\n" 
              + "Try increasing this value if voxels appear missing.\n\n" 
              + "The higher this value, the more GPU memory is used."
          );

          Property(ShowGpuMemoryUsage,
            "(*) Show/Adjust Usage", 
            "(*): Impacts performance.");

          if (ShowGpuMemoryUsage.boolValue)
          {
            Text("   Used / Allocated: ", FontStyle.Bold);

            long memoryAllocated = renderer.LocalResourceGpuMemoryAllocated;
            long memoryUsed = Math.Min(memoryAllocated, renderer.LocalResourceGpuMemoryUsed);
            float memoryUtilizationPercent = 100.0f * (memoryUsed / Mathf.Max(MathUtil.Epsilon, memoryAllocated));
            Text
            (
                "      GPU Memory - " 
              + IntCountString(memoryUsed, true, "B") + " / " 
              + IntCountString(memoryAllocated, true, "B") + " ("
              + memoryUtilizationPercent.ToString("N1") + "%)"
            );

            int voxelsAllocated = renderer.NumVoxelsAllocated;
            int voxelsUsed = Mathf.Min(voxelsAllocated, renderer.NumVoxelsUsed);
            float voxelUtilizationPercent = 100.0f * (voxelsUsed / Mathf.Max(MathUtil.Epsilon, voxelsAllocated));
            Text
            (
                "      Voxels - " 
              + IntCountString(voxelsUsed) + " / " 
              + IntCountString(voxelsAllocated) + " (" 
              + voxelUtilizationPercent.ToString("N1") + "%)"
            );

            int chunksAllocated = renderer.NumChunksAllocated;
            int chunksUsed = Mathf.Min(chunksAllocated, renderer.NumChunksUsed);
            float chunkUtilizationPercent = 100.0f * (chunksUsed / Mathf.Max(MathUtil.Epsilon, chunksAllocated));
            Text
            (
                "      Voxel Chunks - "
              + IntCountString(chunksUsed) + " / " 
              + IntCountString(chunksAllocated) + " ("
              + chunkUtilizationPercent.ToString("N1") + "%)"
            );

            int vertsAllocated = renderer.NumVerticesAllocated;
            int vertsGenerated = Mathf.Min(vertsAllocated, renderer.NumVerticesGenerated);
            float vertUtilizationPercent = 100.0f * (vertsGenerated / Mathf.Max(MathUtil.Epsilon, vertsAllocated));
            Text
            (
                "      Vertices - " 
              + IntCountString(vertsGenerated) + " / " 
              + IntCountString(vertsAllocated) +  " (" 
              + vertUtilizationPercent.ToString("N1") + "%)"
            );

            Property(AutoAdjustBudgetsToHighWaterMarks, "  Auto-Adjust Budgets");
            Property(AutoAdjustBudgetsToHighWaterMarksMarginPercent, "    Margin Percent");

            EditorGUILayout.HelpBox("Showing GPU memory usage impacts performance.", MessageType.Warning);
          }

          Space();
        } // end budgets

        // render
        {
          Header("Render");

          Property(VoxelDensity, 
            "(*^) Voxel Density", 
                "Number of voxels per unit distance.\n\n" 
              + "Higher density means more pixels and more computation.\n\n" 
              + "(*): Impacts performance.\n" 
              + "(^): Impacts GPU memory usage."
          );

          Property(ComputeMode, 
            "Compute Mode", 
                "Controls the timing and frequency of volumetric re-computation.\n\n" 
              + "Auto - Automatic re-computation upon detection of transform changes to brushes. The downside is that transform changes to the renderer will also trigger re-computation, even if the brushes haven't changed.\n\n" 
              + "Manual - Only perform re-computation if this renderer's MarkNeedsRecompute() method is called in script. If used properly, this mode is the most computationally efficient.\n\n" 
              + "Every Frame - Force re-computation every frame.\n\n" 
              + "Time Sliced - Automatic re-computation on a lower frequency than once every frame. Useful for improving performance with staggered computation, or achieving stopmotion-like visuals."
          );

          if (ComputeMode.intValue == (int) MudRendererBase.ComputeModeEnum.TimeSliced)
          {
            Indent();
              Property(TimeSliceMode, 
                "Time Slice Mode", 
                    "By Frames Auto Offset - Automatic re-compute after this many frames, with frame offset determined by internal renderer index.\n\n" 
                  + "By Frames Manual Offset - Automatic re-compute after this many frames, with explicitly specified frame offset.\n\n" 
                  + "By Period Auto Offset - Automatic re-compute after this many seconds, with time offset determined by internal renderer index.\n\n" 
                  + "By Period Manual Offset - Automatic re-compute after this many seconds, with explicitly specified time offset.\n\n"
              );
              switch ((MudRendererBase.TimeSliceModeEnum) TimeSliceMode.intValue)
              {
                case MudRendererBase.TimeSliceModeEnum.ByFramesAutoOffset:
                  Indent();
                    Property(TimeSliceFrames, "Frames");
                  Unindent();
                  break;

                case MudRendererBase.TimeSliceModeEnum.ByFramesManualOffset:
                  Indent();
                    Property(TimeSliceFrames, "Frames");
                    Property(TimeSliceFramesOffset, "Offset");
                  Unindent();
                  break;

                case MudRendererBase.TimeSliceModeEnum.ByPeriodAutoOffset:
                  Indent();
                    Property(TimeSlicePeriod, "Period");
                  Unindent();
                  break;

                case MudRendererBase.TimeSliceModeEnum.ByPeriodManualOffset:
                  Indent();
                    Property(TimeSlicePeriod, "Period");
                    Property(TimeSliceTimeOffset, "Offset");
                  Unindent();
                  break;
              }
            Unindent();
          }

          Property(CastShadows, "Cast Shadows");

          bool receiveShadowsEffective = false;
          var camera = Camera.main;
          if (camera)
          {
            switch (MudRendererBase.RenderPipeline)
            {
              case ResourcesUtil.RenderPipelineEnum.BuiltIn:
                if (camera.actualRenderingPath == RenderingPath.Forward)
                  receiveShadowsEffective = true;
                break;
            }

            if (receiveShadowsEffective)
              Property(ReceiveShadows, "Receive Shadows");
          }

          Property(ForceEvaluateAllBrushes, 
            "Force Evaluate All Brushes", 
                "Check to force evaluation of all brushes. By default, brushes will be skipped for a voxel tree node if their AABBs are not intersecting." 
              + "Checking this option will force all brushes to be evaluated for every voxel tree node.\n\n" 
              + "This option is automatically checked in 2D mode if the 2D/3D Normal Blend value is larger than zero with the presence of any subtractive solid brush.\n\n" 
              + "This option might need to be manually checked if there are subtraction brushes in 2D mode and the SDF value is used in the shader of the render material."
          );

          /*
          Property(HardwareMode, 
            "Hardware Mode", 
                "GPU - The default and original MudBun implementation. Supports all features. Only actively tested for Windows platforms.\n\n" 
              + "CPU - Less performant implementation than GPU mode. Supports only a subset of features, but has a much wider platform compatibility."
          );
          */

          gpuMode = ((MudRendererBase.HardwareModeEnum) HardwareMode.intValue) == MudRendererBase.HardwareModeEnum.Gpu;

          Property(RenderMode, 
            "Render Mode", 
                "Smooth Mesh - Mesh with smooth normals. More performance intensive than flat mesh and splats.\n\n" 
              + "Flat Mesh - Mesh with flat normals.\n\n" 
              + "Circle Splats - Flat circle splats scattered on solid surface. Good for circular textures.\n\n" 
              + "Quad Splats - Flat quad splats scattered on solid surface. Doubles the triangle count compared to circle splats. Good for square textures.\n\n" 
              + "Decal - Decals composed of brushes.\n\n" 
              + "Ray-Marched Surface - [Experimental / URP only] Ray-based render mode that uses ray-marching to render each pixel. Performance is heavily impacted by the number of pixels rendered.\n\n" 
              + "Ray-Traced Voxels - [Experimental / URP only] Ray-based render mode that visualizes voxel cells."
          );

          if (gpuMode)
          {
            switch ((MudRendererBase.RenderModeEnum) RenderMode.intValue)
            {
              case MudRendererBase.RenderModeEnum.FlatMesh:
              case MudRendererBase.RenderModeEnum.SmoothMesh:
              case MudRendererBase.RenderModeEnum.CircleSplats:
              case MudRendererBase.RenderModeEnum.QuadSplats:
                Property(MeshingMode, 
                  "Meshing Mode", 
                      "Marching Cubes - Default meshing algorithm. Good balance between speed and quality.\n\n" 
                    + "Dual Quads - Faster than marching cubes. Gives a stylized blocky look.\n\n" 
                    + "Surface Nets - Slightly slower than marching cubes. Comes with the added benefit of blending with dual quads.\n\n" 
                    + "Dual Contouring - Much slower than surface nets, but better at preserving sharp features. Good for edit-time hard-surface modeling. Comes with the added benefit of blending with dual quads."
                );

                Property(Enable2dMode, 
                  "2D Mode", 
                  "Make everything operate on the 2D XY plane."
                );
                break;

              case MudRendererBase.RenderModeEnum.RayMarchedSurface:
                Property(MaxRayMarchSteps, "Max Ray March Steps");
                Property(RayMarchAccuracy, "Ray March Accuracy");
                Property(RayMarchMaxRayDistance, "Max Ray March Distance");
                EditorGUILayout.HelpBox("The Ray-Marched Surface render mode is experimental and works in URP only.", MessageType.Warning);
                break;

              case MudRendererBase.RenderModeEnum.RayTracedVoxels:
                Property(RayTracedVoxelMode, "Voxel Mode");
                Property(RayTracedVoxelSizeMultiplier, "Voxel Size");
                switch ((MudRendererBase.RayTracedVoxelModeEnum) RayTracedVoxelMode.intValue)
                {
                  case MudRendererBase.RayTracedVoxelModeEnum.FacetedCubes:
                    Property(RayTracedVoxelSmoothCubeNormal, "    Smooth Cube Normal");
                    break;
                  case MudRendererBase.RayTracedVoxelModeEnum.SmoothSpheres:
                  case MudRendererBase.RayTracedVoxelModeEnum.FlatSpheres:
                    Property(RayTracedVoxelSphereFullness, "    Sphere Fullness");
                    EditorGUILayout.BeginHorizontal();
                      EditorGUILayout.PrefixLabel("      Presets");
                      if (GUILayout.Button("Unit"))
                      {
                        RayTracedVoxelSphereFullness.floatValue = 0.0f;
                      }
                      if (GUILayout.Button("Cell"))
                      {
                        RayTracedVoxelSphereFullness.floatValue = 0.565f;
                      }
                      if (GUILayout.Button("Dice"))
                      {
                        RayTracedVoxelSphereFullness.floatValue = 0.765f;
                      }
                    EditorGUILayout.EndHorizontal();
                    break;
                }
                Property(RayTracedVoxelInternalPaddingDistance, 
                  "Internal Padding", 
                     "Distance within SDF isosurface to create additional internal voxels.");
                Property(RayTracedVoxelSizeFadeDistance, 
                  "Size Fade Distance", 
                     "Distance over which to fade voxel size from zero to full size starting from the isosurface.");
                EditorGUILayout.HelpBox("The Ray-Traced Voxels render mode is experimental and works in URP only.", MessageType.Warning);
                break;
            }
          }

          bool meshGenerationRenderMode = false;
          switch ((MudRendererBase.RenderModeEnum) RenderMode.intValue)
          {
            case MudRendererBase.RenderModeEnum.FlatMesh:
            case MudRendererBase.RenderModeEnum.SmoothMesh:
            case MudRendererBase.RenderModeEnum.CircleSplats:
            case MudRendererBase.RenderModeEnum.QuadSplats:
              meshGenerationRenderMode = true;
              break;
          }

          serializedObject.ApplyModifiedProperties();

          // advanced meshing options
          Property(ShowAdvancedGeometryOptions, "Advanced Geometry Options");
          if (ShowAdvancedGeometryOptions.boolValue)
          {
            Indent();
              Property(SurfaceShift, 
                "Surface Shift", 
                "Apply a surface shift along surface normals."
              );
              if (gpuMode 
                  && meshGenerationRenderMode)
              {
                switch (renderer.MeshingMode)
                {
                  case MudRendererBase.MeshingModeEnum.SurfaceNets:
                  case MudRendererBase.MeshingModeEnum.DualContouring:
                    switch (renderer.MeshingMode)
                    {
                      case MudRendererBase.MeshingModeEnum.SurfaceNets:
                        Property(SurfaceNetsDualQuadsBlend, "Dual Quads Blend");
                        //Property(SurfaceNetsBinarySearchIterations, "  Binary Search Iterations"); // hidden as this doesn't seem to buy us much
                        //Property(SurfaceNetsGradientDescentIterations, "Gradient Descent Iterations");
                        //Property(SurfaceNetsGradientDescentFactor, "Gradient Descent Factor");
                        //Property(SurfaceNetsHighAccuracyMode, "(*) High Accuracy");
                        break;
      
                      case MudRendererBase.MeshingModeEnum.DualContouring:
                        Property(DualContouringDualQuadsBlend, "Dual Quads Blend");
                        Property(DualContouringRelaxation, "Relaxation");
                        Property(DualContouringSolverIterations, "Solver Iterations");
                        //Property(DualContouringBinarySearchIterations, "Binary Search Iterations");
                        //Property(DualContouringGradientDescentIterations, "Gradient Descent Iterations");
                        //Property(DualContouringGradientDescentFactor, "Gradient Descent Factor");
                        Property(DualContouringHighAccuracyMode, 
                          "(*) High Accuracy", 
                          "(*): Impacts performance."
                        );
                        break;
                    }
                    break;
                }
              }
            Unindent();
          } // end: advanced meshing options

          // advanced normal options
          if (gpuMode)
          {
            switch (renderer.MeshingMode)
            {
              case MudRendererBase.MeshingModeEnum.MarchingCubes:
              case MudRendererBase.MeshingModeEnum.SurfaceNets:
              case MudRendererBase.MeshingModeEnum.DualContouring:
                switch (renderer.RenderMode)
                {
                  case MudRendererBase.RenderModeEnum.FlatMesh:
                  case MudRendererBase.RenderModeEnum.SmoothMesh:
                    Property(ShowAdvancedNormalOptions, "Advanced Normal Options");
                    if (ShowAdvancedNormalOptions.boolValue)
                    {
                      Indent();
                        if (Enable2dMode.boolValue)
                        {
                          Property(Normal2dFade, 
                            "2D Normal Fade", 
                            "How far inward to fade from outward-pointing normals to flat normals."
                          );
                          Property(Normal2dStrength, 
                            "2D Normal Strength", 
                            "How strong the outward-pointing normals contribute to the final normals."
                          );
                        }
                        Property(NormalQuantization, "Normal Quantization");
                        if (!Enable2dMode.boolValue)
                          Property(EnableAutoSmoothing, "Enable Auto-Smoothing (^)");
                        if (EnableAutoSmoothing.boolValue && !Enable2dMode.boolValue)
                        {
                          Indent();
                            Property(AutoSmoothingMaxAngle, "Auto-Smoothing Max Angle");
                            if (renderer.MeshingMode == MudRendererBase.MeshingModeEnum.DualContouring)
                            {
                              Property(EnableSmoothCorner, 
                                "(*) Enable Smooth Corner", 
                                "(*): Impacts performance."
                              );
                              if (EnableSmoothCorner.boolValue)
                              {
                                Indent();
                                  Property(SmoothCornerSubdivision, 
                                    "(*) Subdivision", 
                                    "(*): Impacts performance."
                                  );
                                  Property(SmoothCornerNormalBlur, "Blur");
                                  Property(SmoothCornerFade, "Fade");
                                Unindent();
                              }
                            }
                          Unindent();
                        }
                        else
                        {
                          if (renderer.RenderMode == MudRendererBase.RenderModeEnum.SmoothMesh)
                            Property(SmoothNormalBlurAbsolute, "Smooth Normal Blur");
                        }
                      Unindent();
                    }
                    break;
                }
                break;
              }
          } // end: advanced normal options

          // advanced splat options
          switch (renderer.RenderModeCategory)
          {
            case MudRendererBase.RenderModeCategoryEnum.Splats:
              Property(ShowAdvancedSplatOptions, "Advanced Splats Options");
              if (ShowAdvancedSplatOptions.boolValue)
              {
                Indent();
                  Property(SplatSize, "Size");
                  Property(SplatSizeJitter, "Size Jitter");
                  Property(SplatNormalShift, "Normal Shift");
                  Property(SplatNormalShiftJitter, "Normal Shift Jitter");
                  Property(SplatColorJitter, "Color Jitter");
                  Property(SplatPositionJitter, "Position Jitter");
                  Property(SplatRotationJitter, "Rotation Jitter");
                  Property(SplatOrientationJitter, "Orientation Jitter");
                  Property(SplatOriginalNormalBlend, "  Original Normal Blend");
                  Property(SplatJitterNoisiness, "Jitter Noisiness");
                  Property(SplatCameraFacing, "Camera Facing");
                  Property(SplatNormalsMatchCameraFacing, "  Normals Match Camera Facing");
                  Property(SplatShadowsMatchCameraFacing, "  Shadows Match Camera Facing");
                  Property(SplatScreenSpaceFlattening, "Screen-Space Flattening");
                  //Property(SplatSmoothNormalBlend, "Smooth Normal Blend");
                Unindent();
              }
              break;
          } // end: advanced splat options

          Space();
        } // end: render

        ShowMaterialOptions();

        // shaders
        {
          Header("Resources");

          if (GUILayout.Button(new GUIContent("Reload (Needed After Shader Edits)", "Reload shaders and GPU resources. This is necessary after editing shaders.")))
          {
            MudRendererBase.ReloadAllShaders();
          }

          Space();
        } // end: shaders

        // mesh utilities
        if (gpuMode 
            && !Enable2dMode.boolValue)
        {
          Header("Mesh Utilities");

          if (MudBun.IsFreeVersion)
          {
            Text("Trial Version Limitations:");
            Text("  - Voxel density limited to " + ((int) MudRendererBase.MaxMeshGenerationVoxelDensityFreeVersion));
            Text("  - Triangle count limited to " + MudRendererBase.MaxMeshGenerationTrianglesFreeVersion);
          }
          
          Property(MeshGenerationLockOnStart, 
            "Lock On Start", 
                "Lock mesh on start in play mode. This can save file size by not saving the mesh, but the performance will take a hit on lock."
          );

          Property(MeshGenerationCreateNewObject, 
            "Create New Object", 
                "Check to create a new object when locking mesh."
          );

          Property(MeshGenerationCreateCollider, 
            "Create Collider", 
                "Check to create a collider when locking mesh."
          );
          if (MeshGenerationCreateCollider.boolValue)
          {
            Indent();
              Property(MeshGenerationColliderVoxelDensity, 
                "Collider Voxel Density", 
                    "Voxel density used for creating collider."
              );
              Property(MeshGenerationForceConvexCollider, 
                "Force Convex Collider"
              );
              Property(MeshGenerationColliderSurfaceShift, 
                "Surface Shift"
              );
              Property(MeshGenerationCreateRigidBody, 
                "Create Rigid Body (Convex)"
              );
              Property(GenerateColliderMeshAssetByEditor, 
                "Generate Mesh Asset", 
                   "Generate a mesh asset. This is needed for prefabs that contain collider meshes."
              );
              if (GenerateColliderMeshAssetByEditor.boolValue)
              {
                if (renderer.GenerateColliderMeshAssetByEditorName.Equals(""))
                {
                  serializedObject.ApplyModifiedProperties();
                  renderer.ValidateAssetNames();
                  serializedObject.Update();
                }
                Indent();
                  Property(GenerateColliderMeshAssetByEditorName, "File Name" );
                Unindent();
              }
            Unindent();
          }

          Property(MeshGenerationRenderableMeshMode, 
            "Renderable Mesh Mode", 
                "None - No renderable mesh is created.\n\n" 
              + "Procedural - Run compute shader once and keep rendering from computed data. No standard mesh will be generated nor serialized.\n\n" 
              + "Mesh Renderer - Run compute shader once and generate a standard mesh. The mesh will be serialized and rendererd using Unity's MeshRenderer or SkinnedMeshRenderer."
          );

          if ((MudRendererBase.RenderableMeshMode) MeshGenerationRenderableMeshMode.intValue == MudRendererBase.RenderableMeshMode.MeshRenderer)
          {
            Indent();
              Property(MeshGenerationAutoRigging, 
                "Auto-Rigging", 
                    "Check to auto-rig locked mesh with brushes flagged as bones."
              );
              Property(MeshGenerationGenerateTextureUV, 
                "Generate Texture UV (UV1)"
              );
              Property(MeshGenerationGenerateLightMapUV, 
                "Generate Light Map UV (UV2)"
              );
              Property(MeshGenerationWeldVertices, 
                "Weld Vertices"
              );
              Property(GenerateMeshAssetByEditor, 
                "Generate Mesh Asset", 
                    "Generate a mesh asset. This is needed for prefabs that contain locked meshes."
              );
              if (GenerateMeshAssetByEditor.boolValue)
              {
                if (renderer.GenerateMeshAssetByEditorName.Equals(""))
                {
                  serializedObject.ApplyModifiedProperties();
                  renderer.ValidateAssetNames();
                  serializedObject.Update();
                }
                Indent();
                  Property(GenerateMeshAssetByEditorName, "File Name" );
                Unindent();
              }
            Unindent();
          }

          Property(RecursiveLockMeshByEditor, 
            "Recursive Lock", 
                "Recursively lock all renderers within this renderer's hierarchy."
          );

          /*
          if (GUILayout.Button("Rectify Non-Unit-Scaled Parents"))
          {
            renderer.RectifyNonUnitScaledParents();
          }
          */

          if (GUILayout.Button("Lock Mesh"))
          {
            LockMesh();
            renderer.MeshGenerationLockOnStartByEditor = true;

            InitSerializedProperties();
          }

          Space();
        } // end: mesh utilities

        // SDF utilities
        if (gpuMode)
        {
          Header("SDF Utilities");

          Property(DrawGenerateSdfGizmos, "Draw SDF Generation Gizmos" );

          if (renderer.GenerateSdfByEditorName.Equals(""))
          {
            serializedObject.ApplyModifiedProperties();
            renderer.ValidateAssetNames();
            serializedObject.Update();
          }
          Property(GenerateSdfByEditorName, "File Name" );

          Property(GenerateSdfCenter, 
            "SDF Center", 
                "Point in SDF mapped to center of SDF texture"
          );
          Property(GenerateSdfDimension, 
            "SDF Dimension", 
                "Dimension in SDF mapped to size of SDF texture"
          );
          Property(GenerateSdfTextureSize, "SDF Texture Size" );

          if (GUILayout.Button("Generate SDF Texture"))
          {
            GenerateSdf();
          }

          Space();
        } // end: SDF utilities

        // optimization
        {
          // TODO
          /*
          Header("Optimization");

          Property(UseCutoffVolume, "Use Cutoff Volume");
          if (UseCutoffVolume.boolValue)
          {
            Indent();
              Property(CutoffVolumeCenter, "Center");
              Property(CutoffVolumeSize, "Size");
            Unindent();
          }

          Space();
          */
        } // end: optimziation

        // debug
        {
          Header("Debug");

          Property(AlwaysDrawGizmos, "Always Draw Gizmos");

          Property(DrawRawBrushBounds, 
            "Draw Raw Brush Bounds", 
                "Draw raw bounding volumes for each brush."
          );

          Property(DrawComputeBrushBounds, 
            "Draw Compute Bounds", 
                "Draw the expanded bounding volume actually used for computation for each brush."
          );

          Property(DrawRenderBounds, 
            "Draw Render Bounds", 
                "Draw the bounding volume used for culling the renderer."
          );

          Property(DrawVoxelNodes, 
            "Draw Voxel Nodes", 
                "Draw hierarchical voxel nodes."
          );
          Indent();
            Property(DrawVoxelNodesDepth, 
              "Node Depth", 
                  "Draw voxel nodes at a specific hierarchical depth.\n\n" 
                + "-1 means drawing all depths."
            );
            Property(DrawVoxelNodesScale, "Node Scale");
          Unindent();

          Space();
        } // end: debug

        // extra info
        {
          Header("Extra Info");
          Text($"Current Tracked Version: {renderer.CurrentTrackedVersion}");
          Text($"Previous Tracked Version: {renderer.PreviousTrackedVersion}");
          Text($"First Tracked Version: {renderer.FirstTrackedVersion}");
          Space();
        } // end: extra info

        // legends
        {
          Header("Legends");

          Text("(*): Impacts performance.");
          Text("(^): Impacts GPU memory usage.");
        } // end: legends
      }
      else // mesh locked?
      {
        if (MeshGenerationRenderableMeshMode.intValue == (int) MudRendererBase.RenderableMeshMode.Procedural)
          ShowMaterialOptions();

        Header("Mesh Utilities");

        Property(RecursiveLockMeshByEditor, 
          "Recursive Unlock", 
              "Recursively unlock all renderers within this renderer's hierarchy."
        );

        Property(RememberLockedMeshMaterialByEditor, "Remember Material");

        if (GUILayout.Button("Unlock Mesh"))
        {
          foreach (var r in targets.Select(x => (MudRenderer) x))
          {
            if (r == null)
              continue;

            if (!r.MeshLocked)
              continue;

            if (RememberLockedMeshMaterialByEditor.boolValue)
              renderer.RememberLockedMeshMaterial();

            UnlockMesh(r.transform, RecursiveLockMeshByEditor.boolValue);
          }
        }
      }

      serializedObject.ApplyModifiedProperties();
    }

    protected virtual void LockMesh() { }

    private void UnlockMesh(Transform t, bool recursive)
    {
      if (t == null)
        return;

      if (recursive)
      {
        for (int i = 0; i < t.childCount; ++i)
          UnlockMesh(t.GetChild(i), recursive);
      }

      var renderer = t.GetComponent<MudRenderer>();
      if (renderer == null)
        return;

      if (!renderer.MeshLocked)
        return;

      renderer.UnlockMesh();
    }

    protected virtual void GenerateSdf() { }

    private static readonly float RepaintInterval = 0.2f;
    private float m_repaintTimer = 0.0f;
    private void Update()
    {
      m_repaintTimer += Time.deltaTime;
      if (m_repaintTimer < RepaintInterval)
        return;

      m_repaintTimer = Mathf.Repeat(m_repaintTimer, RepaintInterval);

      try
      {
        if (Selection.activeGameObject != ((MudRendererBase) target).gameObject)
          return;
      }
      catch (MissingReferenceException)
      {
        // renderer has been destroyed
        return;
      }

      UpdateGpuMemoryUsage();
    }

    private void UpdateGpuMemoryUsage()
    {
      if (!ShowGpuMemoryUsage.boolValue)
        return;
        
      Repaint();
    }

    protected void DestroyAllChildren(Transform t, bool isRoot = true)
    {
      if (t == null)
        return;

      var aChild = new Transform[t.childCount];
      for (int i = 0; i < t.childCount; ++i)
        aChild[i] = t.GetChild(i);
      foreach (var child in aChild)
        DestroyAllChildren(child, false);

      if (!isRoot)
        DestroyImmediate(t.gameObject);
    }

    public bool HasFrameBounds()
    {
      return true;
    }

    public Bounds OnGetFrameBounds()
    {
      var renderer = (MudRendererBase) target;
      var bounds = Aabb.Empty;
      foreach (var brush in renderer.Brushes)
        if (brush != null)
          bounds.Include(brush.BoundsRs);

      if (bounds.IsEmpty)
        return new Bounds(renderer.transform.position, Vector3.one);

      bounds.Expand(renderer.SurfaceShift);
      bounds.Transform(renderer.transform);

      return new Bounds(bounds.Center, bounds.Size);
    }
  }
}

