using HFSM;
using Game.Combat;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

public class DieState : State
{
    public override string Name => "Die";
    private Animator _animator;

    protected override void OnStart()
    {
        _animator = StateMachine.GetComponent<Animator>();
    }

    protected override void OnEnter()
    {
        _animator.SetBool("Died", true);

        _animator.gameObject.SetActive(false);
    }
}