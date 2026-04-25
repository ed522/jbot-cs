namespace Jbot.Nametable.Parse;

internal enum NametableAttribute
{
    NO_CHECKSUM, CHECKSUM, NO_COMPRESSION,
}

internal enum ObjectAttribute
{
    FORCE_SHORT_IDS, NO_COMPRESSION,
}

internal enum FieldAttribute
{
    NO_COMPRESSION, NULLABLE,
}

internal static class Attributes
{
    private static readonly Dictionary<string, NametableAttribute> nametableNames = new()
    {
        { "nochecksum", NametableAttribute.NO_CHECKSUM },
        { "checksum", NametableAttribute.CHECKSUM },
        { "nocompress", NametableAttribute.NO_COMPRESSION },
    };

    private static readonly Dictionary<string, ObjectAttribute> objectNames = new()
    {
        { "short", ObjectAttribute.FORCE_SHORT_IDS },
        { "nocompress", ObjectAttribute.NO_COMPRESSION },
    };

    private static readonly Dictionary<string, FieldAttribute> fieldNames = new()
    {
        { "nocompress", FieldAttribute.NO_COMPRESSION },
        { "nullable", FieldAttribute.NULLABLE },
    };

    public static NametableAttribute ParseNametableAttribute(string str)
    {
        if (!nametableNames.TryGetValue(str, out NametableAttribute value))
        {
            throw new InvalidDocumentException("unknown nametable attribute " + str);
        }

        return value;
    }

    public static ObjectAttribute ParseObjectAttribute(string str)
    {
        if (!objectNames.TryGetValue(str, out ObjectAttribute value))
        {
            throw new InvalidDocumentException("unknown object attribute " + str);
        }

        return value;
    }

    public static FieldAttribute ParseFieldAttribute(string str)
    {
        if (!fieldNames.TryGetValue(str, out FieldAttribute value))
        {
            throw new InvalidDocumentException("unknown field attribute " + str);
        }

        return value;
    }

    public static string GetName(NametableAttribute attrib)
    {
        string? val = (
            from pair in nametableNames
            where pair.Value == attrib
            select pair.Key
        ).FirstOrDefault();

        if (val is not null) return val;
        throw new ArgumentOutOfRangeException(nameof(attrib));
    }

    public static string GetName(ObjectAttribute attrib)
    {
        string? val = (
            from pair in objectNames
            where pair.Value == attrib
            select pair.Key
        ).FirstOrDefault();

        if (val is not null) return val;
        throw new ArgumentOutOfRangeException(nameof(attrib));
    }

    public static string GetName(FieldAttribute attrib)
    {
        string? val = (
            from pair in fieldNames
            where pair.Value == attrib
            select pair.Key
        ).FirstOrDefault();

        if (val is not null) return val;
        throw new ArgumentOutOfRangeException(nameof(attrib));
    }
}
