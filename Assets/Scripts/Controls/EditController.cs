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

    private StructureData _structreData;

    [SerializeField]
    private List<Vector3Int> _initialBlocks;

    private int _capacityOverEstimate = 0;

    [Header("Test")]
    [SerializeField]
    private float _amountToTest;

    [Button("Test Add Water")]
    private void TestAddWater()
    {
        float remain = _structreData.AddWater(_amountToTest);
        Debug.Log($"Storage: {_structreData.StoredWater}, Remains: {remain}");
    }

    [Button("Test Get Water")]
    private void TestGetWater()
    {
        bool success = _structreData.TryGetWater(_amountToTest);
        Debug.Log($"Storage: {_structreData.StoredWater}. " + (success ? "Succeeded." : "Failed."));
    }

    public void Initialize()
    {
        if (_actions == null)
        {
            _actions = new EditInputActions();
        }

        Debug.Log(StructureTileManager.Instance.WorldBoundary.max);
        _structreData = new StructureData((int)StructureTileManager.Instance.WorldBoundary.max.x, (int)StructureTileManager.Instance.WorldBoundary.max.y);
    }

    private void Start()
    {
        foreach (var cellPos in _initialBlocks)
        {
            AddBlockAtCell(cellPos);
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
            && _structreData.CanGrow(_currentCellPos.x, _currentCellPos.y))
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
        _currentCellPos = cellPos;

        // only update when mouse pressed and at valid position
        if (_mouseStatus == MouseStatus.Pressing
            && StructureTileManager.Instance.IsValidPosition(_currentPos)
            && _structreData.CanGrow(_currentCellPos.x, _currentCellPos.y))
        {
            AddBlockAtCell(cellPos);
        }
    }

    private void HandleRightClick(InputAction.CallbackContext context)
    {
        if (StructureTileManager.Instance.IsValidPosition(_currentPos))
        {
            Debug.Log(_structreData.DebugInfo(_currentCellPos.x, _currentCellPos.y));
        }
    }

    /// <summary>
    /// Add a new tile to given cell position.
    /// Regardless of whether it is valid. Need to be checked by callers.
    /// </summary>
    private void AddBlockAtCell(Vector3Int cellPos)
    {
        //if (_tilemap.HasTile(cellPos))
        if (_structreData.HasBlock(cellPos.x, cellPos.y))
        {
            //Debug.Log($"Already have a tile at {cellPos}");
            return;
        }

        //Debug.Log(cellPos);

        //_tilemap.SetTile(cellPos, _tileToAdd);
        _structreData.SetBlock(cellPos.x, cellPos.y);

        // test code
        var cap = _structreData.CapacityOverEstimate;
        if (_capacityOverEstimate != cap)
        {
            Debug.Log($"Capacity overestimate changed.\nCapacity: {cap}, Block: {_structreData.TotalHasBlock}, Containable: {_structreData.TotalContainable}");
        }

        Debug.Log($"True capacity: {_structreData.Capacity}");
    }
}
