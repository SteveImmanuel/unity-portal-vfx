using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PortalSpawner : MonoBehaviour
{
    public float maxRotationSpeed;
    public float majorRadius;
    public float spawnDuration;
    public int spawnRate;
    public float lightIntensity;
    public AnimationCurve spawnCurve;
    public VisualEffect spawningPortal;
    public VisualEffect staticPortal;
    public Light lightPortal;


    private bool hasPlayed;
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        hasPlayed = false;
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (hasPlayed)
            {
                hasPlayed = false;
                StopSpawnSequence();
            }
            else
            {
                hasPlayed = true;
                spawnCoroutine = StartCoroutine(SpawnSequence());
            }
        }
    }

    private void StopSpawnSequence()
    {
        StartCoroutine(TurnOffLight(1f));
        spawningPortal.Stop();
        staticPortal.Stop();
        StopCoroutine(spawnCoroutine);
    }

    private IEnumerator SpawnSequence()
    {
        float arcSequencer = 0;
        float elapsedTime = 0;
        bool staticPortalPlayed = false;
        spawningPortal.Play();

        while (elapsedTime < spawnDuration)
        {
            float progress = elapsedTime / spawnDuration;
            float rotationSpeed = Mathf.Clamp(progress * maxRotationSpeed, 1, maxRotationSpeed);

            elapsedTime += Time.deltaTime;
            arcSequencer = ((arcSequencer + Time.deltaTime) * rotationSpeed) % 1;

            float currentMajorRadius = Mathf.Clamp(majorRadius * spawnCurve.Evaluate(progress), .2f * majorRadius, majorRadius);
            float currentMinorRadius = Random.Range(0, .03f);
            int currentSpawnRate = (int)Mathf.Clamp(spawnRate * spawnCurve.Evaluate(progress), 500, .3f * spawnRate) + Random.Range(-250, 250);
            float currentLightIntensity = lightIntensity * spawnCurve.Evaluate(progress);

            spawningPortal.SetFloat("ArcSequencer", arcSequencer);
            spawningPortal.SetFloat("MajorRadius", currentMajorRadius);
            spawningPortal.SetFloat("MinorRadius", currentMinorRadius);
            spawningPortal.SetInt("SpawnRate", currentSpawnRate);
            lightPortal.intensity = currentLightIntensity;

            if (progress > .5f)
            {
                if (!staticPortalPlayed)
                {
                    staticPortal.Play();
                    staticPortalPlayed = true;
                }
                staticPortal.SetFloat("MajorRadius", currentMajorRadius);
            }

            yield return null;
        }

        staticPortal.SetInt("SpawnRate", spawnRate);
        spawningPortal.Stop();
        yield return null;
    }

    private IEnumerator TurnOffLight(float duration)
    {
        float initialIntensity = lightPortal.intensity;
        float targetIntensity = 0;

        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            lightPortal.intensity = Mathf.Lerp(initialIntensity, targetIntensity, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        lightPortal.intensity = targetIntensity;
    }
}
