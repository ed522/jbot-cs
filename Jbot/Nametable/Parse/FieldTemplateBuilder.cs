namespace Jbot.Nametable.Parse;

public class FieldTemplateBuilder
{

    public EnumSet<DataType>? AllowableTypes { get; set; }
    public List<string>? AllowedObjects { get; set; }
    public List<string>? BoundMembers { get; set; }
    public string? Name { get; set; }
    public ushort? Id { get; set; }
    public bool? UseCompression { get; internal set; }

    public void Check()
    {
        if (AllowableTypes is null)
        {
            throw new InvalidDocumentException("field is missing allowed types");
        }
        else if (Name is null)
        {
            throw new InvalidDocumentException("field is missing name");
        }
        else if (Id is null)
        {
            throw new InvalidDocumentException("field is missing id");
        }
    }
    public FieldTemplate? Build()
    {
        if (AllowableTypes is null || Name is null || Id is null)
        {
            return null;
        }

        return new FieldTemplate()
        {
            Id = (ushort) Id,
            Name = Name,
            AllowableTypes = [..AllowableTypes],
            BoundMembers = BoundMembers?.ToArray(),
            AllowedObjects = AllowedObjects?.ToArray(),
            UseCompression = UseCompression ?? true
        };
    }

}