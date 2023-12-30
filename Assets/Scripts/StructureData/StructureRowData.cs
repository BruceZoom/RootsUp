using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class StructureRowData
{
    private int _xLength;
    private int _yLength;
    // y index of current row
    private int _y;

    private StructureData _structure;

    // 0: no block, 1: has block
    private List<int> _hasBlock;

    // 0: not containable, 1: containable
    private List<int> _containable;

    // the idex of closest block to the left/right
    // -1/xLength stands for no block
    private List<int> _leftBlockIdx;
    private List<int> _rightBlockIdx;

    // -1: no container, otherwise: corresponding container
    private List<int> _containerId;

    internal bool HasBlock(int x) => _hasBlock[x] == 1;
    internal int GetContainerID(int x) => _containerId[x];
    internal bool Containable(int x) => _containable[x] == 1;

    internal int Capacity => _containable.Sum() - _hasBlock.Sum();
    internal int TotalContainable => _containable.Sum();
    internal int TotalHasBlock => _hasBlock.Sum();


    /// <summary>
    /// Total number of containable cell from leftX to rightX.
    /// Both included.
    /// </summary>
    internal int CountContainable(int leftX, int rightX) {
        leftX = leftX >= 0 ? leftX : 0;
        rightX = rightX < _xLength ? rightX : _xLength - 1;
        return _containable.GetRange(leftX, rightX - leftX + 1).Sum();
    }

    internal IEnumerable<int> GetContainers(int leftX, int rightX) => _containerId.GetRange(leftX, rightX - leftX + 1).Distinct();

    internal void SetContainerId(int lx, int rx, int id)
    {
        _containerId.SetRangeValues(lx, rx, id);
    }

    internal void SetContainerId(int x, int id)
    {
        _containerId[x] = id;
    }

    internal void SetBlock(int targetX)
    {
        _hasBlock[targetX] = 1;
        // treat blocks as containable only for computation
        _containable[targetX] = 1;
        // assumes blocks placed will not be removed
        if (_leftBlockIdx[targetX] >= 0)
        {
            _rightBlockIdx[_leftBlockIdx[targetX]] = targetX;
        }
        for (int i = _leftBlockIdx[targetX] + 1; i < targetX; i++)
        {
            _rightBlockIdx[i] = targetX;
        }
        if (_rightBlockIdx[targetX] < _xLength)
        {
            _leftBlockIdx[_rightBlockIdx[targetX]] = targetX;
        }
        for (int i = _rightBlockIdx[targetX] - 1; i > targetX; i--)
        {
            _leftBlockIdx[i] = targetX;
        }
    }

    public string DebugString(int x)
    {
        return $"Has Block: {_hasBlock[x]},\n\t Containable: {_containable[x]}, Container: {_containerId[x]},\n\t Left Block: {_leftBlockIdx[x]}, Right Block: {_rightBlockIdx[x]}";
    }

    /// <summary>
    /// Test if can update containability at a given cell.
    /// If can update, update every cell reachable from it.
    /// </summary>
    /// <param name="leftX">The left most cell changed containability.</param>
    /// <param name="rightX">The right most cell changed containability.</param>
    /// <returns>Whether need to update next row.</returns>
    internal bool TryUpdateContainableFill(int targetX, StructureRowData lastRow, StructureRowData nextRow, Dictionary<int, ContainerData> containerData, out int leftX, out int rightX)
    {
        // no change if already containable
        if (targetX <= 0 || targetX >= _xLength-1 || _containable[targetX] == 1)
        {
            leftX = rightX = -1;
            return false;
        }

        // include cells right below two blocks
        // water flow diagonally if they are empty
        int numContainBelow = lastRow.CountContainable(_leftBlockIdx[targetX], _rightBlockIdx[targetX]);
        //int numEmptyCell = _rightBlockIdx[targetX] - _leftBlockIdx[targetX] - 1;
        int numEmptyCell = _rightBlockIdx[targetX] - _leftBlockIdx[targetX] + 1;
        // if every cell below is containable
        // then this row becomes containable
        if (numContainBelow >= numEmptyCell)
        {
            // exclude walls
            leftX = _leftBlockIdx[targetX] + 1;
            rightX = _rightBlockIdx[targetX] - 1;
            _containable.SetRangeValues(leftX, rightX, 1);

            // find all containers below
            var containers = lastRow.GetContainers(leftX - 1, rightX + 1);
            //Debug.Log($"{leftX - 1},{rightX + 1}: {containers.Count()}");
            var numContainers = containers.Count();
            // if only one element (must be -1), then no container created before
            if (numContainers == 1)
            {
                // create new container
                int containerId = ContainerData.CellPosToContainerID(leftX, _y);
                var container = new ContainerData(_xLength, _yLength, containerId);
                container.AddInterval(_y, leftX, rightX, nextRow);
                containerData.Add(containerId, container);
                // set container id
                _containerId.SetRangeValues(leftX, rightX, containerId);
            }
            // onyl one container
            else if (numContainers == 2)
            {
                // join the container
                foreach (var containerId in containers)
                {
                    if (containerId == -1) continue;

                    containerData[containerId].AddInterval(_y, leftX, rightX, nextRow);
                    //set container id
                    _containerId.SetRangeValues(leftX, rightX, containerId);
                }
            }
            else
            {
                // merge all containers
                int containerId = -1;
                ContainerData container = null;
                foreach (var otherId in containers)
                {
                    if (otherId == -1) continue;
                    if (containerId == -1)
                    {
                        containerId = otherId;
                        container = containerData[containerId];
                        continue;
                    }
                    if (containerId == otherId)
                    {
                        Debug.LogWarning("Cannot merge same container.");
                        continue;
                    }

                    var other = containerData[otherId];
                    container.MergeContainer(other);
                    // remove merged container
                    containerData.Remove(otherId);
                    // set container id
                    other.OverwriteCellContainerId(_structure, containerId);
                }

                // join the new slice to the container
                container.AddInterval(_y, leftX, rightX, nextRow);
                //set container id
                _containerId.SetRangeValues(leftX, rightX, containerId);
            }

            return true;
        }
        // otherwise not containable and there is no change
        else
        {
            leftX = rightX = -1;
            return false;
        }
    }

    /// <summary>
    /// Test if can update containability at some cell in the given range.
    /// If can update, update every cell reachable from it.
    /// </summary>
    /// <param name="leftX">The left most cell changed containability.</param>
    /// <param name="rightX">The right most cell changed containability.</param>
    /// <returns>Whether need to update next row.</returns>
    internal bool TryUpdateContainableRange(int beginX, int endX, StructureRowData lastRow, StructureRowData nextRow, Dictionary<int, ContainerData> containerData, out int leftX, out int rightX)
    {
        beginX = beginX > 0 ? beginX : 0;
        endX = endX < _xLength ? endX : _xLength - 1;

        // default to -1
        leftX = rightX = -1;
        int curX;
        int lx, rx;

        while(GetEmptySlice(beginX, endX, out lx, out rx))
        {
            // set curx to left-most empty cell
            curX = lx;
            // start one cell to the right of right-most empty cell next time
            beginX = rx + 1;
            // if this slice become continable
            if(TryUpdateContainableFill(curX, lastRow, nextRow, containerData, out lx, out rx))
            {
                //Debug.Log($"Filled: [{lx}, {rx}]");
                // update the left-most cell if it is the first slice
                if (leftX == -1)
                {
                    leftX = lx;
                }
                // always extend to right
                rightX = rx;
            }
        }

        // leftX is still -1 if no update
        return leftX != -1;
    }

    /// <summary>
    /// Get next empty slice no ealier than beginX and no later than endX.
    /// </summary>
    /// <param name="leftX">The left most empty cell.</param>
    /// <param name="rightX">The right most empty cell.</param>
    /// <returns>Whether there are empty cells left.</returns>
    internal bool GetEmptySlice(int beginX, int endX, out int leftX, out int rightX)
    {
        int curX = beginX;
        // go to first empty cell
        while (curX <= endX && HasBlock(curX))
        {
            curX += 1;
        }
        // fails if no empty cell
        if (curX > endX)
        {
            leftX = rightX = -1;
            return false;
        }
        else
        {
            leftX = _leftBlockIdx[curX] + 1;
            rightX = _rightBlockIdx[curX] - 1;
            //Debug.Log($"An empty slice: [{leftX}, {rightX}]");
            return true;
        }
    }

    public StructureRowData(int xLength, int yLength, int y, StructureData structure)
    {
        _xLength = xLength;
        _yLength = yLength;
        _y = y;
        _structure = structure;
        _hasBlock = Enumerable.Repeat(0, xLength).ToList();
        _containable = Enumerable.Repeat(0, xLength).ToList();
        _leftBlockIdx = Enumerable.Repeat(-1, xLength).ToList();
        _rightBlockIdx = Enumerable.Repeat(xLength, xLength).ToList();
        _containerId = Enumerable.Repeat(-1, xLength).ToList();
    }
}