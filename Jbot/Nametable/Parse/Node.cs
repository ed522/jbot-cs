using System.Collections.Immutable;

namespace Jbot.Nametable.Parse;

internal enum NodeType
{
    ROOT,
    TOP_LEVEL_ATTRIBUTE_SET, TOP_LEVEL_ATTRIBUTE, VERSION,

    OBJECT, OBJECT_BIND, OBJECT_ATTRIBUTE_SET,
    OBJECT_BIND_TARGET, OBJECT_ATTRIBUTE, OBJECT_ID, OBJECT_NAME,

    FIELD, FIELD_BIND, FIELD_ALLOWS, FIELD_ATTRIBUTE_SET, FIELD_TYPE_SET, FIELD_TYPE,
    FIELD_ATTRIBUTE, FIELD_ID, FIELD_BIND_TARGET, FIELD_ALLOWED_OBJECT, FIELD_NAME,
}

internal class Node(NodeType type, string value, ICollection<Node> children)
{
    public Node(NodeType type) : this(type, "", []) { }
    public Node(NodeType type, string value) : this(type, value, []) { }
    public Node(NodeType type, ICollection<Node> children) : this(type, "", children) { }
    public NodeType Type { get; init; } = type;
    public string Value { get; init; } = value;
    public ImmutableHashSet<Node> Children { get; init; } = [..children];

    public static bool operator ==(Node? left, Node? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Node? left, Node? right) => !(left == right);

    public override bool Equals(object? other)
    {
        return other is Node n
            && n.Type == this.Type
            && n.Value == this.Value
            && n.Children.SetEquals(this.Children);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(this.Type);
        hash.Add(this.Value);

        int childrenHash = 0;

        foreach (var child in this.Children)
            childrenHash ^= child.GetHashCode(); // XOR is order-independent

        hash.Add(childrenHash);
        return hash.ToHashCode();
    }

    public override string ToString()
    {
        return
            $"type={this.Type}, value={this.Value}, children=[{string.Join(", ", this.Children)}]";
    }
}
