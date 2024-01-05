using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using VInspector;

public class EditController : MonoBehaviour
{
    private EditInputActions _actions;

    private Vector2 _currentPos;
    private Vector3Int _currentCellPos;

    public enum MouseStatus { Idle, Pressing };
    private MouseStatus _mouseStatus = MouseStatus.Idle;

    private StructureData _structure;

    [SerializeField]
    private List<Vector3Int> _initialBlocks;

    [SerializeField]
    private Tilemap _highlightTileMap;

    [SerializeField]
    private Tile _highlightTile;

    [Header("Test")]
    [SerializeField]
    private float _amountToTest;

    [Button("Test Add Water")]
    private void TestAddWater()
    {
        float remain = _structure.AddWater(_amountToTest);
        Debug.Log($"Storage: {_structure.StoredWater}, Remains: {remain}");
    }

    [Button("Test Get Water")]
    private void TestGetWater()
    {
        bool success = _structure.TryGetWater(_amountToTest);
        Debug.Log($"Storage: {_structure.StoredWater}. " + (success ? "Succeeded." : "Failed."));
    }

    public void Initialize()
    {
        if (_actions == null)
        {
            _actions = new EditInputActions();
        }

        _structure = SimulationManager.Instance.Structure;
    }

    private void Start()
    {
        foreach (var cellPos in _initialBlocks)
        {
            AddBlockAtCell(cellPos, byPass:true);
        }
    }

    private void OnEnable()
    {
        if (_actions == null)
        {
            _actions = new EditInputActions();
        }

        _actions.Edit.Enable();

        _actions.Edit.MousePosition.performed += HandleMousePosition;
        _actions.Edit.MouseClick.performed += HandleMouseClick;
        _actions.Edit.MouseClick.canceled += HandleMouseRelease;

        _actions.Edit.RightClick.performed += HandleRightClick;
    }

    private void OnDisable()
    {
        _actions.Edit.MousePosition.performed -= HandleMousePosition;
        _actions.Edit.MouseClick.performed -= HandleMouseClick;
        _actions.Edit.MouseClick.canceled -= HandleMouseRelease;

        _actions.Edit.RightClick.performed -= HandleRightClick;

        _actions.Edit.Disable();
    }

    private void HandleMousePosition(InputAction.CallbackContext context)
    {
        UpdateCurrentWorldPosition(Camera.main.ScreenToWorldPoint(context.ReadValue<Vector2>()));
    }

    private void HandleMouseClick(InputAction.CallbackContext context)
    {
        _mouseStatus = MouseStatus.Pressing;

        // only update when click a valid position
        if (StructureTileManager.Instance.IsValidPosition(_currentPos) 
            && _structure.CanGrow(_currentCellPos.x, _currentCellPos.y))
        {
            AddBlockAtCell(_currentCellPos);
        }
    }

    private void HandleMouseRelease(InputAction.CallbackContext context)
    {
        _mouseStatus = MouseStatus.Idle;
    }

    /// <summary>
    /// Update cursor's current world position.
    /// Invoke EnterNewCell when the cursor enters a new cell.
    /// </summary>
    private void UpdateCurrentWorldPosition(Vector2 newPos)
    {
        _currentPos = newPos;

        var newCellPos = StructureTileManager.Instance.WorldToCell(_currentPos);
        if (newCellPos != _currentCellPos)
        {
            EnterNewCell(newCellPos);
        }
    }

    /// <summary>
    /// Invoked when entering new cell.
    /// Updates the cell if LMB down.
    /// </summary>
    private void EnterNewCell(Vector3Int cellPos)
    {
        _highlightTileMap.SetTile(_currentCellPos, null);
        _currentCellPos = cellPos;

        // highlight current tile if valid
        if (StructureTileManager.Instance.IsValidPosition(_currentPos))
        {
            _highlightTileMap.SetTile(_currentCellPos, _highlightTile);
        }

        // only update when mouse pressed and at valid position
        if (_mouseStatus == MouseStatus.Pressing
            && StructureTileManager.Instance.IsValidPosition(_currentPos)
            && _structure.CanGrow(_currentCellPos.x, _currentCellPos.y))
        {
            AddBlockAtCell(cellPos);
        }
        
        // display cost for valid empty cell
        if (StructureTileManager.Instance.IsValidPosition(_currentPos)
            && !_structure.HasBlock(cellPos.x, cellPos.y))
        {
            float waterCost, mineralCost;
            SimulationManager.Instance.GetResourceCostAt(cellPos.x, cellPos.y, out waterCost, out mineralCost);
            Vector3 costPanelPos = StructureTileManager.Instance.GetCellCostPos(cellPos);
            GameplayUIManager.Instance.CostUI.DisplayCost(costPanelPos, waterCost, mineralCost);
        }
        // otherwise, hide cost panel
        else
        {
            GameplayUIManager.Instance.CostUI.HideCost();
        }
    }

    private void HandleRightClick(InputAction.CallbackContext context)
    {
        if (StructureTileManager.Instance.IsValidPosition(_currentPos))
        {
            Debug.Log(_structure.DebugInfo(_currentCellPos.x, _currentCellPos.y));

            float waterCost, mineralCost;
            SimulationManager.Instance.GetResourceCostAt(_currentCellPos.x, _currentCellPos.y, out waterCost, out mineralCost);
            //Debug.Log($"Water Cost: {waterCost}, Mineral Cost: {mineralCost}");
        }
    }

    /// <summary>
    /// Add a new tile to given cell position.
    /// Regardless of whether it is valid. Need to be checked by callers.
    /// </summary>
    private void AddBlockAtCell(Vector3Int cellPos, bool byPass=false)
    {
        //if (_tilemap.HasTile(cellPos))
        if (_structure.HasBlock(cellPos.x, cellPos.y)
            || (!byPass && !SimulationManager.Instance.CanConsumeResourceAt(cellPos.x, cellPos.y)))
        {
            //Debug.Log($"Already have a tile at {cellPos}");
            return;
        }

        //Debug.Log(cellPos);

        SimulationManager.Instance.ConsumeResourceAt(cellPos.x, cellPos.y);

        //_tilemap.SetTile(cellPos, _tileToAdd);
        _structure.SetBlock(cellPos.x, cellPos.y);

        var pos = StructureTileManager.Instance.CellToWorld(cellPos);
        SimulationManager.Instance.TreeBound = new Vector3(pos.x, pos.y, pos.x);

        GameplayUIManager.Instance.DepositUI.SetWaterDeposit(_structure.StoredWater, _structure.Capacity);
        GameplayUIManager.Instance.CostUI.HideCost();

        /*
        // test code
        var cap = _structure.CapacityOverEstimate;
        if (_capacityOverEstimate != cap)
        {
            Debug.Log($"Capacity overestimate changed.\nCapacity: {cap}, Block: {_structure.TotalHasBlock}, Containable: {_structure.TotalContainable}");
        }

        Debug.Log($"True capacity: {_structure.Capacity}");
        */
    }
}
