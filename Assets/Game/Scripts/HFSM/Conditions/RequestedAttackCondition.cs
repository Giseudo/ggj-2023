using HFSM;
using Game.Combat;

public class RequestedAttackCondition : Condition
{
    private Attacker _attacker;

    private void Trigger(Damageable damageable) => Trigger();

    public override void OnStart()
    {
        _attacker = StateMachine.GetComponent<Attacker>();
    }

    public override void OnEnter()
    {
        if (_attacker == null) return;

        _attacker.attacked += Trigger;
    }

    public override void OnExit()
    {
        if (_attacker == null) return;

        _attacker.attacked -= Trigger;
    }
}