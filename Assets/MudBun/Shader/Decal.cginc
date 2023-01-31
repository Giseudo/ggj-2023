/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_DECAL
#define MUDBUN_DECAL

#include "Render/ShaderCommon.cginc"

#if MUDBUN_VALID

#include "BrushFuncs.cginc"

struct DecalResults
{
  bool hit;
  float3 pos;
  SdfBrushMaterial mat;
  float sdfValue; 
};

#define SDF_DECAL_MASKED_BRUSHES(res, p, brushMask, mat)                       \
{                                                                              \
  int iStack = -1;                                                             \
  float3 pStack[kMaxBrushGroupDepth];                                          \
  float resStack[kMaxBrushGroupDepth];                                         \
  SdfBrushMaterial matStack[kMaxBrushGroupDepth];                              \
                                                                               \
  res = kInfinity;                                                             \
  mat = init_brush_material();                                                 \
  float3 groupP = p;                                                           \
  float groupRes = kInfinity;                                                  \
  SdfBrushMaterial groupMat = init_brush_material();                           \
  FOR_EACH_BRUSH_EXTERN_MASK(brushMask,                                        \
    switch (aBrush[iBrush].type)                                               \
    {                                                                          \
      case kSdfBeginGroup:                                                     \
        iStack = min(kMaxBrushGroupDepth - 1, iStack + 1);                     \
        pStack[iStack] = p;                                                    \
        resStack[iStack] = res;                                                \
        matStack[iStack] = mat;                                                \
        res = kInfinity;                                                       \
        mat = init_brush_material();                                           \
        break;                                                                 \
      case kSdfEndGroup:                                                       \
        groupP = p;                                                            \
        groupRes = res;                                                        \
        groupMat = mat;                                                        \
        p = pStack[iStack];                                                    \
        res = resStack[iStack];                                                \
        mat = matStack[iStack];                                                \
        break;                                                                 \
    }                                                                          \
    res = sdf_brush_apply(res, groupRes, groupMat, groupP, aBrush[iBrush], mat); \
    switch (aBrush[iBrush].type)                                               \
    {                                                                          \
      case kSdfEndGroup:                                                       \
        iStack = max(-1, iStack - 1);                                          \
        break;                                                                 \
    }                                                                          \
  );                                                                           \
}

bool sdf_decal_aabb_contains(Aabb aabb, float3 p)
{
  // TODO: use max blend
  aabb.boundsMin -= 5.0f;
  aabb.boundsMax += 5.0f;

  return aabb_contains(aabb, p);
}

// stmt = statements processing "iData" of hit leaf AABB nodes
// will gracefully handle maxed-out stacks
#define SDF_DECAL_AABB_TREE_CONTAINS(tree, root, p, stmt)                      \
{                                                                              \
  int stackTop = 0;                                                            \
  int stack[kAabbTreeNodeStackSize];                                           \
  stack[stackTop] = root;                                                      \
                                                                               \
  int numIters = 0;                                                            \
  while (stackTop >= 0 && numIters < 128 /* safeguard */)                      \
  {                                                                            \
    int index = stack[stackTop--];                                             \
    if (index < 0)                                                             \
      continue;                                                                \
                                                                               \
    if (!sdf_decal_aabb_contains(tree[index].aabb, p))                         \
        continue;                                                              \
                                                                               \
    if (tree[index].iChildA < 0)                                               \
    {                                                                          \
      int iData = tree[index].iData;                                           \
                                                                               \
      stmt                                                                     \
    }                                                                          \
    else                                                                       \
    {                                                                          \
      stackTop = min(stackTop + 1, kAabbTreeNodeStackSize - 1);                \
      stack[stackTop] = tree[index].iChildA;                                   \
      stackTop = min(stackTop + 1, kAabbTreeNodeStackSize - 1);                \
      stack[stackTop] = tree[index].iChildB;                                   \
    }                                                                          \
  }                                                                            \
}

DecalResults sdf_decal
(
  float3 p
)
{
  p = mul(worldToLocal, float4(p, 1.0f)).xyz;

  DecalResults res;
  res.hit = false;
  res.pos = p;
  res.mat = init_brush_material();
  res.sdfValue = kInfinity;

// don't crash Unity previews
#ifdef MUDBUN_PROCEDURAL

  BRUSH_MASK(brushMask);
  BRUSH_MASK_CLEAR_ALL(brushMask);

  SDF_DECAL_AABB_TREE_CONTAINS(aabbTree, aabbRoot, p, 
    BRUSH_MASK_SET(brushMask, iData);
  );

  float d = kInfinity;
  SDF_DECAL_MASKED_BRUSHES(d, p, brushMask, res.mat);

  if (d > 0.0f)
    discard;

  res.sdfValue = d;

#endif

  return res;
}

#else

struct SdfBrushMaterialDummy
{
  float4 color;
  float4 emissionHash;
  float4 metallicSmoothnessSizeTightness;
  float4 textureWeight;
};

struct DecalResults
{
  bool hit;
  float3 pos;
  SdfBrushMaterialDummy mat;
  float sdfValue;
};

DecalResults sdf_decal
(
  float3 p
)
{
  DecalResults res;
  res.hit = false;
  res.pos = p;
  res.mat.color = 0.0f;
  res.mat.emissionHash = 0.0f;
  res.mat.metallicSmoothnessSizeTightness = 0.0f;
  res.mat.textureWeight = 0.0f;
  res.sdfValue = 0.0f;

  return res;
}

#endif

#endif

