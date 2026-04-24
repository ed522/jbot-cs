
using System.Runtime.CompilerServices;

namespace Jbot.Nametable.Parse;

internal static class Compiler
{

    private static void InvalidState(Node node)
    {
        throw new SyntaxException("did not expect token " + node.type + " here, bug in compiler?");
    }
    private static void ThrowIfSet(object? value, string name)
    {
        if (value != null)
        {
            throw new InvalidDocumentException("already set property " + name);
        }
    }

    private static void ThrowIfContains<T>(ICollection<T> list, T value, string listName)
    {
        if (list.Contains(value))
        {
            throw new InvalidDocumentException($"already defined {value} in {listName}");
        }
    }
    private static void ThrowIfContains<T>(ICollection<T> list, Func<T, bool> predicate, string propName, string propValue, string listName)
    {
        if (list.Any(predicate))
        {
            throw new InvalidDocumentException($"already defined an entry with {propName} of {propValue} in {listName}");
        }
    }


    private static List<NametableAttribute> ParseDocumentAttributeSet(Node set)
    {
        List<NametableAttribute> nametableAttributes = [];
        foreach (Node node in set.Children)
        {
            if (node.type != NodeType.TOP_LEVEL_ATTRIBUTE)
            {
                InvalidState(node);
            }
            NametableAttribute attrib = Attributes.ParseNametableAttribute(node.value);
            if (nametableAttributes.Contains(attrib))
            {
                throw new InvalidDocumentException("already defined attribute " + Attributes.GetName(attrib));
            }
            else
            {
                nametableAttributes.Add(attrib);
            }
        }
        return nametableAttributes;
    }
    private static ObjectAttribute[] ParseObjectAttributeSet(Node set)
    {
        List<ObjectAttribute> attributes = [];
        foreach (Node node in set.Children)
        {
            if (node.type != NodeType.OBJECT_ATTRIBUTE)
            {
                InvalidState(node);
            }
            attributes.Add(Attributes.ParseObjectAttribute(node.value));
        }
        return [..attributes];
    }

    private static FieldAttribute[] ParseFieldAttributeSet(Node set)
    {
        List<FieldAttribute> attributes = [];
        foreach (Node node in set.Children)
        {
            if (node.type != NodeType.FIELD_ATTRIBUTE)
            {
                InvalidState(node);
            }
            attributes.Add(Attributes.ParseFieldAttribute(node.value));
        }
        return [..attributes];
    }
    private static FieldTemplateBuilder ParseField(Node field)
    {
        FieldTemplateBuilder currentField = new();

        foreach (Node node in field.Children)
        {
            switch (node.type)
            {
                case NodeType.FIELD_ID:
                    ThrowIfSet(currentField.Id, nameof(currentField.Id));
                    currentField.Id = ushort.Parse(node.value);
                    break;

                case NodeType.FIELD_NAME:
                    ThrowIfSet(currentField.Name, nameof(currentField.Name));
                    currentField.Name = node.value;
                    break;

                case NodeType.FIELD_ALLOWED_OBJECT:
                    currentField.AllowedObjects ??= [];
                    ThrowIfContains(currentField.AllowedObjects, node.value, nameof(currentField.AllowedObjects));
                    currentField.AllowedObjects.Add(node.value);
                    break;

                case NodeType.FIELD_TYPE:
                    currentField.AllowableTypes ??= [];
                    ThrowIfContains(currentField.AllowableTypes, Enum.Parse<DataType>(node.value), nameof(currentField.AllowableTypes));
                    currentField.AllowableTypes.Add(Enum.Parse<DataType>(node.value));
                    break;

                case NodeType.FIELD_BIND_TARGET:
                    currentField.BoundMembers ??= [];
                    ThrowIfContains(currentField.BoundMembers, node.value, nameof(currentField.BoundMembers));
                    currentField.BoundMembers.Add(node.value);
                    break;

                case NodeType.FIELD_ATTRIBUTE_SET:
                    FieldAttribute[] attributes = ParseFieldAttributeSet(node);
                    // add attributes
                    foreach (FieldAttribute attrib in attributes)
                    {
                        switch (attrib)
                        {
                            case FieldAttribute.NO_COMPRESSION:
                                ThrowIfSet(currentField.UseCompression, nameof(currentField.UseCompression));
                                currentField.UseCompression = false;
                                break;
                            case FieldAttribute.NULLABLE:
                                if (currentField.AllowableTypes?.Contains(DataType.NULL) ?? false)
                                {
                                    throw new InvalidDocumentException("field is already defined as nullable");
                                }
                                currentField.AllowableTypes ??= [];
                                currentField.AllowableTypes.Add(DataType.NULL);
                                break;
                            default:
                                InvalidState(node);
                                break;
                        }
                    }

                    break;
            }
        }

        return currentField;
    }

