using System.Diagnostics.CodeAnalysis;

using Jbot.Data;
using Jbot.Model;

using JetBrains.Annotations;

namespace Jbot.IO;

[PublicAPI]
public class Marshaller
{
    private readonly Dictionary<ObjectTemplate, TypedMarshalProvider> _registeredProviders = [];
    private readonly ReflectingConverter _converter;
    
    public Marshaller()
    {
        this._converter = new ReflectingConverter(this);
    }

    public Marshaller(
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

    /// <summary>
    /// Convert a DataObject into a corresponding CLR object.
    /// <h2>Note</h2>
    /// The set of registered providers is queried first, then if no valid provider is found, the 
    /// </summary>
    /// <param name="obj"></param>
    /// <typeparam name="T">The desired result type of the conversion</typeparam>
    /// <returns></returns>
    [RequiresUnreferencedCode("Object binding resolution requires loading a type from a string name, which " +
            "is not compatible with trimming.")]
    public T Unmarshal<T>(DataObject obj) => this.Unmarshal<T>(obj, typeof(T));
    
    // internal version that takes a runtime type
    [RequiresUnreferencedCode("Object binding resolution requires loading a type from a string name, which " +
                              "is not compatible with trimming.")]
    internal T Unmarshal<T>(DataObject obj, Type realType)
    {
        if (this._registeredProviders.TryGetValue(obj.Template, out TypedMarshalProvider marshal))
        {
            // check typing

            // unmarshaling is covariant
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
        return this._converter.Unmarshal<T>(obj);
    }

    [RequiresUnreferencedCode("Object binding resolution requires loading a type from a string name, which " +
                              "is not compatible with trimming.")]
    internal DataObject Marshal<T>(T obj) where T : notnull
    {
        (ObjectTemplate? template, TypedMarshalProvider provider) =
            (from pair in this._registeredProviders
             where pair.Value.TargetType == obj.GetType()
             select pair).FirstOrDefault();

        if (template is not null && provider.Provider is not null)
        {
            return provider.Provider.Marshal(obj);
        }
        
        // reflect
        return this._converter.Marshal(obj);

    }

    public readonly struct TypedMarshalProvider(Type type, IMarshalProvider provider)
    {
        public Type TargetType { get; } = type;
        public IMarshalProvider Provider { get; } = provider;
    }
}
