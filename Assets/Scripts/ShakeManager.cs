using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;
using System.Collections;

public class ShakeManager : MonoBehaviour
{
    public static ShakeManager Instance { get; private set; }

    private CinemachineCamera _cineCam;

    private float shakeTimer;

    private float shakeAmplitude;
    private float shakeFrequency;
    private float shakeDuration;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        _cineCam = GetComponent<CinemachineCamera>();
    }

    public void shakeCam(float intensity, float frequency, float duration)
    {
        CinemachineBasicMultiChannelPerlin perlin = _cineCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        perlin.AmplitudeGain = intensity;
        perlin.FrequencyGain = frequency;

        shakeAmplitude = intensity;
        shakeFrequency = frequency;
        shakeTimer = duration;
        shakeDuration = duration;
    }

    private void Update()
    {
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;

            CinemachineBasicMultiChannelPerlin perlin = _cineCam.GetComponent<CinemachineBasicMultiChannelPerlin>();

            float timeRatio = shakeTimer / shakeDuration;
            perlin.AmplitudeGain = shakeAmplitude * timeRatio;
            perlin.FrequencyGain = shakeFrequency * timeRatio;

            if (shakeTimer <= 0f)
            {
                perlin.AmplitudeGain = 0f;
                perlin.FrequencyGain = 0f;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            shakeCam(3f, 2f, 0.3f);
        }
    }
}