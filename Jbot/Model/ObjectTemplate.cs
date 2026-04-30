using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using JetBrains.Annotations;

namespace Jbot.Model;

[PublicAPI]
public class ObjectTemplate
{
    private Type? _boundType;

    [SetsRequiredMembers]
    internal ObjectTemplate(
        ushort id, string name, IReadOnlyCollection<string>? boundTypeNames,
        IReadOnlyCollection<FieldTemplate> fields, bool useCompression
    ) : this(id, name, boundTypeNames, fields, useCompression,
        !fields.Any(f => f.Id > byte.MaxValue)) { }

    [SetsRequiredMembers]
    internal ObjectTemplate(
        ushort id, string name, IReadOnlyCollection<string>? boundTypeNames,
        IReadOnlyCollection<FieldTemplate> fields, bool useCompression, bool usesShortIds
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

    internal ObjectTemplate() { }
    public required ushort Id { get; init; }
    public required bool UsesShortIds { get; init; }
    public required bool UseCompression { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyCollection<FieldTemplate> Fields { get; init; }

    // all of this is to allow compilation without actually resolving a target type
    // so AoT in a sandbox works, and so it doesn't always need to re-resolve the type
    // BoundTypeNames is always nonnull if _boundType is nonnull, but deferred resolution can make
    // Type null and string not
    public IReadOnlyCollection<string>? BoundTypeNames { get; init; }

    public Type? ResolveType() // TODO: assess whether this is worth keeping public
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

    public bool IsTypeResolved => this._boundType is not null;
    public bool HasBoundType => this.BoundTypeNames is not null && this.BoundTypeNames.Count > 0;

    public FieldTemplate? GetFieldOrNull(ushort id)
    {
        return this.Fields.FirstOrDefault(field => field.Id == id);
    }

    public FieldTemplate? GetFieldOrNull(string name)
    {
        return this.Fields.FirstOrDefault(field => field.Name == name);
    }

    public override string ToString() =>
        $"[id={this.Id}, name={this.Name}, boundType={this.BoundTypeNames}, usesShortIds={this.UsesShortIds}, fields={this.Fields}]";
}
