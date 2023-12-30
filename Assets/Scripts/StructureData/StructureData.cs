using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class StructureData
{
    private int _yLength;

    private List<StructureRowData> _rows;

    Dictionary<int, ContainerData> _containerData;

    public bool HasBlock(int x, int y) => _rows[y].HasBlock(x);

    public int CapacityOverEstimate => _rows.Sum(row => row.Capacity);
    public int TotalContainable => _rows.Sum(row => row.TotalContainable);
    public int TotalHasBlock => _rows.Sum(row => row.TotalHasBlock);

    public int Capacity => _containerData.Sum(kv => kv.Value.Capacity);

    public StructureRowData Row(int y) => _rows[y];

    public string DebugString(int x, int y)
    {
        return $"({x}, {y}): " + _rows[y].DebugString(x);
    }

    public StructureData(int xLength, int yLength)
    {
        _yLength = yLength;

        _rows = new List<StructureRowData>();
        for(int y = 0; y < yLength; y++)
        {
            _rows.Add(new StructureRowData(xLength, yLength, y, this));
        }

        _containerData = new Dictionary<int, ContainerData>();
    }

    public void SetBlock(int targetX, int targetY)
    {
        if (_rows[targetY].HasBlock(targetX))
        {
            return;
        }

        // set block
        _rows[targetY].SetBlock(targetX);

        int containerId = _rows[targetY].GetContainerID(targetX);
        // try to split the container if already a container
        if (containerId != -1)
        {
            // split container
            var newContainers = _containerData[containerId].SplitContainerAt(targetX, targetY, _rows);

            if (newContainers.Count > 1)
            {
                Debug.Log($"{newContainers.Count} new containers created");
            }

            if (newContainers != null)
            {
                // set target container id to -1
                _rows[targetY].SetContainerId(targetX, -1);
                // remove old container
                _containerData.Remove(containerId);
                // add new containers
                foreach (var container in newContainers)
                {
                    _containerData.Add(container.ContainerID, container);
                    // set container id
                    container.OverwriteCellContainerId(this, container.ContainerID);
                }
            }

            // since it already is a container, filling it will not create new ones above it
            return;
        }

        // by default, only this block changes containability
        int leftX = targetX - 1;
        int rightX = targetX + 1;
        int lx, rx;

        // no need to check cells in the same row if it is the lowest row
        // otherwise update containability normally
        if (targetY != 0)
        {
            // try to fill cells to left
            if (_rows[targetY].TryUpdateContainableFill(targetX - 1, _rows[targetY - 1],
                targetY < _yLength - 1 ? _rows[targetY + 1] : null, _containerData, out lx, out rx))
            {
                // extend one cell to left because water can flow diagonally
                // lx will not be 0
                leftX = lx - 1;
            }

            // try to fill cells to right
            if (_rows[targetY].TryUpdateContainableFill(targetX + 1, _rows[targetY - 1],
                targetY < _yLength - 1 ? _rows[targetY + 1] : null, _containerData, out lx, out rx))
            {
                // extend one cell to right because water can flow diagonally
                // rx will not be xLength
                rightX = rx + 1;
            }
        }

        int y = targetY + 1;
        while (y < _yLength
            && _rows[y].TryUpdateContainableRange(leftX, rightX, _rows[y - 1],
                                                  y < _yLength - 1 ? _rows[y + 1] : null,
                                                  _containerData, out lx, out rx))
        {
            // extend one cell to left/right because water can flow diagonally
            leftX = lx - 1;
            rightX = rx + 1;
            //Debug.Log($"Next row to update: [{leftX}, {rightX}]");
            // go to next row
            y += 1;
        }
    }
}