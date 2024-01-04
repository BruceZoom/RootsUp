using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudObjectPool : PassiveSingleton<CloudObjectPool>
{
    [SerializeField]
    private GameObject _cloudPrefab;

    public override void Initialize()
    {
        base.Initialize();
    }
}
