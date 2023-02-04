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
        _splineAnimate?.Play();
        _animator.SetBool("IsMoving", true);
        _damageable.hurted += OnHurt;
    }

    protected override void OnExit()
    {
        _splineAnimate?.Pause();
        _animator.SetBool("IsMoving", false);
        _damageable.hurted -= OnHurt;
    }

    protected override void OnUpdate()
    {
        if (_hasReachedTree) return;
        if (_splineAnimate == null) return;
        
        _splineAnimate.MaxSpeed = _creep.MaxSpeed;

        if (_splineAnimate.NormalizedTime >= .99f)
        {
            _hasReachedTree = true;
            _treeDamageable.Hurt(1); // TODO per creep value
            _damageable.Die();
        }
    }

    private void OnHurt(Damageable damageable)
    {
        _splineAnimate?.Pause();

        DOTween.To(() => 0f, x => {}, 1f, _damageable.HurtTime)
            .OnComplete(() => _splineAnimate?.Play());
    }
}