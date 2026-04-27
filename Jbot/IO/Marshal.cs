using Jbot.Data;
using Jbot.Model;

using JetBrains.Annotations;

namespace Jbot.IO;

[PublicAPI]
public class Marshal
{
    private readonly Dictionary<ObjectTemplate, TypedMarshalProvider> _registeredProviders = [];

    public Marshal()
    {
        // empty
    }

    public Marshal(
        Dictionary<ObjectTemplate, TypedMarshalProvider> providers
    ) : this()
    {
        foreach ((ObjectTemplate template, TypedMarshalProvider provider) in providers)
            this.RegisterProvider(template, provider);
    }

    public void RegisterProvider<T>(ObjectTemplate template, IMarshalProvider marshal)
    {
        this._registeredProviders.Add(template, new TypedMarshalProvider(typeof(T), marshal));
    }

    public void RegisterProvider(ObjectTemplate template, TypedMarshalProvider typedMarshal)
    {
        this._registeredProviders.Add(template, typedMarshal);
    }


    public T Unmarshal<T>(DataObject obj)
    {
        if (this._registeredProviders.TryGetValue(obj.Template, out TypedMarshalProvider marshal))
        {
            // check typing
            Type realType = typeof(T);

            // upconversion is covariant
            if (!marshal.TargetType.IsAssignableTo(realType))
            {
                throw new InvalidOperationException(
                    $"Attempted to convert {obj.Template.Name} to " +
                    $"{realType.FullName}, but the registered " +
                    $"converter is of type {marshal.TargetType.FullName}");
            }

            object val = marshal.Provider.Unmarshal(obj);

            if (val is T t)
            {
                return t;
            }

            // else
            throw new InvalidOperationException($"Marshal of target type " +
                                                $"{marshal.TargetType.FullName} returned a " +
                                                $"value of type {val.GetType().FullName}");
        }

        // reflectively convert
        return ReflectingConverter.Unmarshal<T>(obj);
    }

    public readonly struct TypedMarshalProvider(Type type, IMarshalProvider provider)
    {
        public Type TargetType { get; } = type;
        public IMarshalProvider Provider { get; } = provider;
    }
}
