using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using VInspector;

public class EditController : MonoBehaviour
{
    private EditInputActions _actions;

    [Header("Tile Settings")]
    [SerializeField]
    private Tilemap _tilemap;

    [SerializeField]
    private RuleTile _tileToAdd;

    [SerializeField]
    private BoxCollider2D _worldBoundaryCollider;

    [SerializeField]
    private List<Vector3Int> _initialBlocks;

    private Bounds _worldBoundary;

    private Vector2 _currentPos;
    private Vector3Int _currentCellPos;

    public enum MouseStatus { Idle, Pressing };
    private MouseStatus _mouseStatus = MouseStatus.Idle;

    private StructureData _structreData;

    private bool IsValidPosition => _worldBoundary.Contains(_currentPos)
                                    && IsInScreen(Camera.main.WorldToViewportPoint(_currentPos));

    private bool IsInScreen(Vector2 view) => view.x >= 0 && view.x <= 1 && view.y >= 0 && view.y <= 1;

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


    private void Awake()
    {
        _actions = new EditInputActions();

        _worldBoundary = _worldBoundaryCollider.bounds;
        _worldBoundaryCollider.enabled = false;
        _structreData = new StructureData((int)_worldBoundary.max.x, (int)_worldBoundary.max.y);
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
        if (IsValidPosition && _structreData.CanGrow(_currentCellPos.x, _currentCellPos.y))
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

        var newCellPos = _tilemap.WorldToCell(_currentPos);
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
        if (_mouseStatus == MouseStatus.Pressing && IsValidPosition 
            && _structreData.CanGrow(_currentCellPos.x, _currentCellPos.y))
        {
            AddBlockAtCell(cellPos);
        }
    }

    private void HandleRightClick(InputAction.CallbackContext context)
    {
        if (IsValidPosition)
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

        _tilemap.SetTile(cellPos, _tileToAdd);
        _structreData.SetBlock(cellPos.x, cellPos.y);


        var cap = _structreData.CapacityOverEstimate;
        if (_capacityOverEstimate != cap)
        {
            Debug.Log($"Capacity overestimate changed.\nCapacity: {cap}, Block: {_structreData.TotalHasBlock}, Containable: {_structreData.TotalContainable}");
        }

        Debug.Log($"True capacity: {_structreData.Capacity}");
    }
}
