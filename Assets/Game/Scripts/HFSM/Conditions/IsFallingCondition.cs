using HFSM;
using UnityEngine;
using Game.Combat;

public class IsFallingCondition : Condition
{
    public bool Negate { get; set; }

    public override void OnStart()
    { }

    public override void OnUpdate()
    {
        bool isValid = Mathf.Abs(StateMachine.transform.position.y) > 0.25f;

        if (isValid && !Negate) Trigger();
        if (!isValid && Negate) Trigger();
    }

    public override void OnExit()
    { }
}