using static Jbot.Nametable.Parse.SymbolType;

namespace Jbot.Nametable.Parse;

internal class Parser(Symbol[] symbols)
{
    private readonly Node _root = new(NodeType.ROOT, "", []);
    private uint _nextIndex;

    #region Parser helpers - peek consume etc.

    private Symbol Peek()
    {
        if (this._nextIndex >= symbols.Length)
        {
            throw new SyntaxException("unexpected end of file");
        }

        return symbols[this._nextIndex];
    }

    private Symbol Consume()
    {
        if (this._nextIndex >= symbols.Length)
        {
            throw new SyntaxException("unexpected end of file");
        }

        return symbols[this._nextIndex++];
    }

    private bool Has(SymbolType type) { return this.Peek().type == type; }

    private bool Accept(SymbolType type)
    {
        if (this.Has(type))
        {
            this.Consume();
            return true;
        }

        return false;
    }

    private void Expect(SymbolType type)
    {
        if (!this.Accept(type))
        {
            throw new SyntaxException("expected " + type);
        }
    }

    private Symbol ExpectAndGet(params SymbolType[] types)
    {
        if (!types.Any(this.Has))
        {
            throw new SyntaxException("expected one of " + types);
        }

        return this.Consume();
    }

    private Symbol ExpectAndGet(SymbolType type)
    {
        if (!this.Has(type))
        {
            throw new SyntaxException("expected " + type);
        }

        return this.Consume();
    }

    private static void ThrowUnexpectedSymbol(Symbol symbol)
    {
        throw new SyntaxException("unexpected symbol " + symbol);
    }

    #endregion

    #region Parsing cases

    private void DocumentAttributeDeclaration()
    {
        // looks like: `attrib attr1 [attr2...] ;`
        Node attributeNode = new(NodeType.TOP_LEVEL_ATTRIBUTE_SET, "", []);

        // must have at least 1 attribute
        attributeNode.Children.Add(
            new Node(
                NodeType.TOP_LEVEL_ATTRIBUTE,
                this.ExpectAndGet(IDENTIFIER).value,
                []
            )
        );

        while (this.Has(IDENTIFIER))
        {
            Symbol identifier = this.Consume();

            attributeNode.Children.Add(
                new Node(NodeType.TOP_LEVEL_ATTRIBUTE, identifier.value, [])
            );
        }

        this._root.Children.Add(attributeNode);
        this.Expect(STATEMENT_END);
    }

    private void VersionDeclaration()
    {
        Symbol versionNumber = this.ExpectAndGet(NUMBER);
        this.Expect(STATEMENT_END);

        Node node = new(NodeType.VERSION, versionNumber.value, []);
        this._root.Children.Add(node);
    }

    private void ObjectAttributeDeclaration(Node objectNode)
    {
        Node attributeNode = new(NodeType.OBJECT_ATTRIBUTE_SET, "", []);

        while (this.Has(IDENTIFIER))
        {
            attributeNode.Children.Add(
                new Node(NodeType.OBJECT_ATTRIBUTE, this.Consume().value, []));
        }

        objectNode.Children.Add(attributeNode);
        this.Expect(STATEMENT_END);
    }

    private void ObjectBindDeclaration(Node objectNode)
    {
        Node bindNode = new(NodeType.OBJECT_BIND, "", []);

        bindNode.Children.Add(new Node(NodeType.OBJECT_BIND_TARGET,
            this.ExpectAndGet(DESCENDING_IDENTIFIER).value, []));

        while (this.Has(DESCENDING_IDENTIFIER))
        {
            bindNode.Children.Add(new Node(NodeType.OBJECT_BIND_TARGET,
                this.Consume().value, []));
        }

        objectNode.Children.Add(bindNode);
        this.Expect(STATEMENT_END);
    }

    private void FieldTypeDeclaration(Node fieldNode)
    {
        Node typeNode = new(NodeType.FIELD_TYPE, "", []);

        typeNode.Children.Add(
            new Node(NodeType.FIELD_TYPE, this.ExpectAndGet(IDENTIFIER).value, []));

        while (this.Accept(TYPE_SEPARATOR))
        {
            typeNode.Children.Add(new Node(NodeType.FIELD_TYPE, this.ExpectAndGet(IDENTIFIER).value,
                []));
        }

        fieldNode.Children.Add(typeNode);
    }

    private void FieldAllowsDeclaration(Node fieldNode)
    {
        Node typeNode = new(NodeType.FIELD_ALLOWS, "", []);

        typeNode.Children.Add(new Node(NodeType.FIELD_ALLOWED_OBJECT,
            this.ExpectAndGet(IDENTIFIER).value, []));

        while (this.Accept(TYPE_SEPARATOR))
        {
            typeNode.Children.Add(new Node(NodeType.FIELD_ALLOWED_OBJECT,
                this.ExpectAndGet(IDENTIFIER).value, []));
        }

        fieldNode.Children.Add(typeNode);
    }

