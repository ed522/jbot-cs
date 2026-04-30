using System.Numerics;

using Jbot.Utils;

using JetBrains.Annotations;

namespace Jbot.Data;

/// <summary>
///     Represents a data value, i.e. a pair consisting of a value of any valid type, and a type.
///     User code cannot extend this class.
/// </summary>
[PublicAPI]
public abstract class AbstractDataValue : ICloneable
{
    private object? _value;

    /// <summary>
    ///     Create a DataValue without any initialization. Neither the data nor the type are set.
    ///     NOTE: Make sure to call Set() after construction to intialize the value.
    /// </summary>
    private protected AbstractDataValue()
    {
        // does not perform initialization
    }

    protected object? Value
    {
        get
        {
            if (this.Type == DataType.UNINITIALIZED)
            {
                return null;
            }

            return this._value;
        }
        set => this._value = value;
    }

    public abstract DataType Type { get; protected set; }
    public abstract object Clone();

    public bool IsOfType(DataType type) => this.Type == type;
    public bool IsNull() => this.Value is null;

    public override string ToString() => $"{this.Type}: {this.Value}";

    internal object? GetRawValue() => this.Value;

    #region Setters

    public void SetNull()
    {
        this.Type = DataType.NULL;
        this.Value = null;
    }

    public void Set(DataObject value)
    {
        this.Type = DataType.OBJECT;
        this.Value = value;
    }

    public void Set(DataArray value)
    {
        this.Type = DataType.ARRAY;
        this.Value = value;
    }

    public void Set(byte[] value)
    {
        this.Type = DataType.BINARY;
        this.Value = value;
    }

    public void Set(string value)
    {
        this.Type = DataType.STRING;
        this.Value = value;
    }

    public void Set(bool value)
    {
        this.Type = DataType.BOOLEAN;
        this.Value = value;
    }

    public void Set(sbyte value)
    {
        this.Type = DataType.BYTE;
        this.Value = value;
    }

    public void Set(short value)
    {
        this.Type = DataType.SHORT;
        this.Value = value;
    }

    public void Set(int value)
    {
        this.Type = DataType.INT;
        this.Value = value;
    }

    public void Set(long value)
    {
        this.Type = DataType.LONG;
        this.Value = value;
    }

    public void Set(byte value)
    {
        this.Type = DataType.UBYTE;
        this.Value = value;
    }

    public void Set(ushort value)
    {
        this.Type = DataType.USHORT;
        this.Value = value;
    }

    public void Set(uint value)
    {
        this.Type = DataType.UINT;
        this.Value = value;
    }

    public void Set(ulong value)
    {
        this.Type = DataType.ULONG;
        this.Value = value;
    }

    public void Set(float value)
    {
        this.Type = DataType.FLOAT;
        this.Value = value;
    }

    public void Set(double value)
    {
        this.Type = DataType.DOUBLE;
        this.Value = value;
    }

    public void Set(ScaledDecimal value)
    {
        this.Type = DataType.DECIMAL;
        this.Value = value;
    }

    public void Set(BigInteger value)
    {
        this.Type = DataType.BIGINT;
        this.Value = value;
    }

    #endregion

    #region Explicit accessors

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
    public ScaledDecimal? GetDecimal() => (ScaledDecimal?)this.Value;
    public BigInteger? GetBigInteger() => (BigInteger?)this.Value;

    #endregion

    #region Converting accessors

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
    public ScaledDecimal? GetAsDecimal() => this.Value as ScaledDecimal?;
    public BigInteger? GetAsBigInteger() => this.Value as BigInteger?;

    #endregion
}
