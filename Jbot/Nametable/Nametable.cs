namespace Jbot.Nametable;

public class Nametable
{
    
    public IList<ObjectTemplate> Objects { get; private set; }
    public uint Version { get; private set; }
    public bool UsesShortIds { get; private set; }
    
    public Nametable(uint version, ObjectTemplate[] objects)
    {
        Version = version;
        UsesShortIds = !objects.Any(o => o.Id > byte.MaxValue); 
        
        ObjectTemplate[] clonedObjects = [..objects];
        Objects = clonedObjects.AsReadOnly();
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
