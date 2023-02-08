using UnityEngine;
using HFSM;

[CreateAssetMenu(menuName = "Game/State Machines/Ground Unit")]
public class GroundUnitHFSM : StateMachineAsset
{
    private DetectTargetState _detectAttack = new DetectTargetState();
    private FallState _fall = new FallState();
    private AttackState _attack = new AttackState();
    private HurtState _hurt = new HurtState();
    private DieState _die = new DieState();

    public override State Init(StateMachine context)
    {
        State root = new RootState();

        root.LoadSubState(_detectAttack);
        root.LoadSubState(_fall);
        root.LoadSubState(_attack);
        root.LoadSubState(_hurt);
        root.LoadSubState(_die);

        LoadTransitions(root);

        root.Start(context);

        return root;
    }

    private void LoadTransitions(State root)
    {
        root.AddTransition(_detectAttack, _attack, new Condition[] { new RequestedAttackCondition { } });
        root.AddTransition(_detectAttack, _hurt, new Condition[] { new WasAttackedCondition { } });
        root.AddTransition(_detectAttack, _die, new Condition[] { new HasDiedCondition { } });
        root.AddTransition(_detectAttack, _fall, new Condition[] { new IsFallingCondition { } });
        root.AddTransition(_fall, _detectAttack, new Condition[] { new IsFallingCondition { Negate = true } });
        root.AddTransition(_attack, _detectAttack, new Condition[] { new FinishedAttackCondition { } });
        root.AddTransition(_attack, _hurt, new Condition[] { new WasAttackedCondition { } });
        root.AddTransition(_attack, _die, new Condition[] { new HasDiedCondition { } });
        root.AddTransition(_hurt, _die, new Condition[] { new HasDiedCondition { } });

        _hurt.finished += () => root.ChangeSubState(_detectAttack);
        _die.finished += () => root.StateMachine.gameObject.SetActive(false);
    }
}