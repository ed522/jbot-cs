namespace Jbot.Nametable;

public class ObjectTemplate
{
    
    public ushort Id { get; private set; }
    public bool UsesShortIds { get; private set; }
    public string? Name { get; private set; }
    public Type? BoundType { get; private set; }
    public IList<FieldTemplate> Fields { get; private set; }

    public ObjectTemplate(ushort id, string? name, Type? boundType, FieldTemplate[] fields)
    {
        Id = id;
        Name = name;
        BoundType = boundType;
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
        return BoundType != null;
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
        return $"[id={Id}, name={Name}, boundType={BoundType?.FullName}, usesShortIds={UsesShortIds}, fields={Fields}]";
    }

}