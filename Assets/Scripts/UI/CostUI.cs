using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CostUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _waterCostText;

    [SerializeField]
    private TextMeshProUGUI _mineralCostText;

    public void DisplayCost(Vector3 position, float waterCost, float mineralCost)
    {
        gameObject.SetActive(true);
        _waterCostText.enabled = waterCost > 0;
        _mineralCostText.enabled = mineralCost > 0;

        _waterCostText.text = $"Water Cost: {waterCost} m^3";
        _mineralCostText.text = $"Mineral Cost: {mineralCost} kg";

        transform.position = position;
    }

    public void HideCost()
    {
        gameObject.SetActive(false);
    }
}
