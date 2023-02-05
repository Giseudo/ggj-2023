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

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace MudBun
{
  public class Janitor : Singleton<Janitor>
  {
    private class DisposalRecord
    {
      public int Frame = -1;
      public List<ComputeBuffer> Buffers = new List<ComputeBuffer>();
    }

    private Queue<DisposalRecord> m_queue = new Queue<DisposalRecord>();

    #if MUDBUN_DEV
    private int m_lastQueuedFrame = -1;
    #endif

    public static void Dispose(ComputeBuffer buffer)
    {
      var instance = Instance;

      if (instance != null)
      {
        instance.Queue(buffer);
      }
      else
      {
        buffer.Release();
        //Debug.Log($"Disposed immediately: {buffer}");
      }
    }

    protected override void OnDisable()
    {
      base.OnDisable();

      FlushAll();
    }

    protected override void OnDestroy()
    {
      base.OnDestroy();

      FlushAll();
    }

    protected override void OnApplicationQuit()
    {
      base.OnApplicationQuit();

      FlushAll();
    }

    private void Update()
    {
      TryFlush();

      //Debug.Log($"Update frame {Time.renderedFrameCount}");

      #if UNITY_EDITOR
      if (!Application.isPlaying 
          && m_queue.Count > 0)
      {
        EditorApplication.QueuePlayerLoopUpdate();
      }
      #endif
    }

    private void Queue(params ComputeBuffer[] buffers)
    {
      DisposalRecord record;
      if (m_queue.Count > 0 
          && m_queue.Peek().Frame == Time.renderedFrameCount)
      {
        record = m_queue.Peek();
      }
      else
      {
        record = new DisposalRecord() { Frame = Time.renderedFrameCount };
        m_queue.Enqueue(record);
      }

      foreach (var buffer in buffers)
        record.Buffers.Add(buffer);

      #if MUDBUN_DEV
      if (m_lastQueuedFrame != Time.renderedFrameCount)
      {
        //Debug.Log($"Queued at frame {Time.renderedFrameCount}");
      }
      m_lastQueuedFrame = Time.renderedFrameCount;
      #endif

      #if UNITY_EDITOR
      if (!Application.isPlaying)
        EditorApplication.QueuePlayerLoopUpdate();
      #endif
    }

    private void TryFlush()
    {
      while (m_queue.Count > 0 
             && Time.renderedFrameCount > m_queue.Peek().Frame + 1)
      {
        var record = m_queue.Dequeue();
        foreach (var buffer in record.Buffers)
          buffer.Release();

        //Debug.Log($"Flushed record for frame {record.Frame} at frame {Time.renderedFrameCount}");
      }
    }

    private void FlushAll()
    {
      foreach (var record in m_queue)
        foreach (var buffer in record.Buffers)
          buffer.Release();

      m_queue.Clear();

      //Debug.Log($"Flushed all at frame {Time.renderedFrameCount}");
    }
  }
}


