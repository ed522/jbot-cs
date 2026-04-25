using System.Reflection;

namespace Jbot.Nametable;

public class ObjectTemplate
{
    public ushort Id { get; }
    public bool UsesShortIds { get; }
    public bool UseCompression { get; }
    public string Name { get; }
    public IList<FieldTemplate> Fields { get; }

    // all of this is to allow compilation without actually resolving a target type
    // so AoT in a sandbox works, and so it doesn't always need to re-resolve the type
    // BoundTypeNames is always nonnull if _boundType is nonnull, but deferred resolution can make
    // Type null and string not
    public IList<string>? BoundTypeNames { get; }
    private Type? _boundType;

    public Type? ResolveType()
    {
        if (this._boundType is not null) return this._boundType;
        if (this.BoundTypeNames is null || this.BoundTypeNames.Count == 0) return null;

        foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? possibleType
                = (from t in a.GetTypes()
                   where t.FullName is not null && this.BoundTypeNames.Contains(t.FullName)
                   select t).FirstOrDefault();

            if (possibleType is not null)
            {
                this._boundType = possibleType;
                return this._boundType;
            }
        }

        return null;
    }

    public ObjectTemplate(
        ushort id, string name, string[]? boundTypeNames, FieldTemplate[] fields,
        bool useCompression
    ) : this(id, name, boundTypeNames, fields, useCompression,
        !fields.Any(f => f.Id > byte.MaxValue)) { }

    public ObjectTemplate(
        ushort id, string name, string[]? boundTypeNames, FieldTemplate[] fields,
        bool useCompression, bool usesShortIds
    )
    {
        this.Id = id;
        this.Name = name;
        this.BoundTypeNames = boundTypeNames;
        this.UseCompression = useCompression;
        this.UsesShortIds = usesShortIds;

        FieldTemplate[] clonedFields = [..fields];
        this.Fields = clonedFields.AsReadOnly();
    }

    public bool HasBoundType()
    {
        return this.BoundTypeNames is not null && this.BoundTypeNames.Count > 0;
    }

    public FieldTemplate? GetFieldOrNull(ushort id)
    {
        return this.Fields.FirstOrDefault(field => field.Id == id);
    }

    public FieldTemplate? GetFieldOrNull(string name)
    {
        return this.Fields.FirstOrDefault(field => field.Name == name);
    }

    public override string ToString()
    {
        return
            $"[id={this.Id}, name={this.Name}, boundType={this.BoundTypeNames}, usesShortIds={this.UsesShortIds}, fields={this.Fields}]";
    }
}
