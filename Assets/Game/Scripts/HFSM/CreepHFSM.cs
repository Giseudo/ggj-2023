using UnityEngine;
using HFSM;

[CreateAssetMenu(menuName = "Game/State Machines/Creep")]
public class CreepHFSM : StateMachineAsset
{
    private MarchState _march = new MarchState();
    private HurtState _hurt = new HurtState();
    private DieState _die = new DieState();
    private AttackState _attack = new AttackState();

    public override State Init(StateMachine context)
    {
        State root = new RootState();

        root.LoadSubState(_march);
        root.LoadSubState(_hurt);
        root.LoadSubState(_die);
        root.LoadSubState(_attack);

        LoadTransitions(root);

        root.Start(context);

        return root;
    }

    private void LoadTransitions(State root)
    {
        root.AddTransition(_march, _die, new Condition[] { new HasDiedCondition { } });
        root.AddTransition(_march, _hurt, new Condition[] { new WasAttackedCondition { } });
        root.AddTransition(_hurt, _die, new Condition[] { new HasDiedCondition { } });
        root.AddTransition(_march, _attack, new Condition[] { new RequestedAttackCondition { } });
        root.AddTransition(_attack, _march, new Condition[] { new FinishedAttackCondition { }, new WaitCondition { Seconds = 3f } }, Operator.Or);
        root.AddTransition(_attack, _hurt, new Condition[] { new WasAttackedCondition { } });
        root.AddTransition(_attack, _die, new Condition[] { new HasDiedCondition { } });

        _hurt.finished += () => root.ChangeSubState(_march);
    }
}