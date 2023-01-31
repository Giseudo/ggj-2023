/*****************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

namespace MudBun
{
  public class PathUtil
  {
    #if MUDBUN_FREE
    public static string ResourceRoot => "MudBunFree/Resources";
    public static string MaterialRoot => "MudBunFree/Material";
    #else
    public static string ResourceRoot => "MudBun/Resources";
    public static string MaterialRoot => "MudBun/Material";
    #endif

    public static string CommonFolder => $"Common";
    public static string ComputeFolder => $"Compute";
    public static string RenderFolder = $"Render";
    public static string MaterialFolder => $"{MaterialRoot}";

    public static string GetRenderPipelineFull(ResourcesUtil.RenderPipelineEnum renderPipeline)
    {
      switch (renderPipeline)
      {
        case ResourcesUtil.RenderPipelineEnum.BuiltIn:
          return "Built-In RP";
        case ResourcesUtil.RenderPipelineEnum.URP:
          return "URP";
        case ResourcesUtil.RenderPipelineEnum.HDRP:
          return "HDRP";
      }

      return "";
    }
    public static string RenderPipelineFull => GetRenderPipelineFull(MudRendererBase.RenderPipeline);

    public static string GetRenderPipelinePacked(ResourcesUtil.RenderPipelineEnum renderPipeline)
    {
      switch (renderPipeline)
      {
        case ResourcesUtil.RenderPipelineEnum.BuiltIn:
          return "BuiltInRP";
        case ResourcesUtil.RenderPipelineEnum.URP:
          return "URP";
        case ResourcesUtil.RenderPipelineEnum.HDRP:
          return "HDRP";
      }
      return "UNKNOWN";
    }
    public static string RenderPipelinePacked => GetRenderPipelinePacked(MudRendererBase.RenderPipeline);

    public static string VoxelGen => $"{ComputeFolder}/VoxelGen";
    public static string MarchingCubes => $"{ComputeFolder}/MarchingCubes";
    public static string DualMeshing => $"{ComputeFolder}/DualMeshing";
    public static string SurfaceNets => $"{ComputeFolder}/SurfaceNets";
    public static string DualContouring => $"{ComputeFolder}/DualContouring";
    public static string RayTracedVoxels => $"{ComputeFolder}/RayTracedVoxels";
    public static string NoiseCache => $"{ComputeFolder}/NoiseCache";
    public static string TextureSlicer => $"{ComputeFolder}/TextureSlicer";
    public static string MeshLock => $"{ComputeFolder}/MeshLock";
    public static string SdfGen => $"{ComputeFolder}/SdfGen";

    public static string NoiseTexture => $"{CommonFolder}/PerlinNoise";

    public static string DefaultLockedMeshMaterial => $"{RenderFolder}/{RenderPipelineFull}/Default Mud Locked Mesh ({RenderPipelineFull})";

    public static string DefaultMeshSingleTexturedMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Default Mud Mesh Single-Textured ({RenderPipelineFull})";
    public static string DefaultSplatSingleTexturedMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Default Mud Splat Single-Textured ({RenderPipelineFull})";
    public static string DefaultMeshMultiTexturedMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Default Mud Mesh Multi-Textured ({RenderPipelineFull})";
    public static string DefaultSplatMultiTexturedMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Default Mud Splat Multi-Textured ({RenderPipelineFull})";
    public static string DefaultDecalMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Default Mud Decal ({RenderPipelineFull})";
    public static string DefaultRayMarchedSurfaceMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Default Mud Ray-Marched Surface ({RenderPipelineFull})";
    public static string DefaultRayTracedVoxelMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Default Mud Ray-Traced Voxels ({RenderPipelineFull})";

    public static string AlphaBlendedTransparentMeshMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Presets/Alpha-Blended Transparent Mesh Render Material ({RenderPipelineFull})";
    public static string ClayMeshMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Presets/Clay Mesh Render Material ({RenderPipelineFull})";
    public static string ClaymationMeshMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Presets/Claymation Mesh Render Material ({RenderPipelineFull})";
    public static string OutlineMeshMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Presets/Outline Mesh Render Material ({RenderPipelineFull})";
    public static string SdfRippleMeshMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Presets/SDF Ripple Mesh Render Material ({RenderPipelineFull})";
    public static string StopmotionMeshMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Presets/Stopmotion Mesh Render Material ({RenderPipelineFull})";

    public static string BrushStrokesSplatMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Presets/Brush Strokes Splat Render Material ({RenderPipelineFull})";
    public static string FloaterSplatMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Presets/Floater Splat Render Material ({RenderPipelineFull})";
    public static string FloofSplatMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Presets/Floof Splat Render Material ({RenderPipelineFull})";
    public static string LeafSplatMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Presets/Leaf Splat Render Material ({RenderPipelineFull})";
    public static string StopmotionSplatMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Presets/Stopmotion Splat Render Material ({RenderPipelineFull})";

    public static string DecalPaintMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Presets/Decal Paint Render Material ({RenderPipelineFull})";
    public static string DecalDarkenMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Presets/Decal Darken Render Material ({RenderPipelineFull})";
    public static string DecalLightenMaterial => $"{MaterialFolder}/{RenderPipelineFull}/Presets/Decal Lighten Render Material ({RenderPipelineFull})";
  }
}

