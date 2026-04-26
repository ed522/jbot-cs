namespace Jbot.Model.Parse;

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
        if (this.Id is null)
        {
            throw new InvalidDocumentException("object is missing id");
        }

        if (this.Fields is null || this.Fields.Count == 0)
        {
            throw new InvalidDocumentException("object is missing fields");
        }

        if (this.Name is null)
        {
            throw new InvalidDocumentException("object is missing name");
        }
    }

    public ObjectTemplate? Build()
    {
        if (this.Id is null || this.Fields is null || this.Name is null)
        {
            return null;
        }

        if ((this.ForcesShortIds ?? false) && this.Fields.Any(f => f.Id > byte.MaxValue))
        {
            throw new InvalidDocumentException(
                "cannot use short IDs if a field has an ID over 255");
        }

        this.UseCompression ??= true;
        this.ForcesShortIds ??= false;

        if ((bool)this.ForcesShortIds)
        {
            return new ObjectTemplate
            {
                Id = (ushort)this.Id,
                Name = this.Name,
                BoundTypeNames = this.BoundTypeNames?.AsReadOnly(),
                Fields = this.Fields.AsReadOnly(),
                UseCompression = this.UseCompression ?? true,
                UsesShortIds = this.ForcesShortIds ?? false,
            };
        }

        return new ObjectTemplate((ushort)this.Id, this.Name, this.BoundTypeNames?.AsReadOnly(),
            this.Fields.AsReadOnly(), (bool)this.UseCompression);
    }
}
