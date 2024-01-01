using ARCProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StructureTileManager : MonoSingleton<StructureTileManager>
{
    [SerializeField]
    private EditController _editController;

    [Header("Tile Settings")]
    [SerializeField]
    private Tilemap _structureTilemap;

    [SerializeField]
    private RuleTile _structureTile;

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

    public override void Initialize()
    {
        // need to handle synchronization
        // design choice: always turn on collider but in collision free layer
        //_worldBoundaryCollider.enabled = false;

        _editController.Initialize();
    }

    public void SetBlock(Vector3Int cellPos)
    {
        _structureTilemap.SetTile(cellPos, _structureTile);
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
