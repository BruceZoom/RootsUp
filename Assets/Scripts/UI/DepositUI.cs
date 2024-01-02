using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DepositUI : MonoBehaviour
{
    [SerializeField]
    private Slider _waterDepositSlider;

    [SerializeField]
    private TextMeshProUGUI _waterDepositText;

    [SerializeField]
    private TextMeshProUGUI _mineralDepositText;

    private float _waterCapacity;

    private float _waterDeposit;

    public void SetWaterCapaciy(float capacity)
    {
        if (_waterCapacity == capacity) return;

        _waterCapacity = capacity;
        UpdateWaterDepositUI();
    }

    public void SetWaterDeposit(float deposit, float capacity=-1)
    {
        if (_waterDeposit == deposit && capacity < 0) return;

        if (capacity >= 0)
        {
            _waterCapacity = capacity;
        }
        _waterDeposit = deposit;
        UpdateWaterDepositUI();
    }

    public void SetMineralDeposit(float deposit)
    {
        _mineralDepositText.text = $"Mineral Deposit: {deposit:0.0} kg";
    }

    private void UpdateWaterDepositUI()
    {
        _waterDepositSlider.maxValue = _waterCapacity;
        _waterDepositSlider.value = _waterDeposit;
        _waterDepositText.text = $"{_waterDeposit:0.0} m^3 / {_waterCapacity:0.0} m^3";
    }
}
