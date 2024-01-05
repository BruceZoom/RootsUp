using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class ObjectPool<T> : MonoSingleton<ObjectPool<T>> where T : MonoBehaviour
{
    private List<T> _objectPool;

    [SerializeField]
    private int _initialSize = 10;

    [SerializeField]
    private GameObject _prefab;

    public override void Initialize()
    {
        _objectPool = new List<T>();
        for (int i = 0; i < _initialSize; i++)
        {
            var obj = NewObject();
            obj.gameObject.SetActive(false);
            _objectPool.Add(obj);
        }
    }

    private T NewObject()
    {
        var go = GameObject.Instantiate(_prefab, transform);

        return OnCreate(go.GetComponent<T>());
    }

    virtual protected T OnCreate(T newOjb)
    {
        return newOjb;
    }

    public T GetObject()
    {
        if (_objectPool.Count > 0)
        {
            var obj = _objectPool.Pop();
            obj.gameObject.SetActive(true);
            return OnGet(obj);
        }
        else
        {
            return NewObject();
        }
    }

    virtual protected T OnGet(T obj)
    {
        return obj;
    }

    public void ReturnObject(T obj)
    {
        obj.gameObject.SetActive(false);
        _objectPool.Add(OnReturn(obj));
    }

    virtual protected T OnReturn(T obj)
    {
        return obj;
    }
}