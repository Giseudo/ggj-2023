using UnityEngine;
using HFSM;

[CreateAssetMenu(menuName = "Game/State Machines/Spawner Unit")]
public class SpawnerUnitHFSM : StateMachineAsset
{
    private ControlUnitState _controlUnit = new ControlUnitState();
    private DieState _die = new DieState();

    public override State Init(StateMachine context)
    {
        State root = new RootState();

        root.LoadSubState(_controlUnit);
        root.LoadSubState(_die);

        LoadTransitions(root);

        return root;
    }

    private void LoadTransitions(State root)
    {
        root.AddTransition(_controlUnit, _die, new Condition[] { new HasDiedCondition { } });
    }
}