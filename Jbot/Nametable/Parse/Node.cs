namespace Jbot.Nametable.Parse;

internal enum NodeType
{
    ROOT,
    TOP_LEVEL_ATTRIBUTE_SET, TOP_LEVEL_ATTRIBUTE, VERSION,
    OBJECT, OBJECT_BIND, OBJECT_ATTRIBUTE_SET,
    OBJECT_BIND_TARGET, OBJECT_ATTRIBUTE, OBJECT_ID, OBJECT_NAME, 
    FIELD, FIELD_BIND, FIELD_ALLOWS, FIELD_ATTRIBUTE_SET, FIELD_TYPE,
    FIELD_ATTRIBUTE, FIELD_ID, FIELD_BIND_TARGET, FIELD_ALLOWED_OBJECT, FIELD_NAME
}

internal class Node(NodeType type, string value, Node[] children)
{
    public readonly NodeType type = type;
    public readonly string value = value;
    public List<Node> Children { get; init; } = [.. children];
}