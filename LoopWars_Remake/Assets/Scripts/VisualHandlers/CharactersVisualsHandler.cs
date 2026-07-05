using UnityEngine;

public static class CharactersVisualsHandler
{
    #region Prefabs
    private static GameObject bloodParticlesPrefab;
    private static GameObject deathParticlesPrefab;
    private static GameObject jumpParticlesPrefab;
    private static GameObject dashParticlesPrefab;
    #endregion

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Debug.Log("OnInitialize");

        bloodParticlesPrefab = ParticlesHandler.GetPrefab("BloodParticle");
        deathParticlesPrefab = ParticlesHandler.GetPrefab("PlayerDeathParticle");
        jumpParticlesPrefab = ParticlesHandler.GetPrefab("JumpParticles");
        dashParticlesPrefab = ParticlesHandler.GetPrefab("DashParticles");

        Jump.onJumpStatic += OnCharacterJumped;
        Jump.onWallJumpStatic += OnCharacterWallJumped;
        Dash.onDashStatic += OnCharacterDashed;
        HealthSystem.onCharacterDamaged += OnCharacterDamaged;
        HealthSystem.onCharactersHealthChanged += OnCharactersHealthChanged;
        HealthSystem.onCharacterDied += OnCharacterDied;
    }

    private static void OnCharacterJumped(Character character)
    {
        try
        {
            ParticlesHandler.SpawnParticles(jumpParticlesPrefab, character.movementManager.footPosition.position, Vector2.up);
        }
        catch
        {

        }
    }

    private static void OnCharacterWallJumped(Character character, Vector2 direction)
    {
        try
        {
            ParticlesHandler.SpawnParticles(jumpParticlesPrefab, character.movementManager.footPosition.position, direction);
        }
        catch
        {

        }
    }

    private static void OnCharacterDashed(Character character)
    {
        ParticleSystem particles = ParticlesHandler.SpawnParticles(dashParticlesPrefab, character.transform.position, character.transform.up);
        particles?.transform.SetParent(character.transform);
    }

    private static void OnCharacterDamaged(Character character, Transform damageObject, float damage)
    {
        try
        {
            Vector2 directionToTheBullet = damageObject.position - character.transform.position;
            Vector2 spawnPosition = new Vector2(character.transform.position.x, character.transform.position.y) + new Vector2(directionToTheBullet.normalized.x * character.collider.size.x / 2f, directionToTheBullet.normalized.y * character.collider.size.y / 2f);
            ParticlesHandler.SpawnParticles(bloodParticlesPrefab, spawnPosition, -damageObject.right);
        }
        catch
        {

        }
    }

    private static void OnCharactersHealthChanged(Character character, float oldHealth, float curHealth)
    {

    }

    private static void OnCharacterDied(Character character)
    {
        try
        {
            ParticlesHandler.SpawnParticles(deathParticlesPrefab, character.transform.position, Vector2.up);
        }
        catch
        {

        }
    }
}
