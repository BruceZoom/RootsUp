using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

[Serializable]
public class WeatherSimulator
{
    private List<CloudData> _cloudData;

    private float _lastUpdateTime;
    private float _nextGenerationTime;

    [SerializeField]
    public Vector2 _defaultCloudSize;
    [SerializeField]
    private LayerMask _cloudLayer;

    [SerializeField]
    private float _currentMaxCloud = 1;
    [SerializeField]
    private float _maxCloudOverTime = 1/300f;
    [SerializeField, Range(0f, 1f)]
    private float _maxGenerateProp = 0.5f;
    [SerializeField]
    private float _generationCD = 60f;

    [SerializeField]
    private float _minCloudY = 8;
    [SerializeField]
    private float _minCloudScale = 0.75f;
    [SerializeField]
    private float _maxCloudScale = 1.5f;
    [SerializeField]
    private Vector2 _generationPostionExpand;

    [SerializeField]
    private float _minRainDuration = 120f;
    [SerializeField]
    private float _maxRainDuration = 360f;
    [SerializeField]
    private float _minRainDensity = 1.25f;
    [SerializeField]
    private float _maxRainDensity = 1.5f;

    [SerializeField]
    private Vector2 _initialCloudPosition;
    [SerializeField]
    private float _initialCloudScale = 1f;
    [SerializeField]
    private float _initialCloudDuration = 300f;
    [SerializeField]
    private float _initialCloudDensity = 1.25f;


    public void Simulate(float currentTime)
    {
        List<CloudData> cloudToRemove = new List<CloudData>();
        foreach (CloudData cloudData in _cloudData)
        {
            if (cloudData.Simulate(currentTime))
            {
                cloudToRemove.Add(cloudData);
            }
        }
        foreach(CloudData cloudData in cloudToRemove)
        {
            _cloudData.Remove(cloudData);
        }

        // do not generate if exceeds max cloud limit
        _currentMaxCloud += (currentTime - _lastUpdateTime) * _maxCloudOverTime;
        _lastUpdateTime = currentTime;
        if (_cloudData.Count + 1 >= _currentMaxCloud || currentTime < _nextGenerationTime) return;

        // generation propability proportional to the empty slots
        float p = UnityEngine.Random.Range(0f, 1f);
        //Debug.Log((1f - _cloudData.Count / _currentMaxCloud) * _maxGenerateProp);
        if ((1f - _cloudData.Count / _currentMaxCloud) * _maxGenerateProp > p)
        {
            Vector3 bound = SimulationManager.Instance.TreeBound;
            CloudController.CloudConfig config = new CloudController.CloudConfig();
            config.X = UnityEngine.Random.Range(bound.x - _generationPostionExpand.x, bound.z + _generationPostionExpand.x);
            config.Y = UnityEngine.Random.Range(_minCloudY, Mathf.Max(_minCloudY, bound.y + _generationPostionExpand.y));
            config.Scale = UnityEngine.Random.Range(_minCloudScale, _maxCloudScale);

            // fails to generate if exceeds world boundary collide with others
            if (StructureTileManager.Instance.WorldBoundary.Contains(new Vector2(config.X, config.Y)) &&
                Physics2D.BoxCast(new Vector2(config.X - 0.1f, config.Y), _defaultCloudSize * config.Scale, 0f, Vector2.right, 0.2f, _cloudLayer))
            {
                // delay generation
                Debug.Log("Collide with existing cloud.");
                _currentMaxCloud -= 10 * _maxCloudOverTime;
                return;
            }

            // decide duration and start spawn
            config.Duration = UnityEngine.Random.Range(_minRainDuration, _maxRainDuration);
            config.Density = UnityEngine.Random.Range(_minRainDensity, _maxRainDensity);
            var cloud = new CloudData(config, _defaultCloudSize);
            cloud.Start(currentTime);
            _cloudData.Add(cloud);
            _nextGenerationTime = currentTime + _generationCD;
        }
    }

    public IEnumerable<Vector3Int> GetRainRange()
    {
        foreach(var cloud in _cloudData)
        {
            if (cloud.Raining)
            {
                yield return cloud.RainRange;
            }
        }
    }

    public void Start(float startTime)
    {
        _lastUpdateTime = startTime;
        _nextGenerationTime = startTime;

        if (_currentMaxCloud < 2)
        {
            SetUpInitialCloud(startTime);
        }
    }

    private void SetUpInitialCloud(float startTime)
    {
        CloudController.CloudConfig config = new CloudController.CloudConfig();
        config.X = _initialCloudPosition.x;
        config.Y = _initialCloudPosition.y;
        config.Scale = _initialCloudScale;
        config.Duration = _initialCloudDuration;
        config.Density = _initialCloudDensity;

        var cloud = new CloudData(config, _defaultCloudSize);
        cloud.Start(startTime, true);
        _cloudData.Add(cloud);
    }


    public void Initialize()
    {
        _cloudData = new List<CloudData>();
    }
}
