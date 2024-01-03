using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoSingleton<GameplayManager>
{
    [SerializeField]
    private StructureTileManager _overworldTileManager;
    [SerializeField]
    private SimulationManager _simulationManager;
    [SerializeField]
    private GameplayUIManager _gameplayUIManager;

    [SerializeField]
    private EditController _editController;

    [SerializeField]
    private VillagerObjectPool _villagerObjectPool;

    public override void Initialize()
    {
        _gameplayUIManager.Initialize();
        _overworldTileManager.Initialize();
        _simulationManager.Initialize();

        _editController.Initialize();
        _villagerObjectPool.Initialize();
    }
}
