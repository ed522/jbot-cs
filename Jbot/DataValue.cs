using System.Numerics;

namespace Jbot;

public class DataValue
{
    protected object? Value { get; set; }
    public virtual DataType Type { get; protected set; }

    public bool IsOfType(DataType type) => Type.Equals(type);

    // lots of repetitive definitions
    public DataValue() => this.SetNull();
    public DataValue(DataObject value) => this.Set(value);
    public DataValue(DataArray value) => this.Set(value);
    public DataValue(byte[] value) => this.Set(value);
    public DataValue(string value) => this.Set(value);
    public DataValue(bool value) => this.Set(value);
    public DataValue(sbyte value) => this.Set(value);
    public DataValue(short value) => this.Set(value);
    public DataValue(int value) => this.Set(value);
    public DataValue(long value) => this.Set(value);
    public DataValue(byte value) => this.Set(value);
    public DataValue(ushort value) => this.Set(value);
    public DataValue(uint value) => this.Set(value);
    public DataValue(ulong value) => this.Set(value);
    public DataValue(float value) => this.Set(value);
    public DataValue(double value) => this.Set(value);
    public DataValue(decimal value) => this.Set(value);
    public DataValue(BigInteger value) => this.Set(value);

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

    public bool IsNull() => this.Value == null;

    public DataObject? GetObject() => (DataObject?)this.Value;
    public DataArray? GetArray() => (DataArray?)this.Value;
    public byte[]? GetBinary() => (byte[]?)this.Value;
    public string? GetString() => (string?)this.Value;
    public bool? GetBoolean() => (bool?)this.Value;
    public sbyte? GetByte() => (sbyte?)this.Value;
    public short? GetShort() => (short?)this.Value;
    public int? GetInt() => (int?)this.Value;
    public long? GetLong() => (long?)this.Value;
    public byte? GetUByte() => (byte?)this.Value;
    public ushort? GetUShort() => (ushort?)this.Value;
    public uint? GetUInt() => (uint?)this.Value;
    public ulong? GetULong() => (ulong?)this.Value;
    public float? GetFloat() => (float?)this.Value;
    public double? GetDouble() => (double?)this.Value;
    public decimal? GetDecimal() => (decimal?)this.Value;
    public BigInteger? GetBigInteger() => (BigInteger?)this.Value;

    public DataObject? GetAsObject() => this.Value as DataObject;
    public DataArray? GetAsArray() => this.Value as DataArray;
    public byte[]? GetAsBinary() => this.Value as byte[];
    public string? GetAsString() => this.Value as string;
    public bool? GetAsBoolean() => this.Value as bool?;
    public sbyte? GetAsByte() => this.Value as sbyte?;
    public short? GetAsShort() => this.Value as short?;
    public int? GetAsInt() => this.Value as int?;
    public long? GetAsLong() => this.Value as long?;
    public byte? GetAsUByte() => this.Value as byte?;
    public ushort? GetAsUShort() => this.Value as ushort?;
    public uint? GetAsUInt() => this.Value as uint?;
    public ulong? GetAsULong() => this.Value as ulong?;
    public float? GetAsFloat() => this.Value as float?;
    public double? GetAsDouble() => this.Value as double?;
    public decimal? GetAsDecimal() => this.Value as decimal?;
    public BigInteger? GetAsBigInteger() => this.Value as BigInteger?;

    public override string ToString()
    {
        return $"{this.Type}: {this.Value}";
    }

}