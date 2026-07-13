using System.Collections;
using UnityEngine;

public class ParticlesHandler : MonoBehaviour
{


    public static ParticleSystem SpawnParticles(GameObject particlesPrefab, Vector2 position, Vector2 upVector, Color color, float lifeTime)
    {
        if (particlesPrefab == null) return null;
        Transform particles = GameObject.Instantiate(particlesPrefab).transform;
        particles.position = position;
        particles.up = upVector;

        ParticleSystem particleSystem = particles.GetComponent<ParticleSystem>();
        var main = particleSystem.main;
        main.startColor = new ParticleSystem.MinMaxGradient(ColorsManager.GetTonedColor(particleSystem.main.startColor.colorMin, color), ColorsManager.GetTonedColor(particleSystem.main.startColor.colorMax, color));
        main.duration = lifeTime;
        particleSystem.Play();
        if(!particleSystem.loop)
            MonoCommandsStarter.Instance.StartCoroutine(ParticleLifeCoro(particles.gameObject, particleSystem.startLifetime + particleSystem.duration));

        return particleSystem;
    }

    public static ParticleSystem SpawnParticles(GameObject particlesPrefab, Vector2 position, Vector2 upVector, Color color)
    {
        if (particlesPrefab == null) return null;
        Transform particles = GameObject.Instantiate(particlesPrefab).transform;
        particles.position = position;
        particles.up = upVector;

        ParticleSystem particleSystem = particles.GetComponent<ParticleSystem>();
        var main = particleSystem.main;
        main.startColor = new ParticleSystem.MinMaxGradient(ColorsManager.GetTonedColor(particleSystem.main.startColor.colorMin, color), ColorsManager.GetTonedColor(particleSystem.main.startColor.colorMax, color));
        particleSystem.Play();
        if (!particleSystem.loop)
            MonoCommandsStarter.Instance.StartCoroutine(ParticleLifeCoro(particles.gameObject, particleSystem.startLifetime + particleSystem.duration));

        return particleSystem;
    }

    public static ParticleSystem SpawnParticles(GameObject particlesPrefab, Vector2 position, Vector2 upVector)
    {
        return SpawnParticles(particlesPrefab, position, upVector, Color.white);
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