    private void FieldBindDeclaration(Node fieldNode)
    {
        Node bindNode = new(NodeType.FIELD_BIND, "", []);

        this.Expect(DECL_FIELD_BIND);

        bindNode.Children.Add(new Node(NodeType.FIELD_BIND_TARGET,
            this.ExpectAndGet(DESCENDING_IDENTIFIER).value, []));

        while (this.Accept(DECL_FIELD_BIND))
        {
            bindNode.Children.Add(new Node(NodeType.FIELD_BIND_TARGET,
                this.ExpectAndGet(DESCENDING_IDENTIFIER).value, []));
        }

        fieldNode.Children.Add(bindNode);
    }

    private void FieldAttribute(Node fieldNode)
    {
        Node attributeNode = new(NodeType.FIELD_ATTRIBUTE_SET, "", []);

        attributeNode.Children.Add(new Node(NodeType.FIELD_ATTRIBUTE,
            this.ExpectAndGet(IDENTIFIER).value, []));

        while (this.Has(IDENTIFIER))
        {
            attributeNode.Children.Add(new Node(NodeType.FIELD_ATTRIBUTE, this.Consume().value,
                []));
        }

        fieldNode.Children.Add(attributeNode);
    }

    private void ObjectFieldDeclaration(Node objectNode)
    {
        Node fieldNode = new(NodeType.FIELD, "", []);

        fieldNode.Children.Add(new Node(NodeType.FIELD_ID, this.ExpectAndGet(IDENTIFIER).value,
            []));
        // check for any of the possibilities

        while (!this.Has(STATEMENT_END) && !this.Has(BLOCK_START))
        {
            if (this.Accept(DECL_TYPE))
            {
                this.FieldTypeDeclaration(fieldNode);
            }
            else if (this.Accept(DECL_ALLOWS))
            {
                this.FieldAllowsDeclaration(fieldNode);
            }
            else if (this.Has(DECL_FIELD_BIND))
            {
                this.FieldBindDeclaration(fieldNode);
            }
            else if (this.Has(IDENTIFIER))
            {
                this.FieldAttribute(fieldNode);
            }
            else
            {
                ThrowUnexpectedSymbol(this.Peek());
            }
        }

        Symbol next = this.ExpectAndGet(STATEMENT_END, BLOCK_START);

        if (next.type == STATEMENT_END)
        {
            objectNode.Children.Add(fieldNode);
            return;
        }

        // long-form body
        while (!this.Has(BLOCK_END))
        {
            if (this.Accept(DECL_TYPE))
            {
                this.FieldTypeDeclaration(fieldNode);
                this.Expect(STATEMENT_END);
            }
            else if (this.Accept(DECL_ALLOWS))
            {
                this.FieldAllowsDeclaration(fieldNode);
                this.Expect(STATEMENT_END);
            }
            else if (this.Accept(DECL_BIND))
            {
                this.FieldBindDeclaration(fieldNode);
                this.Expect(STATEMENT_END);
            }
            else if (this.Accept(DECL_ATTRIB))
            {
                this.FieldAttribute(fieldNode);
                this.Expect(STATEMENT_END);
            }
            else
            {
                ThrowUnexpectedSymbol(this.Peek());
            }
        }

        objectNode.Children.Add(fieldNode);
        this.Expect(BLOCK_END);
    }

    private void ObjectDeclaration()
    {
        Node objectNode = new(NodeType.OBJECT, "", []);

        Symbol name = this.ExpectAndGet(IDENTIFIER);
        objectNode.Children.Add(new Node(NodeType.OBJECT_NAME, name.value, []));

        if (this.Accept(DECL_ID))
        {
            objectNode.Children.Add(new Node(NodeType.OBJECT_ID, this.ExpectAndGet(NUMBER).value,
                []));
        }

        this.Expect(BLOCK_START);

        while (true)
        {
            if (this.Accept(DECL_ATTRIB))
            {
                this.ObjectAttributeDeclaration(objectNode);
            }
            else if (this.Accept(DECL_BIND))
            {
                this.ObjectBindDeclaration(objectNode);
            }
            else if (this.Accept(DECL_FIELD))
            {
                this.ObjectFieldDeclaration(objectNode);
            }
            else if (this.Accept(BLOCK_END))
            {
                this._root.Children.Add(objectNode);
                return;
            }
            else
            {
                ThrowUnexpectedSymbol(this.Peek());
            }
        }
    }

    private void Statement()
    {
        if (this.Accept(DECL_ATTRIB))
        {
            this.DocumentAttributeDeclaration();
        }
        else if (this.Accept(DECL_VERSION))
        {
            this.VersionDeclaration();
        }
        else if (this.Accept(DECL_OBJECT))
        {
            this.ObjectDeclaration();
        }
        else
        {
            ThrowUnexpectedSymbol(this.Peek());
        }
    }

    #endregion

    public Node Parse()
    {
        while (this._nextIndex < symbols.Length) this.Statement();
        return this._root;
    }

    public static Node Parse(Symbol[] symbols) { return new Parser(symbols).Parse(); }
}
