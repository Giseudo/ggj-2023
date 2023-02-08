using HFSM;
using Game.Combat;

public class FinishedAttackCondition : Condition
{
    private Attacker _attacker;

    public override void OnStart()
    {
        _attacker = StateMachine.GetComponent<Attacker>();
    }

    public override void OnEnter()
    {
        _attacker.finishedAttack += Trigger;

        if (!_attacker.IsAttacking)
            Trigger();
    }

    public override void OnExit()
    {
        _attacker.finishedAttack -= Trigger;
    }
}