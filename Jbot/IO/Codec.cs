using Jbot.Data;
using Jbot.Model;

using JetBrains.Annotations;

namespace Jbot.IO;

[PublicAPI]
public class Codec(Nametable nametable)
{
    public readonly Nametable nametable = nametable;

    public static DataObject Serialize(object? obj) { }
}
