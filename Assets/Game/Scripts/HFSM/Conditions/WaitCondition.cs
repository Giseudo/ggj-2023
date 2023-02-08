using HFSM;
using UnityEngine;

public class WaitCondition : Condition
{
    public float EnterTime { get; set; }
    public float Seconds { get; set; }

    public override void OnEnter()
    {
        EnterTime = Time.time;
    }

    public override void OnUpdate()
    {
        if (EnterTime + Seconds < Time.time)
            Trigger();
    }
}