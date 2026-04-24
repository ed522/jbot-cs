using static Jbot.Nametable.Parse.SymbolType;

namespace Jbot.Nametable.Parse;
internal class Parser(Symbol[] symbols)
{
    private readonly Node root = new(NodeType.ROOT, "", []);
    private readonly Symbol[] symbols = symbols;
    private uint nextIndex = 0;

    private Symbol Peek()
    {
        return symbols[nextIndex];
    }
    private Symbol Consume()
    {
        return symbols[nextIndex++];
    }

    private bool Has(SymbolType type)
    {
        return Peek().type == type;
    }
    private bool Accept(SymbolType type)
    {
        if (Has(type))
        {
            Consume();
            return true;
        }

        return false;
    }
    private void Expect(SymbolType type)
    {
        if (!Accept(type))
        {
            throw new SyntaxException("expected " + type);
        }
    }
    private Symbol ExpectAndGet(params SymbolType[] types)
    {
        foreach (SymbolType type in types)
        {
            if (Has(type)) return Consume();
        }
        throw new SyntaxException("expected one of " + types.ToString());
    }
    private Symbol ExpectAndGet(SymbolType type)
    {
        if (!Has(type))
        {
            throw new SyntaxException("expected " + type);
        }

        return Consume();
    }

    private static void ThrowUnexpectedSymbol(Symbol symbol)
    {
        throw new SyntaxException("unexpected symbol " + symbol.ToString());
    }

    private void DocumentAttributeDeclaration()
    {
        Node attributeNode = new(NodeType.TOP_LEVEL_ATTRIBUTE_SET, "", []);
        while (Has(IDENTIFIER))
        {
            Symbol identifier = Consume();
            attributeNode.Children.Add(new Node(NodeType.TOP_LEVEL_ATTRIBUTE, identifier.value, []));
        }
        root.Children.Add(attributeNode);
        Expect(STATEMENT_END);
    }
    private void VersionDeclaration()
    {
        Symbol versionNumber = ExpectAndGet(NUMBER);
        Expect(STATEMENT_END);

        Node node = new(NodeType.VERSION, versionNumber.value, []);
        root.Children.Add(node);
    }

    private void ObjectAttributeDeclaration(Node objectNode)
    {
        Node attributeNode = new(NodeType.OBJECT_ATTRIBUTE_SET, "", []);
        while (Has(IDENTIFIER))
        {
            attributeNode.Children.Add(new Node(NodeType.OBJECT_ATTRIBUTE, Consume().value, []));
        }
        objectNode.Children.Add(attributeNode);
        Expect(STATEMENT_END);
    }
    private void ObjectBindDeclaration(Node objectNode)
    {
        Node bindNode = new(NodeType.OBJECT_BIND, "", []);
        bindNode.Children.Add(new Node(NodeType.OBJECT_BIND_TARGET, ExpectAndGet(DESCENDING_IDENTIFIER).value, []));
        objectNode.Children.Add(bindNode);
        Expect(STATEMENT_END);
    }

    private void FieldTypeDeclaration(Node fieldNode)
    {
        Node typeNode = new(NodeType.FIELD_TYPE, "", []);
        typeNode.Children.Add(new Node(NodeType.FIELD_TYPE, ExpectAndGet(IDENTIFIER).value, []));
        while (Accept(TYPE_SEPARATOR))
        {
            typeNode.Children.Add(new Node(NodeType.FIELD_TYPE, ExpectAndGet(IDENTIFIER).value, []));
        }
        fieldNode.Children.Add(typeNode);
    }
    private void FieldAllowsDeclaration(Node fieldNode)
    {
        Node typeNode = new(NodeType.FIELD_ALLOWS, "", []);
        typeNode.Children.Add(new Node(NodeType.FIELD_ALLOWED_OBJECT, ExpectAndGet(IDENTIFIER).value, []));
        while (Accept(TYPE_SEPARATOR))
        {
            typeNode.Children.Add(new Node(NodeType.FIELD_ALLOWED_OBJECT, ExpectAndGet(IDENTIFIER).value, []));
        }
        fieldNode.Children.Add(typeNode);
    }
    private void FieldBindDeclaration(Node fieldNode)
    {
        Node bindNode = new(NodeType.FIELD_BIND, "", []);
        
        Expect(DECL_FIELD_BIND);
        bindNode.Children.Add(new Node(NodeType.FIELD_BIND_TARGET, ExpectAndGet(DESCENDING_IDENTIFIER).value, []));
        while (Accept(DECL_FIELD_BIND))
        {
            bindNode.Children.Add(new Node(NodeType.FIELD_BIND_TARGET, ExpectAndGet(DESCENDING_IDENTIFIER).value, []));
        }

        fieldNode.Children.Add(bindNode);
    }
    private void FieldAttribute(Node fieldNode)
    {
        Node attributeNode = new(NodeType.FIELD_ATTRIBUTE_SET, "", []);
        attributeNode.Children.Add(new Node(NodeType.FIELD_ATTRIBUTE, ExpectAndGet(IDENTIFIER).value, []));
        while (Has(IDENTIFIER))
        {
            attributeNode.Children.Add(new Node(NodeType.FIELD_ATTRIBUTE, Consume().value, []));
        }
        fieldNode.Children.Add(attributeNode);
    }

