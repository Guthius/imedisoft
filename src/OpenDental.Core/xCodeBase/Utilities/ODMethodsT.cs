namespace CodeBase;

public static class ODMethodsT
{
    public static T Coalesce<T>(T input) where T : new()
    {
        return input ?? new T();
    }
}