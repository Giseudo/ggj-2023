using HFSM;
using UnityEngine;
using Game.Combat;

public class NewTargetPositionCondition : Condition
{
    private Unit _unit;

    public override void OnStart()
    {
        _unit = StateMachine.GetComponent<Unit>();
    }

    public override void OnEnter()
    {
        _unit.targetPositionChanged += OnTargetPositionChange;
    }

    public override void OnExit()
    {
        _unit.targetPositionChanged -= OnTargetPositionChange;
    }

    private void OnTargetPositionChange(Vector3 position) => Trigger();
}