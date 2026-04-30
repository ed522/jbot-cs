using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;

using Jbot.Data;
using Jbot.Model;
using Jbot.Utils;

namespace Jbot.IO;

internal class ReflectingConverter(Marshaller marshal)
{
    private readonly Dictionary<ObjectTemplate, Resolver.ObjectInfo> _infoDict = [];

    private object? Unmarshal(AbstractDataValue data)
    {
        switch (data.Type)
        {
            case DataType.NULL:
                return null;
            case DataType.OBJECT:
                DataObject? obj = data.GetObject();
                if (obj is null) return null;
                // resolve object type
                if (!this._infoDict.TryGetValue(obj.Template, out Resolver.ObjectInfo info))
                {
                    info = Resolver.ResolveObject(obj.Template);
                    this._infoDict.Add(obj.Template, info);
                }
                // requires the marshal's unmarshal call to use marshal providers, and to do reflection
                return marshal.Unmarshal<object>(obj, info.TargetType);

            case DataType.ARRAY:
                DataArray? arr = data.GetArray();
                if (arr is null) return null;
                object?[] elements = new object[arr.Count];

                // recurses
                for (int i = 0; i < arr.Count; i++)
                {
                    AbstractDataValue val = arr[i];
                    // recurse using this (current) method, since this part can be objects, or plain values
                    elements[i] = this.Unmarshal(arr[i]);
                }

                return elements;

            default:
                return data.GetRawValue();
        }
    }

    [RequiresUnreferencedCode("Object binding resolution requires loading a type from a string name, which " +
                              "is not compatible with trimming.")]
    public T Unmarshal<T>(DataObject obj)
    {
        // cache resolved object
        if (!this._infoDict.TryGetValue(obj.Template, out Resolver.ObjectInfo info))
        {
            info = Resolver.ResolveObject(obj.Template);
            this._infoDict.Add(obj.Template, info);
        }

        if (!info.TargetType.IsAssignableTo(typeof(T)))
        {
            throw new ArgumentException($"Cannot assign object type {info.TargetType}" +
                                        $"to desired type {typeof(T)}");
        }


        // do reflection
        if (info.Initializer.ParameterMap.Count != 0)
        {
            // has parameters - can return the value immediately
            object?[] args = new object[info.Initializer.ParameterMap.Count];

            foreach (string key in info.Initializer.ParameterMap.Keys)
            {
                args[info.Initializer.ParameterIndexMap[key]] = this.Unmarshal(obj.Get(key));
            }

            return (T?)info.Initializer.Method.Invoke(null, args) ??
                   throw new InvalidOperationException($"Initializer for object {obj.Template.Name} " +
                                                       $"must not return null");
        }
        // construct and initialize
        T instance = (T?)info.Initializer.Method.Invoke(null, []) ??
                   throw new InvalidOperationException($"Initializer for object {obj.Template.Name} " +
                                                       $"must not return null");

        foreach ((string key, FieldInfo field) in info.Fields)
        {
            field.SetValue(instance, this.Unmarshal(obj.Get(key)));
        }

        foreach ((string key, PropertyInfo prop) in info.Props)
        {
            prop.SetValue(instance, this.Unmarshal(obj.Get(key)));
        }

        return instance;
    }

    [RequiresUnreferencedCode("Object binding resolution requires loading a type from a string name, which " +
                              "is not compatible with trimming.")]
    public DataObject Marshal<T>(T obj) where T : notnull
    {
        Type targetType = obj.GetType();

        // check for resolved type
        ObjectTemplate? template = (from t in this._infoDict.Keys
                                    where this._infoDict[t].TargetType == targetType
                                    select t).FirstOrDefault();
        Resolver.ObjectInfo info;

        if (template is null)
        {
            // find type, prefer templates that have already had their bound types resolved
            template = (from t in this._infoDict.Keys
                        orderby t.IsTypeResolved descending
                        where t.ResolveType() == targetType
                        select t).FirstOrDefault();

            if (template is null)
            {
                throw new InvalidOperationException($"No template is bound to the type " +
                                                    $"{targetType.FullName}");
            }

            info = Resolver.ResolveObject(template);
            this._infoDict.Add(template, info);
        }
        else
        {
            info = this._infoDict[template];
        }

        // set each field
        DataObject dataObj = new(template);

        foreach ((string key, FieldInfo field) in info.Fields)
        {
            this.MarshalValue(dataObj.Fields[key], field.GetValue(obj));
        }

        foreach ((string key, PropertyInfo prop) in info.Props)
        {
            this.MarshalValue(dataObj.Fields[key], prop.GetValue(obj));
        }

        return dataObj;
    }
    
    private void MarshalValue(AbstractDataValue data, object? value)
    {
        // each method sets data type based on the object's CLR type
        // (trust me a switch is worse)
        if (value is null) data.SetNull();
        else if (value is string str) data.Set(str);
        else if (value is bool b) data.Set(b);
        else if (value is sbyte i8) data.Set(i8);
        else if (value is short s16) data.Set(s16);
        else if (value is int i32) data.Set(i32);
        else if (value is long i64) data.Set(i64);
        else if (value is byte u8) data.Set(u8);
        else if (value is ushort u16) data.Set(u16);
        else if (value is uint u32) data.Set(u32);
        else if (value is ulong u64) data.Set(u64);
        else if (value is float f32) data.Set(f32);
        else if (value is double f64) data.Set(f64);
        else if (value is ScaledDecimal dec) data.Set(dec);
        else if (value is BigInteger bigInt) data.Set(bigInt);
        else if (value is byte[] bin) data.Set(bin);
        else if (value is Array arr)
        {
            DataArray dataArr = [];
            foreach (object? element in arr)
            {
                // actual value is not important since this is overwritten immediately
                DataValue elementValue = new();
                // NOTE: arrays that contain themselves will recurse
                // infinitely, but have you considered not doing that?
                this.MarshalValue(elementValue, element);
                dataArr.Add(elementValue);
            }

            data.Set(dataArr);
        }
        else
        {
            // objects can be anything
            data.Set(marshal.Marshal(value));
        }
    }
}
