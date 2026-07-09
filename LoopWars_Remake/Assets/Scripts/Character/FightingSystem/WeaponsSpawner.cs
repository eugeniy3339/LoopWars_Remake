using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class WeaponsSpawner : NetworkBehaviour
{
    [SerializeField] private float maxSpawnCooldown = 4f;
    [SerializeField] private float maxSpawnCooldownRandomMultiplier = 1.2f;
    private float curSpawnCooldown;

    private WeaponToPickUp[] spawnedWeapons { get { return FindObjectsOfType<WeaponToPickUp>(); } }

    private WeaponSpawnType curWeaponSpawnType = WeaponSpawnType.RandomPosition;
    private Transform characterToSpawnNearOf;

    private void Awake()
    {
        curSpawnCooldown = GetRandomizedTime(maxSpawnCooldown, maxSpawnCooldownRandomMultiplier);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
            enabled = false;
    }



    private void Update()
    {
        SpawnCooldown();
    }

    private void SpawnCooldown()
    {
        if (MapHandler.Instance == null) return;
        curSpawnCooldown -= Time.deltaTime;
        if (curSpawnCooldown <= 0f)
            OnSpawnCooldown();
    }

    private void OnSpawnCooldown()
    {
        SpawnWeapon(); 
    }



    private void SpawnWeapon()
    {
        WeaponScriptableObject weaponToSpawn = GetRandomWeapon();
        if (curWeaponSpawnType == WeaponSpawnType.RandomPosition)
            SpawnWeapon(GetRandomWeaponSpawnPosition(), weaponToSpawn);
        else
            SpawnWeapon(GetRandomWeaponSpawnPositionNearTheCharacter(characterToSpawnNearOf), weaponToSpawn);

        curSpawnCooldown = maxSpawnCooldown;
        curWeaponSpawnType = WeaponSpawnType.RandomPosition;
    }

    private void SpawnWeapon(Vector2 position, WeaponScriptableObject weaponScriptableObject)
    {
        if (!IsServer) return;

        GameObject weapon = Instantiate(weaponScriptableObject.weaponToPickupPrefab);
        WeaponToPickUp weaponToPickUp = weapon.GetComponent<WeaponToPickUp>();

        weapon.transform.position = position;
        weaponToPickUp.NetworkObject.Spawn(true);
    }



    private Vector2 GetRandomWeaponSpawnPosition((float, float) minMaxHorizontalPosition, float height, float startHorizontalPos = 0f)
    {
        return new Vector2(startHorizontalPos + Random.Range(minMaxHorizontalPosition.Item1, minMaxHorizontalPosition.Item2), height);
    }

    private Vector2 GetRandomWeaponSpawnPosition()
    {
        Vector2 minWeaponSpawnPosition = MapHandler.Instance.minWeaponsSpawnPos.position;
        Vector2 maxWeaponSpawnPosition = MapHandler.Instance.maxWeaponsSpawnPos.position;

        return GetRandomWeaponSpawnPosition((minWeaponSpawnPosition.x, maxWeaponSpawnPosition.x), maxWeaponSpawnPosition.y);
    }

    private Vector2 GetRandomWeaponSpawnPositionNearTheCharacter(Transform character)
    {
        Vector2 maxWeaponSpawnPosition = MapHandler.Instance.maxWeaponsSpawnPos.position;
        (float, float) minMaxPositionMultiplier = (-3f, 3f);

        return GetRandomWeaponSpawnPosition(minMaxPositionMultiplier, maxWeaponSpawnPosition.y, character.transform.position.x);
    }

    private WeaponScriptableObject GetRandomWeapon()
    {
        return WeaponsListScriptableObject.Instance.weapons[Random.Range(0, WeaponsListScriptableObject.Instance.weapons.Count)];
    }

    private float GetRandomizedTime(float time, float maxRandomMultiplier)
    {
        return time * Random.Range(1f, maxRandomMultiplier);
    }



    private enum WeaponSpawnType
    {
        RandomPosition,
        NearThePlayer
    }
}
