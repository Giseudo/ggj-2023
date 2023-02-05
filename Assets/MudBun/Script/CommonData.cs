/*****************************************************************************/
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

namespace MudBun
{
  [StructLayout(LayoutKind.Sequential, Pack = 0)]
  [Serializable]
  public struct CameraInfo
  {
    public static readonly int Stride = 12 * sizeof(float);

    public Vector4 Position;
    public Vector4 Direction;
    public Vector4 Up;

    public CameraInfo(Transform cameraTransform)
    {
      Position = cameraTransform.position;
      Position.w = 1.0f;

      Direction = cameraTransform.forward;
      Direction.w = 0.0f;

      Up = cameraTransform.up;
      Up.w = 0.0f;
    }
  }
}

