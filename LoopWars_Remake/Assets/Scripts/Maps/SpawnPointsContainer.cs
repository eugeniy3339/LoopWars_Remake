using UnityEngine;

public class SpawnPointsContainer : MonoBehaviour
{
    public Transform[] spawnPoints { get {
            int childCount = transform.childCount;
            Transform[] children = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
            {
                children[i] = transform.GetChild(i);
            }

            return children;
        } 
    }
}
