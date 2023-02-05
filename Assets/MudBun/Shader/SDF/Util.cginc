/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_SDF_UTIL
#define MUDBUN_SDF_UTIL

#define SDF_SAMPLE_MASKED_BRUSHES(res, p, brushMask, mat)                      \
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

// macro that generates less inline code
#define SDF_SAMPLE_NORMAL(normal, p, brushMask, h)                             \
  {                                                                            \
    float3 aSign[4] =                                                          \
    {                                                                          \
      float3( 1.0f, -1.0f, -1.0f),                                             \
      float3(-1.0f, -1.0f,  1.0f),                                             \
      float3(-1.0f,  1.0f, -1.0f),                                             \
      float3( 1.0f,  1.0f,  1.0f),                                             \
    };                                                                         \
    float3 aDelta[4] =                                                         \
    {                                                                          \
      float3( (h), -(h), -(h)),                                                \
      float3(-(h), -(h),  (h)),                                                \
      float3(-(h),  (h), -(h)),                                                \
      float3( (h * 1.0001f), (h * 1.0002f), (h * 1.0003f)),                    \
    };                                                                         \
    float3 s = 0.0f;                                                           \
    SdfBrushMaterial nmat;                                                     \
    [loop] for (int iDelta = 0; iDelta < 4; ++iDelta)                          \
    {                                                                          \
      float d = kInfinity;                                                     \
      float3 pWithDelta = p + aDelta[iDelta];                                  \
      SDF_SAMPLE_MASKED_BRUSHES(d, pWithDelta, brushMask, nmat);               \
      s += aSign[iDelta] * d;                                                  \
    }                                                                          \
    normal = normalize_safe(s, float3(0.0f, 0.0f, 0.0f));                      \
  }

#endif
