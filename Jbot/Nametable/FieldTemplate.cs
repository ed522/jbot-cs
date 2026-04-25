using Jbot.Data;
using Jbot.Utils;

using JetBrains.Annotations;

namespace Jbot.Nametable;

[PublicAPI]
public class FieldTemplate
{
    internal FieldTemplate(
        ushort id, string name, DataType[] allowableTypes,
        string[]? boundMembers, string[]? allowedObjects, bool useCompression
    ) : this()
    {
        this.AllowableTypes = ((EnumSet<DataType>)[..allowableTypes]).AsReadOnly();
        this.Name = name;
        this.Id = id;
        this.BoundMembers = boundMembers?.AsReadOnly();
        this.AllowedObjects = allowedObjects?.AsReadOnly();
        this.UseCompression = useCompression;
    }

    internal FieldTemplate() { }
    public required EnumSet<DataType> AllowableTypes { get; init; }
    public required string Name { get; init; }
    public required ushort Id { get; init; }
    public IReadOnlyCollection<string>? BoundMembers { get; init; }
    public IReadOnlyCollection<string>? AllowedObjects { get; init; }
    public required bool UseCompression { get; init; }

    public bool AllowsType(DataType type) => this.AllowableTypes.Contains(type);

    public override string ToString() =>
        $"[id={this.Id}, name={this.Name}, types={this.AllowableTypes}]";
}