    private void ObjectFieldDeclaration(Node objectNode)
    {
        Node fieldNode = new(NodeType.FIELD, "", []);
        fieldNode.Children.Add(new Node(NodeType.FIELD_ID, ExpectAndGet(IDENTIFIER).value, []));
        // check for any of the possibilities

        while (!Has(STATEMENT_END) && !Has(BLOCK_START))
        {
            if (Accept(DECL_TYPE))
            {
                FieldTypeDeclaration(fieldNode);
            }
            else if (Accept(DECL_ALLOWS))
            {
                FieldAllowsDeclaration(fieldNode);
            }
            else if (Has(DECL_FIELD_BIND))
            {
                FieldBindDeclaration(fieldNode);
            }
            else if (Has(IDENTIFIER))
            {
                FieldAttribute(fieldNode);
            }
            else
            {
                throw new SyntaxException("unexpected symbol " + Peek().ToString());
            }
        }
        Symbol next = ExpectAndGet(STATEMENT_END, BLOCK_START);
        if (next.type == STATEMENT_END)
        {
            objectNode.Children.Add(fieldNode);
            return;
        }

        // long-form body
        while (!Has(BLOCK_END))
        {
            if (Accept(DECL_TYPE))
            {
                FieldTypeDeclaration(fieldNode);
                Expect(STATEMENT_END);
            }
            else if (Accept(DECL_ALLOWS))
            {
                FieldAllowsDeclaration(fieldNode);
                Expect(STATEMENT_END);
            }
            else if (Accept(DECL_BIND))
            {
                FieldBindDeclaration(fieldNode);
                Expect(STATEMENT_END);
            }
            else if (Accept(DECL_ATTRIB))
            {
                FieldAttribute(fieldNode);
                Expect(STATEMENT_END);
            }
            else
            {
                ThrowUnexpectedSymbol(Peek());
            }
        }
        objectNode.Children.Add(fieldNode);
        Expect(BLOCK_END);

    }

    private void ObjectDeclaration()
    {
        Node objectNode = new(NodeType.OBJECT, "", []);

        Symbol name = ExpectAndGet(IDENTIFIER);
        objectNode.Children.Add(new Node(NodeType.OBJECT_NAME, name.value, []));

        if (Accept(DECL_ID))
        {
            objectNode.Children.Add(new Node(NodeType.OBJECT_ID, ExpectAndGet(NUMBER).value, []));
        }

        Expect(BLOCK_START);
        
        while (true) {
            if (Accept(DECL_ATTRIB))
            {
                ObjectAttributeDeclaration(objectNode);
            }
            else if (Accept(DECL_BIND))
            {
                ObjectBindDeclaration(objectNode);
            }
            else if (Accept(DECL_FIELD))
            {
                ObjectFieldDeclaration(objectNode);
            }
            else if (Accept(BLOCK_END))
            {
                root.Children.Add(objectNode);
                return;
            }
            else
            {
                ThrowUnexpectedSymbol(Peek());
            }
        }
    }

    private void Statement()
    {
        if (Accept(DECL_ATTRIB))
        {
            DocumentAttributeDeclaration();
        }
        else if (Accept(DECL_VERSION))
        {
            VersionDeclaration();
        }
        else if (Accept(DECL_OBJECT))
        {
            ObjectDeclaration();
        }
        else
        {
            ThrowUnexpectedSymbol(Peek());
        }
    }
    public Node Parse()
    {
        while (this.nextIndex < this.symbols.Length) Statement();
        return this.root;
    }

}