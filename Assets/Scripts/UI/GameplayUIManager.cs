using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayUIManager : PassiveSingleton<GameplayUIManager>
{
    [SerializeField]
    private DepositUI _depositUI;

    [SerializeField]
    private CostUI _costUI;

    public DepositUI DepositUI => _depositUI;
    public CostUI CostUI => _costUI;
    public override void Initialize()
    {
        base.Initialize();
    }
}
