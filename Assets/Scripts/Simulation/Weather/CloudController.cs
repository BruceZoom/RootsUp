using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using VInspector;

public class CloudController : MonoBehaviour
{
    [SerializeField]
    private Transform _cloudBody;

    [SerializeField]
    private ParticleSystem _rainParticle;

    [SerializeField]
    private ParticleSystem _foregroundRainParticle;

    [SerializeField]
    private ParticleSystem _backgroundRainParticle;

    public delegate void CloudActionCallback();

    [SerializeField]
    private float _spawnSpeed;

    private float _initialWidth;

    [SerializeField]
    private float _graduallyStopTime;

    [SerializeField]
    private float _disappearTime;

    [Serializable]
    public struct CloudConfig {
        public float X;
        public float Y;
        public float Scale;
        public float Density;
        public float Duration;
    }

    [SerializeField]
    private CloudConfig _config;


    [Button("PrepareRain")]
    private void PrepareRain()
    {
        Initialize();
        SetUpCloud(_config);
    }


    public void Initialize()
    {
        _initialWidth = GetComponent<BoxCollider2D>().size.x;
    }


    public void SetUpCloud(CloudConfig config)
    {
        _config = config;
        transform.position = new Vector3(config.X, config.Y, transform.position.z);
        transform.localScale = Vector3.one * config.Scale;

        var shape = _rainParticle.shape;
        shape.radius = _initialWidth / 2 * config.Scale;
        shape = _foregroundRainParticle.shape;
        shape.radius = _initialWidth / 2 * config.Scale;
        shape = _backgroundRainParticle.shape;
        shape.radius = _initialWidth / 2 * config.Scale;

        var emission = _rainParticle.emission;
        emission.rateOverTime = _config.Density * _initialWidth * config.Scale;
        emission = _foregroundRainParticle.emission;
        emission.rateOverTime = _config.Density * _initialWidth * config.Scale;
        emission = _backgroundRainParticle.emission;
        emission.rateOverTime = _config.Density * _initialWidth * config.Scale;

        StopRain();
    }

    [Button("StartRain")]
    private void StartSpawn()
    {
        StartSpawn(null);
    }

    public void StartSpawn(CloudActionCallback finishCallback)
    {
        StopRain();
        _cloudBody.DOScale(1f, _config.Scale / _spawnSpeed)
                 .From(0f)
                 .SetEase(Ease.Linear)
                 .OnComplete(() =>
                 {
                     StartRain();
                     finishCallback?.Invoke();
                 });
    }

    public void ForceStart()
    {
        _cloudBody.localScale = Vector3.one * _config.Scale;
        StartRain();
    }

    private void StartRain()
    {
        _rainParticle.Play();
        _foregroundRainParticle.Play();
        _backgroundRainParticle.Play();
    }

    [Button("StopRain")]
    private void GraduallyStopRain()
    {
        GraduallyStopRain(null, null);
    }

    public void GraduallyStopRain(CloudActionCallback rainStopCallback, CloudActionCallback finishCallback)
    {
        _cloudBody.DOScaleY(0.5f, _graduallyStopTime)
                  .SetEase(Ease.Linear)
                  .OnComplete(() =>
                  {
                      StopRain();
                      rainStopCallback?.Invoke();
                      transform.DOScale(0f, _disappearTime)
                               .SetEase(Ease.InQuad)
                               .OnComplete(() =>
                               {
                                   finishCallback?.Invoke();
                               });
                  });
    }

    private void StopRain()
    {
        _rainParticle.Stop();
        _foregroundRainParticle.Stop();
        _backgroundRainParticle.Stop();
    }
}
