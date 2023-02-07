using HFSM;
using UnityEngine;
using Game.Combat;

public class DetectTargetState : State
{
    public override string Name => "Detect Target";

    private bool _isAttacking;
    private Attacker _attacker;
    private Damageable _closestTarget;
    private ProjectileLauncher _projectileLauncher;
    private LayerMask _enemyLayer;

    protected override void OnStart()
    {
        _attacker = StateMachine.GetComponent<Attacker>();
        _projectileLauncher = StateMachine.GetComponent<ProjectileLauncher>();

        _enemyLayer = 1 << (StateMachine.gameObject.layer == LayerMask.NameToLayer("Creep")
            ? LayerMask.NameToLayer("GroundUnit")
            : LayerMask.NameToLayer("Creep"));
    }

    protected override void OnUpdate()
    {
        CheckColliders();

        if (_closestTarget == null) return;

        Attack();
    }

    protected override void OnExit()
    {
        _isAttacking = false;
    }

    private void CheckColliders()
    {
        if (_attacker == null) return;

        if (_closestTarget != null && !_closestTarget.IsDead)
        {
            if ((_attacker.transform.position - _closestTarget.transform.position).magnitude < _attacker.FovRadius)
                return;
        }

        if (_isAttacking) return;

        _closestTarget = null;

        Collider closestCollider = null;
        Collider[] colliders = Physics.OverlapSphere(_attacker.transform.position, _attacker.FovRadius, _enemyLayer);

        if (colliders.Length == 0) return;

        float minDistance = float.MaxValue;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            float distance = (_attacker.transform.position - collider.transform.position).sqrMagnitude;

            if (distance < minDistance)
            {
                minDistance = distance;
                closestCollider = collider;
            }
        }

        if (closestCollider == null) return;

        closestCollider.TryGetComponent<Damageable>(out Damageable damageable);

        if (damageable == null) return;

        if (!damageable.IsDead)
            _closestTarget = damageable;
    }

    private void Attack()
    {
        if (_closestTarget == null) return;
        if (!_closestTarget.TryGetComponent<Damageable>(out Damageable damageable)) return;
        if (!_attacker.Attack(damageable)) return;
        if (damageable.IsDead) return;

        _projectileLauncher?.SetTarget(_closestTarget?.transform);

        _attacker.transform.LookAt(damageable.transform.position);
        _isAttacking = true;
    }
}