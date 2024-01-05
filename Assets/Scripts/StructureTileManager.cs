using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class StructureTileManager : PassiveSingleton<StructureTileManager>
{
    [Header("Tile Settings")]
    [SerializeField]
    private Tilemap _structureTilemap;

    [SerializeField]
    private RuleTile _structureTile;

    [SerializeField]
    private float _structureTileAnimTime = 0.2f;

    [SerializeField]
    private Tilemap _waterTilemap;

    [SerializeField]
    private AnimatedTile _waterSurfaceTile;

    [SerializeField]
    private Tile _waterBodyTile;

    [SerializeField]
    private int _waterSurfaceAnimFrames;

    [SerializeField]
    private BoxCollider2D _worldBoundaryCollider;

    public Bounds WorldBoundary => _worldBoundaryCollider.bounds;

    public bool IsValidPosition(Vector2 pos) => WorldBoundary.Contains(pos)
                                    && IsInScreen(Camera.main.WorldToViewportPoint(pos));
    public bool IsInScreen(Vector2 view) => view.x >= 0 && view.x <= 1 && view.y >= 0 && view.y <= 1;

    public Vector3Int WorldToCell(Vector2 pos) => _structureTilemap.WorldToCell(pos);
    public Vector3 CellToWorld(Vector3Int pos) => _structureTilemap.GetCellCenterWorld(pos);

    public Vector3 GetCellCostPos(Vector3Int cellPos) =>
        Camera.main.WorldToScreenPoint(_structureTilemap.GetCellCenterWorld(cellPos) + Vector3.up * 0.5f);

    public override void Initialize()
    {
        base.Initialize();

        // need to handle synchronization
        // design choice: always turn on collider but in collision free layer
        //_worldBoundaryCollider.enabled = false;

        //_editController.Initialize();
    }

    public void SetBlock(Vector3Int cellPos)
    {
        _structureTilemap.SetTile(cellPos, _structureTile);
        // animation
        var tileScale = 0f;
        var tileTransform = _structureTilemap.GetTransformMatrix(cellPos);
        _structureTilemap.SetTileFlags(cellPos, TileFlags.None);
        DOTween.To(() => tileScale,
                   v => {
                       _structureTilemap.SetTransformMatrix(cellPos, tileTransform * Matrix4x4.Scale(Vector3.one * v));
                       tileScale = v;
                   }, 1f, _structureTileAnimTime)
                .From(0f)
                .OnComplete(() => {
                    _structureTilemap.RefreshTile(cellPos);
                });
    }

    public void SetWaterBody(Vector3Int interval)
    {
        for(int x = interval.x; x <= interval.z; x++)
        {
            _waterTilemap.SetTile(new Vector3Int(x, interval.y, 0), _waterBodyTile);
        }
    }

    public void ClearWaterTile(Vector3Int interval)
    {
        for (int x = interval.x; x <= interval.z; x++)
        {
            _waterTilemap.SetTile(new Vector3Int(x, interval.y, 0), null);
        }
    }

    public void SetWaterSurface(Vector3Int interval)
    {
        for (int x = interval.x; x <= interval.z; x++)
        {
            _waterTilemap.SetTile(new Vector3Int(x, interval.y, 0), _waterSurfaceTile);
            _waterTilemap.SetAnimationFrame(new Vector3Int(x, interval.y, 0), x % _waterSurfaceAnimFrames);
        }
    }
}
