using System;

namespace CodeBase;

public class ODTuple<T1, T2>(T1 item1, T2 item2)
{
    public T1 Item1 { get; } = item1;
    public T2 Item2 { get; } = item2;

    public static implicit operator Tuple<T1, T2>(ODTuple<T1, T2> tuple)
    {
        return Tuple.Create(tuple.Item1, tuple.Item2);
    }

    public static implicit operator ODTuple<T1, T2>(Tuple<T1, T2> tuple)
    {
        return new ODTuple<T1, T2>(tuple.Item1, tuple.Item2);
    }
}