/*****************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

using UnityEngine;
using UnityEngine.Rendering;

namespace MudBun
{
  public class ResourcesUtil
  {
    public enum RenderPipelineEnum
    {
      Unknown = -1,
      BuiltIn,
      URP,
      HDRP,
      Count
    }

    public static RenderPipelineEnum DetermineRenderPipeline(bool skipCache = false)
    {
      if (!skipCache && s_renderPipeline >= 0)
        return s_renderPipeline;

      string rpAsset = "";
      if (GraphicsSettings.renderPipelineAsset != null)
        rpAsset = GraphicsSettings.renderPipelineAsset.GetType().Name;

      if (rpAsset.Equals("HDRenderPipelineAsset"))
        s_renderPipeline = RenderPipelineEnum.HDRP;
      else if (rpAsset.Equals("UniversalRenderPipelineAsset"))
        s_renderPipeline = RenderPipelineEnum.URP;
      else
        s_renderPipeline = RenderPipelineEnum.BuiltIn;

      return s_renderPipeline;
    }

    protected static RenderPipelineEnum s_renderPipeline = (RenderPipelineEnum)(-1);
    public static RenderPipelineEnum RenderPipeline => DetermineRenderPipeline();

    public static ComputeShader VoxelGen => GetComputeResource(PathUtil.VoxelGen);
    public static ComputeShader MarchingCubes => GetComputeResource(PathUtil.MarchingCubes);
    public static ComputeShader DualMeshing => GetComputeResource(PathUtil.DualMeshing);
    public static ComputeShader SurfaceNets => GetComputeResource(PathUtil.SurfaceNets);
    public static ComputeShader DualContouring => GetComputeResource(PathUtil.DualContouring);
    public static ComputeShader RayTracedVoxels => GetComputeResource(PathUtil.RayTracedVoxels);
    public static ComputeShader NoiseCache => GetComputeResource(PathUtil.NoiseCache);
    public static ComputeShader TextureSlicer => GetComputeResource(PathUtil.TextureSlicer);
    public static ComputeShader MeshLock => GetComputeResource(PathUtil.MeshLock);
    public static ComputeShader SdfGen => GetComputeResource(PathUtil.SdfGen);

    public static Texture NoiseTexture => GetTextureResource(PathUtil.NoiseTexture);

    public static ComputeShader GetComputeResource(string resourcePath)
    {
      var compute = Resources.Load<ComputeShader>(resourcePath);
      if (compute == null)
        Debug.LogError($"MudBun: Compute shader \"{PathUtil.ResourceRoot}/{resourcePath}.compute\" not found.");
      return compute;
    }

    public static Texture GetTextureResource(string resourcePath)
    {
      var texture = Resources.Load<Texture>(resourcePath);
      if (texture == null)
        Debug.LogError($"MudBun: Texture \"{PathUtil.ResourceRoot}/{resourcePath}.*\" not found.");
      return texture;
    }

    // this needs to be available at run-time
    public static Material DefaultLockedMeshMaterial => GetMaterialResource(PathUtil.DefaultLockedMeshMaterial);

    public static Material GetMaterialResource(string resourcePath)
    {
      var compute = Resources.Load<Material>(resourcePath);
      if (compute == null)
        Debug.LogError($"MudBun: Material \"{PathUtil.ResourceRoot}/{resourcePath}.mat\" not found.");
      return compute;
    }
  }
}

