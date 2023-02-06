using UnityEngine;
using HFSM;

[CreateAssetMenu(menuName = "Game/State Machines/Unit")]
public class UnitHFSM : StateMachineAsset
{
    private DetectTargetState _idle = new DetectTargetState();
    private AttackState _attack = new AttackState();

    public override State Init(StateMachine context)
    {
        State root = new RootState();

        root.LoadSubState(_idle);
        root.LoadSubState(_attack);

        LoadTransitions(root);

        root.Start(context);

        return root;
    }

    private void LoadTransitions(State root)
    {
        root.AddTransition(_idle, _attack, new Condition[] { new RequestedAttackCondition { } });
        root.AddTransition(_attack, _idle, new Condition[] { new FinishedAttackCondition { } });
    }
}