namespace Jbot.Nametable;

public class Nametable
{
    public IList<ObjectTemplate> Objects { get; }
    public uint Version { get; }
    public bool UsesShortIds { get; }
    public bool AllowsCompression { get; }

    /// <summary>
    /// Add a checksum to messages that were serialized using this table.
    /// </summary>
    public bool UsesChecksum { get; }

    internal Nametable(
        ObjectTemplate[] objects, uint version, bool allowsCompression, bool usesChecksum
    ) :
        this(objects, version, allowsCompression, usesChecksum,
            !objects.Any(o => o.Id > byte.MaxValue)) { }

    internal Nametable(
        ObjectTemplate[] objects, uint version, bool allowsCompression, bool usesChecksum,
        bool usesShortIds
    )
    {
        this.Objects = ((ObjectTemplate[])[..objects]).AsReadOnly();
        this.UsesShortIds = usesShortIds;
        this.Version = version;
        this.AllowsCompression = allowsCompression;
        this.UsesChecksum = usesChecksum;
    }

    public Nametable(uint version, ObjectTemplate[] objects)
    {
        this.Version = version;
        this.UsesShortIds = !objects.Any(o => o.Id > byte.MaxValue);
        this.Objects = ((ObjectTemplate[])[..objects]).AsReadOnly();
    }

    public ObjectTemplate? GetObjectOrNull(ushort id)
    {
        foreach (ObjectTemplate obj in this.Objects)
        {
            if (obj.Id == id)
            {
                return obj;
            }
        }

        return null;
    }

    public ObjectTemplate? GetObjectOrNull(string name)
    {
        foreach (ObjectTemplate obj in this.Objects)
        {
            if (obj.Name == name)
            {
                return obj;
            }
        }

        return null;
    }

    public override string ToString()
    {
        return
            $"[version={this.Version}, usesShortIds={this.UsesShortIds}, objects={this.Objects}]";
    }
}
