using System.Numerics;

namespace Jbot.Utils;

public readonly struct ScaledDecimal
{
    public BigInteger UnscaledValue { get; }
    public int Scale { get; }

    public ScaledDecimal(BigInteger unscaledValue, int scale)
    {
        this.UnscaledValue = unscaledValue;
        this.Scale = scale;
    }

    public ScaledDecimal(decimal value)
    {
        int[] bits = decimal.GetBits(value);

        BigInteger unscaled = (new BigInteger((uint)bits[2]) << 64) |
                              (new BigInteger((uint)bits[1]) << 32) |
                              new BigInteger((uint)bits[0]);

        if ((bits[3] & 0x80000000) != 0)
        {
            unscaled = -unscaled;
        }

        this.UnscaledValue = unscaled;
        this.Scale = (bits[3] >> 16) & 0xFF;
    }
}
