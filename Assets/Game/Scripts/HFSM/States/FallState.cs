using HFSM;
using Game.Combat;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

public class FallState : State
{
    public override string Name => "Fall";
    private Animator _animator;
    private bool _isFalling;

    protected override void OnStart()
    {
        _animator = StateMachine.GetComponent<Animator>();
    }

    protected override void OnEnter()
    {
        _animator.SetBool("IsFalling", true);
        _isFalling = true;
    }

    protected override void OnExit()
    {
        _animator.SetBool("IsFalling", false);
        _isFalling = false;
    }

    protected override void OnUpdate()
    {
        if (!_isFalling) return;

        bool isValid = Mathf.Abs(StateMachine.transform.position.y) > 0.25f;

        if (isValid) return;

        _isFalling = false;

        Finish();
    }
}