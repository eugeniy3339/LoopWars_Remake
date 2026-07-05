using System.Collections;
using UnityEngine;

public class ParticlesHandler : MonoBehaviour
{
    public static ParticleSystem SpawnParticles(GameObject particlesPrefab, Vector2 position, Vector2 upVector)
    {
        if (particlesPrefab == null) return null;
        Transform particles = GameObject.Instantiate(particlesPrefab).transform;
        particles.position = position;
        particles.up = upVector;

        ParticleSystem particleSystem = particles.GetComponent<ParticleSystem>();
        particleSystem.Play();
        MonoCommandsStarter.Instance.StartCoroutine(ParticleLifeCoro(particles.gameObject, particleSystem.startLifetime));

        return particleSystem;
    }

    public static GameObject GetPrefab(string name)
    {
        return Resources.Load<GameObject>("VFX/" + name);
    }

    private static IEnumerator ParticleLifeCoro(GameObject particlesObject, float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        GameObject.Destroy(particlesObject);
    }
}
