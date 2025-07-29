using System.Numerics;

namespace Jbot;

public enum DataType
{
    NULL, OBJECT, ARRAY, BINARY, STRING, BOOLEAN,
    BYTE, SHORT, INT, LONG, UBYTE, UINT, USHORT, ULONG,
    FLOAT, DOUBLE, DECIMAL, BIGINT
}

public class DataValue
{
    private const string OBJECT_WRONG_TYPE_MESSAGE = "Object is of the wrong type";

    protected object? Value { get; set; }
    public virtual DataType Type { get; protected set; }

    // extensible public interface
    protected virtual void SetType(DataType type)
    {
        this.Type = type;
    }

    // immutable public interface
    public bool IsOfType(DataType type)
    {
        return Type.Equals(type);
    }

    // public interface (repetitive type-specific definitions)
    public DataValue()
    {
        this.SetNull();
    }
    public DataValue(DataObject value)
    {
        this.Set(value);
    }
    public DataValue(DataArray value)
    {
        this.Set(value);
    }
    public DataValue(byte[] value)
    {
        this.Set(value);
    }
    public DataValue(string value)
    {
        this.Set(value);
    }
    public DataValue(bool value)
    {
        this.Set(value);
    }
    public DataValue(sbyte value)
    {
        this.Set(value);
    }
    public DataValue(short value)
    {
        this.Set(value);
    }
    public DataValue(int value)
    {
        this.Set(value);
    }
    public DataValue(long value)
    {
        this.Set(value);
    }
    public DataValue(byte value)
    {
        this.Set(value);
    }
    public DataValue(ushort value)
    {
        this.Set(value);
    }
    public DataValue(uint value)
    {
        this.Set(value);
    }
    public DataValue(ulong value)
    {
        this.Set(value);
    }
    public DataValue(float value)
    {
        this.Set(value);
    }
    public DataValue(double value)
    {
        this.Set(value);
    }
    public DataValue(decimal value)
    {
        this.Set(value);
    }
    public DataValue(BigInteger value)
    {
        this.Set(value);
    }

    public void SetNull()
    {
        this.Value = null;
        this.Type = DataType.NULL;
    }
    public void Set(DataObject value)
    {
        this.Value = value;
        this.Type = DataType.OBJECT;
    }
    public void Set(DataArray value)
    {
        this.Value = value;
        this.Type = DataType.ARRAY;
    }
    public void Set(byte[] value)
    {
        this.Value = value;
        this.Type = DataType.BINARY;
    }
    public void Set(string value)
    {
        this.Value = value;
        this.Type = DataType.STRING;
    }
    public void Set(bool value)
    {
        this.Value = value;
        this.Type = DataType.BOOLEAN;
    }
    public void Set(sbyte value)
    {
        this.Value = value;
        this.Type = DataType.BYTE;
    }
    public void Set(short value)
    {
        this.Value = value;
        this.Type = DataType.SHORT;
    }
    public void Set(int value)
    {
        this.Value = value;
        this.Type = DataType.INT;
    }
    public void Set(long value)
    {
        this.Value = value;
        this.Type = DataType.LONG;
    }
    public void Set(byte value)
    {
        this.Value = value;
        this.Type = DataType.UBYTE;
    }
    public void Set(ushort value)
    {
        this.Value = value;
        this.Type = DataType.USHORT;
    }
    public void Set(uint value)
    {
        this.Value = value;
        this.Type = DataType.UINT;
    }
    public void Set(ulong value)
    {
        this.Value = value;
        this.Type = DataType.ULONG;
    }
    public void Set(float value)
    {
        this.Value = value;
        this.Type = DataType.FLOAT;
    }
    public void Set(double value)
    {
        this.Value = value;
        this.Type = DataType.DOUBLE;
    }
    public void Set(decimal value)
    {
        this.Value = value;
        this.Type = DataType.DECIMAL;
    }
    public void Set(BigInteger value)
    {
        this.Value = value;
        this.Type = DataType.BIGINT;
    }

    public bool IsNull()
    {
        return this.Value == null;
    }

