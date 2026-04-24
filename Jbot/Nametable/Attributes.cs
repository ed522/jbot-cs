using Jbot.Nametable.Parse;

namespace Jbot.Nametable;

internal enum NametableAttribute
{
    NO_CRC, NO_COMPRESSION
}
internal enum ObjectAttribute
{
    FORCE_SHORT_IDS, NO_COMPRESSION
}
internal enum FieldAttribute
{
    NO_COMPRESSION, NULLABLE
}

internal static class Attributes
{

    private static readonly Dictionary<string, NametableAttribute> nametableNames = new()
    {
        { "nocrc", NametableAttribute.NO_CRC },
        { "nocompress", NametableAttribute.NO_COMPRESSION }
    };
    private static readonly Dictionary<string, ObjectAttribute> objectNames = new()
    {
        { "short", ObjectAttribute.FORCE_SHORT_IDS },
        { "nocompress", ObjectAttribute.NO_COMPRESSION }
    };
    private static readonly Dictionary<string, FieldAttribute> fieldNames = new()
    {
        { "nocompress", FieldAttribute.NO_COMPRESSION },
        { "nullable", FieldAttribute.NULLABLE }
    };
    
    public static NametableAttribute ParseNametableAttribute(string str)
    {
        if (!nametableNames.TryGetValue(str, out NametableAttribute value))
            throw new InvalidDocumentException("unknown nametable attribute " + str);
        return value;
    }
    public static ObjectAttribute ParseObjectAttribute(string str)
    {
        if (!objectNames.TryGetValue(str, out ObjectAttribute value))
            throw new InvalidDocumentException("unknown object attribute " + str);
        return value;
    }
    public static FieldAttribute ParseFieldAttribute(string str)
    {
        if (!fieldNames.TryGetValue(str, out FieldAttribute value))
            throw new InvalidDocumentException("unknown field attribute " + str);
        return value;
    }

    public static string GetName(NametableAttribute attrib)
    {
        foreach (KeyValuePair<string, NametableAttribute> pair in nametableNames)
        {
            if (pair.Value == attrib)
            {
                return pair.Key;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(attrib));
    }
    public static string GetName(ObjectAttribute attrib)
    {
        foreach (KeyValuePair<string, ObjectAttribute> pair in objectNames)
        {
            if (pair.Value == attrib)
            {
                return pair.Key;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(attrib));
    }
    public static string GetName(FieldAttribute attrib)
    {
        foreach (KeyValuePair<string, FieldAttribute> pair in fieldNames)
        {
            if (pair.Value == attrib)
            {
                return pair.Key;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(attrib));
    }

}
