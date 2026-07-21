using UnityEngine;

public static class CharactersSoundsManager
{
    #region Sounds
    private static SoundScriptableObject jumpSound;
    private static SoundScriptableObject dashSound;
    private static SoundScriptableObject hitSound;
    private static SoundScriptableObject deathSound;
    private static SoundsListScriptableObject stepSound;
    #endregion

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        jumpSound = Resources.Load<SoundScriptableObject>("Sounds/Jump");
        dashSound = Resources.Load<SoundScriptableObject>("Sounds/Dash");
        hitSound = Resources.Load<SoundScriptableObject>("Sounds/Hit");
        deathSound = Resources.Load<SoundScriptableObject>("Sounds/Death");
        stepSound = Resources.Load<SoundsListScriptableObject>("Sounds/Step");

        Jump.onJumpStatic += OnCharacterJumped;
        Jump.onWallJumpStatic += OnCharacterWallJumped;
        Dash.onDashStatic += OnCharacterDashed;
        HealthSystem.onCharacterDamaged += OnCharacterDamaged;
        HealthSystem.onCharacterDied += OnCharacterDied;
        MovementManager.onCharacterSteped += OnCharacterSteped;
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
        SoundsManager.StartSound(deathSound, null);
    }

    private static void OnCharacterSteped(Character character)
    {
        SoundsManager.StartSound(stepSound, null);
    }
}
