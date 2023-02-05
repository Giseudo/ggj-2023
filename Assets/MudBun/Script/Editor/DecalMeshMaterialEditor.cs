/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MudBun
{
  public class DecalMeshMaterialEditor : ShaderGUI
  {
    public override void OnGUI(MaterialEditor editor, MaterialProperty[] aProp)
    {
      var _EdgeFadeDistance = FindProperty("_EdgeFadeDistance", aProp);
      if (_EdgeFadeDistance != null)
      {
        editor.ShaderProperty(_EdgeFadeDistance, _EdgeFadeDistance.displayName);
        _EdgeFadeDistance.floatValue = Mathf.Max(0.0f, _EdgeFadeDistance.floatValue);
      }

      var _EdgeFadeColor = FindProperty("_EdgeFadeColor", aProp);
      editor.ShaderProperty(_EdgeFadeColor, _EdgeFadeColor.displayName);

      var _ColorBlendSrc = FindProperty("_ColorBlendSrc", aProp);
      BlendMode colorBlendSrcEnum = (BlendMode) _ColorBlendSrc.floatValue;
      colorBlendSrcEnum = (BlendMode) EditorGUILayout.EnumPopup("Color Blend Src", colorBlendSrcEnum);
      _ColorBlendSrc.floatValue = (float) colorBlendSrcEnum;

      var _ColorBlendDst = FindProperty("_ColorBlendDst", aProp);
      BlendMode colorBlendDstEnum = (BlendMode) _ColorBlendDst.floatValue;
      colorBlendDstEnum = (BlendMode) EditorGUILayout.EnumPopup("Color Blend Dst", colorBlendDstEnum);
      _ColorBlendDst.floatValue = (float) colorBlendDstEnum;

      var _StencilMask = FindProperty("_StencilMask", aProp);
      editor.ShaderProperty(_StencilMask, _StencilMask.displayName);

      EditorGUILayout.Space();

      editor.RenderQueueField();
      editor.DoubleSidedGIField();
    }
  }
}

