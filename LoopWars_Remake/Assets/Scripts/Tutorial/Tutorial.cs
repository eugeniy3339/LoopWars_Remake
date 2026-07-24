using LoopWars.GameMode;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private Character player;
    [SerializeField] private Transform weaponsSpawnerTransform;

    private void Start()
    {
        player.NetworkObject.SpawnWithOwnership(NetworkManager.Singleton.LocalClientId, true);
        FindObjectOfType<WeaponToPickUp>()?.NetworkObject.Spawn(true);
    }

    public static void EndTutorial()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(0);
    }

    public static void StartTutorial()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(
            ipv4Address: "127.0.0.1",
            port: 7777,
            listenAddress: "0.0.0.0");

        bool startedHost = NetworkManager.Singleton.StartHost();
        if (!startedHost)
            return;
        GameMode.multiplayerMode = LoopWars.GameMode.PlayMode.Tutorial;
        SceneManager.LoadScene(2);
    }

    private void OnThrewTheWeapon(Character character, WeaponScriptableObject weaponScriptableObject)
    {
        SpawnWeapon();
    }

    private void SpawnWeapon()
    {
        WeaponToPickUp weaponToPickup = Instantiate(WeaponsListScriptableObject.Instance.GetRandomWeapon().weaponToPickupPrefab).GetComponent<WeaponToPickUp>();
        weaponToPickup.transform.position = weaponsSpawnerTransform.position;
        weaponToPickup.NetworkObject.Spawn(true);
    }

    private void OnCharacterDied(Character character)
    {
        EndTutorial();
    }

    private void OnEnable()
    {
        WeaponManager.onThrewTheWeapon += OnThrewTheWeapon;
        HealthSystem.onCharacterDied += OnCharacterDied;
    }

    private void OnDisable()
    {
        WeaponManager.onThrewTheWeapon -= OnThrewTheWeapon;
        HealthSystem.onCharacterDied -= OnCharacterDied;
    }
}
