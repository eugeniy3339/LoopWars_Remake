using UnityEngine;

public class EndTutorialTrigger : Trigger
{
    protected override void OnTriggerEnt(Character character)
    {
        base.OnTriggerEnt(character);
        Tutorial.EndTutorial();
    }
}
