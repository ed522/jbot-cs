using Jbot.Data;
using Jbot.Model;

using JetBrains.Annotations;

namespace Jbot.IO;

[PublicAPI]
public class Serializer(Nametable nametable)
{
    private const int BUFFER_INITIAL_CAPACITY_B = 1024;
    
    public readonly Nametable nametable = nametable;
    
    public Span<byte> Serialize(object obj, Marshaller marshal)
    {
        return this.Serialize(marshal.Marshal(obj));
    }
    public T Deserialize<T>(Span<byte> data, Marshaller marshal)
    {
        return marshal.Unmarshal<T>(this.Deserialize(data));
    }
    
    // object:
    // byte/short: object ID (depends on short ID flag in nametable)
    // byte/short: field count (depends on object short ID flag)
    // n fields: field data
    
    // field:
    // byte: tag
    // some bytes: value (depends on type)
    
    // plain number value: encode directly
    
    // string/binary:
    // varint: length
    // n bytes: value
    
    // object:
    // varint: length
    // n bytes: data (serialized in this format)
    
    // array:
    // varint: entry count
    // n entries: 
    
    public Span<byte> Serialize(DataObject obj)
    {
        MemoryStream stream = new(BUFFER_INITIAL_CAPACITY_B);
        
    }

    private void SerializeField(AbstractDataValue value, MemoryStream stream)
    {
        
    }

    public DataObject Deserialize(Span<byte> data)
    {
        
    }
}
