namespace Jbot.Nametable;

public class FieldTemplate()
{
    public required EnumSet<DataType> AllowableTypes { get; init; }
    public required string Name { get; init; }
    public required ushort Id { get; init; }
    public IList<string>? BoundMembers { get; init; }
    public IList<string>? AllowedObjects { get; init; }
    public required bool UseCompression { get; init; }

    public FieldTemplate(
        ushort id, string name, DataType[] allowableTypes, 
        string[]? boundMembers, string[]? allowedObjects, bool useCompression
    ): this()
    {
        AllowableTypes = ((EnumSet<DataType>) [..allowableTypes]).AsReadOnly();
        Name = name;
        Id = id;
        BoundMembers = boundMembers?.AsReadOnly();
        AllowedObjects = allowedObjects?.AsReadOnly();
        UseCompression = useCompression;
    }

    public bool AllowsType(DataType type)
    {
        return AllowableTypes.Contains(type);
    }

    public override string ToString()
    {
        return $"[id={Id}, name={Name}, types={AllowableTypes}]";
    }

}
