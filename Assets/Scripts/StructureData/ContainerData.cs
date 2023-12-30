using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContainerData
{
    public static int CellPosToContainerID(int x, int y) => 1000 * x + y;
    public static Vector2Int ContainerIDToCellPos(int id) => new Vector2Int(id / 1000, id % 1000);

    // 1th-d: row index
    // 2nd-d: targetX: left-most empty cell, targetY: right-most empty cell
    private List<List<Vector2Int>> _containerRows;

    private int _containerId;
    private int _xLength;
    private int _yLength;

    // 1th-d: row index (_yLength+1 row), data: left/right-most leak point
    private List<int> _leftLeakX;
    private List<int> _rightLeakX;

    public int ContainerID => _containerId;

    public bool ContainsCell(int x, int y) => _containerRows[y].Any(i => i.x <= x && x <= i.y);

    public Vector2Int FindIntervalCover(int x, int y) => _containerRows[y].Find(i => i.x <= x && x <= i.y);
    public List<Vector2Int> FindIntervalOverlap(int lx, int rx, int y) => _containerRows[y].FindAll(i =>
        i.x <= lx && lx <= i.y || i.x <= rx && rx <= i.y).ToList();

    public List<Vector2Int> ContainerRowAt(int y) => _containerRows[y];
    public int LeftLeakX(int y) => _leftLeakX[y];
    public int RightLeakX(int y) => _rightLeakX[y];

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

    public void AddInterval(int y, int lx, int rx, StructureRowData nextRow, bool byPass=false)
    {
        if (lx > rx) return;

        _containerRows[y].Add(new Vector2Int(lx, rx));

        if (byPass) return;

        // may introduce new leak on left
        if (nextRow == null || !nextRow.Containable(lx-1))
        {
            _leftLeakX[y + 1] = _leftLeakX[y] < lx - 1 ? _leftLeakX[y] : lx - 1;
        }
        // may introduce new leak on right
        if (nextRow == null || !nextRow.Containable(rx + 1))
        {
            _rightLeakX[y + 1] = _rightLeakX[y] < rx + 1 ? _rightLeakX[y] : rx + 1;
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

    public List<ContainerData> SplitContainerAt(int targetX, int targetY, List<StructureRowData> rows)
    {
        Vector2Int targetInterval = FindIntervalCover(targetX, targetY);
        if (targetInterval == null)
        {
            Debug.LogWarning("Cannot split at a cell which is not a container.");
            return null;
        }

        _containerRows[targetY].Remove(targetInterval);
        // it is already a container, filling it will not create new leak
        // therefore by pass check
        AddInterval(targetY, targetInterval.x, targetX - 1, null, true);
        AddInterval(targetY, targetX + 1, targetInterval.y, null, true);

        List<ContainerData> newContainers = new List<ContainerData>();
        for (int y = 0; y < _yLength; y++)
        {
            while (_containerRows[y].Count > 0)
            {
                Vector2Int botSlice = _containerRows[y].Pop(0);
                ContainerData newContainer =
                        new ContainerData(_xLength, _yLength, CellPosToContainerID(botSlice.x, y));
                newContainer.AddInterval(y, botSlice.x, botSlice.y, y < _yLength ? rows[y] : null);
                newContainers.Add(newContainer);
                // use BFS to generate the new container
                List<Vector3Int> bfsQueue = new List<Vector3Int>();
                bfsQueue.Add(new Vector3Int(botSlice.x, y, botSlice.y));
                while(bfsQueue.Count > 0)
                {
                    var interval = bfsQueue.Pop(0);
                    var slices = FindIntervalOverlap(interval.x, interval.z, interval.y);
                    foreach (var slice in slices)
                    {
                        _containerRows[interval.y].Remove(slice);
                        newContainer.AddInterval(interval.y, slice.x, slice.y,
                                                interval.y < _yLength ? rows[interval.y] : null);
                        bfsQueue.Add(new Vector3Int(slice.x, interval.y, slice.y));
                    }
                }
            }
        }

        return newContainers;
    }

    public ContainerData MergeContainer(ContainerData other)
    {
        if (other.ContainerID == this.ContainerID)
        {
            Debug.LogWarning($"Cannot merge the same container with itself: {ContainerID}");
            return this;
        }

        for (int y = 0; y <= _containerRows.Count; y++)
        {
            _containerRows[y].AddRange(other.ContainerRowAt(y));
            _leftLeakX[y] = _leftLeakX[y] < other.LeftLeakX(y) ? _leftLeakX[y] : other.LeftLeakX(y);
            _rightLeakX[y] = _rightLeakX[y] > other.RightLeakX(y) ? _rightLeakX[y] : other.RightLeakX(y);
        }
        _leftLeakX[_yLength] = _leftLeakX[_yLength] < other.LeftLeakX(_yLength) ? _leftLeakX[_yLength] : other.LeftLeakX(_yLength);
        _rightLeakX[_yLength] = _rightLeakX[_yLength] > other.RightLeakX(_yLength) ? _rightLeakX[_yLength] : other.RightLeakX(_yLength);

        return this;
    }

    public void OverwriteCellContainerId(StructureData structure, int containerId)
    {
        foreach (var interval in AllIntervals())
        {
            structure.Row(interval.y).SetContainerId(interval.x, interval.z, containerId);
        }
    }

    public ContainerData(int xLength, int yLength, int containerId)
    {
        _containerId = containerId;
        _xLength = xLength;
        _yLength = yLength;
        _containerRows = new List<List<Vector2Int>>();
        for (int y = 0; y < yLength; y++)
        {
            _containerRows.Add(new List<Vector2Int>());
        }
        // xLength for no leak
        _leftLeakX = Enumerable.Repeat(xLength, yLength+1).ToList();
        // -1 for no leak
        _rightLeakX = Enumerable.Repeat(-1, yLength+1).ToList();
    }

    ~ContainerData()
    {
        _containerRows.Clear();
    }
}