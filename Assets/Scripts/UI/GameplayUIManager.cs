using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayUIManager : PassiveSingleton<GameplayUIManager>
{
    [SerializeField]
    private DepositUI _depositUI;

    public DepositUI DepositUI => _depositUI;

    public override void Initialize()
    {
        base.Initialize();
    }
}
