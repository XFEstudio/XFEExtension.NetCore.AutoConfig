using System.Collections;

namespace XFEExtension.NetCore.AutoConfig;

/// <summary>
/// 配置文件列表
/// </summary>
/// <typeparam name="TProfile">配置文件类型</typeparam>
/// <typeparam name="TValue">列表泛型</typeparam>
public class ProfileList<TProfile, TValue> : ICollection<TValue>, IEnumerable<TValue>, IEnumerable, IList<TValue>, IReadOnlyCollection<TValue>, IReadOnlyList<TValue>, ICollection, IList
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
        XFEProfile.SaveProfile(typeof(TProfile));
    }

    ///<inheritdoc/>
    public int Add(object? value)
    {
        var result = ((IList)_innerList).Add(value);
        XFEProfile.SaveProfile(typeof(TProfile));
        return result;
    }

    ///<inheritdoc/>
    public void AddRange(IEnumerable<TValue> collection)
    {
        _innerList.AddRange(collection);
        XFEProfile.SaveProfile(typeof(TProfile));
    }

    ///<inheritdoc/>
    public void AddRange(ReadOnlySpan<TValue> source)
    {
        _innerList.AddRange(source);
        XFEProfile.SaveProfile(typeof(TProfile));
    }

    ///<inheritdoc/>
    public void Clear()
    {
        ((ICollection<TValue>)_innerList).Clear();
        XFEProfile.SaveProfile(typeof(TProfile));
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
        XFEProfile.SaveProfile(typeof(TProfile));
    }

    ///<inheritdoc/>
    public void Insert(int index, object? value)
    {
        ((IList)_innerList).Insert(index, value);
        XFEProfile.SaveProfile(typeof(TProfile));
    }

    ///<inheritdoc/>
    public bool Remove(TValue item)
    {
        var result = ((ICollection<TValue>)_innerList).Remove(item);
        XFEProfile.SaveProfile(typeof(TProfile));
        return result;
    }

    ///<inheritdoc/>
    public void Remove(object? value)
    {
        ((IList)_innerList).Remove(value);
        XFEProfile.SaveProfile(typeof(TProfile));
    }

    ///<inheritdoc/>
    public void RemoveAt(int index)
    {
        ((IList<TValue>)_innerList).RemoveAt(index);
        XFEProfile.SaveProfile(typeof(TProfile));
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_innerList).GetEnumerator();
}
