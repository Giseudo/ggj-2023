using HFSM;
using Game.Combat;
using Game.Navigation;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using Freya;

public class MoveToTargetPositionState : State
{
    public override string Name => "Move To Target Position";
    private WalkState _walk = new WalkState();
    private Unit _unit;
    private Character _character;

    protected override void OnStart()
    {
        _unit = StateMachine.GetComponent<Unit>();
        _character = StateMachine.GetComponent<Character>();

        LoadSubState(_walk);
    }

    protected override void OnEnter()
    { }

    protected override void OnExit()
    {
        _character.Move(Vector3.zero);
    }

    protected override void OnUpdate()
    {
        Vector3 direction = (_unit.TargetPosition - _character.transform.position).normalized;

        _character.Move(direction);
    }
}