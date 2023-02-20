using HFSM;
using UnityEngine;
using Game.Navigation;

public class IsMovingCondition : Condition
{
    private Character _character;
    public bool Negate { get; set; }

    public override void OnStart()
    {
        _character = StateMachine.GetComponent<Character>();
    }

    public override void OnEnter() => Validate();
    public override void OnUpdate() => Validate();

    private void Validate()
    {
        bool isValid = _character.MoveDirection != Vector3.zero;

        if (isValid && !Negate) Trigger();
        if (!isValid && Negate) Trigger();
    }
}