    private static ObjectTemplateBuilder ParseObject(Node obj)
    {
        // handle: attribs, name, id, binding, fields
        ObjectTemplateBuilder currentObject = new();
        ushort lowestFieldId = 0;

        foreach (Node node in obj.Children)
        {
            switch (node.type)
            {
                case NodeType.OBJECT_ID:
                    ThrowIfSet(currentObject.Id, nameof(currentObject.Id));
                    currentObject.Id = ushort.Parse(node.value);
                    break;

                case NodeType.OBJECT_NAME:
                    ThrowIfSet(currentObject.Name, nameof(currentObject.Name));
                    currentObject.Name = node.value;
                    break;

                case NodeType.OBJECT_BIND_TARGET:
                    currentObject.BoundTypeNames ??= [];
                    ThrowIfContains(currentObject.BoundTypeNames, node.value, nameof(currentObject.BoundTypeNames));
                    currentObject.BoundTypeNames.Add(node.value);
                    break;

                case NodeType.OBJECT_ATTRIBUTE_SET:
                    ObjectAttribute[] attributes = ParseObjectAttributeSet(node);
                    // add attributes
                    foreach (ObjectAttribute attrib in attributes)
                    {
                        switch (attrib)
                        {
                            case ObjectAttribute.FORCE_SHORT_IDS:
                                ThrowIfSet(currentObject.ForcesShortIds, nameof(currentObject.ForcesShortIds));
                                currentObject.ForcesShortIds = true;
                                break;
                            case ObjectAttribute.NO_COMPRESSION:
                                ThrowIfSet(currentObject.UseCompression, nameof(currentObject.UseCompression));
                                currentObject.UseCompression = false;
                                break;
                            default:
                                InvalidState(node);
                                break; // not necessary
                        }
                    }
                    break;

                case NodeType.FIELD:
                    currentObject.Fields ??= [];
                    FieldTemplateBuilder field = ParseField(node);
                    // means: if the current object is unbound but the field is, complain
                    // also do so if the current object is bound but the field is missing its bind targets

                    if ((currentObject.BoundTypeNames?.Count ?? 0) == 0 && (field.BoundMembers?.Count ?? 0) >= 0)
                    {
                        throw new InvalidDocumentException($"cannot bind field {field.Name} to a member if the object has no bind target");
                    }
                    if ((currentObject.BoundTypeNames?.Count ?? 0) >= 0 && (field.BoundMembers?.Count ?? 0) == 0)
                    {
                        throw new InvalidDocumentException($"field {field.Name} has no bind target when object is bound");
                    }
                    // if field has no ID, auto assign. increment if auto-assigned or using the auto-assign ID since it's not in the list.
                    // go through the list and keep incrementing as long as there's one that already has the next ID.
                    field.Id ??= lowestFieldId;
                    if (field.Id == lowestFieldId) lowestFieldId++;
                    while (currentObject.Fields.Any(f => lowestFieldId == f.Id))
                    {
                        field.Id ??= lowestFieldId;
                        lowestFieldId++;
                    }

                    field.Check();
                    FieldTemplate fieldTemplate = field.Build()!;
                    // if there's already a field with this ID, throw
                    ThrowIfContains(
                        currentObject.Fields, f => f.Id == fieldTemplate.Id, 
                        nameof(fieldTemplate.Id), fieldTemplate.Id.ToString(), nameof(currentObject.Fields)
                    );
                    currentObject.Fields.Add(fieldTemplate);
                    break;

                default:
                    // don't know what to do with this node
                    InvalidState(node);
                    break;
            }
        }

        return currentObject;
    }

    public static Nametable Compile(Node root)
    {
        List<ObjectTemplate> objects = [];
        List<NametableAttribute> nametableAttributes = [];
        uint? version = null;
        ushort lowestId = 0;
        foreach (Node node in root.Children) {
            if (node.type == NodeType.VERSION)
            {
                version = uint.Parse(node.value);
            }
            else if (node.type == NodeType.TOP_LEVEL_ATTRIBUTE_SET)
            {
                nametableAttributes.AddRange(ParseDocumentAttributeSet(node));
            }
            else if (node.type == NodeType.OBJECT)
            {
                ObjectTemplateBuilder builder = ParseObject(node);
                builder.Id ??= lowestId;
                if (builder.Id == lowestId) lowestId++;
                while (objects.Any(o => lowestId == o.Id))
                {
                    lowestId++;
                }

                builder.Check();
                ObjectTemplate fieldTemplate = builder.Build()!;
                // if there's already a field with this ID, throw
                ThrowIfContains(
                    objects, f => f.Id == fieldTemplate.Id, 
                    nameof(fieldTemplate.Id), fieldTemplate.Id.ToString(), nameof(objects)
                );
                objects.Add(fieldTemplate);
            }
            else
            {
                InvalidState(node);
            }
        }

        version ??= uint.MaxValue; // choose something, not that important
        return new Nametable([..objects], (uint) version,
            !nametableAttributes.Contains(NametableAttribute.NO_COMPRESSION),
            !nametableAttributes.Contains(NametableAttribute.NO_CRC));
    }

}