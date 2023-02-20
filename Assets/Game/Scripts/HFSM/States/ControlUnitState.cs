using HFSM;
using Game.Combat;
using UnityEngine;
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
    private float _lastAttackTime;

    protected override void OnStart()
    {
        _unit = StateMachine.GetComponent<Unit>();
        _attacker = StateMachine.GetComponent<Attacker>();
        _launcher = StateMachine.GetComponent<ProjectileLauncher>();
        _unit.SetTargetPosition(_launcher.Target.position);

        LoadSubState(_idle);
        LoadSubState(_attack);

        AddTransition(_idle, _attack, new Condition[] { new RequestedAttackCondition { } });
        AddTransition(_attack, _idle, new Condition[] { new FinishedAttackCondition { } });
    }

    protected override void OnEnter()
    {
        _launcher.launched += OnLaunchUnit;
        _launcher.destroyed += OnDestroyUnit;
    }

    protected override void OnExit()
    {
        _launcher.launched -= OnLaunchUnit;
        _launcher.destroyed -= OnDestroyUnit;
    }

    protected override void OnUpdate()
    {
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
    }

    private void OnDestroyUnit(IProjectile projectile)
    {
        if (!projectile.GameObject.TryGetComponent<Unit>(out Unit unit)) return;

        unit.SetParent(null);
    }
}