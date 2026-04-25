namespace Jbot.Nametable;

public class Nametable
{
    public IList<ObjectTemplate> Objects { get; private set; }
    public uint Version { get; private set; }
    public bool UsesShortIds { get; private set; }
    public bool AllowsCompression { get; private set; }
    public bool UsesCRC { get; private set; }

    internal Nametable(ObjectTemplate[] objects, uint version, bool allowsCompression, bool usesCRC)
    {
        Objects = ((ObjectTemplate[])[..objects]).AsReadOnly();
        UsesShortIds = !objects.Any(o => o.Id > byte.MaxValue);
        Version = version;
        AllowsCompression = allowsCompression;
        UsesCRC = usesCRC;
    }

    public Nametable(uint version, ObjectTemplate[] objects)
    {
        Version = version;
        UsesShortIds = !objects.Any(o => o.Id > byte.MaxValue);
        Objects = ((ObjectTemplate[])[..objects]).AsReadOnly();
    }

    public ObjectTemplate? GetObjectOrNull(ushort id)
    {
        foreach (ObjectTemplate obj in Objects)
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
        foreach (ObjectTemplate obj in Objects)
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
        return $"[version={Version}, usesShortIds={UsesShortIds}, objects={Objects}]";
    }
}
