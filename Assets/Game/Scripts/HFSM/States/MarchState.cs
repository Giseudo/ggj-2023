using HFSM;
using Game.Combat;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

public class MarchState : State
{
    public override string Name => "March";
    private Animator _animator;
    private Damageable _damageable;
    private SplineAnimate _splineAnimate;

    protected override void OnStart()
    {
        _animator = StateMachine.GetComponent<Animator>();
        _damageable = StateMachine.GetComponent<Damageable>();
        _splineAnimate = StateMachine.GetComponent<SplineAnimate>();
    }

    protected override void OnEnter()
    {
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
    { }

    private void OnHurt(Damageable damageable)
    {
        _splineAnimate?.Pause();

        DOTween.To(() => 0f, x => {}, 1f, 1f)
            .OnComplete(() => _splineAnimate?.Play());
    }
}