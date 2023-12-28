using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using static EditController;

public class EditController : MonoBehaviour
{
    private EditInputActions _actions;

    [SerializeField]
    private Tilemap _tilemap;

    [SerializeField]
    private Tile _tileToAdd;

    private Vector2 _currentPos;
    private Vector3Int _currentCellPos;

    public enum MouseStatus { Idle, Pressing };
    private MouseStatus _mouseStatus = MouseStatus.Idle;

    private void Awake()
    {
        _actions = new EditInputActions();
    }

    private void OnEnable()
    {
        _actions.Edit.Enable();

        _actions.Edit.MousePosition.performed += HandleMousePosition;
        _actions.Edit.MouseClick.performed += HandleMouseClick;
        _actions.Edit.MouseClick.canceled += HandleMouseRelease;
    }

    private void OnDisable()
    {
        _actions.Edit.MousePosition.performed -= HandleMousePosition;
        _actions.Edit.MouseClick.performed -= HandleMouseClick;
        _actions.Edit.MouseClick.canceled -= HandleMouseRelease;

        _actions.Edit.Disable();
    }

    private void HandleMousePosition(InputAction.CallbackContext context)
    {
        UpdateCurrentWorldPosition(Camera.main.ScreenToWorldPoint(context.ReadValue<Vector2>()));
    }

    private void HandleMouseClick(InputAction.CallbackContext context)
    {
        _mouseStatus = MouseStatus.Pressing;

        UpdateTileAtCell(_currentCellPos);
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

        if (_mouseStatus == MouseStatus.Pressing)
        {
            UpdateTileAtCell(cellPos);
        }
    }

    private void UpdateTileAtCell(Vector3Int cellPos)
    {
        if (_tilemap.HasTile(cellPos))
        {
            Debug.Log($"Already have a tile at {cellPos}");
            return;
        }

        _tilemap.SetTile(cellPos, _tileToAdd);
    }
}
