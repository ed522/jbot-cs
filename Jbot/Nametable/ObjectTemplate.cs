using System.Reflection;

namespace Jbot.Nametable;

public class ObjectTemplate
{
    
    public ushort Id { get; private init; }
    public bool UsesShortIds { get; private init; }
    public bool UseCompression { get; private init; }
    public string? Name { get; private init; }
    public IList<FieldTemplate> Fields { get; private init; }

    // all of this is to allow compilation without actually resolving a target type
    // so AoT in a sandbox works, and so it doesn't always need to re-resolve the type
    // BoundTypeName is always nonnull if BoundType is nonnull, but deferred resolution can make Type null and string not
    public string[]? BoundTypeNames { get; private init; } = null;
    private Type? _boundType = null;

    public Type? ResolveType()
    {
        if (_boundType is not null) return _boundType;
        if (BoundTypeNames is null) return null;

        foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? possibleType = (from t in a.GetTypes() where BoundTypeNames.Contains(t.FullName) select t).FirstOrDefault();
            if (possibleType != default)
            {
                _boundType = possibleType;
                return _boundType;
            }
        }

        return null;
    }

    public ObjectTemplate(ushort id, string? name, string[]? boundTypeNames, FieldTemplate[] fields, bool useCompression)
    {
        Id = id;
        Name = name;
        BoundTypeNames = boundTypeNames;
        UseCompression = useCompression;
        UsesShortIds = !fields.Any(f => f.Id > byte.MaxValue);
        
        FieldTemplate[] clonedFields = [..fields];
        Fields = clonedFields.AsReadOnly();
    }

    public bool HasName()
    {
        return Name != null;
    }
    public bool HasBoundType()
    {
        return BoundTypeNames != null && BoundTypeNames?.Length > 0;
    }

    public FieldTemplate? GetFieldOrNull(ushort id)
    {
        foreach (FieldTemplate field in Fields)
        {
            if (field.Id == id)
            {
                return field;
            }
        }

        return null;
    }
    public FieldTemplate? GetFieldOrNull(string name)
    {
        foreach (FieldTemplate field in Fields)
        {
            if (field.Name == name)
            {
                return field;
            }
        }

        return null;
    }

    public override string ToString()
    {
        return $"[id={Id}, name={Name}, boundType={BoundTypeNames}, usesShortIds={UsesShortIds}, fields={Fields}]";
    }

}