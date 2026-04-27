using JetBrains.Annotations;

namespace Jbot.IO;

/// <summary>
/// Specifies that a specific field or property must not be included when converting to a 
/// `DataObject`.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class TransientAttribute : Attribute;
