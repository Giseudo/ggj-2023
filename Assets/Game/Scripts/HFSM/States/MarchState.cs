using HFSM;
using Game.Combat;
using Game.Core;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using System;

public class MarchState : State
{
    public override string Name => "March";
    private Animator _animator;
    private Damageable _damageable;
    private Damageable _treeDamageable;
    private Creep _creep;
    private bool _hasReachedTree;
    public float t = 0;
    private DetectTargetState _detectTarget = new DetectTargetState();

    protected override void OnStart()
    {
        _animator = StateMachine.GetComponent<Animator>();
        _damageable = StateMachine.GetComponent<Damageable>();
        _creep = StateMachine.GetComponent<Creep>();

        LoadSubState(_detectTarget);
    }

    protected override void OnEnter()
    {
        GameManager.MainTree?.TryGetComponent<Damageable>(out _treeDamageable);

        if (_creep == null) return;

        _hasReachedTree = false;
        _animator.SetBool("IsMoving", true);
        _creep.Move();
    }

    protected override void OnExit()
    {
        if (_creep == null) return;

        _animator.SetBool("IsMoving", false);
        _creep.Stop();
    }

    protected override void OnUpdate()
    {
        if (_creep == null) return;
        if (_treeDamageable == null) return;
        if (_damageable.IsDead) return;
        if (_hasReachedTree) return;

        if ((_creep.transform.position - _treeDamageable.transform.position).magnitude < 2f)
        {
            _hasReachedTree = true;
            _treeDamageable.Hurt(1);
            _damageable.Die();
        }
    }
}