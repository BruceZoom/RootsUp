using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class StructureData
{
    private int _yLength;

    private List<StructureRowData> _rows;

    public bool HasBlock(int x, int y) => _rows[y].HasBlock(x);

    public int TotalCapacity => _rows.Sum(row => row.Capacity);
    public int TotalContainable => _rows.Sum(row => row.TotalContainable);
    public int TotalHasBlock => _rows.Sum(row => row.TotalHasBlock);

    public StructureData(int xLength, int yLength)
    {
        _yLength = yLength;

        _rows = new List<StructureRowData>();
        for(int y = 0; y < yLength; y++)
        {
            _rows.Add(new StructureRowData(xLength));
        }
    }

    public void SetBlock(int targetX, int targetY)
    {
        // set block
        _rows[targetY].SetBlock(targetX);

        // by default, only this block changes containability
        int leftX = targetX - 1;
        int rightX = targetX + 1;
        int lx, rx;

        // no need to check cells in the same row if it is the lowest row
        // otherwise update containability normally
        if (targetY != 0)
        {
            // try to fill cells to left
            if(_rows[targetY].TryUpdateContainableFill(targetX - 1, _rows[targetY - 1], out lx, out rx))
            {
                // extend one cell to left because water can flow diagonally
                // lx will not be 0
                leftX = lx - 1;
            }

            // try to fill cells to right
            if(_rows[targetY].TryUpdateContainableFill(targetX + 1, _rows[targetY - 1], out lx, out rx))
            {
                // extend one cell to right because water can flow diagonally
                // rx will not be xLength
                rightX = rx + 1;
            }
        }

        int y = targetY + 1;
        while(y < _yLength && _rows[y].TryUpdateContainableRange(leftX, rightX, _rows[y-1], out lx, out rx))
        {
            // extend one cell to left/right because water can flow diagonally
            leftX = lx - 1;
            rightX = rx + 1;
            // go to next row
            y += 1;
        }
    }
}