using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public abstract class SerializableDictionaryBase
{
    public abstract class Storage
    {
    }

    protected class Dictionary<TKey, TValue> : System.Collections.Generic.Dictionary<TKey, TValue>
    {
        public Dictionary()
        {
        }

        public Dictionary(IDictionary<TKey, TValue> dict) : base(dict)
        {
        }

        public Dictionary(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public ref struct SerializableDictionaryEnumerator<TKey, TValue>
    {
        private System.Collections.Generic.Dictionary<TKey, TValue>.Enumerator _enumerator;

        public SerializableDictionaryEnumerator(
            System.Collections.Generic.Dictionary<TKey, TValue> targetDictionary)
        {
            _enumerator = targetDictionary.GetEnumerator();
        }

        public KeyValuePair<TKey, TValue> Current => _enumerator.Current;

        public bool MoveNext() => _enumerator.MoveNext();
    }
}

[Serializable]
public abstract class SerializableDictionaryBase<TKey, TValue, TValueStorage> : SerializableDictionaryBase,
    IDictionary<TKey, TValue>, IDictionary, ISerializationCallbackReceiver, IDeserializationCallback, ISerializable
{
    Dictionary<TKey, TValue> m_dict;
    [SerializeField] TKey[] m_keys;
    [SerializeField] TValueStorage[] m_values;

    public SerializableDictionaryBase()
    {
        m_dict = new Dictionary<TKey, TValue>();
    }

    public SerializableDictionaryBase(IDictionary<TKey, TValue> dict)
    {
        m_dict = new Dictionary<TKey, TValue>(dict);
    }

    protected abstract void SetValue(TValueStorage[] storage, int i, TValue value);
    protected abstract TValue GetValue(TValueStorage[] storage, int i);

    public void CopyFrom(IDictionary<TKey, TValue> dict)
    {
        m_dict.Clear();
        foreach (var kvp in dict)
        {
            m_dict[kvp.Key] = kvp.Value;
        }
    }


    public void OnAfterDeserialize()
    {
        if (m_keys != null && m_values != null && m_keys.Length == m_values.Length)
        {
            m_dict.Clear();
            int n = m_keys.Length;
            for (int i = 0; i < n; ++i)
            {
                if (m_keys[i] == null)
                    continue;
                m_dict[m_keys[i]] = GetValue(m_values, i);
            }

            m_keys = null;
            m_values = null;
        }
    }

    public void OnBeforeSerialize()
    {
        int n = m_dict.Count;
        m_keys = new TKey[n];
        m_values = new TValueStorage[n];

        int i = 0;
        foreach (var kvp in m_dict)
        {
            m_keys[i] = kvp.Key;
            SetValue(m_values, i, kvp.Value);
            ++i;
        }
    }

    public SerializableDictionaryEnumerator<TKey, TValue> GetEnumerator() => new(m_dict);

    #region IDictionary<TKey, TValue>

    public ICollection<TKey> Keys
    {
        get { return ((IDictionary<TKey, TValue>)m_dict).Keys; }
    }

    public ICollection<TValue> Values
    {
        get { return ((IDictionary<TKey, TValue>)m_dict).Values; }
    }

    public int Count
    {
        get { return ((IDictionary<TKey, TValue>)m_dict).Count; }
    }

    public bool IsReadOnly
    {
        get { return ((IDictionary<TKey, TValue>)m_dict).IsReadOnly; }
    }

    public TValue this[TKey key]
    {
        get { return ((IDictionary<TKey, TValue>)m_dict)[key]; }
        set { ((IDictionary<TKey, TValue>)m_dict)[key] = value; }
    }

    public void Add(TKey key, TValue value)
    {
        ((IDictionary<TKey, TValue>)m_dict).Add(key, value);
    }

    public bool ContainsKey(TKey key)
    {
        return ((IDictionary<TKey, TValue>)m_dict).ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
        return m_dict.Remove(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return m_dict.TryGetValue(key, out value);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        ((IDictionary<TKey, TValue>)m_dict).Add(item);
    }

    public void Clear()
    {
        m_dict.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return ((IDictionary<TKey, TValue>)m_dict).Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((IDictionary<TKey, TValue>)m_dict).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return ((IDictionary<TKey, TValue>)m_dict).Remove(item);
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return ((IDictionary<TKey, TValue>)m_dict).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IDictionary<TKey, TValue>)m_dict).GetEnumerator();
    }

    #endregion

    #region IDictionary

    public bool IsFixedSize
    {
        get { return ((IDictionary)m_dict).IsFixedSize; }
    }

    ICollection IDictionary.Keys
    {
        get { return ((IDictionary)m_dict).Keys; }
    }

    ICollection IDictionary.Values
    {
        get { return ((IDictionary)m_dict).Values; }
    }

    public bool IsSynchronized
    {
        get { return ((IDictionary)m_dict).IsSynchronized; }
    }

    public object SyncRoot
    {
        get { return ((IDictionary)m_dict).SyncRoot; }
    }

    public object this[object key]
    {
        get { return ((IDictionary)m_dict)[key]; }
        set { ((IDictionary)m_dict)[key] = value; }
    }

    public void Add(object key, object value)
    {
        ((IDictionary)m_dict).Add(key, value);
    }

    public bool Contains(object key)
    {
        return ((IDictionary)m_dict).Contains(key);
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        return ((IDictionary)m_dict).GetEnumerator();
    }

    public void Remove(object key)
    {
        ((IDictionary)m_dict).Remove(key);
    }

    public void CopyTo(Array array, int index)
    {
        ((IDictionary)m_dict).CopyTo(array, index);
    }

    #endregion

    #region IDeserializationCallback

    public void OnDeserialization(object sender)
    {
        ((IDeserializationCallback)m_dict).OnDeserialization(sender);
    }

    #endregion

    #region ISerializable

    protected SerializableDictionaryBase(SerializationInfo info, StreamingContext context)
    {
        m_dict = new Dictionary<TKey, TValue>(info, context);
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        ((ISerializable)m_dict).GetObjectData(info, context);
    }

    #endregion
}

public static class SerializableDictionary
{
    public class Storage<T> : SerializableDictionaryBase.Storage
    {
        public T data;
    }
}

[Serializable]
public class SerializableDictionary<TKey, TValue> : SerializableDictionaryBase<TKey, TValue, TValue>
{
    public SerializableDictionary()
    {
    }

    public SerializableDictionary(IDictionary<TKey, TValue> dict) : base(dict)
    {
    }

    protected SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    protected override TValue GetValue(TValue[] storage, int i)
    {
        return storage[i];
    }

    protected override void SetValue(TValue[] storage, int i, TValue value)
    {
        storage[i] = value;
    }
}

[Serializable]
public class
    SerializableDictionary<TKey, TValue, TValueStorage> : SerializableDictionaryBase<TKey, TValue, TValueStorage>
    where TValueStorage : SerializableDictionary.Storage<TValue>, new()
{
    public SerializableDictionary()
    {
    }

    public SerializableDictionary(IDictionary<TKey, TValue> dict) : base(dict)
    {
    }

    protected SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    protected override TValue GetValue(TValueStorage[] storage, int i)
    {
        return storage[i].data;
    }

    protected override void SetValue(TValueStorage[] storage, int i, TValue value)
    {
        storage[i] = new TValueStorage();
        storage[i].data = value;
    }
}


[Serializable]
public abstract class SerializableReferenceDictionaryBase<TKey, TValue, TValueStorage> : SerializableDictionaryBase,
    IDictionary<TKey, TValue>, IDictionary, ISerializationCallbackReceiver, IDeserializationCallback, ISerializable
{
    Dictionary<TKey, TValue> m_dict;
    [SerializeField] TKey[] m_keys;
    [SerializeReference] TValueStorage[] m_values;

    public SerializableReferenceDictionaryBase()
    {
        m_dict = new Dictionary<TKey, TValue>();
    }

    public SerializableReferenceDictionaryBase(IDictionary<TKey, TValue> dict)
    {
        m_dict = new Dictionary<TKey, TValue>(dict);
    }

    protected abstract void SetValue(TValueStorage[] storage, int i, TValue value);
    protected abstract TValue GetValue(TValueStorage[] storage, int i);

    public void CopyFrom(IDictionary<TKey, TValue> dict)
    {
        m_dict.Clear();
        foreach (var kvp in dict)
        {
            m_dict[kvp.Key] = kvp.Value;
        }
    }


    public void OnAfterDeserialize()
    {
        if (m_keys != null && m_values != null && m_keys.Length == m_values.Length)
        {
            m_dict.Clear();
            int n = m_keys.Length;
            for (int i = 0; i < n; ++i)
            {
                if (m_keys[i] == null)
                    continue;
                m_dict[m_keys[i]] = GetValue(m_values, i);
            }

            m_keys = null;
            m_values = null;
        }
    }

    public void OnBeforeSerialize()
    {
        int n = m_dict.Count;
        m_keys = new TKey[n];
        m_values = new TValueStorage[n];

        int i = 0;
        foreach (var kvp in m_dict)
        {
            m_keys[i] = kvp.Key;
            SetValue(m_values, i, kvp.Value);
            ++i;
        }
    }

    public SerializableDictionaryEnumerator<TKey, TValue> GetEnumerator() => new(m_dict);

    #region IDictionary<TKey, TValue>

    public ICollection<TKey> Keys
    {
        get { return ((IDictionary<TKey, TValue>)m_dict).Keys; }
    }

    public ICollection<TValue> Values
    {
        get { return ((IDictionary<TKey, TValue>)m_dict).Values; }
    }

    public int Count
    {
        get { return ((IDictionary<TKey, TValue>)m_dict).Count; }
    }

    public bool IsReadOnly
    {
        get { return ((IDictionary<TKey, TValue>)m_dict).IsReadOnly; }
    }

    public TValue this[TKey key]
    {
        get { return ((IDictionary<TKey, TValue>)m_dict)[key]; }
        set { ((IDictionary<TKey, TValue>)m_dict)[key] = value; }
    }

    public void Add(TKey key, TValue value)
    {
        ((IDictionary<TKey, TValue>)m_dict).Add(key, value);
    }

    public bool ContainsKey(TKey key)
    {
        return ((IDictionary<TKey, TValue>)m_dict).ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
        return m_dict.Remove(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return m_dict.TryGetValue(key, out value);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        ((IDictionary<TKey, TValue>)m_dict).Add(item);
    }

    public void Clear()
    {
        ((IDictionary<TKey, TValue>)m_dict).Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return ((IDictionary<TKey, TValue>)m_dict).Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((IDictionary<TKey, TValue>)m_dict).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return ((IDictionary<TKey, TValue>)m_dict).Remove(item);
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return ((IDictionary<TKey, TValue>)m_dict).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IDictionary<TKey, TValue>)m_dict).GetEnumerator();
    }

    #endregion

    #region IDictionary

    public bool IsFixedSize
    {
        get { return ((IDictionary)m_dict).IsFixedSize; }
    }

    ICollection IDictionary.Keys
    {
        get { return ((IDictionary)m_dict).Keys; }
    }

    ICollection IDictionary.Values
    {
        get { return ((IDictionary)m_dict).Values; }
    }

    public bool IsSynchronized
    {
        get { return ((IDictionary)m_dict).IsSynchronized; }
    }

    public object SyncRoot
    {
        get { return ((IDictionary)m_dict).SyncRoot; }
    }

    public object this[object key]
    {
        get { return ((IDictionary)m_dict)[key]; }
        set { ((IDictionary)m_dict)[key] = value; }
    }

    public void Add(object key, object value)
    {
        ((IDictionary)m_dict).Add(key, value);
    }

    public bool Contains(object key)
    {
        return ((IDictionary)m_dict).Contains(key);
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        return ((IDictionary)m_dict).GetEnumerator();
    }

    public void Remove(object key)
    {
        ((IDictionary)m_dict).Remove(key);
    }

    public void CopyTo(Array array, int index)
    {
        ((IDictionary)m_dict).CopyTo(array, index);
    }

    #endregion

    #region IDeserializationCallback

    public void OnDeserialization(object sender)
    {
        ((IDeserializationCallback)m_dict).OnDeserialization(sender);
    }

    #endregion

    #region ISerializable

    protected SerializableReferenceDictionaryBase(SerializationInfo info, StreamingContext context)
    {
        m_dict = new Dictionary<TKey, TValue>(info, context);
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        ((ISerializable)m_dict).GetObjectData(info, context);
    }

    #endregion
}


[Serializable]
public class
    SerializableReferenceDictionary<TKey, TValue> : SerializableReferenceDictionaryBase<TKey, TValue, TValue>
{
    public SerializableReferenceDictionary()
    {
    }

    public SerializableReferenceDictionary(IDictionary<TKey, TValue> dict) : base(dict)
    {
    }

    protected SerializableReferenceDictionary(SerializationInfo info, StreamingContext context) : base(info,
        context)
    {
    }

    protected override TValue GetValue(TValue[] storage, int i)
    {
        return storage[i];
    }

    protected override void SetValue(TValue[] storage, int i, TValue value)
    {
        storage[i] = value;
    }
}