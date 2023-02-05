using System;

namespace MudBun
{
  /// <summary>
  /// Registers a static method with signature <c>SdfBrushEvalFunc</c> for a specific brush type for CPU-based SDF brush computation.
  /// </summary>
  public class RegisterSdfBrushEvalFuncAttribute : Attribute
  {
    private int m_brushType;
    public int BrushType => m_brushType;

    public RegisterSdfBrushEvalFuncAttribute(SdfBrush.TypeEnum brushType)
    {
      m_brushType = (int) brushType;
    }

    public RegisterSdfBrushEvalFuncAttribute(int brushType)
    {
      m_brushType = brushType;
    }
  }
}

