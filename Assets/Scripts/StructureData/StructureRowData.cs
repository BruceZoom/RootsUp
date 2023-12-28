using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

public class StructureRowData
{
    private int _xLength;

    // 0: no block, 1: has block
    private List<int> _hasBlock;

    // 0: not containable, 1: containable
    private List<int> _containable;

    // the idex of closest block to the left/right
    // -1/xLength stands for no block
    private List<int> _leftBlockIdx;
    private List<int> _rightBlockIdx;

    internal bool HasBlock(int x) => _hasBlock[x] == 1;

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

    internal void SetBlock(int targetX)
    {
        _hasBlock[targetX] = 1;
        // treat blocks as containable only for computation
        _containable[targetX] = 1;
        // assumes blocks placed will not be removed
        for (int i = _leftBlockIdx[targetX]+1; i < targetX; i++)
        {
            _rightBlockIdx[i] = targetX;
        }
        for(int i = _rightBlockIdx[targetX]-1; i > targetX; i--)
        {
            _leftBlockIdx[i] = targetX;
        }
    }

    /// <summary>
    /// Test if can update containability at a given cell.
    /// If can update, update every cell reachable from it.
    /// </summary>
    /// <param name="leftX">The left most cell changed containability.</param>
    /// <param name="rightX">The right most cell changed containability.</param>
    /// <returns>Whether need to update next row.</returns>
    internal bool TryUpdateContainableFill(int targetX, StructureRowData lastRow, out int leftX, out int rightX)
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
            leftX = _leftBlockIdx[targetX] + 1;
            rightX = _rightBlockIdx[targetX] - 1;
            _containable.SetRangeValues(leftX, rightX, 1);
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
    internal bool TryUpdateContainableRange(int beginX, int endX, StructureRowData lastRow, out int leftX, out int rightX)
    {
        beginX = beginX > 0 ? beginX : 0;
        endX = endX < _xLength ? endX : _xLength - 1;

        // default to -1
        leftX = rightX = -1;
        int curX;
        int lx, rx;

        while(GetNextEmptySlice(beginX, endX, out lx, out rx))
        {
            // set curx to left-most empty cell
            curX = lx;
            // start one cell to the right of right-most empty cell next time
            beginX = rx + 1;
            // if this slice become continable
            if(TryUpdateContainableFill(curX, lastRow, out lx, out rx))
            {
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
        return leftX == -1;
    }

    /// <summary>
    /// Get next empty slice no ealier than beginX and no later than endX.
    /// </summary>
    /// <param name="leftX">The left most empty cell.</param>
    /// <param name="rightX">The right most empty cell.</param>
    /// <returns>Whether there are empty cells left.</returns>
    internal bool GetNextEmptySlice(int beginX, int endX, out int leftX, out int rightX)
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
            leftX = curX;
            rightX = _rightBlockIdx[curX] - 1;
            return true;
        }
    }

    public StructureRowData(int xLength)
    {
        _xLength = xLength;
        _hasBlock = Enumerable.Repeat(0, xLength).ToList();
        _containable = Enumerable.Repeat(0, xLength).ToList();
        _leftBlockIdx = Enumerable.Repeat(-1, xLength).ToList();
        _rightBlockIdx = Enumerable.Repeat(xLength, xLength).ToList();
    }
}