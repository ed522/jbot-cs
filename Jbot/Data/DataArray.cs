using JetBrains.Annotations;

namespace Jbot.Data;

[PublicAPI]
public class DataArray : List<AbstractDataValue>, ICloneable
{
    public DataArray(IEnumerable<AbstractDataValue> values) => this.AddRange(values);

    public object Clone() =>
        new DataArray(
            from value in this
            select (AbstractDataValue)value.Clone()
        );
}
