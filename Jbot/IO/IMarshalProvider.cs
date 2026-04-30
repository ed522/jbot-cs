using Jbot.Data;

using JetBrains.Annotations;

namespace Jbot.IO;

[PublicAPI]
public interface IMarshalProvider
{
    object Unmarshal(DataObject obj);
    DataObject Marshal(object obj);
}
