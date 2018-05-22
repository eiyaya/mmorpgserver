#region using

using System.Collections;
using System.Collections.Generic;

#endregion

public class Iterator<T> : IEnumerable<T>
{
    public Iterator(IEnumerator<T> iter)
    {
        m_iterator = iter;
    }

    public Iterator(IEnumerable<T> enumerable)
    {
        m_iterator = enumerable.GetEnumerator();
    }

    private readonly IEnumerator<T> m_iterator;

    public IEnumerator<T> GetEnumerator()
    {
        return m_iterator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_iterator;
    }
}

public static class IEnumeratorExtension
{
    public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> t)
    {
        return new Iterator<T>(t);
    }
}