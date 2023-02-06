using HFSM;
using Game.Combat;
using Game.Core;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

public class MarchState : State
{
    public override string Name => "March";
    private Animator _animator;
    private Damageable _damageable;
    private Damageable _treeDamageable;
    private SplineAnimate _splineAnimate;
    private Creep _creep;
    private bool _hasReachedTree;

    protected override void OnStart()
    {
        _animator = StateMachine.GetComponent<Animator>();
        _damageable = StateMachine.GetComponent<Damageable>();
        _splineAnimate = StateMachine.GetComponent<SplineAnimate>();
        _creep = StateMachine.GetComponent<Creep>();
    }

    protected override void OnEnter()
    {
        GameManager.MainTree.TryGetComponent<Damageable>(out _treeDamageable);

        _hasReachedTree = false;
        _animator.SetBool("IsMoving", true);
    }

    protected override void OnExit()
    {
        _animator.SetBool("IsMoving", false);
    }

    float t = 0;
    protected override void OnUpdate()
    {
        if (_hasReachedTree) return;
        if (_splineAnimate == null) return;

        t += _creep.MaxSpeed * _creep.SpeedMultiplier * Time.deltaTime * Time.deltaTime;

        Vector3 position = _splineAnimate.Container.EvaluatePosition(t);
        Vector3 tangent = _splineAnimate.Container.EvaluateTangent(t);

        _creep.transform.LookAt(_creep.transform.position + tangent);
        _creep.transform.position = position;

        if ((position - _treeDamageable.transform.position).magnitude < 2f)
        {
            _hasReachedTree = true;
            _treeDamageable.Hurt(1); // TODO per creep value
            _damageable.Die();
        }
    }
}