using System;
using UnityEngine;

public class VillagerData
{
    private VillagerController _bindedController;

    [Serializable]
    public struct VillagerSetting
    {
        public float IdleTimeMin;

        public float IdleTimeMax;

        public float HomeTimeMin;

        public float HomeTimeMax;

        public float GetWaterTime;

        public float WaterConsumption;

        public int DeathCounter;

        [HideInInspector]
        public float _homeX;
    }
    private VillagerSetting _setting;

    public enum VillaterState { AtHome, Idle, GettingWater, GoingHome };
    private VillaterState _state;

    private float _nextUpdateTime;

    private bool _dead = false;

    private int _satisfiedCounter = 0;

    public int Satisfaction => _satisfiedCounter;


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
                    _nextUpdateTime += _setting.GetWaterTime + UnityEngine.Random.Range(_setting.IdleTimeMin, _setting.IdleTimeMax);
                    break;
                case VillaterState.Idle:
                    GoHome();
                    _nextUpdateTime += UnityEngine.Random.Range(_setting.HomeTimeMin, _setting.HomeTimeMax);
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
        _bindedController.GoGetWater(_setting.GetWaterTime, FinishGetWater);
        _bindedController.ToggleEmoji(VillagerController.EmojiName.Water, true);
    }

    private void FinishGetWater()
    {
        // if there are enough water then get water and start idle
        if (SimulationManager.Instance.Tree.CanConsumeResource(_setting.WaterConsumption, 0f))
        {
            Debug.Log("Nice!");
            SimulationManager.Instance.Tree.ConsumeResource(_setting.WaterConsumption, 0f);
            // increase satisfaction
            _satisfiedCounter += 1;
            StartIdle();
            if (_satisfiedCounter > 0)
            {
                _bindedController.ToggleEmoji(VillagerController.EmojiName.Happy, true);
            }
            else
            {
                _bindedController.ToggleEmoji(VillagerController.EmojiName.Unhappy, true);
            }
        }
        // if there are not enough water
        else
        {
            Debug.Log("I am thirsty!");
            // decrease satifaction
            _satisfiedCounter = Mathf.Min(_satisfiedCounter - 1, -1);
            // die if didn't water enough water for days
            if (_satisfiedCounter <= -_setting.DeathCounter)
            {
                _bindedController.ToggleEmoji(VillagerController.EmojiName.Dead, true);
                Die();
            }
            // otherwise start idle
            else
            {
                StartIdle();
                _bindedController.ToggleEmoji(VillagerController.EmojiName.Unhappy, true);
            }
        }
    }

    private void Die()
    {
        _dead = true;

        _bindedController.Die(delegate {
            VillagerObjectPool.Instance.ReturnVillager(_bindedController);
            _bindedController = null;
        });
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