using HFSM;
using Game.Combat;

public class WasAttackedCondition : Condition
{
    private Damageable _damageable;

    private void Trigger(Damageable damageable) => Trigger();

    public override void OnStart()
    {
        _damageable = StateMachine.GetComponent<Damageable>();
    }

    public override void OnEnter()
    {
        _damageable.hurted += Trigger;
    }

    public override void OnExit()
    {
        _damageable.hurted -= Trigger;
    }
}