/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

using UnityEngine;

namespace MudBun
{
  public class TextureUtil
  {
    private static readonly int ThreadGroupSize = 32;

    public static Texture3D RenderTextureToTexture3D(Texture3D output, RenderTexture rt)
    {
      Vector3Int dimensions = new Vector3Int(rt.width, rt.height, rt.volumeDepth);

      var textureSlicer = ResourcesUtil.TextureSlicer;
      if (textureSlicer == null)
        return null;

      Texture2D[] slices = new Texture2D[dimensions.z];

      textureSlicer.SetInt("resolution", dimensions.z);
      textureSlicer.SetTexture(0, "volumeTexture", rt);

      for (int layer = 0; layer < dimensions.z; ++layer)
      {
        var renderTexture = new RenderTexture(dimensions.x, dimensions.y, 0, RenderTextureFormat.RFloat);
        renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        textureSlicer.SetTexture(0, "slice", renderTexture);
        textureSlicer.SetInt("layer", layer);
        textureSlicer.Dispatch(0, Mathf.CeilToInt(dimensions.x / (float) ThreadGroupSize), Mathf.CeilToInt(dimensions.y / (float)ThreadGroupSize), 1);

        slices[layer] = ConvertFromRenderTexture(renderTexture);
      }

      var tex = Tex3DFromTex2DArray(output, slices, dimensions);
      return tex;
    }

    private static Texture3D Tex3DFromTex2DArray(Texture3D output, Texture2D[] slices, Vector3Int dimensions)
    {
      if (output == null)
        output = new Texture3D(dimensions.x, dimensions.y, dimensions.z, TextureFormat.RFloat, false);

      output.filterMode = FilterMode.Trilinear;
      Color[] outputPixels = output.GetPixels();

      for (int z = 0; z < dimensions.z; z++)
      {
        Color c = slices[z].GetPixel(0, 0);
        Color[] layerPixels = slices[z].GetPixels();
        for (int x = 0; x < dimensions.x; x++)
          for (int y = 0; y < dimensions.y; y++)
          {
            outputPixels[x + dimensions.x * (y + z * dimensions.y)] = layerPixels[x + y * dimensions.x];
          }
      }

      output.SetPixels(outputPixels);
      output.Apply();

      return output;
    }

    private static Texture2D ConvertFromRenderTexture(RenderTexture rt)
    {
      Texture2D output = new Texture2D(rt.width, rt.height, TextureFormat.RFloat, false);
      RenderTexture.active = rt;
      output.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
      output.Apply();
      return output;
    }
  }
}
