using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PortalController : MonoBehaviour
{
    public VisualEffect spawningPortal;
    public VisualEffect staticPortal;
    public Light light;
    public float lightIntensity;

    public float portalRadius;
    public float minAngularSpeed;


    private bool hasPortalSpawned;
    private bool firstClick;
    private float targetRadius;
    private Vector2 centerPoint;
    private Vector2 rightVector;
    private float oldRadian;

    private void Awake()
    {
        hasPortalSpawned = false;
        firstClick = true;
        rightVector = new Vector2(1f, 0);
        targetRadius = Screen.height * .25f;
    }

    void Update()
    {
        

        if (hasPortalSpawned)
        {
            if (Input.GetMouseButtonDown(0))
            {
                hasPortalSpawned = false;
                StartCoroutine(FlipLight(false, .8f));
                staticPortal.Stop();
            }
        }
        else
        {

            if (Input.GetMouseButton(1))
            {
                if (firstClick)
                {
                    spawningPortal.Play();
                    firstClick = false;
                    centerPoint = GetMousePositionInScreen();
                    transform.position = GetMousePositionInWorld();
                }

                Vector2 currentMousePos = GetMousePositionInScreen();
                Vector2 direction = (currentMousePos - centerPoint);
                float magnitude = direction.magnitude;
                float radian = Mathf.Acos(Vector2.Dot(direction / magnitude, rightVector));

                if (direction.y < 0)
                {
                    radian = Mathf.PI * 2 - radian;
                }

                float currentAngularSpeed = Mathf.Abs(radian - oldRadian) / Time.deltaTime;
                float currentRadius = Mathf.Min((magnitude / targetRadius) * portalRadius, portalRadius);
                int currentSpawnRate = (int)((currentAngularSpeed / minAngularSpeed) * (currentRadius / portalRadius) * 4000);
                currentSpawnRate = Mathf.Clamp(currentSpawnRate, 500, 4000);

                spawningPortal.SetFloat("ArcSequencer", RadianToArcSequencer(radian));
                spawningPortal.SetFloat("MajorRadius", currentRadius);
                spawningPortal.SetInt("SpawnRate", currentSpawnRate);

                oldRadian = radian;

                if (currentAngularSpeed >= minAngularSpeed && currentRadius == portalRadius && Input.GetMouseButtonDown(0))
                {
                    staticPortal.SetFloat("MajorRadius", portalRadius);
                    staticPortal.Play();
                    StartCoroutine(FlipLight(true, .5f));
                    hasPortalSpawned = true;
                    StopSpawnSequence();
                }
            }
            else
            {
                StopSpawnSequence();
            }
        }
    }

    private void StopSpawnSequence()
    {
        firstClick = true;
        spawningPortal.Stop();
    }

    private Vector3 GetMousePositionInWorld()
    {
        Vector3 rawMousePosition = Input.mousePosition;
        rawMousePosition.z = 22f;
        return Camera.main.ScreenToWorldPoint(rawMousePosition);
    }

    private Vector2 GetMousePositionInScreen()
    {
        return new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    }

    private float RadianToArcSequencer(float radian)
    {
        float normalizedRadian = radian / (Mathf.PI * 2);
        normalizedRadian -= .25f;
        if (normalizedRadian < 0)
        {
            normalizedRadian += 1;
        }
        return 1 - normalizedRadian;
    }

    private IEnumerator FlipLight(bool on, float duration)
    {
        float targetIntensity = lightIntensity;

        if (!on)
        {
            targetIntensity = 0;
        }

        float elapsedTime = 0;
        float initialIntensity = light.intensity;
        while (elapsedTime < duration)
        {
            light.intensity = Mathf.Lerp(initialIntensity, targetIntensity, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        light.intensity = targetIntensity;
    }
}
