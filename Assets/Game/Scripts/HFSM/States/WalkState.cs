using HFSM;
using Game.Combat;
using Game.Navigation;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

public class WalkState : State
{
    public override string Name => "Walk";
    private Animator _animator;
    private Character _character;

    protected override void OnStart()
    {
        _animator = StateMachine.GetComponent<Animator>();
        _character = StateMachine.GetComponent<Character>();
    }

    protected override void OnEnter()
    {
        _animator.SetFloat("Speed", 1f);
    }

    protected override void OnExit()
    {
        _animator.SetFloat("Speed", 0f);
    }

    protected override void OnUpdate()
    {
        _character.transform.position += _character.Velocity * Time.deltaTime;
        _character.transform.LookAt(_character.Velocity.normalized);
    }
}