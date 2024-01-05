using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudObjectPool : ObjectPool<CloudController>
{
    protected override CloudController OnCreate(CloudController newOjb)
    {
        newOjb.Initialize();

        return base.OnCreate(newOjb);
    }
}
