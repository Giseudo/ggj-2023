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

using UnityEngine;

namespace MudBun
{
  public class MeshUtil
  {
    public static int EmissionHashUvIndex = 2;
    public static int MetallicSmoothnessUvIndex = 3;

    public static readonly float PositionTolerance = 1e-4f;
    public static readonly float NormalTolerance = 1e-2f;
    public static readonly float UvTolerance = 1e-4f;
    public static readonly float PositionToleranceSqr = PositionTolerance * PositionTolerance;
    public static readonly float NormalToleratnceSqr = NormalTolerance * NormalTolerance;
    public static readonly float UvToleratnceSqr = UvTolerance * UvTolerance;

    struct VertKey
    {
      public Vector3 Pos;
      public Vector3 Norm;
      public Vector2 Uv;

      public override int GetHashCode()
      {
        int hash = Codec.Hash(Pos);
        hash = Codec.HashConcat(hash, Norm);
        hash = Codec.HashConcat(hash, Uv);
        return hash;
      }

      public override bool Equals(object obj)
      {
          return 
            obj is VertKey other 
            && (Pos - other.Pos).sqrMagnitude < PositionToleranceSqr + MathUtil.Epsilon 
            && (Norm - other.Norm).sqrMagnitude < NormalToleratnceSqr + MathUtil.Epsilon 
            && (Uv - other.Uv).sqrMagnitude < UvToleratnceSqr + MathUtil.Epsilon;
      }
    }

    private static readonly Vector3[] s_aRenderBoxProxyVert = 
    {
      new Vector3(-0.5f, -0.5f, -0.5f), 
      new Vector3( 0.5f, -0.5f, -0.5f), 
      new Vector3(-0.5f,  0.5f, -0.5f), 
      new Vector3( 0.5f,  0.5f, -0.5f), 
      new Vector3(-0.5f, -0.5f,  0.5f), 
      new Vector3( 0.5f, -0.5f,  0.5f), 
      new Vector3(-0.5f,  0.5f,  0.5f), 
      new Vector3( 0.5f,  0.5f,  0.5f), 
    };

    private static readonly int[] s_aRenderBoxProxyIndex = 
    {
       0, 1, 3, 0, 3, 2, 
       0, 2, 6, 0, 6, 4, 
       0, 4, 5, 0, 5, 1, 
       7, 6, 2, 7, 2, 3, 
       7, 5, 4, 7, 4, 6, 
       7, 3, 1, 7, 1, 5, 
    };

    private static Vector2 Quantize(Vector2 v, float step)
    {
      Vector2 s = new Vector2(Mathf.Sign(v.x), Mathf.Sign(v.y));
      v += 0.5f * step * Vector2.one;
      v = VectorUtil.CompDiv(v, step * Vector3.one);
      v = VectorUtil.Abs(v);
      v = new Vector2(Mathf.Floor(v.x), Mathf.Floor(v.y));
      v = VectorUtil.CompMul(s * step, v);
      return v;
    }

    private static Vector3 Quantize(Vector3 v, float step)
    {
      Vector3 s = new Vector3(Mathf.Sign(v.x), Mathf.Sign(v.y), Mathf.Sign(v.z));
      v += 0.5f * step * Vector3.one;
      v = VectorUtil.CompDiv(v, step * Vector3.one);
      v = VectorUtil.Abs(v);
      v = new Vector3(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z));
      v = VectorUtil.CompMul(s * step, v);
      return v;
    }

