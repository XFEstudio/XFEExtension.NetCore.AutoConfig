using System.Collections;

namespace XFEExtension.NetCore.AutoConfig;

/// <summary>
/// 配置文件列表
/// </summary>
/// <typeparam name="TValue">列表泛型</typeparam>
public class ProfileList<TValue> : ICollection<TValue>, IEnumerable<TValue>, IEnumerable, IList<TValue>, IReadOnlyCollection<TValue>, IReadOnlyList<TValue>, ICollection, IList
{
    private readonly List<TValue> _innerList;

    /// <summary>
    /// 创建空列表
    /// </summary>
    public ProfileList() => _innerList = [];

    /// <summary>
    /// 使用已有列表创建
    /// </summary>
    /// <param name="innerList">已有列表</param>
    public ProfileList(List<TValue> innerList) => _innerList = innerList;

    ///<inheritdoc/>
    public TValue this[int index] { get => ((IList<TValue>)_innerList)[index]; set => ((IList<TValue>)_innerList)[index] = value; }
    object? IList.this[int index] { get => ((IList)_innerList)[index]; set => ((IList)_innerList)[index] = value; }

    /// <summary>
    /// 当前配置文件实例
    /// </summary>
    public XFEProfile? CurrentProfile { get; set; }

    ///<inheritdoc/>
    public int Count => ((ICollection<TValue>)_innerList).Count;

    ///<inheritdoc/>
    public bool IsReadOnly => ((ICollection<TValue>)_innerList).IsReadOnly;

    ///<inheritdoc/>
    public bool IsSynchronized => ((ICollection)_innerList).IsSynchronized;

    ///<inheritdoc/>
    public object SyncRoot => ((ICollection)_innerList).SyncRoot;

    ///<inheritdoc/>
    public bool IsFixedSize => ((IList)_innerList).IsFixedSize;

    ///<inheritdoc/>
    public void Add(TValue item)
    {
        ((ICollection<TValue>)_innerList).Add(item);
        CurrentProfile?.InstanceSaveProfile();
    }

    ///<inheritdoc/>
    public int Add(object? value)
    {
        var result = ((IList)_innerList).Add(value);
        CurrentProfile?.InstanceSaveProfile();
        return result;
    }

    ///<inheritdoc/>
    public void AddRange(IEnumerable<TValue> collection)
    {
        _innerList.AddRange(collection);
        CurrentProfile?.InstanceSaveProfile();
    }

    ///<inheritdoc/>
    public void AddRange(ReadOnlySpan<TValue> source)
    {
        _innerList.AddRange(source);
        CurrentProfile?.InstanceSaveProfile();
    }

    ///<inheritdoc/>
    public void Clear()
    {
        ((ICollection<TValue>)_innerList).Clear();
        CurrentProfile?.InstanceSaveProfile();
    }

    ///<inheritdoc/>
    public bool Contains(TValue item) => ((ICollection<TValue>)_innerList).Contains(item);

    ///<inheritdoc/>
    public bool Contains(object? value) => ((IList)_innerList).Contains(value);

    ///<inheritdoc/>
    public void CopyTo(TValue[] array, int arrayIndex) => ((ICollection<TValue>)_innerList).CopyTo(array, arrayIndex);

    ///<inheritdoc/>
    public void CopyTo(Array array, int index) => ((ICollection)_innerList).CopyTo(array, index);

    ///<inheritdoc/>
    public IEnumerator<TValue> GetEnumerator() => ((IEnumerable<TValue>)_innerList).GetEnumerator();

    ///<inheritdoc/>
    public int IndexOf(TValue item) => ((IList<TValue>)_innerList).IndexOf(item);

    ///<inheritdoc/>
    public int IndexOf(object? value) => ((IList)_innerList).IndexOf(value);

    ///<inheritdoc/>
    public void Insert(int index, TValue item)
    {
        ((IList<TValue>)_innerList).Insert(index, item);
        CurrentProfile?.InstanceSaveProfile();
    }

    ///<inheritdoc/>
    public void Insert(int index, object? value)
    {
        ((IList)_innerList).Insert(index, value);
        CurrentProfile?.InstanceSaveProfile();
    }

    ///<inheritdoc/>
    public bool Remove(TValue item)
    {
        var result = ((ICollection<TValue>)_innerList).Remove(item);
        CurrentProfile?.InstanceSaveProfile();
        return result;
    }

    ///<inheritdoc/>
    public void Remove(object? value)
    {
        ((IList)_innerList).Remove(value);
        CurrentProfile?.InstanceSaveProfile();
    }

    ///<inheritdoc/>
    public void RemoveAt(int index)
    {
        ((IList<TValue>)_innerList).RemoveAt(index);
        CurrentProfile?.InstanceSaveProfile();
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_innerList).GetEnumerator();
}
