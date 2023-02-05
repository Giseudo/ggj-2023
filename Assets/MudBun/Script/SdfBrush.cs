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
using System.Runtime.InteropServices;

using UnityEngine;

#if MUDBUN_BURST
using Unity.Burst;
#endif

namespace MudBun
{
  [StructLayout(LayoutKind.Sequential, Pack = 0)]
  [Serializable]
#if MUDBUN_BURST
  [BurstCompile]
#endif
  public struct SdfBrushMaterial
  {
    public static readonly int Stride = 4 * sizeof(int) + 16 * sizeof(float);

    public Color Color;
    public Color EmissionHash;
    public Vector4 MetallicSmoothnessSizeTightness;
    public Vector4 TextureWeight;

    public int BrushIndex;
    public int Padding0;
    public int Padding1;
    public int Padding2;

    public static SdfBrushMaterial New =>
      new SdfBrushMaterial()
      {
        Color = Color.white, 
        EmissionHash = Color.black, 
        MetallicSmoothnessSizeTightness = Vector4.zero, 
        TextureWeight = Vector4.zero, 
        BrushIndex = -1, 
        Padding0 = 0, 
        Padding1 = 0, 
        Padding2 = 0, 
      };

#if MUDBUN_BURST
    [BurstCompile]
#endif
    public static void Lerp(in SdfBrushMaterial a, in SdfBrushMaterial b, float t, out SdfBrushMaterial ret)
    {
      ret = 
        new SdfBrushMaterial()
        {
          Color = Color.Lerp(a.Color, b.Color, t), 
          EmissionHash = Color.Lerp(a.EmissionHash, b.EmissionHash, t), 
          MetallicSmoothnessSizeTightness = Vector4.Lerp(a.MetallicSmoothnessSizeTightness, b.MetallicSmoothnessSizeTightness, t), 
          TextureWeight = Vector4.Lerp(a.TextureWeight, b.TextureWeight, t), 
        };
      ret.EmissionHash.a = t < 0.5f ? a.EmissionHash.a : b.EmissionHash.a;
      ret.BrushIndex = t < 0.5f ? a.BrushIndex : b.BrushIndex;
    }
  }

  [StructLayout(LayoutKind.Sequential, Pack = 0)]
  [Serializable]
  public struct SdfBrushMaterialCompressed
  {
    public static readonly int Stride = 4 * sizeof(uint) + 4 * sizeof(float);

    public uint Color;
    public uint EmissionTightness;
    public uint TextureWeight;
    public int BrushIndex;

    public float MetallicSmoothness;
    public float Size;
    public float Hash;
    public float Padding0;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 0)]
  [Serializable]
  public struct SdfBrush
  {
    public static readonly int Stride = 8 * sizeof(int) + 32 * sizeof(float);

    public enum TypeEnum
    {
      Nop = -1, 

      // groups
      GroupStart = -2, 
      GroupEnd = -3, 

      // primitives
      Box = 0, 
      Sphere, 
      Cylinder, 
      Torus, 
      SolidAngle, 

      // effects
      Particle = 100, 
      ParticleSystem, 
      UniformNoise, 
      CurveSimple, 
      CurveFull, 

      // distortion
      FishEye = 200, 
      Pinch, 
      Twist, 
      Quantize, 

      // modifiers
      Onion = 300, 
      NoiseModifier, 
    }

    public enum OperatorEnum
    {
      Union, 
      Subtract, 
      Intersect, 
      Dye, 
      CullInside, 
      CullOutside, 
      NoOp = -1, 
    }

    public enum BoundaryShapeEnum
    {
      Box, 
      Sphere, 
      Cylinder, 
      Torus, 
      SolidAngle, 
    }

    public enum NoiseTypeEnum
    {
      Perlin = -1, 
      BakedPerlin, 
      Triangle, 
    }

    public enum FlagBit
    {
      Hidden, 
      FlipX, 
      MirrorX, 
      CountAsBone, 
      CreateMirroredBone, 
      ContributeMaterial, 
      LockNoisePosition, 
      SphericalNoiseCoordinates, 
    }

    public int Type;
    public int Operator;
    public int Proxy;
    public int Index;

    public Vector3 Position;
    public float Blend;

    public Quaternion Rotation;

    public Vector3 Size;
    public float Radius;

    public Vector4 Data0;
    public Vector4 Data1;
    public Vector4 Data2;
    public Vector4 Data3;

    public Bits32 Flags;
    public int MaterialIndex;
    public int BoneIndex;
    public int Padding0;

    public float Hash;
    public float Padding1;
    public float Padding2;
    public float Padding3;

    public static SdfBrush New()
    {
      SdfBrush brush;
      brush.Type = -1;
      brush.Operator = 0;
      brush.Proxy = -1;
      brush.Index = -1;

      brush.Position = Vector3.zero;
      brush.Blend = 0.0f;

      brush.Rotation = Quaternion.identity;

      brush.Size = Vector3.one;
      brush.Radius = 0.0f;

      brush.Data0 = Vector4.zero;
      brush.Data1 = Vector4.zero;
      brush.Data2 = Vector4.zero;
      brush.Data3 = Vector4.zero;

      brush.Flags = new Bits32(0);
      brush.MaterialIndex = -1;
      brush.BoneIndex = -1;
      brush.Padding0 = 0;

      brush.Hash = 0.0f;
      brush.Padding1 = 0.0f;
      brush.Padding2 = 0.0f;
      brush.Padding3 = 0.0f;

      return brush;
    }
  }
}