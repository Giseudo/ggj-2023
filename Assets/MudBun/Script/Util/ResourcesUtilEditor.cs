/*****************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#if UNITY_EDITOR
using System.Collections.Generic;

using UnityEditor;
#endif

using UnityEngine;

namespace MudBun
{
  public class ResourcesUtilEditor
  {
#if UNITY_EDITOR
    // default init/fallback materials
    public static Material DefaultMeshMaterial => DefaultMeshSingleTexturedMaterial;
    public static Material DefaultSplatMaterial => DefaultSplatSingleTexturedMaterial;
    public static Material DefaultDecalMaterial => GetMaterial(PathUtil.DefaultDecalMaterial);
    public static Material DefaultRayMarchedSurfaceMaterial => GetMaterial(PathUtil.DefaultRayMarchedSurfaceMaterial, true);
    public static Material DefaultRayTracedVoxelMaterial => GetMaterial(PathUtil.DefaultRayTracedVoxelMaterial, true);

    // default materials
    public static Material DefaultMeshSingleTexturedMaterial => GetMaterial(PathUtil.DefaultMeshSingleTexturedMaterial);
    public static Material DefaultSplatSingleTexturedMaterial => GetMaterial(PathUtil.DefaultSplatSingleTexturedMaterial);
    public static Material DefaultMeshMultiTexturedMaterial => GetMaterial(PathUtil.DefaultMeshMultiTexturedMaterial);
    public static Material DefaultSplatMultiTexturedMaterial => GetMaterial(PathUtil.DefaultSplatMultiTexturedMaterial);

    // preset mesh materials
    public static Material AlphaBlendedTransparentMeshMaterial => GetMaterial(PathUtil.AlphaBlendedTransparentMeshMaterial);
    public static Material ClayMeshMaterial => GetMaterial(PathUtil.ClayMeshMaterial);
    public static Material ClaymationMeshMaterial => GetMaterial(PathUtil.ClaymationMeshMaterial);
    public static Material OutlineMeshMaterial => GetMaterial(PathUtil.OutlineMeshMaterial);
    public static Material SdfRippleMeshMaterial => GetMaterial(PathUtil.SdfRippleMeshMaterial);
    public static Material StopmotionMeshMaterial => GetMaterial(PathUtil.StopmotionMeshMaterial);

    // preset splat materials
    public static Material BrushStrokesSplatMaterial => GetMaterial(PathUtil.BrushStrokesSplatMaterial);
    public static Material FloaterSplatMaterial => GetMaterial(PathUtil.FloaterSplatMaterial);
    public static Material FloofSplatMaterial => GetMaterial(PathUtil.FloofSplatMaterial);
    public static Material LeafSplatMaterial => GetMaterial(PathUtil.LeafSplatMaterial);
    public static Material StopmotionSplatMaterial => GetMaterial(PathUtil.StopmotionSplatMaterial);

    // preset decal materials
    public static Material DecalPaintMaterial => GetMaterial(PathUtil.DecalPaintMaterial);
    public static Material DecalDarkenMaterial => GetMaterial(PathUtil.DecalDarkenMaterial);
    public static Material DecalLightenMaterial => GetMaterial(PathUtil.DecalLightenMaterial);

    private static Dictionary<string, Material> s_materialMap = new Dictionary<string, Material>();

    public static Material GetMaterial(string path, bool allowNull = false)
    {
      if (s_materialMap.TryGetValue(path, out var existingMat))
        return existingMat;

      var mat = AssetDatabase.LoadAssetAtPath<Material>($"Assets/{path}.mat");

      if (mat == null 
          && !allowNull)
      {
        Debug.LogError($"MudBun: Cannot load renderer material at \"{path}.mat\". Did you forget to import \"{PathUtil.RenderPipelineFull}\"?");
      }

      if (mat != null)
        s_materialMap[path] = mat;

      return mat;
    }
#endif
  }
}

