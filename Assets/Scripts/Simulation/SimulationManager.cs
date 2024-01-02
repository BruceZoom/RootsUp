using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : PassiveSingleton<SimulationManager>
{
    private TreeData _treeData;

    private float _simTimer = 0f;
    private float _nextSimTime;

    [Header("Simulation Settings")]
    [SerializeField]
    private float _simInterval = 1f;

    [Header("Rain Settings")]
    [SerializeField]
    private float _mineralRate = 0.1f;

    [SerializeField]
    private float _branchDepositRate = 0.1f;

    [SerializeField]
    private float _containerDepositRate = 0.2f;

    [Header("World Settings")]
    [SerializeField]
    private Vector2 _overWorldCenter;

    [SerializeField]
    private Vector3Int _initialRainRange;

    [Header("Cost Settings")]
    [SerializeField]
    private float _waterCostMultiplier = 0.2f;

    [SerializeField]
    private float _mineralCostMultiplier = 0.25f;

    [SerializeField]
    private float _mineralCostStartDist = 16;


    public StructureData Structure => _treeData.StructureData;

    public override void Initialize()
    {
        base.Initialize();

        Debug.Log(StructureTileManager.Instance.WorldBoundary.max);
        _treeData = new TreeData((int)StructureTileManager.Instance.WorldBoundary.max.x, (int)StructureTileManager.Instance.WorldBoundary.max.y);
    }

    private void Start()
    {
        _nextSimTime = _simTimer + _simInterval;
    }

    private void Update()
    {
        _simTimer += Time.deltaTime;
        if (_simTimer >= _nextSimTime)
        {
            _nextSimTime += _simInterval;

            Simulate();
        }
    }

    private void Simulate()
    {
        var containable = _treeData.ContainableInRange(_initialRainRange.x, _initialRainRange.z, _initialRainRange.y);
        var blocks = _treeData.BlocksInRange(_initialRainRange.x, _initialRainRange.z, _initialRainRange.y);

        float rainToAdd = ((float)blocks * _branchDepositRate + (float)(containable - blocks) * _containerDepositRate) * _simInterval;
        
        bool updateWater = _treeData.DepositRain(rainToAdd, _mineralRate);

        if (updateWater)
        {
            GameplayUIManager.Instance.DepositUI.SetWaterDeposit(_treeData.WaterDeposit);
        }
        GameplayUIManager.Instance.DepositUI.SetMineralDeposit(_treeData.MineralDeposit);
    }

    public void GetResourceCostAt(int x, int y, out float waterCost, out float mineralCost)
    {
        float mDist = Mathf.Abs(_overWorldCenter.x - x) + Mathf.Abs(_overWorldCenter.y - y);

        waterCost = Mathf.Ceil(mDist * _waterCostMultiplier);
        mineralCost = Mathf.Max((mDist - _mineralCostStartDist), 0);
        mineralCost = Mathf.Ceil(mineralCost * mineralCost * _mineralCostMultiplier);
    }

    public bool CanConsumeResourceAt(int x, int y)
    {
        float waterCost, mineralCost;
        GetResourceCostAt(x, y, out waterCost, out mineralCost);
        return _treeData.CanConsumeResource(waterCost, mineralCost);
    }

    public void ConsumeResourceAt(int x, int y)
    {
        float waterCost, mineralCost;
        GetResourceCostAt(x, y, out waterCost, out mineralCost);
        _treeData.ConsumeResource(waterCost, mineralCost);
    }    
}
