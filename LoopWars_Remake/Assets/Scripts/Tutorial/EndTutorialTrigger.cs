using UnityEngine;

public class EndTutorialTrigger : Trigger
{
    protected override void OnTrigger(Character character)
    {
        base.OnTrigger(character);
        Tutorial.EndTutorial();
    }
}
