using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class TileSpaceDictionary : SerializableDictionary<Vector2Int, TileSpace> {}
[Serializable] public class UIRectFloatDictionary : SerializableDictionary<GameObject, float> {}
[Serializable] public class UIRectBoolDictionary : SerializableDictionary<GameObject, bool> {}


[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    private readonly List<TKey> keys = new List<TKey>();
    
    [SerializeField]
    private readonly List<TValue> values = new List<TValue>();
    
    // save the dictionary to lists
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach(KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }
    
    // load dictionary from lists
    public void OnAfterDeserialize()
    {
        this.Clear();

        if(keys.Count != values.Count)
            throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable.", keys.Count, values.Count));

        for(int i = 0; i < keys.Count; i++)
            this.Add(keys[i], values[i]);
    }
}



