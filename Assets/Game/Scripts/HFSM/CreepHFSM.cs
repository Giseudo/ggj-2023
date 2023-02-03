using UnityEngine;
using HFSM;

[CreateAssetMenu(menuName = "Game/State Machines/Creep")]
public class CreepHFSM : StateMachineAsset
{
    private MarchState _march = new MarchState();
    private DieState _die = new DieState();

    public override State Init(StateMachine context)
    {
        State root = new RootState();

        root.LoadSubState(_march);
        root.LoadSubState(_die);

        LoadTransitions(root);

        root.Start(context);

        return root;
    }

    private void LoadTransitions(State root)
    {
        root.AddTransition(_march, _die, new Condition[] { new HasDiedCondition { } });
    }
}