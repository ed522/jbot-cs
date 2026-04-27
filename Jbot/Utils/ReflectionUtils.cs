using JetBrains.Annotations;

namespace Jbot.Utils;

public static class ReflectionUtils
{
    [PublicAPI]
    public static void ThrowIfTypeWrong<T>(object obj)
    {
        if (obj is not T)
        {
            throw new InvalidOperationException($"Cannot convert from type {obj.GetType().FullName} " +
                                                $"to {typeof(T).FullName}");
        }
    }
}
