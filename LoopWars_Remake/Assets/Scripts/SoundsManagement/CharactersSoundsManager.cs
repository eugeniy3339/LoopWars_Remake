using UnityEngine;

public static class CharactersSoundsManager
{
    #region Sounds
    private static SoundsListScriptableObject charactersSounds;
    private static SoundScriptableObject jumpSound;
    private static SoundScriptableObject dashSound;
    private static SoundScriptableObject hitSound;
    #endregion

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        charactersSounds = Resources.Load<SoundsListScriptableObject>("Sounds/CharactersSounds");

        jumpSound = charactersSounds.GetSound("Jump");
        dashSound = charactersSounds.GetSound("Dash");
        hitSound = charactersSounds.GetSound("Hit");

        Jump.onJumpStatic += OnCharacterJumped;
        Jump.onWallJumpStatic += OnCharacterWallJumped;
        Dash.onDashStatic += OnCharacterDashed;
        HealthSystem.onCharacterDamaged += OnCharacterDamaged;
        HealthSystem.onCharacterDied += OnCharacterDied;
    }

    private static void OnCharacterJumped(Character character)
    {
        SoundsManager.StartSound(jumpSound, character.transform);
    }

    private static void OnCharacterWallJumped(Character character, Vector2 direction)
    {
        SoundsManager.StartSound(jumpSound, character.transform);
    }

    private static void OnCharacterDashed(Character character, float dashTime)
    {
        SoundsManager.StartSound(dashSound, character.transform);
    }

    private static void OnCharacterDamaged(Character character, Transform damageObjectTransform, float damage)
    {
        SoundsManager.StartSound(hitSound, character.transform);
    }

    private static void OnCharacterDied(Character character)
    {
        SoundsManager.StartSound(hitSound, null);
    }
}
