using Jbot.Data;
using Jbot.Model;

namespace Jbot.IO;

public static class ReflectingConverter
{
    private static readonly Dictionary<ObjectTemplate, Resolver.ObjectInfo> _infoDict = [];

    private static object Unmarshal(object obj)
    {
        switch (obj)
        {
            case DataObject dataObj:
                return Unmarshal<object>(dataObj);

            case DataArray arr:
                object[] elements = new object[arr.Count];

                for (int i = 0; i < arr.Count; i++)
                {
                    elements[i] = Unmarshal(arr[i]);
                }

                return elements;

            default:
                return obj;
        }
    }

    public static T Unmarshal<T>(DataObject obj)
    {
        // cache resolved object
        if (_infoDict.TryGetValue(obj.Template, out Resolver.ObjectInfo info))
        {
            info = Resolver.ResolveObject(obj);
            _infoDict.Add(obj.Template, info);
        }

        if (!info.TargetType.IsAssignableTo(typeof(T)))
        {
            throw new ArgumentException($"Cannot assign object type {info.TargetType}" +
                                        $"to desired type {typeof(T)}");
        }

        // do reflection
        object[] args = new object[info.Initializer.ParameterMap.Count];

        foreach (string key in info.Initializer.ParameterMap.Keys) { }

        T instance = (T)info.Initializer.Method.Invoke(null);
    }
}
