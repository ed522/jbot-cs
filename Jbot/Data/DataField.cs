using System.Numerics;

using Jbot.Nametable;
using Jbot.Utils;

using JetBrains.Annotations;

namespace Jbot.Data;

[PublicAPI]
public sealed class DataField : AbstractDataValue
{
    private DataType _type;
    public FieldTemplate Template { get; }

    public override DataType Type
    {
        get => this._type;
        protected set
        {
            if (value == DataType.UNINITIALIZED)
            {
                throw new ArgumentException("Cannot set a field to be uninitialized after " +
                                            "initialization");
            }

            if (!this.Template.AllowsType(value))
            {
                throw new ArgumentException($"The type {value} is not allowed for this field");
            }

            this._type = value;
        }
    }

    private DataField(FieldTemplate template, DataType type, object? value)
    {
        this.Template = template;
        this.Type = type;
        this.Value = value;
    }

    #region Helper constructors (with predefined value)

    // does not initialize with any value/type
    private DataField(FieldTemplate template) => this.Template = template;

    public DataField(FieldTemplate template, DataObject value) : this(template) => this.Set(value);
    public DataField(FieldTemplate template, DataArray value) : this(template) => this.Set(value);
    public DataField(FieldTemplate template, byte[] value) : this(template) => this.Set(value);
    public DataField(FieldTemplate template, string value) : this(template) => this.Set(value);
    public DataField(FieldTemplate template, bool value) : this(template) => this.Set(value);
    public DataField(FieldTemplate template, sbyte value) : this(template) => this.Set(value);
    public DataField(FieldTemplate template, short value) : this(template) => this.Set(value);
    public DataField(FieldTemplate template, int value) : this(template) => this.Set(value);
    public DataField(FieldTemplate template, long value) : this(template) => this.Set(value);
    public DataField(FieldTemplate template, byte value) : this(template) => this.Set(value);
    public DataField(FieldTemplate template, ushort value) : this(template) => this.Set(value);
    public DataField(FieldTemplate template, uint value) : this(template) => this.Set(value);
    public DataField(FieldTemplate template, ulong value) : this(template) => this.Set(value);
    public DataField(FieldTemplate template, float value) : this(template) => this.Set(value);
    public DataField(FieldTemplate template, double value) : this(template) => this.Set(value);

    public DataField(FieldTemplate template, ScaledDecimal value) : this(template) =>
        this.Set(value);

    public DataField(FieldTemplate template, BigInteger value) : this(template) => this.Set(value);

    #endregion

    public override DataField Clone()
    {
        if (this.Value is ICloneable o)
            return new DataField(this.Template, this.Type, o.Clone());

        return new DataField(this.Template, this.Type, this.Value);
    }
}
