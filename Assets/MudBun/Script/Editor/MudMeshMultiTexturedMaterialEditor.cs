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

namespace MudBun
{
  public class MudMeshMultiTexturedMaterialEditor : ShaderGUI
  {
    public override void OnGUI(MaterialEditor editor, MaterialProperty[] aProp)
    {
      var _AlphaCutoutThreshold = FindProperty("_AlphaCutoutThreshold", aProp);
      editor.ShaderProperty(_AlphaCutoutThreshold, _AlphaCutoutThreshold.displayName);

      var _Dithering = FindProperty("_Dithering", aProp);
      editor.ShaderProperty(_Dithering, _Dithering.displayName);

      var _DitherTexture = FindProperty("_DitherTexture", aProp);
      editor.ShaderProperty(_DitherTexture, _DitherTexture.displayName);

      var _DitherTextureSize = FindProperty("_DitherTextureSize", aProp);
      editor.ShaderProperty(_DitherTextureSize, _DitherTextureSize.displayName);

      var _RandomDither = FindProperty("_RandomDither", aProp);
      editor.ShaderProperty(_RandomDither, _RandomDither.displayName);

      EditorGUILayout.Space();

      var _UseTex0 = FindProperty("_UseTex0", aProp);
      editor.ShaderProperty(_UseTex0, _UseTex0.displayName);
      if (_UseTex0.floatValue > 0.0f)
      {
        var _MainTex = FindProperty("_MainTex", aProp);
        var _MainTexX = FindProperty("_MainTexX", aProp);
        var _MainTexY = FindProperty("_MainTexY", aProp);
        var _MainTexZ = FindProperty("_MainTexZ", aProp);
        editor.ShaderProperty(_MainTex, _MainTex.displayName);
        editor.ShaderProperty(_MainTexX, _MainTexX.displayName);
        editor.ShaderProperty(_MainTexY, _MainTexY.displayName);
        editor.ShaderProperty(_MainTexZ, _MainTexZ.displayName);
        EditorGUILayout.Space();
      }

      var _UseTex1 = FindProperty("_UseTex1", aProp);
      editor.ShaderProperty(_UseTex1, _UseTex1.displayName);
      if (_UseTex1.floatValue > 0.0f)
      {
        var _Tex1 = FindProperty("_Tex1", aProp);
        var _Tex1X = FindProperty("_Tex1X", aProp);
        var _Tex1Y = FindProperty("_Tex1Y", aProp);
        var _Tex1Z = FindProperty("_Tex1Z", aProp);
        editor.ShaderProperty(_Tex1, _Tex1.displayName);
        editor.ShaderProperty(_Tex1X, _Tex1X.displayName);
        editor.ShaderProperty(_Tex1Y, _Tex1Y.displayName);
        editor.ShaderProperty(_Tex1Z, _Tex1Z.displayName);
        EditorGUILayout.Space();
      }

      var _UseTex2 = FindProperty("_UseTex2", aProp);
      editor.ShaderProperty(_UseTex2, _UseTex2.displayName);
      if (_UseTex2.floatValue > 0.0f)
      {
        var _Tex2 = FindProperty("_Tex2", aProp);
        var _Tex2X = FindProperty("_Tex2X", aProp);
        var _Tex2Y = FindProperty("_Tex2Y", aProp);
        var _Tex2Z = FindProperty("_Tex2Z", aProp);
        editor.ShaderProperty(_Tex2, _Tex2.displayName);
        editor.ShaderProperty(_Tex2X, _Tex2X.displayName);
        editor.ShaderProperty(_Tex2Y, _Tex2Y.displayName);
        editor.ShaderProperty(_Tex2Z, _Tex2Z.displayName);
        EditorGUILayout.Space();
      }

      var _UseTex3 = FindProperty("_UseTex3", aProp);
      editor.ShaderProperty(_UseTex3, _UseTex3.displayName);
      if (_UseTex3.floatValue > 0.0f)
      {
        var _Tex3 = FindProperty("_Tex3", aProp);
        var _Tex3X = FindProperty("_Tex3X", aProp);
        var _Tex3Y = FindProperty("_Tex3Y", aProp);
        var _Tex3Z = FindProperty("_Tex3Z", aProp);
        editor.ShaderProperty(_Tex3, _Tex3.displayName);
        editor.ShaderProperty(_Tex3X, _Tex3X.displayName);
        editor.ShaderProperty(_Tex3Y, _Tex3Y.displayName);
        editor.ShaderProperty(_Tex3Z, _Tex3Z.displayName);
      }

      EditorGUILayout.Space();

      var _UseNorm0 = FindProperty("_UseNorm0", aProp);
      editor.ShaderProperty(_UseNorm0, _UseNorm0.displayName);
      if (_UseNorm0.floatValue > 0.0f)
      {
        var _MainNorm = FindProperty("_MainNorm", aProp);
        var _MainNormX = FindProperty("_MainNormX", aProp);
        var _MainNormY = FindProperty("_MainNormY", aProp);
        var _MainNormZ = FindProperty("_MainNormZ", aProp);
        editor.ShaderProperty(_MainNorm, _MainNorm.displayName);
        editor.ShaderProperty(_MainNormX, _MainNormX.displayName);
        editor.ShaderProperty(_MainNormY, _MainNormY.displayName);
        editor.ShaderProperty(_MainNormZ, _MainNormZ.displayName);
        EditorGUILayout.Space();
      }

      var _UseNorm1 = FindProperty("_UseNorm1", aProp);
      editor.ShaderProperty(_UseNorm1, _UseNorm1.displayName);
      if (_UseNorm1.floatValue > 0.0f)
      {
        var _Norm1 = FindProperty("_Norm1", aProp);
        var _Norm1X = FindProperty("_Norm1X", aProp);
        var _Norm1Y = FindProperty("_Norm1Y", aProp);
        var _Norm1Z = FindProperty("_Norm1Z", aProp);
        editor.ShaderProperty(_Norm1, _Norm1.displayName);
        editor.ShaderProperty(_Norm1X, _Norm1X.displayName);
        editor.ShaderProperty(_Norm1Y, _Norm1Y.displayName);
        editor.ShaderProperty(_Norm1Z, _Norm1Z.displayName);
        EditorGUILayout.Space();
      }

      var _UseNorm2 = FindProperty("_UseNorm2", aProp);
      editor.ShaderProperty(_UseNorm2, _UseNorm2.displayName);
      if (_UseNorm2.floatValue > 0.0f)
      {
        var _Norm2 = FindProperty("_Norm2", aProp);
        var _Norm2X = FindProperty("_Norm2X", aProp);
        var _Norm2Y = FindProperty("_Norm2Y", aProp);
        var _Norm2Z = FindProperty("_Norm2Z", aProp);
        editor.ShaderProperty(_Norm2, _Norm2.displayName);
        editor.ShaderProperty(_Norm2X, _Norm2X.displayName);
        editor.ShaderProperty(_Norm2Y, _Norm2Y.displayName);
        editor.ShaderProperty(_Norm2Z, _Norm2Z.displayName);
        EditorGUILayout.Space();
      }

      var _UseNorm3 = FindProperty("_UseNorm3", aProp);
      editor.ShaderProperty(_UseNorm3, _UseNorm3.displayName);
      if (_UseNorm3.floatValue > 0.0f)
      {
        var _Norm3 = FindProperty("_Norm3", aProp);
        var _Norm3X = FindProperty("_Norm3X", aProp);
        var _Norm3Y = FindProperty("_Norm3Y", aProp);
        var _Norm3Z = FindProperty("_Norm3Z", aProp);
        editor.ShaderProperty(_Norm3, _Norm3.displayName);
        editor.ShaderProperty(_Norm3X, _Norm3X.displayName);
        editor.ShaderProperty(_Norm3Y, _Norm3Y.displayName);
        editor.ShaderProperty(_Norm3Z, _Norm3Z.displayName);
      }

      EditorGUILayout.Space();

      editor.RenderQueueField();
      editor.DoubleSidedGIField();
    }
  }
}



