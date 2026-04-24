namespace Jbot.Nametable.Parse;

internal class ObjectTemplateBuilder
{
    
    public ushort? Id { get; set; }
    public bool? ForcesShortIds { get; set; }
    public string? Name { get; set; } // nullable
    public IList<string>? BoundTypeNames { get; set; }
    public IList<FieldTemplate>? Fields { get; set; }
    public bool? UseCompression { get; internal set; }

    public void Check()
    {
        if (Id is null)
        {
            throw new InvalidDocumentException("object is missing id");
        }
        else if (Fields is null || Fields.Count == 0)
        {
            throw new InvalidDocumentException("object is missing fields");
        }
    }

    public ObjectTemplate? Build()
    {
        if (Id is null || Fields is null)
        {
            return null;
        }

        if (Fields.Any(f => f.Id > byte.MaxValue) && (ForcesShortIds ?? false))
        {
            throw new InvalidDocumentException("cannot use short IDs if a field has an ID over 255");
        }

        return new ObjectTemplate((ushort) Id, Name, BoundTypeNames?.ToArray(), [..Fields], UseCompression ?? true);
    }

}