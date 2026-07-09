using LoopWars.GameMode;
using LoopWars.Players;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

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
        bloodParticlesPrefab = ParticlesHandler.GetPrefab("BloodParticle");
        deathParticlesPrefab = ParticlesHandler.GetPrefab("PlayerDeathParticle");
        jumpParticlesPrefab = ParticlesHandler.GetPrefab("JumpParticles");
        dashParticlesPrefab = ParticlesHandler.GetPrefab("DashParticles");

        Jump.onJumpStatic += OnCharacterJumped;
        Jump.onWallJumpStatic += OnCharacterWallJumped;
        Dash.onDashStatic += OnCharacterDashed;
        Dash.onDashEndStatic += OnCharacterDashEnded;
        HealthSystem.onCharacterDamaged += OnCharacterDamaged;
        HealthSystem.onCharactersHealthChanged += OnCharactersHealthChanged;
        HealthSystem.onCharacterDied += OnCharacterDied;
        Character.onCharacterSpawned += OnCharacterSpawned;
        MovementManager.onStartedMoving += OnCharacterStartedMoving;
        MovementManager.onStopedMoving += OnCharacterStopedMoving;
        MovementManager.onGroundedStatic += OnCharacterGrounded;
        MovementManager.onUngroundedStatic += OnCharacterUngrounded;
    }

    private static void OnCharacterSpawned(Character character)
    {
        Player player = PlayersContainer.GetPlayerByCharacter(character);

        if (player == null) return;
        character.objectColorsHandler.ColorSprites(player.color);
    }

    private static void OnCharacterJumped(Character character)
    {
        character.animator.Play("Jump");
        Player player = PlayersContainer.GetPlayerByCharacter(character);
        ParticlesHandler.SpawnParticles(jumpParticlesPrefab, character.movementManager.footPosition.position, Vector2.up, player.color);
    }

    private static void OnCharacterWallJumped(Character character, Vector2 direction)
    {
        Player player = PlayersContainer.GetPlayerByCharacter(character);
        ParticlesHandler.SpawnParticles(jumpParticlesPrefab, character.movementManager.footPosition.position, direction, player.color);
    }

    private static void OnCharacterDashed(Character character)
    {
        character.animator.Play("Dash");
        character.animator.SetBool("Dashing", true);
        Player player = PlayersContainer.GetPlayerByCharacter(character);
        ParticleSystem particles = ParticlesHandler.SpawnParticles(dashParticlesPrefab, character.transform.position, character.transform.up, player.color);
        particles?.transform.SetParent(character.transform);
    }

    private static void OnCharacterDashEnded(Character character)
    {
        character.animator.SetBool("Dashing", false);
    }

    private static void OnCharacterDamaged(Character character, Transform damageObject, float damage)
    {
        try
        {
            Player player = PlayersContainer.GetPlayerByCharacter(character);
            Vector2 directionToTheBullet = damageObject.position - character.transform.position;
            Vector2 spawnPosition = new Vector2(character.transform.position.x, character.transform.position.y) + new Vector2(directionToTheBullet.normalized.x * character.collider.size.x / 2f, directionToTheBullet.normalized.y * character.collider.size.y / 2f);
            ParticlesHandler.SpawnParticles(bloodParticlesPrefab, spawnPosition, -damageObject.right, player.color);
        }
        catch
        {

        }
    }

    private static void OnCharactersHealthChanged(Character character, float oldHealth, float curHealth)
    {

    }

    private static void OnCharacterGrounded(Character character)
    {
        character.animator.SetBool("Grounded", true);
    }

    private static void OnCharacterUngrounded(Character character)
    {
        character.animator.SetBool("Grounded", false);
    }

    private static void OnCharacterStartedMoving(Character character)
    {
        character.animator.SetFloat("x", 1f);
    }

    private static void OnCharacterStopedMoving(Character character)
    {
        character.animator.SetFloat("x", 0f);
    }

    private static void OnCharacterDied(Character character)
    {
        try
        {
            Player player = PlayersContainer.GetPlayerByCharacter(character);
            ParticlesHandler.SpawnParticles(deathParticlesPrefab, character.transform.position, Vector2.up, player.color);
        }
        catch
        {

        }
    }
}
