using HFSM;
using Game.Combat;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

public class FallState : State
{
    public override string Name => "Fall";
    private Animator _animator;

    protected override void OnStart()
    {
        _animator = StateMachine.GetComponent<Animator>();
    }

    protected override void OnEnter()
    {
        _animator.SetBool("IsFalling", true);
    }

    protected override void OnExit()
    {
        _animator.SetBool("IsFalling", false);
    }
}