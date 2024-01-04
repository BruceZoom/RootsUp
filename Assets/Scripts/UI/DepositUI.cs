using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DepositUI : MonoBehaviour
{
    [SerializeField]
    private Slider _waterDepositSlider;

    [SerializeField]
    private float _waterSliderAnimTime = 0.25f;

    [SerializeField]
    private TextMeshProUGUI _waterDepositText;

    [SerializeField]
    private float _waterTextAnimTime = 0.25f;

    [SerializeField]
    private TextMeshProUGUI _mineralDepositText;

    [SerializeField]
    private float _mineralTextAnimTime = 0.25f;

    [SerializeField]
    private TextMeshProUGUI _populationText;

    [SerializeField]
    private float _populationTextAnimTime = 0.25f;

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
        _mineralDepositText.DOText($"Mineral Deposit: {deposit:0.0} kg", _mineralTextAnimTime);
    }

    private void UpdateWaterDepositUI()
    {
        if (_waterDepositSlider.maxValue != _waterCapacity)
        {
            _waterDepositSlider.DOMaxValue(_waterCapacity, _waterSliderAnimTime);
        }
        //_waterDepositSlider.maxValue = _waterCapacity;
        if (_waterDepositSlider.value != _waterDeposit)
        {
            //_waterDepositSlider.DOKill();
            _waterDepositSlider.DOValue(_waterDeposit, _waterSliderAnimTime);
        }
        _waterDepositText.DOText($"{_waterDeposit:0.0} m^3 / {_waterCapacity:0} m^3", _waterTextAnimTime);
    }

    public void SetPopulation(int population)
    {
        _populationText.DOText($"Population: {population}", _populationTextAnimTime);
    }
}
