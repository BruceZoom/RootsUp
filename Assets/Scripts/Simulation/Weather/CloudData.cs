using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudData
{
    private CloudController _cloudController;
    private CloudController.CloudConfig _config;
    private float _stopTime;

    private bool _dead;
    private bool _needUpdate;

    private Vector3Int _rainRange;
    
    public bool Raining {  get; private set; }
    public Vector2 CloudPosition => new Vector2(_config.X, _config.Y);
    public float Scale => _config.Scale;

    public Vector3Int RainRange => _rainRange;

    public void Start(float currentTime, bool forceStart=false)
    {
        _cloudController = CloudObjectPool.Instance.GetObject();
        _cloudController.SetUpCloud(_config);
        if (forceStart)
        {
            _cloudController.ForceStart();
            CreateRainArea();
        }
        else
        {
            _cloudController.StartSpawn(CreateRainArea);
        }
        _stopTime = currentTime + _config.Duration;
        _needUpdate = true;
        _dead = false;
    }

    public bool Simulate(float currentTime)
    {
        if (_needUpdate && currentTime >= _stopTime)
        {
            _cloudController.GraduallyStopRain(ClearRainArea, RemoveCloud);
            _needUpdate = false;
        }

        return _dead;
    }

    private void CreateRainArea()
    {
        Raining = true;
    }

    private void ClearRainArea()
    {
        Raining = false;
    }


    private void RemoveCloud()
    {
        CloudObjectPool.Instance.ReturnObject(_cloudController);
        _dead = true;
    }


    public CloudData(CloudController.CloudConfig config, Vector2 initSize)
    {
        _config = config;
        Raining = false;

        var size = config.Scale * initSize;
        var left = StructureTileManager.Instance.WorldToCell(new Vector2(config.X - size.x/2, config.Y + size.y/2));
        var right = StructureTileManager.Instance.WorldToCell(new Vector2(config.X + size.x / 2, config.Y + size.y / 2));
        _rainRange = new Vector3Int(left.x, left.y, right.x);
    }
}
