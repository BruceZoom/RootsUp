using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class PopulationSimulator
{
    private List<VillagerData> _villagerData;

    [SerializeField]
    private int _initialVillagers;

    [SerializeField]
    private float _defaultHomeX = 24f;

    [SerializeField, Range(0f, 1f)]
    private float _maxReproduceProp = 0.5f;

    [SerializeField]
    private float _minReproduceAvgSat = 3;

    [SerializeField]
    private VillagerData.VillagerSetting _villagerSetting;

    private List<VillagerData> _deadVillagers;

    public void Simulate(float currentTime)
    {
        _deadVillagers.Clear();
        foreach (var villager in _villagerData)
        {
            if(villager.Simulate(currentTime))
            {
                _deadVillagers.Add(villager);
            }
        }
        _villagerData.RemoveAll(v => _deadVillagers.Contains(v));

        // reproduce
        float avgSat = (float)_villagerData.Sum(v => Mathf.Max(v.Satisfaction, 0)) / _villagerData.Count;
        if (avgSat > _minReproduceAvgSat)
        {
            float p = UnityEngine.Random.Range(0f, 1f);
            // m - m/x > p
            // m - p > m/x
            // (m - p) x > m
            // (m - p)^2 x > m^2
            p = _maxReproduceProp - p;
            Debug.Log($"Reproduce weight: {(avgSat - _minReproduceAvgSat + 1) * p * p}");
            if (_maxReproduceProp * _maxReproduceProp < (avgSat - _minReproduceAvgSat + 1) * p * p)
            {
                var villager = new VillagerData(_villagerSetting);
                _villagerData.Add(villager);
                villager.Start(currentTime + UnityEngine.Random.Range(0, 3));
            }
        }

        GameplayUIManager.Instance.DepositUI.SetPopulation(_villagerData.Count);
    }

    public void Start(float startTime)
    {
        foreach (var villager in _villagerData)
        {
            villager.Start(startTime + UnityEngine.Random.Range(0, 3));
        }
        GameplayUIManager.Instance.DepositUI.SetPopulation(_villagerData.Count);
    }

    public void Initialize()
    {
        _villagerSetting._homeX = _defaultHomeX;
     
        _villagerData = new List<VillagerData>();
        for (int i = 0; i < _initialVillagers; i++)
        {
            _villagerData.Add(new VillagerData(_villagerSetting));
        }
        _deadVillagers = new List<VillagerData>();
    }
}