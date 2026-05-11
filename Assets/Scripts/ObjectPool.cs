using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly T _prefab;
    private readonly Transform _container;
    private readonly Stack<T> _inactive = new();

    public ObjectPool(T prefab, Transform container, int preWarm = 0)
    {
        _prefab = prefab;
        _container = container;

        for (int i = 0; i < preWarm; i++)
            Return(Object.Instantiate(_prefab, _container));
    }

    public T Get(Transform parent = null)
    {
        T obj = _inactive.Count > 0 ? _inactive.Pop() : Object.Instantiate(_prefab, _container);

        if (parent != null)
            obj.transform.SetParent(parent, worldPositionStays: false);

        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(_container);
        _inactive.Push(obj);
    }
}
