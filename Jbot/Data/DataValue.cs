using System.Numerics;

namespace Jbot.Data;

public class DataValue
{
    // disable cognitive complexity warnings because this code is in reality very simple
    // this mess is courtesy of the warning system not recognizing extensions
#pragma warning disable IDE0079
#pragma warning disable S3776
    public static DataType DetermineType(object data)
#pragma warning restore S3776, IDE0079
    {
        if (data is null) return DataType.NULL;
        else if (data is Array) return DataType.ARRAY;
        else if (data is byte[]) return DataType.BINARY;
        else if (data is string) return DataType.STRING;
        else if (data is bool) return DataType.BOOLEAN;
        else if (data is sbyte) return DataType.BYTE;
        else if (data is short) return DataType.SHORT;
        else if (data is int) return DataType.INT;
        else if (data is long) return DataType.LONG;
        else if (data is byte) return DataType.UBYTE;
        else if (data is ushort) return DataType.USHORT;
        else if (data is uint) return DataType.UINT;
        else if (data is ulong) return DataType.ULONG;
        else if (data is float) return DataType.FLOAT;
        else if (data is double) return DataType.DOUBLE;
        else if (data is decimal) return DataType.DECIMAL;
        else if (data is BigInteger) return DataType.BIGINT;
        else return DataType.OBJECT;
    }
}

public enum DataType
{
    NULL, OBJECT, ARRAY, BINARY, STRING, BOOLEAN,
    BYTE, SHORT, INT, LONG, UBYTE, UINT, USHORT, ULONG,
    FLOAT, DOUBLE, DECIMAL, BIGINT
}