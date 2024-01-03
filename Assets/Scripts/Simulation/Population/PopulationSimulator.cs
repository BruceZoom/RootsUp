using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PopulationSimulator
{
    private List<VillagerData> _villagerData;

    [SerializeField]
    private int _initialVillagers;

    [SerializeField]
    private float _defaultHomeX = 24f;

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
    }

    public void Start(float startTime)
    {
        foreach (var villager in _villagerData)
        {
            villager.Start(startTime);
        }
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