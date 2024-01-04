using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
using UnityEngine.UIElements;

public class TreeData
{
    private StructureData _structureData;

    private float _mineralDeposit = 0;

    public StructureData StructureData => _structureData;

    public float MineralDeposit => _mineralDeposit;
    public float WaterDeposit => _structureData.StoredWater;
    public float WaterCapacity => _structureData.Capacity;

    public int BlocksInRange(int lx, int rx, int y) => _structureData.BlocksInRange(lx, rx, y);

    public int ContainableInRange(int lx, int rx, int y) => _structureData.ContainableInRange(lx, rx, y);

    public bool CanConsumeResource(float waterAmount, float mineralAmount) => mineralAmount <= _mineralDeposit && waterAmount <= _structureData.StoredWater;

    /// <summary>
    /// Deposit rain. Returns whether any water is deposited.
    /// </summary>
    public bool DepositRain(float amount, float mineralRate)
    {
        var remain = _structureData.AddWater(amount);
        _mineralDeposit += mineralRate * amount;

        return remain != amount;
    }

    public void ConsumeResource(float waterAmount, float mineralAmount)
    {
        bool succeed = _structureData.TryGetWater(waterAmount);
        _mineralDeposit -= mineralAmount;
    }

    public TreeData(int xLength, int yLength)
    {
        _structureData = new StructureData(xLength, yLength);
    }
}
