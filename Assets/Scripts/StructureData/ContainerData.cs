using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class ContainerData
{
    public static int CellPosToContainerID(int x, int y) => 1000 * y + x;
    public static Vector2Int ContainerIDToCellPos(int id) => new Vector2Int(id % 1000, id / 1000);

    public class ContainerRowData
    {
        public int x;
        public int y;
        public float content;

        public ContainerRowData(int lx, int rx, float amount=0)
        {
            this.x = lx;
            this.y = rx;
            this.content = amount;
        }

        public Vector3Int ToInterval(int y) => new Vector3Int(x, y, this.y);

        public float Volume => y - x + 1;
        public float AmountPerCell => content / Volume;
    }

    // 1th-d: row index
    // 2nd-d: X: left-most empty cell, Y: right-most empty cell, Z: amoung stored in this slice
    private List<List<ContainerRowData>> _containerRows;
    // slices with water filled
    // FIXME: consider change to List<Dictionary<Vector2Int, float>> to record stored water in each slice
    //private List<List<Vector2Int>> _filledRows;

    private int _containerId;
    private int _xLength;
    private int _yLength;
    private StructureData _structure;

    // 1th-d: row index (_yLength+1 row), data: left/right-most leak point
    private List<int> _leftLeakX;
    private List<int> _rightLeakX;

    private List<Vector3Int> _topRows;
    //private float _storedWater = 0;

    public int ContainerID => _containerId;

    public bool ContainsCell(int x, int y) => _containerRows[y].Any(i => i.x <= x && x <= i.y);

    public ContainerRowData FindIntervalCover(int x, int y) => _containerRows[y].Find(i => i.x <= x && x <= i.y);
    public List<ContainerRowData> FindIntervalOverlap(int lx, int rx, int y) =>
        _containerRows[y].FindAll(i => i.x <= lx && lx <= i.y ||
                                       i.x <= rx && rx <= i.y ||
                                       lx <= i.x && i.y <= rx).ToList();

    public List<ContainerRowData> ContainerRowAt(int y) => _containerRows[y];
    public int LeftLeakX(int y) => _leftLeakX[y];
    public int RightLeakX(int y) => _rightLeakX[y];
    //public List<Vector2Int> FilledRowAt(int y) => _filledRows[y];

    //public float StoredWater => _storedWater;
    public float StoredWater => _containerRows.Sum(row => row.Sum(v => v.content));
    public int Capacity => _containerRows.GetRange(0, GetLowestLeakY())
                                .Sum(row => row.Sum(v => v.y - v.x + 1));
    public bool IsFull => StoredWater >= Capacity;

    public IEnumerable<ContainerRowData> ContainerRows => _containerRows.SelectMany(i => i);

    public float? ContentAt(int x, int y) => FindIntervalCover(x, y)?.AmountPerCell;

    public List<Vector3Int> TopRows => _topRows;

    /*
    public float MaxCapCurrentFilledCell => _filledRows.Sum(r => r.Sum(v => v.y - v.x + 1));
    public int CurrentConsumingRowY => Mathf.Max(_filledRows.FindLastIndex(r => r.Count > 0), 0);
    public int CurrentFillingRowY
    {
        get
        {
            for (int y = 0; y < _yLength; y++)
            {
                if (_filledRows[y].Count != _containerRows[y].Count)
                {
                    return y == 0 ? y : y - 1;
                }
            }
            return _yLength - 1;
        }
    }
    public float MaxCapFilledRowAt(int y) => _filledRows[y].Sum(v => v.y - v.x + 1);
    */

    /// <summary>
    /// Add specific amount of water.
    /// Returns the amount exceeds capacity.
    /// </summary>
    public float AddWater(float amount, out float remain)
    {
        remain = amount;
        // do nothing if it is already full
        if (StoredWater >= Capacity) return remain;

        for (int y = 0; y < GetLowestLeakY(); y++)
        {
            foreach (var row in _containerRows[y])
            {
                remain = amount - (row.Volume - row.content);
                remain = remain > 0 ? remain : 0;
                row.content += amount - remain;
                // only when this row changes or it is not a top row
                // will it become a candidate
                if (amount != remain && !_topRows.Contains(row.ToInterval(y)))
                {
                    // add this row as a candidate
                    _topRows.Add(row.ToInterval(y));
                    if (y > 0)
                    {
                        foreach(var rowBelow in FindIntervalOverlap(row.x - 1, row.y + 1, y - 1))
                        {
                            // if was potentially a surface or empty only if it is a candidate
                            if (_topRows.Remove(rowBelow.ToInterval(y - 1)))
                            {
                                StructureTileManager.Instance.SetWaterBody(rowBelow.ToInterval(y - 1));
                            }
                        }
                    }
                }
                amount = remain;

                if (remain <= 0) break;
            }
            if (remain <= 0) break;
        }

        foreach(var row in _topRows)
        {
            StructureTileManager.Instance.SetWaterSurface(row);
        }

        return remain;
    }

    /// <summary>
    /// Get and consume specific amount of water.
    /// If there is not enough water, consumes all that remains
    /// and returns the requested amount exceeds storage.
    /// </summary>
    public float GetWater(float amount, out float remain)
    {
        remain = amount;
        // do nothing if it is already empty
        if (StoredWater <= 0) return remain;

        var origTopRows = _topRows.ToList();
        for (int y = GetLowestLeakY() - 1; y >= 0; y--)
        {
            foreach (var row in _containerRows[y])
            {
                remain = amount - row.content;
                remain = remain > 0 ? remain : 0;
                row.content -= amount - remain;
                // only need to search further if the current row becomes empty
                if (amount != remain && row.content == 0)
                {
                    // current row is no longer top row
                    _topRows.Remove(row.ToInterval(y));
                    StructureTileManager.Instance.ClearWaterTile(row.ToInterval(y));

                    // find all possible top rows
                    if (y > 0)
                    {
                        foreach (var rowBelow in FindIntervalOverlap(row.x - 1, row.y + 1, y - 1))
                        {
                            _topRows.Add(rowBelow.ToInterval(y - 1));
                        }
                    }
                }
                amount = remain;

                if (remain <= 0) break;
            }
            if (remain <= 0) break;
        }

        foreach (var row in _topRows)
        {
            if (origTopRows.Contains(row)) continue;

            StructureTileManager.Instance.SetWaterSurface(row);
        }

        return remain;
    }

    public int GetLowestLeakY()
    {
        int leftY = _leftLeakX.FindIndex(x => x < _xLength);
        int rightY = _rightLeakX.FindIndex(x => x >= 0);
        return leftY != -1 ? (
                    rightY != -1 ? Mathf.Min(leftY, rightY) : leftY
                ) : (
                    rightY != -1 ? rightY : _yLength
                );
    }

    public IEnumerable<Vector3Int> AllIntervals()
    {
        for (int y = 0; y < _yLength; y++)
        {
            foreach(var i in _containerRows[y])
            {
                yield return new Vector3Int(i.x, y, i.y);
            }
        }
    }

    public void AddInterval(int y, int lx, int rx, StructureRowData nextRow, bool byPass=false, float amount=0)
    {
        if (lx > rx) return;

        _containerRows[y].Add(new ContainerRowData(lx, rx, amount));

        if (byPass) return;

        // may introduce new leak on left
        if (nextRow == null || !nextRow.Containable(lx-1))
        {
            _leftLeakX[y + 1] = _leftLeakX[y] < lx - 1 ? _leftLeakX[y] : lx - 1;
        }
        // may introduce new leak on right
        if (nextRow == null || !nextRow.Containable(rx + 1))
        {
            _rightLeakX[y + 1] = rx + 1 < _rightLeakX[y] ? _rightLeakX[y] : rx + 1;
        }
        // may fix old leak if left wall covers left leak
        if (lx-1 <= _leftLeakX[y])
        {
            _leftLeakX[y] = _xLength;
        }
        // may fix old leak if left wall covers left leak
        if (_rightLeakX[y] <= rx+1)
        {
            _rightLeakX[y] = -1;
        }
    }

    public void AddInterval(int y, ContainerRowData row, StructureRowData nextRow, bool byPass = false)
    {
        AddInterval(y, row.x, row.y, nextRow, byPass, row.content);
        //newContainer.AddInterval(y, botSlice.x, botSlice.y, y < _yLength ? rows[y] : null);
    }

    /*
    public void AddFilledInterval(int y, int lx, int rx)
    {
        if (lx > rx) return;

        _filledRows[y].Add(new Vector2Int(lx, rx));
    }
    */

    public List<ContainerData> SplitContainerAt(int targetX, int targetY, List<StructureRowData> rows, out float spilledWater)
    {
        // TODO: how to correctly split top rows?

        ContainerRowData targetSlice = FindIntervalCover(targetX, targetY);
        if (targetSlice == null)
        {
            Debug.LogWarning("Cannot split at a cell which is not a container.");
            spilledWater = 0;
            return null;
        }

        // clear tile
        StructureTileManager.Instance.ClearWaterTile(new Vector3Int(targetX, targetY, targetX));

        _containerRows[targetY].Remove(targetSlice);
        spilledWater = targetSlice.AmountPerCell;
        // it is already a container, filling it will not create new leak
        // therefore by pass check
        // fill with original content
        AddInterval(targetY, targetSlice.x, targetX - 1, null, true, spilledWater * (targetX - targetSlice.x));
        AddInterval(targetY, targetX + 1, targetSlice.y, null, true, spilledWater * (targetSlice.y - targetX));
        Debug.Log(_containerRows.SelectMany(i => i).Count());
        if (_topRows.Remove(targetSlice.ToInterval(targetY)))
        {
            _topRows.Add(new Vector3Int(targetSlice.x, targetY, targetX - 1));
            _topRows.Add(new Vector3Int(targetX + 1, targetY, targetSlice.y));
        }

        /*
        // split filled water
        // FIXME: cause bugs, no way to compute individule storage
        if (_filledRows[targetY].Contains(targetInterval))
        {
            _filledRows[targetY].Remove(targetInterval);
            AddFilledInterval(targetY, targetInterval.x, targetX - 1);
            AddFilledInterval(targetY, targetX + 1, targetInterval.y);
        }
        */

        List<ContainerData> newContainers = new List<ContainerData>();
        for (int y = 0; y < _yLength; y++)
        {
            while (_containerRows[y].Count > 0)
            {
                //Debug.Log($"before: {_containerRows[y].Count}");
                ContainerRowData botSlice = _containerRows[y].Pop(0);
                //Debug.Log(_containerRows[y].Count);
                ContainerData newContainer =
                        new ContainerData(_xLength, _yLength, CellPosToContainerID(botSlice.x, y), _structure);
                newContainers.Add(newContainer);
                
                // use BFS to generate the new container
                //List<Vector3Int> bfsQueue = new List<Vector3Int>();
                List<Tuple<ContainerRowData, int>> bfsQueue = new List<Tuple<ContainerRowData, int>>();

                // discover root node
                //bfsQueue.Add(new Vector3Int(botSlice.x, y, botSlice.y));
                bfsQueue.Add(new Tuple<ContainerRowData, int>(botSlice, y));
                // remove from to-visit list
                _containerRows[y].Remove(botSlice);

                while (bfsQueue.Count > 0)
                {
                    // visit a node
                    var node = bfsQueue.Pop(0);
                    var curSlice = node.Item1;
                    var curY = node.Item2;
                    // start process data
                    // transfer interval
                    newContainer.AddInterval(curY, curSlice, curY + 1 < _yLength ? rows[curY + 1] : null);
                    // transfer top row
                    var curInterval = curSlice.ToInterval(curY);
                    if (_topRows.Contains(curInterval))
                    {
                        newContainer.TopRows.Add(curInterval);
                        _topRows.Remove(curInterval);
                    }

                    // search below
                    if (0 < curY)
                    {
                        var lastY = curY - 1;
                        // extend in both direction because water flows diagonally
                        var slices = FindIntervalOverlap(curSlice.x - 1, curSlice.y + 1, lastY);
                        foreach (var slice in slices)
                        {
                            // discover node
                            bfsQueue.Add(new Tuple<ContainerRowData, int>(slice, lastY));
                            // remove from to-visit list
                            _containerRows[lastY].Remove(slice);
                        }
                    }
                    // search above
                    if (curY < _yLength - 1)
                    {
                        var nextY = curY + 1;
                        // extend in both direction because water flows diagonally
                        var slices = FindIntervalOverlap(curSlice.x - 1, curSlice.y + 1, nextY);
                        foreach (var slice in slices)
                        {
                            // discover node
                            bfsQueue.Add(new Tuple<ContainerRowData, int>(slice, nextY));
                            // remove from to-visit list
                            _containerRows[nextY].Remove(slice);
                        }
                    }
                }
            }
        }
        Debug.Log($"remaining top row: {_topRows.Count}");

        return newContainers;
    }

    public ContainerData MergeContainer(ContainerData other)
    {
        if (other.ContainerID == this.ContainerID)
        {
            Debug.LogWarning($"Cannot merge the same container with itself: {ContainerID}");
            return this;
        }

        for (int y = 0; y < _containerRows.Count; y++)
        {
            _containerRows[y].AddRange(other.ContainerRowAt(y));
            // merge filled tiles
            //_filledRows[y].AddRange(FilledRowAt(y));
            _leftLeakX[y] = _leftLeakX[y] < other.LeftLeakX(y) ? _leftLeakX[y] : other.LeftLeakX(y);
            _rightLeakX[y] = _rightLeakX[y] > other.RightLeakX(y) ? _rightLeakX[y] : other.RightLeakX(y);
        }
        _leftLeakX[_yLength] = _leftLeakX[_yLength] < other.LeftLeakX(_yLength) ? _leftLeakX[_yLength] : other.LeftLeakX(_yLength);
        _rightLeakX[_yLength] = _rightLeakX[_yLength] > other.RightLeakX(_yLength) ? _rightLeakX[_yLength] : other.RightLeakX(_yLength);
        // merge stored water
        //_storedWater += other.StoredWater;

        _topRows.AddRange(other.TopRows);

        return this;
    }

    public void OverwriteCellContainerId(StructureData structure, int containerId)
    {
        foreach (var interval in AllIntervals())
        {
            structure.Row(interval.y).SetContainerId(interval.x, interval.z, containerId);
        }
    }

    internal void FixLeak(int x, int y)
    {
        if (_leftLeakX[y] == x)
        {
            _leftLeakX[y] = _xLength;
        }
        if (_rightLeakX[y] ==  x)
        {
            _rightLeakX[y] = -1;
        }
    }

    public ContainerData(int xLength, int yLength, int containerId, StructureData structure)
    {
        _containerId = containerId;
        _xLength = xLength;
        _yLength = yLength;
        _structure = structure;
        _containerRows = new List<List<ContainerRowData>>();
        //_filledRows = new List<List<Vector2Int>>();
        _topRows = new List<Vector3Int>();
        for (int y = 0; y < yLength; y++)
        {
            _containerRows.Add(new List<ContainerRowData>());
            //_filledRows.Add(new List<Vector2Int>());
        }
        // xLength for no leak
        _leftLeakX = Enumerable.Repeat(xLength, yLength+1).ToList();
        // -1 for no leak
        _rightLeakX = Enumerable.Repeat(-1, yLength+1).ToList();
    }

    ~ContainerData()
    {
        _containerRows.Clear();
        //_filledRows.Clear();
        _topRows.Clear();
        Debug.Log($"clear {_containerId}");
    }
}
