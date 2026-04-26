using System.Numerics;

using Jbot.Nametable;
using Jbot.Utils;

using JetBrains.Annotations;

namespace Jbot.Data;

[PublicAPI]
public sealed class DataField : AbstractDataValue
{
    public FieldTemplate Template { get; }

    private DataType _type = DataType.UNINITIALIZED;
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

    // NOTE: value is not validated to be of the right type! be careful when using this constructor
    private DataField(FieldTemplate template, DataType type, object? value)
    {
        this.Template = template;
        this.Type = type;
        this.Value = value;
    }

    // does not initialize with any value/type
    private DataField(FieldTemplate template) => this.Template = template;
    /// <summary>
    /// Create a new DataField that is uninitialized. The type will be set to `UNINITIALIZED` and
    /// any attempt to access the value will return `null`. <br />
    /// This is not a constructor because uninitialized values should only be created in specific
    /// circumstances.
    /// </summary>
    /// <param name="template"></param>
    /// <returns></returns>
    internal static DataField ofUninitialized(FieldTemplate template) => new(template);

    #region Helper constructors (with predefined value)

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

    public void Validate()
    {
        // bad inspection here
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (this.Type == DataType.UNINITIALIZED)
        {
            throw new InvalidOperationException($"Field '{this.Template.Name}' is uninitialized");
        }

        if (this.Type == DataType.OBJECT && this.Value is DataObject dataObject)
        {
            if (this.Template.AllowedObjects is { Count: >0 } && 
                !this.Template.AllowedObjects.Contains(dataObject.Template.Name))
            {
                throw new InvalidOperationException(
                    $"Field '{this.Template.Name}' has object '{dataObject.Template.Name}', " + 
                    $"which must be (but is not) one of " +
                    $"[{string.Join(", ", this.Template.AllowedObjects)}]");
            }
        }
    }
}
