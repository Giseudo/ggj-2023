using HFSM;
using UnityEngine;
using Game.Combat;

public class IsAtTargetPositionCondition : Condition
{
    private Unit _unit;
    public bool Negate { get; set; }

    public override void OnStart()
    {
        _unit = StateMachine.GetComponent<Unit>();
    }

    public override void OnUpdate()
    {
        Validate();
    }

    public void Validate()
    {
        float distance = (_unit.transform.position - _unit.TargetPosition).magnitude;
        bool isValid = distance < 3f;

        if (isValid && !Negate) Trigger();
        if (!isValid && Negate) Trigger();
    }
}