using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableStringHashSet : ISerializationCallbackReceiver
{
    [SerializeField]
    private List<string> items = new List<string>();

    [NonSerialized]
    private HashSet<string> hashSet;

    public void OnBeforeSerialize()
    {
        // シリアライズ前に List を HashSet から更新
        if (hashSet != null)
            items = new List<string>(hashSet);
    }

    public void OnAfterDeserialize()
    {
        // デシリアライズ後に HashSet を復元
        if (items == null) items = new List<string>();
        hashSet = new HashSet<string>(items);
    }

    private HashSet<string> EnsureHashSet()
    {
        if (hashSet == null) hashSet = new HashSet<string>(items ?? new List<string>());
        return hashSet;
    }

    public bool Add(string value)
    {
        var set = EnsureHashSet();
        if (set.Add(value))
        {
            items.Add(value);
            return true;
        }
        return false;
    }

    public bool Remove(string value)
    {
        var set = EnsureHashSet();
        if (set.Remove(value))
        {
            items.Remove(value);
            return true;
        }
        return false;
    }

    public bool Contains(string value) => EnsureHashSet().Contains(value);

    public void Clear()
    {
        EnsureHashSet().Clear();
        items.Clear();
    }

    public HashSet<string> GetHashSet() => EnsureHashSet();

    public List<string> ToList() => new List<string>(items);
}