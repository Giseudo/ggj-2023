using HFSM;
using UnityEngine;
using Game.Combat;

public class UnitIdleState : State
{
    public override string Name => "Idle";

    private bool _isAttacking;
    private Attacker _attacker;
    private Collider _closestTarget;

    protected override void OnStart()
    {
        _attacker = StateMachine.GetComponent<Attacker>();
    }

    protected override void OnUpdate()
    {
        if (_isAttacking) return;

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
        _closestTarget = null;

        Collider[] colliders = Physics.OverlapSphere(_attacker.transform.position, _attacker.FovRadius, 1 << LayerMask.NameToLayer("Creep"));

        if (colliders.Length == 0) return;

        float minDistance = float.MaxValue;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            float distance = (_attacker.transform.position - collider.transform.position).sqrMagnitude;

            if (distance < minDistance)
            {
                minDistance = distance;
                _closestTarget = collider;
            }
        }
    }

    private void Attack()
    {
        if (_closestTarget == null) return;
        if (!_closestTarget.TryGetComponent<Damageable>(out Damageable damageable)) return;
        if (damageable.IsDead) return;

        _attacker.Attack(damageable);
        _attacker.transform.LookAt(damageable.transform.position);
        _isAttacking = true;
    }
}