    public static void Weld(Mesh mesh, int textureUvIndex = -1)
    {
      var aOldVert = mesh.vertices;
      var aOldNorm = mesh.normals;
      var aOldColor = mesh.colors;
      var aOldBoneWeight = mesh.boneWeights;
      var aOldBindPose = mesh.bindposes;
      var aOldTextureUv = textureUvIndex >= 0 ? new List<Vector2>() : null;
      var aOldEmissionHash = new List<Vector4>();
      var aOldMetallicSmoothness = new List<Vector2>();
      if (textureUvIndex >= 0)
        mesh.GetUVs(0, aOldTextureUv);
      mesh.GetUVs(EmissionHashUvIndex, aOldEmissionHash);
      mesh.GetUVs(MetallicSmoothnessUvIndex, aOldMetallicSmoothness);

      var aOldIndex = mesh.GetIndices(0);

      //var vertToIndexMap = new Dictionary<int, int>();
      var vertToIndexMap = new Dictionary<VertKey, int>();
      var indexToIndexMap = new int[aOldVert.Length];
      for (int i = 0; i < aOldIndex.Length; ++i)
      {
        int index = aOldIndex[i];
        var key = 
          new VertKey 
          { 
            Pos = Quantize(aOldVert[index], PositionTolerance), 
            Norm = Quantize(aOldNorm[index], NormalTolerance), 
            Uv = textureUvIndex >= 0 ? Quantize(aOldTextureUv[index], UvTolerance) : Vector2.zero
          };

        int newIndex = -1;
        if (!vertToIndexMap.TryGetValue(key, out newIndex))
        {
          newIndex = vertToIndexMap.Count;
          vertToIndexMap.Add(key, newIndex);

          // debugger-friendly duplicate code
          indexToIndexMap[i] = newIndex;
        }
        else
        {
          // debugger-friendly duplicate code
          indexToIndexMap[i] = newIndex;
        }
      }

      int numUniqueVerts = vertToIndexMap.Count;
      var aNewVert = new Vector3[numUniqueVerts];
      var aNewNorm = new Vector3[numUniqueVerts];
      var aNewColor = new Color[numUniqueVerts];
      var aNewTextureUv = textureUvIndex >= 0 ? new Vector2[numUniqueVerts] : null;
      var aNewEmissionHash = new Vector4[numUniqueVerts];
      var aNewMetallicSmoothness = new Vector2[numUniqueVerts];
      var aNewBoneWeight = new BoneWeight[numUniqueVerts];
      var aNewBindPose = aOldBindPose; // bind poses aren't changed
      for (int oldIndex = 0; oldIndex < indexToIndexMap.Length; ++oldIndex)
      {
        int newIndex = indexToIndexMap[oldIndex];
        aNewVert[newIndex] = aOldVert[oldIndex];
        aNewNorm[newIndex] = aOldNorm[oldIndex];
        aNewColor[newIndex] = aOldColor[oldIndex];
        if (textureUvIndex >= 0)
          aNewTextureUv[newIndex] = aOldTextureUv[oldIndex];
        aNewEmissionHash[newIndex] = aOldEmissionHash[oldIndex];
        aNewMetallicSmoothness[newIndex] = aOldMetallicSmoothness[oldIndex];

        if (aOldBoneWeight != null && aOldBoneWeight.Length >= aOldVert.Length)
          aNewBoneWeight[newIndex] = aOldBoneWeight[oldIndex];
      }

      var aNewIndex = new int[aOldIndex.Length];
      for (int i = 0; i < aOldIndex.Length; ++i)
      {
        aNewIndex[i] = indexToIndexMap[aOldIndex[i]];
      }

      var topology = mesh.GetTopology(0);
      mesh.Clear();
      mesh.SetVertices(aNewVert);
      mesh.SetNormals(aNewNorm);
      mesh.SetColors(aNewColor);
      if (aOldBoneWeight != null)
      {
        mesh.boneWeights = aNewBoneWeight;
        mesh.bindposes = aNewBindPose;
      }
      if (textureUvIndex >= 0)
      {
        mesh.SetUVs(textureUvIndex, aNewTextureUv);
      }
      mesh.SetUVs(EmissionHashUvIndex, aNewEmissionHash);
      mesh.SetUVs(MetallicSmoothnessUvIndex, aNewMetallicSmoothness);
      mesh.SetIndices(aNewIndex, topology, 0);
    }

    private static Vector3[] s_aRenderBoxProxyVertBuffer;
    public static void UpdateRenderBoxProxy(ref Mesh mesh, Aabb bounds)
    {
      if (mesh == null)
      {
        mesh = new Mesh();
      }

      if (s_aRenderBoxProxyVertBuffer == null 
         || s_aRenderBoxProxyVertBuffer.Length != s_aRenderBoxProxyVert.Length)
      {
        s_aRenderBoxProxyVertBuffer = new Vector3[s_aRenderBoxProxyVert.Length];
      }

      Vector3 size = bounds.Size;
      Vector3 center = bounds.Center;

      for (int i = 0, n = s_aRenderBoxProxyVert.Length; i < n; ++i)
      {
        s_aRenderBoxProxyVertBuffer[i] = VectorUtil.CompMul(size, s_aRenderBoxProxyVert[i]) + center;
      }

      mesh.vertices = s_aRenderBoxProxyVertBuffer;
      mesh.SetIndices(s_aRenderBoxProxyIndex, MeshTopology.Triangles, 0);
    }

    private static Mesh s_invertedUnitBoxMesh;
    public static Mesh InvertedUnitBox
    {
      get
      {
        if (s_invertedUnitBoxMesh != null)
          return s_invertedUnitBoxMesh;

        s_invertedUnitBoxMesh = new Mesh();

        Vector3[] aVert = 
        {
          new Vector3(-0.5f, -0.5f, -0.5f), 
          new Vector3(-0.5f,  0.5f, -0.5f), 
          new Vector3( 0.5f,  0.5f, -0.5f), 
          new Vector3( 0.5f, -0.5f, -0.5f), 
          new Vector3(-0.5f, -0.5f,  0.5f), 
          new Vector3(-0.5f,  0.5f,  0.5f), 
          new Vector3( 0.5f,  0.5f,  0.5f), 
          new Vector3( 0.5f, -0.5f,  0.5f), 
        };

        int[] aIndex = 
        {
          0, 2, 1, 0, 3, 2, 
          3, 6, 2, 3, 7, 6, 
          7, 5, 6, 7, 4, 5, 
          4, 1, 5, 4, 0, 1, 
          1, 6, 5, 1, 2, 6, 
          0, 7, 3, 0, 4, 7, 
        };

        s_invertedUnitBoxMesh.vertices = aVert;
        s_invertedUnitBoxMesh.SetIndices(aIndex, MeshTopology.Triangles, 0);

        return s_invertedUnitBoxMesh;
      }
    }
  }
}