    public DataObject GetObject()
    {
        return this.GetAsObject() ?? throw new InvalidOperationException(OBJECT_WRONG_TYPE_MESSAGE);
    }
    public DataArray GetArray()
    {
        return this.GetAsArray() ?? throw new InvalidOperationException(OBJECT_WRONG_TYPE_MESSAGE);
    }
    public byte[] GetBinary()
    {
        return this.GetAsBinary() ?? throw new InvalidOperationException(OBJECT_WRONG_TYPE_MESSAGE);
    }
    public string GetString()
    {
        return this.GetAsString() ?? throw new InvalidOperationException(OBJECT_WRONG_TYPE_MESSAGE);
    }
    public bool GetBoolean()
    {
        return this.GetAsBoolean() ?? throw new InvalidOperationException(OBJECT_WRONG_TYPE_MESSAGE);
    }
    public sbyte GetByte()
    {
        return this.GetAsByte() ?? throw new InvalidOperationException(OBJECT_WRONG_TYPE_MESSAGE);
    }
    public short GetShort()
    {
        return this.GetAsShort() ?? throw new InvalidOperationException(OBJECT_WRONG_TYPE_MESSAGE);
    }
    public int GetInt()
    {
        return this.GetAsInt() ?? throw new InvalidOperationException(OBJECT_WRONG_TYPE_MESSAGE);
    }
    public long GetLong()
    {
        return this.GetAsLong() ?? throw new InvalidOperationException(OBJECT_WRONG_TYPE_MESSAGE);
    }
    public byte GetUByte()
    {
        return this.GetAsUByte() ?? throw new InvalidOperationException(OBJECT_WRONG_TYPE_MESSAGE);
    }
    public ushort GetUShort()
    {
        return this.GetAsUShort() ?? throw new InvalidOperationException(OBJECT_WRONG_TYPE_MESSAGE);
    }
    public uint GetUInt()
    {
        return this.GetAsUInt() ?? throw new InvalidOperationException(OBJECT_WRONG_TYPE_MESSAGE);
    }
    public ulong GetULong()
    {
        return this.GetAsULong() ?? throw new InvalidOperationException(OBJECT_WRONG_TYPE_MESSAGE);
    }
    public float GetFloat()
    {
        return this.GetAsFloat() ?? throw new InvalidOperationException(OBJECT_WRONG_TYPE_MESSAGE);
    }
    public double GetDouble()
    {
        return this.GetAsDouble() ?? throw new InvalidOperationException(OBJECT_WRONG_TYPE_MESSAGE);
    }
    public decimal GetDecimal()
    {
        return this.GetAsDecimal() ?? throw new InvalidOperationException(OBJECT_WRONG_TYPE_MESSAGE);
    }
    public BigInteger GetBigInteger()
    {
        return this.GetAsBigInteger() ?? throw new InvalidOperationException(OBJECT_WRONG_TYPE_MESSAGE);
    }
    
    public DataObject? GetAsObject()
    {
        return this.Value as DataObject;
    }
    public DataArray? GetAsArray()
    {
        return this.Value as DataArray;
    }
    public byte[]? GetAsBinary()
    {
        return this.Value as byte[];
    }
    public string? GetAsString()
    {
        return this.Value as string;
    }
    public bool? GetAsBoolean()
    {
        return this.Value as bool?;
    }
    public sbyte? GetAsByte()
    {
        return this.Value as sbyte?;
    }
    public short? GetAsShort()
    {
        return this.Value as short?;
    }
    public int? GetAsInt()
    {
        return this.Value as int?;
    }
    public long? GetAsLong()
    {
        return this.Value as long?;
    }
    public byte? GetAsUByte()
    {
        return this.Value as byte?;
    }
    public ushort? GetAsUShort()
    {
        return this.Value as ushort?;
    }
    public uint? GetAsUInt()
    {
        return this.Value as uint?;
    }
    public ulong? GetAsULong()
    {
        return this.Value as ulong?;
    }
    public float? GetAsFloat()
    {
        return this.Value as float?;
    }
    public double? GetAsDouble()
    {
        return this.Value as double?;
    }
    public decimal? GetAsDecimal()
    {
        return this.Value as decimal?;
    }
    public BigInteger? GetAsBigInteger()
    {
        return this.Value as BigInteger?;
    }

}