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
  public class MudDistortion : MudBrush
  {
    public enum OperatorEnum
    {
      Distort = -100, 
    }

    public override bool IsSuccessorModifier => true;

    public virtual float MaxDistortion => 0.0f;

    public override void FillBrushData(ref SdfBrush brush, int iBrush)
    {
      base.FillBrushData(ref brush, iBrush);

      brush.Operator = (int) OperatorEnum.Distort;
      brush.Blend = MaxDistortion;
    }

    public override void DrawGizmosRs()
    {
      base.DrawGizmosRs();

      Color prevColor = Gizmos.color;

      Gizmos.color = 
        IsSelected() 
          ? GizmosUtil.OutlineSelected 
          : GizmosUtil.OutlineDefault;

      DrawOutlineGizmosRs();

      Gizmos.color = prevColor;
    }
  }
}

