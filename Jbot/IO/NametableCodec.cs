using System.IO.Hashing;
using System.Text;

using Jbot.Data;
using Jbot.Nametable;
using Jbot.Utils;

using JetBrains.Annotations;

namespace Jbot.IO;

[PublicAPI]
public class NametableCodec
{
    private const int WRITE_BUFFER_SIZE_INITIAL = 8192;

    private const int NT_ALLOWS_COMPRESSION = 0x0000_0001;
    private const int NT_USES_SHORT_IDS = 0x0000_0002;
    private const uint NT_USES_CHECKSUM = 0x0000_0004;

    private const int OBJ_ALLOWS_COMPRESSION = 0x0000_0001;
    private const int OBJ_USES_SHORT_IDS = 0x0000_0002;

    private const int FIELD_ALLOWS_COMPRESSION = 0x0000_0001;
    private const byte OBJECT_START = 0xA0;
    private const byte OBJECT_END = 0xAF;
    private const byte FIELD_START = 0xB0;
    private const byte FIELD_END = 0xBF;

    private static ReadOnlySpan<byte> FILE_MAGIC => "JBNT\0\0\0\0"u8;

    // all values unsigned
    // format:
    // - long - magic number
    // - int - table version
    // - int - table flags
    // - short - object count
    // - [... objects]
    // - long - checksum (of everything before this)

    // object:
    // - byte - object start
    // - short - ID
    // - short - name length
    // - x bytes - name (UTF8)
    // - short - flags
    // - short - bound type count
    // - repeated:
    //   - short - bound type length
    //   - x bytes - bound type name (UTF8)
    // - short - field count
    // - [... fields]
    // - byte: object end

    // field:
    // - byte - field start
    // - short - ID
    // - short - name length
    // - x bytes - name (UTF8)
    // - short - flags
    // - int - allowed type bitmap
    // - short - bound member count
    // - repeated:
    //   - short - bound member name length
    //   - x bytes - bound member name
    // - short - allowed object count
    // - repeated:
    //   - short - allowed object name length
    //   - x bytes - allowed object name
    // - byte: field end

    private static uint MakeNametableFlags(Nametable.Nametable nametable) =>
        (nametable.UsesShortIds ? NT_USES_SHORT_IDS : 0u) |
        (nametable.AllowsCompression ? NT_ALLOWS_COMPRESSION : 0u) |
        (nametable.UsesChecksum ? NT_USES_CHECKSUM : 0u);

    private static ushort MakeObjectFlags(ObjectTemplate obj) =>
        (ushort)((obj.UseCompression ? OBJ_ALLOWS_COMPRESSION : 0) |
                 (obj.UsesShortIds ? OBJ_USES_SHORT_IDS : 0));

    private static ushort MakeFieldFlags(FieldTemplate field) =>
        (ushort)(field.UseCompression ? FIELD_ALLOWS_COMPRESSION : 0);

    private static uint MakeAllowedTypesBitmap(FieldTemplate field)
    {
        uint bitmap = 0;

        foreach (DataType type in field.AllowableTypes)
        {
            bitmap |= 1u << (int)type;
        }

        return bitmap;
    }

    private static void ParseNametableFlags(
        uint flags, out bool allowsCompression, out bool usesShortIds, out bool usesChecksum
    )
    {
        allowsCompression = (flags & NT_ALLOWS_COMPRESSION) != 0;
        usesShortIds = (flags & NT_USES_SHORT_IDS) != 0;
        usesChecksum = (flags & NT_USES_CHECKSUM) != 0;
    }

    private static void ParseObjectFlags(
        ushort flags, out bool useCompression, out bool usesShortIds
    )
    {
        useCompression = (flags & OBJ_ALLOWS_COMPRESSION) != 0;
        usesShortIds = (flags & OBJ_USES_SHORT_IDS) != 0;
    }

    private static void ParseFieldFlags(ushort flags, out bool useCompression)
    {
        useCompression = (flags & FIELD_ALLOWS_COMPRESSION) != 0;
    }

    private static DataType[] ParseAllowedTypesBitmap(uint bitmap)
    {
        List<DataType> types = [];
        // LINQ says: for all DataType values, select those that have their bit set in the bitmask
        types.AddRange(Enum.GetValues<DataType>().Where(type => (bitmap & (1u << (int)type)) != 0));

        return [.. types];
    }

    private static void Expect(byte actual, byte expected)
    {
        if (actual != expected)
        {
            throw new InvalidDataException(
                $"Malformed nametable: expected 0x{expected:X2}, got 0x{actual:X2}");
        }
    }

    private static string ReadString(BinaryReader reader)
    {
        ushort length = reader.ReadUInt16();
        byte[] bytes = reader.ReadBytes(length);
        return Encoding.UTF8.GetString(bytes);
    }

    private static void WriteField(FieldTemplate field, MemoryStream stream)
    {
        stream.WriteByte(FIELD_START);
        stream.Write(field.Id);
        stream.Write(field.Name);
        stream.Write(MakeFieldFlags(field));
        stream.Write(MakeAllowedTypesBitmap(field));

        checked
        {
            stream.Write((ushort)(field.BoundMembers?.Count ?? 0));
        }

        foreach (string name in field.BoundMembers ?? [])
        {
            stream.Write(name);
        }

        checked
        {
            stream.Write((ushort)(field.AllowedObjects?.Count ?? 0));
        }

        foreach (string name in field.AllowedObjects ?? [])
        {
            stream.Write(name);
        }

        stream.WriteByte(FIELD_END);
    }

