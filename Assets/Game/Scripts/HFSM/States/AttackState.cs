using HFSM;
using Game.Combat;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

public class AttackState : State
{
    public override string Name => "Attack";
    private Animator _animator;
    private Attacker _attacker;

    protected override void OnStart()
    {
        _animator = StateMachine.GetComponent<Animator>();
        _attacker = StateMachine.GetComponent<Attacker>();
    }

    protected override void OnEnter()
    {
        _animator.SetTrigger("Attacked");
        _animator.speed = _attacker.AttackSpeed;
    }

    protected override void OnExit()
    {
        _animator.speed = 1f;
    }
}