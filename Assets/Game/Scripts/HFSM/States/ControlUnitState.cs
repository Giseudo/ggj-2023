using HFSM;
using Game.Combat;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Splines;
using DG.Tweening;

public class ControlUnitState : State
{
    public override string Name => "Control Unit";
    private Unit _unit;
    private Attacker _attacker;
    private ProjectileLauncher _launcher;
    private IdleState _idle = new IdleState();
    private AttackState _attack = new AttackState();
    private float _startTime;
    private float _lastAttackTime;
    private bool _changedTarget;
    private List<Unit> _children = new List<Unit>();

    protected override void OnStart()
    {
        _unit = StateMachine.GetComponent<Unit>();
        _attacker = StateMachine.GetComponent<Attacker>();
        _launcher = StateMachine.GetComponent<ProjectileLauncher>();
        _unit.SetTargetPosition(_launcher.Target.position);
        _startTime = Time.time;

        LoadSubState(_idle);
        LoadSubState(_attack);

        AddTransition(_idle, _attack, new Condition[] { new RequestedAttackCondition { } });
        AddTransition(_attack, _idle, new Condition[] { new FinishedAttackCondition { } });
    }

    protected override void OnEnter()
    {
        _launcher.launched += OnLaunchUnit;
        _launcher.destroyed += OnDestroyUnit;
        _unit.targetPositionChanged += OnTargetChange;
    }

    protected override void OnExit()
    {
        _launcher.launched -= OnLaunchUnit;
        _launcher.destroyed -= OnDestroyUnit;
        _unit.targetPositionChanged -= OnTargetChange;
    }

    protected override void OnUpdate()
    {
        if (!_changedTarget) return;

        bool reachedLimit = _launcher.Pool.CountActive >= _launcher.Limit;

        if (_lastAttackTime + _attacker.AttackWaitTime < Time.time && !reachedLimit)
        {
            _attacker.Attack(null);
            _lastAttackTime = Time.time;
        }
    }

    private void OnLaunchUnit(IProjectile projectile)
    {
        if (!projectile.GameObject.TryGetComponent<Unit>(out Unit unit)) return;

        unit.SetParent(_unit);
        unit.SetTargetPosition(_unit.TargetPosition);

        _children.Add(unit);
    }

    private void OnTargetChange(Vector3 position)
    {
        for (int i = 0; i < _children.Count; i++)
        {
            Unit unit = _children[i];
            unit.SetTargetPosition(position);
        }

        if (_changedTarget) return;

        float timer = 0f;

        DOTween.To(() => 0f, x => {
            timer += Time.deltaTime;

            if (timer > .33f)
            {
                _attacker.Attack(null, true);
                timer = 0f;
            }
        }, 1f, 2f);

        _changedTarget = true;
    }

    private void OnDestroyUnit(IProjectile projectile)
    {
        if (!projectile.GameObject.TryGetComponent<Unit>(out Unit unit)) return;

        unit.SetParent(null);

        _children.Remove(unit);
    }
}