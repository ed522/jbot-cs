using System.Numerics;

using Jbot.Utils;

namespace Jbot.Data;

public sealed class DataValue : AbstractDataValue
{
    private DataValue(DataType type, object? value)
    {
        this.Type = type;
        this.Value = value;
    }

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
    public DataValue(ScaledDecimal value) => this.Set(value);
    public DataValue(BigInteger value) => this.Set(value);

    public override DataType Type { get; protected set; }

    public override DataValue Clone()
    {
        if (this.Value is ICloneable c)
        {
            return new DataValue(this.Type, c.Clone());
        }

        return new DataValue(this.Type, this.Value);
    }
}
