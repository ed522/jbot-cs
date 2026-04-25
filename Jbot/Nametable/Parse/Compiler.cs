namespace Jbot.Nametable.Parse;

internal static class Compiler
{
    private static void InvalidState(Node node)
    {
        throw new SyntaxException("did not expect token " + node.Type + " here, bug in parser?");
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

    private static void ThrowIfContains<T>(
        ICollection<T> list, Func<T, bool> predicate, string propName, string propValue,
        string listName
    )
    {
        if (list.Any(predicate))
        {
            throw new InvalidDocumentException(
                $"already defined an entry with {propName} of {propValue} in {listName}");
        }
    }


    private static List<NametableAttribute> ParseDocumentAttributeSet(Node set)
    {
        List<NametableAttribute> nametableAttributes = [];

        foreach (Node node in set.Children)
        {
            if (node.Type != NodeType.TOP_LEVEL_ATTRIBUTE)
            {
                InvalidState(node);
            }

            NametableAttribute attrib = Attributes.ParseNametableAttribute(node.Value);

            if (nametableAttributes.Contains(attrib))
            {
                throw new InvalidDocumentException("already defined attribute " +
                                                   Attributes.GetName(attrib));
            }

            nametableAttributes.Add(attrib);
        }

        return nametableAttributes;
    }

    private static ObjectAttribute[] ParseObjectAttributeSet(Node set)
    {
        List<ObjectAttribute> attributes = [];

        foreach (Node node in set.Children)
        {
            if (node.Type != NodeType.OBJECT_ATTRIBUTE)
            {
                InvalidState(node);
            }

            ObjectAttribute attrib = Attributes.ParseObjectAttribute(node.Value);

            if (attributes.Contains(attrib))
            {
                throw new InvalidDocumentException("already defined attribute " +
                                                   Attributes.GetName(attrib));
            }

            attributes.Add(Attributes.ParseObjectAttribute(node.Value));
        }

        return [..attributes];
    }

    private static FieldAttribute[] ParseFieldAttributeSet(Node set)
    {
        List<FieldAttribute> attributes = [];

        foreach (Node node in set.Children)
        {
            if (node.Type != NodeType.FIELD_ATTRIBUTE)
            {
                InvalidState(node);
            }

            FieldAttribute attrib = Attributes.ParseFieldAttribute(node.Value);

            if (attributes.Contains(attrib))
            {
                throw new InvalidDocumentException("already defined attribute " +
                                                   Attributes.GetName(attrib));
            }

            attributes.Add(Attributes.ParseFieldAttribute(node.Value));
        }

        return [..attributes];
    }

    private static FieldTemplateBuilder ParseField(Node field)
    {
        FieldTemplateBuilder currentField = new();

        foreach (Node node in field.Children)
        {
            switch (node.Type)
            {
                case NodeType.FIELD_ID:
                    ThrowIfSet(currentField.Id, nameof(currentField.Id));
                    currentField.Id = ushort.Parse(node.Value);
                    break;

                case NodeType.FIELD_NAME:
                    ThrowIfSet(currentField.Name, nameof(currentField.Name));
                    currentField.Name = node.Value;
                    break;

                case NodeType.FIELD_ALLOWED_OBJECT:
                    currentField.AllowedObjects ??= [];

                    ThrowIfContains(currentField.AllowedObjects, node.Value,
                        nameof(currentField.AllowedObjects));

                    currentField.AllowedObjects.Add(node.Value);
                    break;

                case NodeType.FIELD_TYPE_SET:
                    currentField.AllowableTypes ??= [];

                    foreach (Node child in node.Children)
                    {
                        ThrowIfContains(currentField.AllowableTypes,
                            Enum.Parse<DataType>(child.Value), nameof(currentField.AllowableTypes));

                        currentField.AllowableTypes.Add(Enum.Parse<DataType>(child.Value));
                    }

                    break;

                case NodeType.FIELD_BIND_TARGET:
                    currentField.BoundMembers ??= [];

                    ThrowIfContains(currentField.BoundMembers, node.Value,
                        nameof(currentField.BoundMembers));

                    currentField.BoundMembers.Add(node.Value);
                    break;

                case NodeType.FIELD_ATTRIBUTE_SET:
                    FieldAttribute[] attributes = ParseFieldAttributeSet(node);

                    // add attributes
                    foreach (FieldAttribute attrib in attributes)
                    {
                        switch (attrib)
                        {
                            case FieldAttribute.NO_COMPRESSION:
                                ThrowIfSet(currentField.UseCompression,
                                    nameof(currentField.UseCompression));

                                currentField.UseCompression = false;
                                break;

                            case FieldAttribute.NULLABLE:
                                if (currentField.AllowableTypes?.Contains(DataType.NULL) ?? false)
                                {
                                    throw new InvalidDocumentException(
                                        "field is already defined as nullable");
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

                default:
                    InvalidState(node);
                    break;
            }
        }

        return currentField;
    }

    private static ObjectTemplateBuilder ParseObject(Node obj)
    {
        ObjectTemplateBuilder currentObject = new();
        ushort lowestFieldId = 0;

        foreach (Node node in obj.Children)
        {
            switch (node.Type)
            {
                // every simple (single) property can only be set once
                case NodeType.OBJECT_ID:
                    ThrowIfSet(currentObject.Id, nameof(currentObject.Id));
                    currentObject.Id = ushort.Parse(node.Value);
                    break;

                case NodeType.OBJECT_NAME:
                    ThrowIfSet(currentObject.Name, nameof(currentObject.Name));
                    currentObject.Name = node.Value;
                    break;

                case NodeType.OBJECT_BIND_TARGET:
                    currentObject.BoundTypeNames ??= [];

                    ThrowIfContains(currentObject.BoundTypeNames, node.Value,
                        nameof(currentObject.BoundTypeNames));

                    currentObject.BoundTypeNames.Add(node.Value);
                    break;

                case NodeType.OBJECT_ATTRIBUTE_SET:
                    ObjectAttribute[] attributes = ParseObjectAttributeSet(node);

                    // add attributes
                    foreach (ObjectAttribute attrib in attributes)
                    {
                        switch (attrib)
                        {
                            case ObjectAttribute.FORCE_SHORT_IDS:
                                ThrowIfSet(currentObject.ForcesShortIds,
                                    nameof(currentObject.ForcesShortIds));

                                currentObject.ForcesShortIds = true;
                                break;

                            case ObjectAttribute.NO_COMPRESSION:
                                ThrowIfSet(currentObject.UseCompression,
                                    nameof(currentObject.UseCompression));

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
                    FieldTemplateBuilder fieldBuilder = ParseField(node);
                    // means: if the current object is unbound but the field is, complain
                    // also do so if the current object is bound but the field is missing its bind targets

                    if ((currentObject.BoundTypeNames?.Count ?? 0) == 0 &&
                        (fieldBuilder.BoundMembers?.Count ?? 0) >= 0)
                    {
                        throw new InvalidDocumentException(
                            $"cannot bind field {fieldBuilder.Name} to a member if the object has no bind target");
                    }

                    if ((currentObject.BoundTypeNames?.Count ?? 0) >= 0 &&
                        (fieldBuilder.BoundMembers?.Count ?? 0) == 0)
                    {
                        throw new InvalidDocumentException(
                            $"field {fieldBuilder.Name} has no bind target when object is bound");
                    }

                    // if field has no ID, auto assign. increment if auto-assigned or using the auto-assign ID since it's not in the list.
                    // go through the list and keep incrementing as long as there's one that already has the next ID.
                    fieldBuilder.Id ??= lowestFieldId;
                    if (fieldBuilder.Id == lowestFieldId) lowestFieldId++;

                    while (currentObject.Fields.Any(f => lowestFieldId == f.Id))
                    {
                        fieldBuilder.Id ??= lowestFieldId;
                        lowestFieldId++;
                    }

                    fieldBuilder.Check();
                    FieldTemplate field = fieldBuilder.Build()!;
                    // if there's already a field with this ID, throw

                    // Access to modified closure: not applicable since lambda is immediately run
                    ThrowIfContains(
                        // ReSharper disable AccessToModifiedClosure
                        currentObject.Fields, f => field.Id == f.Id,
                        // ReSharper restore AccessToModifiedClosure
                        nameof(field.Id), field.Id.ToString(), nameof(currentObject.Fields)
                    );

                    currentObject.Fields.Add(field);
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

        foreach (Node node in root.Children)
        {
            switch (node.Type)
            {
                case NodeType.VERSION:
                    ThrowIfSet(version, nameof(version));
                    version = uint.Parse(node.Value);
                    break;

                case NodeType.TOP_LEVEL_ATTRIBUTE_SET:
                    nametableAttributes.AddRange(ParseDocumentAttributeSet(node));
                    break;

                case NodeType.OBJECT:
                    ObjectTemplateBuilder builder = ParseObject(node);
                    builder.Id ??= lowestId;
                    if (builder.Id == lowestId) lowestId++;

                    while (objects.Any(o => lowestId == o.Id))
                    {
                        lowestId++;
                    }

                    builder.Check();
                    ObjectTemplate obj = builder.Build()!;

                    // if there's already a field with this ID, throw
                    // inspection: same reason as above, lambda gets run immediately so there's no danger
                    ThrowIfContains(
                        // ReSharper disable once AccessToModifiedClosure
                        objects, f => f.Id == obj.Id,
                        nameof(obj.Id), obj.Id.ToString(), nameof(objects)
                    );

                    objects.Add(obj);
                    break;

                default:
                    InvalidState(node);
                    break;
            }
        }

        version ??= uint
            .MaxValue; // not 0 since that could conceivably be used for v0, this is a good canary

        return new Nametable([..objects], (uint)version,
            !nametableAttributes.Contains(NametableAttribute.NO_COMPRESSION),
            !nametableAttributes.Contains(NametableAttribute.NO_CRC));
    }
}
