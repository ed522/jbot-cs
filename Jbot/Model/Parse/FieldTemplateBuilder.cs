using Jbot.Data;
using Jbot.Utils;

namespace Jbot.Model.Parse;

internal class FieldTemplateBuilder
{
    public EnumSet<DataType>? AllowableTypes { get; set; }
    public List<string>? AllowedObjects { get; set; }
    public List<string>? BoundMembers { get; set; }
    public string? Name { get; set; }
    public ushort? Id { get; set; }
    public bool? UseCompression { get; internal set; }

    public void Check()
    {
        if (this.AllowableTypes is null)
        {
            throw new InvalidDocumentException("field is missing allowed types");
        }

        if (this.Name is null)
        {
            throw new InvalidDocumentException("field is missing name");
        }

        if (this.Id is null)
        {
            throw new InvalidDocumentException("field is missing id");
        }
    }

    public FieldTemplate? Build()
    {
        if (this.AllowableTypes is null || this.Name is null || this.Id is null)
        {
            return null;
        }

        return new FieldTemplate
        {
            Id = (ushort)this.Id,
            Name = this.Name,
            AllowableTypes = [..this.AllowableTypes],
            BoundMembers = this.BoundMembers?.ToArray(),
            AllowedObjects = this.AllowedObjects?.ToArray(),
            UseCompression = this.UseCompression ?? true,
        };
    }
}
