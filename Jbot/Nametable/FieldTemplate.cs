namespace Jbot.Nametable;

public class FieldTemplate
{
    public EnumSet<DataType> AllowableTypes { get; private init; }
    public string Name { get; private init; }
    public ushort Id { get; private init; }

    public FieldTemplate(ushort id, string name, DataType[] allowableTypes)
    {
        Id = id;
        Name = name;
        AllowableTypes = [..allowableTypes];
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
