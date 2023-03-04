using UnityEngine;
using HFSM;

[CreateAssetMenu(menuName = "Game/State Machines/Unit")]
public class UnitHFSM : StateMachineAsset
{
    private DetectTargetState _detectAttack = new DetectTargetState();
    private AttackState _attack = new AttackState();
    private DieState _die = new DieState();

    public override State Init(StateMachine origin)
    {
        State root = new RootState();

        root.LoadSubState(_detectAttack);
        root.LoadSubState(_attack);
        root.LoadSubState(_die);

        LoadTransitions(root);

        return root;
    }

    private void LoadTransitions(State root)
    {
        root.AddTransition(_detectAttack, _attack, new Condition[] { new RequestedAttackCondition { } });
        root.AddTransition(_attack, _detectAttack, new Condition[] { new FinishedAttackCondition { } });
        root.AddTransition(_detectAttack, _die, new Condition[] { new HasDiedCondition { } });
        root.AddTransition(_attack, _die, new Condition[] { new HasDiedCondition { } });
    }
}