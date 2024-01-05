using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StructureData
{
    private int _xLength;
    private int _yLength;

    private List<StructureRowData> _rows;

    private SortedDictionary<int, ContainerData> _containerData;

    private int _currentContainerId = -1;

    // buffered value of water storage
    private float _waterStorage = -1;

    public bool HasBlock(int x, int y) => _rows[y].HasBlock(x);
    public bool CanGrow(int x, int y) => 0 <= y && y < _yLength && 0 <= x && x < _xLength && (
                                            // at least one adjacent block
                                            (0 < y && _rows[y - 1].HasBlock(x)) ||
                                            (y < _yLength - 1 && _rows[y + 1].HasBlock(x)) ||
                                            (0 < x && _rows[y].HasBlock(x - 1)) ||
                                            (x < _xLength - 1 && _rows[y].HasBlock(x + 1))
                                        );

    public int CapacityOverEstimate => _rows.Sum(row => row.Capacity);
    public int TotalContainable => _rows.Sum(row => row.TotalContainable);
    public int TotalHasBlock => _rows.Sum(row => row.TotalHasBlock);

    public int Capacity => _containerData.Sum(kv => kv.Value.Capacity);

    public int BlocksInRange(int lx, int rx, int y) => _rows.GetRange(0, Mathf.Min(y, _yLength - 1)).Sum(r => r.CountBlocks(lx, rx));

    public int ContainableInRange(int lx, int rx, int y) => _rows.GetRange(0, Mathf.Min(y, _yLength - 1)).Sum(r => r.CountContainable(lx, rx));

    public StructureRowData Row(int y) => _rows[y];

    public float StoredWater
    {
        get {
            //Debug.Log($"True storage: {_containerData.Sum(kv => kv.Value.StoredWater)}");
            //Debug.Log($"Buffered storage: {_waterStorage}");
            if (_waterStorage == -1)
            {
                _waterStorage = _containerData.Sum(kv => kv.Value.StoredWater);
            }
            return _waterStorage;
        }
    }

    public int CurrentContainerId
    {
        get
        {
            if (_currentContainerId == -1)
            {
                _currentContainerId = _containerData.Count > 0 ? _containerData.ElementAt(0).Key : -1;
            }
            return _currentContainerId;
        }
        set
        {
            _currentContainerId = value;
        }
    }

    public int NextContainerId
    {
        get
        {
            var kvp = _containerData.FirstOrDefault(p => !p.Value.IsFull);
            if (kvp.Equals(default(KeyValuePair<int, ContainerData>)))
            {
                return -1;
            }
            else
            {
                _currentContainerId = kvp.Key;
                return _currentContainerId;
            }
        }
    }

    public string DebugInfo(int x, int y)
    {
        var containerId = _rows[y].ContainerID(x);
        float content = 0f;
        if (_containerData.ContainsKey(containerId))
        {
            content = (_containerData[containerId].ContentAt(x, y) ?? 0f);
        }
        else if(containerId != -1)
        {
            Debug.LogError("The cell has an invalid container id.");
        }

        return $"Cell Info: Position: {(x, y)}, " + _rows[y].DebugInfo(x) + $" , Content: {content}";
    }

    /// <summary>
    /// Add specific amount of water to the structure.
    /// Returns the amount exceeds total capacity.
    /// </summary>
    public float AddWater(float amount)
    {
        foreach (var container in _containerData.Values)
        {
            if (container.AddWater(amount, out var remain) <= 0)
            {
                _waterStorage = -1;
                return 0;
            }
            else
            {
                amount = remain;
            }
        }
        _waterStorage = -1;
        return amount;

        /*
        // no container available yet
        if (CurrentContainerId == -1)
        {
            return amount;
        }
        // loop until all water added
        while (_containerData[CurrentContainerId].AddWater(amount, out var remain) <= 0)
        {
            var nextId = NextContainerId;
            amount = remain;
            // run out of container
            if (nextId == -1)
            {
                return remain;
            }
            // otherwise switch to next container
            CurrentContainerId = nextId;
        }
        return amount;
        */
    }

    /// <summary>
    /// Get and consumes specific amount of water.
    /// If not enough water, consumes none and returns false.
    /// </summary>
    public bool TryGetWater(float amount)
    {
        if (amount > StoredWater)
        {
            return false;
        }

        foreach (var container in _containerData.Values.Reverse())
        {
            if (container.GetWater(amount, out var remain) <= 0)
            {
                _waterStorage = -1;
                return true;
            }
            else
            {
                amount = remain;
            }
        }

        //Debug.LogError("Should not consume water if there is not enough water in the first place.");
        return amount <= 0;
    }

    /// <summary>
    /// Add a block at given position. Returns whether need UI updates.
    /// </summary>
    public bool SetBlock(int targetX, int targetY)
    {
        if (_rows[targetY].HasBlock(targetX))
        {
            return false;
        }

        // set block
        _rows[targetY].SetBlock(targetX);
        StructureTileManager.Instance.SetBlock(new Vector3Int(targetX, targetY));
        // fix potential leak regarless of whether the block creates a container
        if (targetY > 0)
        {
            _rows[targetY-1].FixLeak(targetX, targetY, _containerData);
        }

        int containerId = _rows[targetY].ContainerID(targetX);
        // try to split the container if already a container
        if (containerId != -1)
        {
            // split container
            var newContainers = _containerData[containerId].SplitContainerAt(targetX, targetY, _rows, out var spilledWater);

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

            // add spilled water back to new containers
            float remain = AddWater(spilledWater);

            _waterStorage = -1;
            // since it already is a container, filling it will not create new ones above it
            return true;
        }

        // by default, only this block changes containability
        int leftX = targetX - 1;
        int rightX = targetX + 1;
        int lx, rx;
        bool needUpdate = false;

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
                needUpdate = true;
            }

            // try to fill cells to right
            if (_rows[targetY].TryUpdateContainableFill(targetX + 1, _rows[targetY - 1],
                targetY < _yLength - 1 ? _rows[targetY + 1] : null, _containerData, out lx, out rx))
            {
                // extend one cell to right because water can flow diagonally
                // rx will not be xLength
                rightX = rx + 1;
                needUpdate = true;
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
            needUpdate = true;
        }

        return needUpdate;
    }

    public StructureData(int xLength, int yLength)
    {
        _xLength = xLength;
        _yLength = yLength;

        _rows = new List<StructureRowData>();
        for (int y = 0; y < yLength; y++)
        {
            _rows.Add(new StructureRowData(xLength, yLength, y, this));
        }

        _containerData = new SortedDictionary<int, ContainerData>();
    }
}