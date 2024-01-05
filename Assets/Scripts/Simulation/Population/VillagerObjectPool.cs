using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VillagerObjectPool : ObjectPool<VillagerController>
{
    [SerializeField]
    private float _defaultY = -3.5f;

    protected override VillagerController OnCreate(VillagerController newOjb)
    {
        newOjb.transform.position = new Vector3(0, _defaultY, 0);

        return base.OnCreate(newOjb);
    }

    protected override VillagerController OnGet(VillagerController obj)
    {
        obj.Revive();

        return base.OnGet(obj);
    }
}


/*
public class VillagerObjectPool : PassiveSingleton<VillagerObjectPool>
{
    private List<VillagerController> _villagerPool;

    [SerializeField]
    private int _initialVillager = 10;

    [SerializeField]
    private GameObject _villagerPrefab;

    [SerializeField]
    private float _defaultY = -3.5f;

    public override void Initialize()
    {
        base.Initialize();

        _villagerPool = new List<VillagerController>();
        for (int i = 0; i < _initialVillager; i++)
        {
            var villager = NewVillager();
            villager.gameObject.SetActive(false);
            _villagerPool.Add(villager);
        }
        Debug.Log($"init pooled: {_villagerPool.Count}");
    }

    private VillagerController NewVillager()
    {
        var go = GameObject.Instantiate(_villagerPrefab, transform);
        go.transform.position = new Vector3(0, _defaultY, 0);
        return go.GetComponent<VillagerController>();
    }

    public VillagerController GetVillager()
    {
        if (_villagerPool.Count > 0)
        {
            var villager = _villagerPool.Pop();
            villager.gameObject.SetActive(true);
            villager.Revive();

            return villager;
        }
        else
        {
            Debug.Log("creating new");
            return NewVillager();
        }
    }

    public void ReturnVillager(VillagerController villager)
    {
        villager.gameObject.SetActive(false);
        _villagerPool.Add(villager);
    }
}
*/