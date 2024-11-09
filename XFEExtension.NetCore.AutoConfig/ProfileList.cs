using System.Collections;

namespace XFEExtension.NetCore.AutoConfig;

/// <summary>
/// 配置文件列表
/// </summary>
/// <typeparam name="T">列表泛型</typeparam>
/// <typeparam name="P">配置文件泛型</typeparam>
public class ProfileList<T, P> : ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IList
{
    private readonly List<T> innerList;

    /// <summary>
    /// 空列表
    /// </summary>
    public ProfileList() => innerList = [];

    /// <summary>
    /// 使用已有列表创建
    /// </summary>
    /// <param name="innerList">已有列表</param>
    public ProfileList(List<T> innerList) => this.innerList = innerList;

    ///<inheritdoc/>
    public T this[int index] { get => ((IList<T>)innerList)[index]; set => ((IList<T>)innerList)[index] = value; }
    object? IList.this[int index] { get => ((IList)innerList)[index]; set => ((IList)innerList)[index] = value; }

    ///<inheritdoc/>
    public int Count => ((ICollection<T>)innerList).Count;

    ///<inheritdoc/>
    public bool IsReadOnly => ((ICollection<T>)innerList).IsReadOnly;

    ///<inheritdoc/>
    public bool IsSynchronized => ((ICollection)innerList).IsSynchronized;

    ///<inheritdoc/>
    public object SyncRoot => ((ICollection)innerList).SyncRoot;

    ///<inheritdoc/>
    public bool IsFixedSize => ((IList)innerList).IsFixedSize;

    ///<inheritdoc/>
    public void Add(T item)
    {
        ((ICollection<T>)innerList).Add(item);
        XFEProfile.SaveProfile(typeof(P));
    }

    ///<inheritdoc/>
    public int Add(object? value)
    {
        var result = ((IList)innerList).Add(value);
        XFEProfile.SaveProfile(typeof(P));
        return result;
    }

    ///<inheritdoc/>
    public void AddRange(IEnumerable<T> collection)
    {
        innerList.AddRange(collection);
        XFEProfile.SaveProfile(typeof(P));
    }

    ///<inheritdoc/>
    public void AddRange(ReadOnlySpan<T> source)
    {
        innerList.AddRange(source);
        XFEProfile.SaveProfile(typeof(P));
    }

    ///<inheritdoc/>
    public void Clear()
    {
        ((ICollection<T>)innerList).Clear();
        XFEProfile.SaveProfile(typeof(P));
    }

    ///<inheritdoc/>
    public bool Contains(T item)
    {
        return ((ICollection<T>)innerList).Contains(item);
    }

    ///<inheritdoc/>
    public bool Contains(object? value)
    {
        return ((IList)innerList).Contains(value);
    }

    ///<inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
        ((ICollection<T>)innerList).CopyTo(array, arrayIndex);
    }

    ///<inheritdoc/>
    public void CopyTo(Array array, int index)
    {
        ((ICollection)innerList).CopyTo(array, index);
    }

    ///<inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)innerList).GetEnumerator();
    }

    ///<inheritdoc/>
    public int IndexOf(T item)
    {
        return ((IList<T>)innerList).IndexOf(item);
    }

    ///<inheritdoc/>
    public int IndexOf(object? value)
    {
        return ((IList)innerList).IndexOf(value);
    }

    ///<inheritdoc/>
    public void Insert(int index, T item)
    {
        ((IList<T>)innerList).Insert(index, item);
        XFEProfile.SaveProfile(typeof(P));
    }

    ///<inheritdoc/>
    public void Insert(int index, object? value)
    {
        ((IList)innerList).Insert(index, value);
        XFEProfile.SaveProfile(typeof(P));
    }

    ///<inheritdoc/>
    public bool Remove(T item)
    {
        var result = ((ICollection<T>)innerList).Remove(item);
        XFEProfile.SaveProfile(typeof(P));
        return result;
    }

    ///<inheritdoc/>
    public void Remove(object? value)
    {
        ((IList)innerList).Remove(value);
        XFEProfile.SaveProfile(typeof(P));
    }

    ///<inheritdoc/>
    public void RemoveAt(int index)
    {
        ((IList<T>)innerList).RemoveAt(index);
        XFEProfile.SaveProfile(typeof(P));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)innerList).GetEnumerator();
    }
}
