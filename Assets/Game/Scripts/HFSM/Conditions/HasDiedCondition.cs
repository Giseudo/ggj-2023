using HFSM;
using Game.Combat;

public class HasDiedCondition : Condition
{
    private Damageable _damageable;

    private void Trigger(Damageable damageable) => Trigger();

    public override void OnStart()
    {
        _damageable = StateMachine.GetComponent<Damageable>();
    }

    public override void OnEnter()
    {
        _damageable.died += Trigger;
    }

    public override void OnExit()
    {
        _damageable.died -= Trigger;
    }
}