    private static FieldTemplate ReadField(BinaryReader reader)
    {
        Expect(reader.ReadByte(), FIELD_START);
        ushort id = reader.ReadUInt16();
        string name = ReadString(reader);
        ushort flags = reader.ReadUInt16();
        ParseFieldFlags(flags, out bool useCompression);
        uint allowedTypesBitmap = reader.ReadUInt32();
        DataType[] allowedTypes = ParseAllowedTypesBitmap(allowedTypesBitmap);

        ushort boundMemberCount = reader.ReadUInt16();
        string[] boundMembers = new string[boundMemberCount];

        for (int i = 0; i < boundMemberCount; i++)
        {
            boundMembers[i] = ReadString(reader);
        }

        ushort allowedObjectCount = reader.ReadUInt16();
        string[] allowedObjects = new string[allowedObjectCount];

        for (int i = 0; i < allowedObjectCount; i++)
        {
            allowedObjects[i] = ReadString(reader);
        }

        Expect(reader.ReadByte(), FIELD_END);

        return new FieldTemplate
        {
            AllowableTypes = [..allowedTypes],
            Name = name,
            Id = id,
            UseCompression = useCompression,
            BoundMembers = boundMembers,
            AllowedObjects = allowedObjects,
        };
    }

    private static void WriteObject(ObjectTemplate obj, MemoryStream stream)
    {
        stream.WriteByte(OBJECT_START);
        stream.Write(obj.Id);
        stream.Write(obj.Name);
        stream.Write(MakeObjectFlags(obj));

        checked
        {
            stream.Write((ushort)(obj.BoundTypeNames?.Count ?? 0));
        }

        foreach (string name in obj.BoundTypeNames ?? [])
        {
            stream.Write(name);
        }

        checked
        {
            stream.Write((ushort)obj.Fields.Count);
        }

        foreach (FieldTemplate field in obj.Fields)
        {
            WriteField(field, stream);
        }

        stream.WriteByte(OBJECT_END);
    }

    private static ObjectTemplate ReadObject(BinaryReader reader)
    {
        Expect(reader.ReadByte(), OBJECT_START);
        ushort id = reader.ReadUInt16();
        string name = ReadString(reader);
        ushort flags = reader.ReadUInt16();
        ParseObjectFlags(flags, out bool useCompression, out bool usesShortIds);

        ushort boundTypeCount = reader.ReadUInt16();
        string[] boundTypeNames = new string[boundTypeCount];

        for (int i = 0; i < boundTypeCount; i++)
        {
            boundTypeNames[i] = ReadString(reader);
        }

        ushort fieldCount = reader.ReadUInt16();
        FieldTemplate[] fields = new FieldTemplate[fieldCount];

        for (int i = 0; i < fieldCount; i++)
        {
            fields[i] = ReadField(reader);
        }

        Expect(reader.ReadByte(), OBJECT_END);
        return new ObjectTemplate(id, name, boundTypeNames, fields, useCompression, usesShortIds);
    }

    public static byte[] Serialize(Nametable.Nametable nametable)
    {
        MemoryStream stream = new(WRITE_BUFFER_SIZE_INITIAL);

        stream.Write(FILE_MAGIC);

        // write nametable header
        stream.Write(nametable.Version);
        stream.Write(MakeNametableFlags(nametable));

        checked
        {
            stream.Write((ushort)nametable.Objects.Count);
        }

        foreach (ObjectTemplate obj in nametable.Objects)
        {
            WriteObject(obj, stream);
        }

        // read everything back for CRC
        stream.Position = 0;
        byte[] hash = XxHash3.Hash(stream.ToArray());
        stream.Seek(0, SeekOrigin.End);
        stream.Write(hash);

        return stream.ToArray();
    }

    public static Nametable.Nametable Deserialize(byte[] data)
    {
        if (data.Length < 8) // XxHash size
        {
            throw new InvalidDataException("Not enough data");
        }

        byte[] dataWithoutChecksum = data[..^8];
        byte[] expectedChecksum = data[^8..];
        byte[] actualChecksum = XxHash3.Hash(dataWithoutChecksum);

        if (!actualChecksum.AsSpan().SequenceEqual(expectedChecksum))
        {
            throw new InvalidDataException("Invalid checksum");
        }

        using MemoryStream stream = new(dataWithoutChecksum);
        using BinaryReader reader = new(stream);

        byte[] magic = reader.ReadBytes(8);

        // FILE_MAGIC is a read-only span
        if (!magic.AsSpan().SequenceEqual(FILE_MAGIC))
        {
            throw new InvalidDataException("Invalid magic number - wrong file type");
        }

        uint version = reader.ReadUInt32();
        uint flags = reader.ReadUInt32();

        ParseNametableFlags(flags, out bool allowsCompression, out bool usesShortIds,
            out bool usesChecksum);

        ushort objectCount = reader.ReadUInt16();
        ObjectTemplate[] objects = new ObjectTemplate[objectCount];

        for (int i = 0; i < objectCount; i++)
        {
            objects[i] = ReadObject(reader);
        }

        return new Nametable.Nametable(objects, version, allowsCompression, usesChecksum,
            usesShortIds);
    }

    public static void WriteFile(string file, Nametable.Nametable nametable)
    {
        File.WriteAllBytes(file, Serialize(nametable));
    }

    public static Nametable.Nametable ReadFile(string file) => Deserialize(File.ReadAllBytes(file));
}
