using Unity.Netcode;
using UnityEngine;

public class NeededObjects : NetworkBehaviour
{
    private void Awake()
    {
        if (NetworkManager.Singleton.IsServer)
            NetworkObject.Spawn(true);  
    }
}
