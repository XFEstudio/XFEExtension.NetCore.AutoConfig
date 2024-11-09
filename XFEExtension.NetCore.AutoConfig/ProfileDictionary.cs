using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace XFEExtension.NetCore.AutoConfig;

/// <summary>
/// 配置文件字典
/// </summary>
/// <typeparam name="TProfile">配置文件类型</typeparam>
/// <typeparam name="TKey">字典Key泛型</typeparam>
/// <typeparam name="TValue">字典Value泛型</typeparam>
public class ProfileDictionary<TProfile, TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>, ICollection, IDictionary, IDeserializationCallback, ISerializable where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _innerDictionary;

    /// <summary>
    /// 创建空字典
    /// </summary>
    public ProfileDictionary() => _innerDictionary = [];

    /// <summary>
    /// 使用现有字典创建
    /// </summary>
    /// <param name="innerDictionary">现有字典</param>
    public ProfileDictionary(Dictionary<TKey, TValue> innerDictionary) => _innerDictionary = innerDictionary;

    ///<inheritdoc/>
    public TValue this[TKey key] { get => ((IDictionary<TKey, TValue>)_innerDictionary)[key]; set => ((IDictionary<TKey, TValue>)_innerDictionary)[key] = value; }
    ///<inheritdoc/>
    public object? this[object key] { get => ((IDictionary)_innerDictionary)[key]; set => ((IDictionary)_innerDictionary)[key] = value; }

    ///<inheritdoc/>
    public int Count => ((ICollection<KeyValuePair<TKey, TValue>>)_innerDictionary).Count;

    ///<inheritdoc/>
    public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)_innerDictionary).IsReadOnly;

    ///<inheritdoc/>
    public ICollection<TKey> Keys => ((IDictionary<TKey, TValue>)_innerDictionary).Keys;

    ///<inheritdoc/>
    public ICollection<TValue> Values => ((IDictionary<TKey, TValue>)_innerDictionary).Values;

    ///<inheritdoc/>
    public bool IsSynchronized => ((ICollection)_innerDictionary).IsSynchronized;

    ///<inheritdoc/>
    public object SyncRoot => ((ICollection)_innerDictionary).SyncRoot;

    ///<inheritdoc/>
    public bool IsFixedSize => ((IDictionary)_innerDictionary).IsFixedSize;

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => ((IReadOnlyDictionary<TKey, TValue>)_innerDictionary).Keys;

    ICollection IDictionary.Keys => ((IDictionary)_innerDictionary).Keys;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => ((IReadOnlyDictionary<TKey, TValue>)_innerDictionary).Values;

    ICollection IDictionary.Values => ((IDictionary)_innerDictionary).Values;

    ///<inheritdoc/>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)_innerDictionary).Add(item);
        XFEProfile.SaveProfile(typeof(TProfile));
    }

    ///<inheritdoc/>
    public void Add(TKey key, TValue value)
    {
        ((IDictionary<TKey, TValue>)_innerDictionary).Add(key, value);
        XFEProfile.SaveProfile(typeof(TProfile));
    }

    ///<inheritdoc/>
    public void Add(object key, object? value)
    {
        ((IDictionary)_innerDictionary).Add(key, value);
        XFEProfile.SaveProfile(typeof(TProfile));
    }

    ///<inheritdoc/>
    public void Clear()
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)_innerDictionary).Clear();
        XFEProfile.SaveProfile(typeof(TProfile));
    }

    ///<inheritdoc/>
    public bool Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_innerDictionary).Contains(item);

    ///<inheritdoc/>
    public bool Contains(object key) => ((IDictionary)_innerDictionary).Contains(key);

    ///<inheritdoc/>
    public bool ContainsKey(TKey key) => ((IDictionary<TKey, TValue>)_innerDictionary).ContainsKey(key);

    ///<inheritdoc/>
    public bool ContainsValue(TValue value) => _innerDictionary.ContainsValue(value);

    ///<inheritdoc/>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)_innerDictionary).CopyTo(array, arrayIndex);

    ///<inheritdoc/>
    public void CopyTo(Array array, int index) => ((ICollection)_innerDictionary).CopyTo(array, index);

    ///<inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>)_innerDictionary).GetEnumerator();

    ///<inheritdoc/>
    [Obsolete]
    public void GetObjectData(SerializationInfo info, StreamingContext context) => ((ISerializable)_innerDictionary).GetObjectData(info, context);

    ///<inheritdoc/>
    public void OnDeserialization(object? sender) => ((IDeserializationCallback)_innerDictionary).OnDeserialization(sender);

    ///<inheritdoc/>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        var result = ((ICollection<KeyValuePair<TKey, TValue>>)_innerDictionary).Remove(item);
        XFEProfile.SaveProfile(typeof(TProfile));
        return result;
    }

    ///<inheritdoc/>
    public bool Remove(TKey key)
    {
        var result = ((IDictionary<TKey, TValue>)_innerDictionary).Remove(key);
        XFEProfile.SaveProfile(typeof(TProfile));
        return result;
    }

    ///<inheritdoc/>
    public void Remove(object key)
    {
        ((IDictionary)_innerDictionary).Remove(key);
        XFEProfile.SaveProfile(typeof(TProfile));
    }

    ///<inheritdoc/>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => ((IDictionary<TKey, TValue>)_innerDictionary).TryGetValue(key, out value);

    ///<inheritdoc/>
    public bool TryAdd(TKey key, TValue value)
    {
        var result = _innerDictionary.TryAdd(key, value);
        XFEProfile.SaveProfile(typeof(TProfile));
        return result;
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_innerDictionary).GetEnumerator();

    IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)_innerDictionary).GetEnumerator();
}
