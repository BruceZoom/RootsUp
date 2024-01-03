using System;
using UnityEngine;

public class VillagerData
{
    private VillagerController _bindedController;

    [Serializable]
    public struct VillagerSetting
    {
        public float _idleTimeMin;

        public float _idleTimeMax;

        public float _homeTimeMin;

        public float _homeTimeMax;

        [HideInInspector]
        public float _homeX;
    }
    private VillagerSetting _setting;

    public enum VillaterState { AtHome, Idle, GettingWater, GoingHome };
    private VillaterState _state;

    private float _nextUpdateTime;

    private bool _dead = false;


    public void Start(float startTime)
    {
        _nextUpdateTime = startTime;
    }

    /// <summary>
    /// Simulate the villager at given time.
    /// Returns whether the villager is dead.
    /// </summary>
    public bool Simulate(float currentTime)
    {
        if (_dead) return true;

        if (_nextUpdateTime <= currentTime)
        {
            switch (_state)
            {
                case VillaterState.AtHome:
                    LeaveHome();
                    GoGetWater();
                    _state = VillaterState.Idle;
                    _nextUpdateTime += UnityEngine.Random.Range(_setting._idleTimeMin, _setting._idleTimeMax);
                    break;
                case VillaterState.Idle:
                    GoHome();
                    _nextUpdateTime += UnityEngine.Random.Range(_setting._homeTimeMin, _setting._homeTimeMax);
                    break;
                default:
                    // queuing the current update if the previous has not finished
                    break;
            }
        }
        return false;
    }

    private void LeaveHome()
    {
        if (_bindedController != null)
        {
            Debug.LogError("Didn't return binded controller.");
            VillagerObjectPool.Instance.ReturnVillager(_bindedController);
        }
        _bindedController = VillagerObjectPool.Instance.GetVillager();
        var pos = _bindedController.gameObject.transform.position;
        pos.x = _setting._homeX + UnityEngine.Random.Range(-0.1f, 0.1f);
        _bindedController.gameObject.transform.position = pos;
    }

    private void GoGetWater()
    {
        if (_bindedController == null)
        {
            Debug.LogError("Didn't bind controller.");
            return;
        }

        _state = VillaterState.GettingWater;
        _bindedController.GoGetWater(FinishGetWater);
    }

    private void FinishGetWater()
    {
        // TODO: if not enough water, die
        // TODO: if enough water, go idle
        StartIdle();
    }

    private void StartIdle()
    {
        if (_bindedController == null)
        {
            Debug.LogError("Didn't bind controller.");
            return;
        }

        _state = VillaterState.Idle;
        _bindedController.StartIdle();
    }

    private void GoHome()
    {
        if (_bindedController == null)
        {
            Debug.LogError("Didn't bind controller.");
            return;
        }

        _state = VillaterState.GoingHome;
        _bindedController.StopIdle();
        _bindedController.GoHome(_setting._homeX, ArriveHome);
    }

    private void ArriveHome()
    {
        _state = VillaterState.AtHome;
        VillagerObjectPool.Instance.ReturnVillager(_bindedController);
        _bindedController = null;
    }

    public VillagerData(VillagerSetting villagerSetting)
    {
        _setting = villagerSetting;

        _state = VillaterState.AtHome;
    }
}