using UnityEngine;
using HFSM;

[CreateAssetMenu(menuName = "Game/State Machines/Creep")]
public class CreepHFSM : StateMachineAsset
{
    private MarchState _march = new MarchState();
    private HurtState _hurt = new HurtState();
    private DieState _die = new DieState();

    public override State Init(StateMachine context)
    {
        State root = new RootState();

        root.LoadSubState(_march);
        root.LoadSubState(_hurt);
        root.LoadSubState(_die);

        LoadTransitions(root);

        root.Start(context);

        return root;
    }

    private void LoadTransitions(State root)
    {
        root.AddTransition(_march, _die, new Condition[] { new HasDiedCondition { } });
        root.AddTransition(_march, _hurt, new Condition[] { new WasAttackedCondition { } });
        root.AddTransition(_hurt, _die, new Condition[] { new HasDiedCondition { } });

        _hurt.finished += () => root.ChangeSubState(_march);
    }
}