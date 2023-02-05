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
        _attacker.attacked += Trigger;
    }

    public override void OnExit()
    {
        _attacker.attacked -= Trigger;
    }
}