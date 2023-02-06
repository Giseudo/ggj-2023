using HFSM;
using Game.Combat;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

public class HurtState : State
{
    public override string Name => "Hurt";
    private Animator _animator;
    private Damageable _damageable;
    private float _lastHurtTime;

    protected override void OnStart()
    {
        _animator = StateMachine.GetComponent<Animator>();
        _damageable = StateMachine.GetComponent<Damageable>();
    }

    protected override void OnEnter()
    {
        _lastHurtTime = Time.time;
        _animator.SetTrigger("WasAttacked");
    }

    protected override void OnExit()
    { }

    protected override void OnUpdate()
    {
        if (_damageable.IsDead)
            return;

        if (_lastHurtTime + _damageable.HurtTime < Time.time)
            Finish();
    }
}