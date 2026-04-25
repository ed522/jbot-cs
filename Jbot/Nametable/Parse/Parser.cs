using static Jbot.Nametable.Parse.SymbolType;

namespace Jbot.Nametable.Parse;

internal class Parser(Symbol[] symbols)
{
    private uint _nextIndex;
    private readonly HashSet<Node> _currentNodes = []; // mutable
    private readonly HashSet<Node> _currentAttributeNodes = []; // mutable

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

    private HashSet<Node> DocumentAttributeDeclaration()
    {
        // looks like: `attrib attr1 [attr2...] ;`
        // Node attributeNode = new(NodeType.TOP_LEVEL_ATTRIBUTE_SET, "", []);
        HashSet<Node> currentChildren =
        [
            // must have at least 1 attribute
            new(NodeType.TOP_LEVEL_ATTRIBUTE, this.ExpectAndGet(IDENTIFIER).value, []),
        ];

        while (this.Has(IDENTIFIER))
        {
            Symbol identifier = this.Consume();

            currentChildren.Add(
                new Node(NodeType.TOP_LEVEL_ATTRIBUTE, identifier.value, [])
            );
        }

        this.Expect(STATEMENT_END);

        return currentChildren;
    }

    private Node VersionDeclaration()
    {
        Symbol versionNumber = this.ExpectAndGet(NUMBER);
        this.Expect(STATEMENT_END);

        return new Node(NodeType.VERSION, versionNumber.value, []);
    }

    private HashSet<Node> ObjectAttributeDeclaration()
    {
        HashSet<Node> attributes = [];

        while (this.Has(IDENTIFIER))
        {
            attributes.Add(new Node(NodeType.OBJECT_ATTRIBUTE, this.Consume().value, []));
        }

        this.Expect(STATEMENT_END);
        return attributes;
    }

    private HashSet<Node> ObjectBindDeclaration()
    {
        HashSet<Node> currentChildren =
        [
            new(NodeType.OBJECT_BIND_TARGET, this.ExpectAndGet(DESCENDING_IDENTIFIER).value, []),
        ];

        while (this.Has(DESCENDING_IDENTIFIER))
        {
            currentChildren.Add(new Node(NodeType.OBJECT_BIND_TARGET,
                this.Consume().value, []));
        }

        this.Expect(STATEMENT_END);

        return currentChildren;
    }

    private HashSet<Node> FieldTypeDeclaration()
    {
        HashSet<Node> currentChildren =
        [
            new(NodeType.FIELD_TYPE, this.ExpectAndGet(IDENTIFIER).value, []),
        ];

        while (this.Accept(TYPE_SEPARATOR))
        {
            currentChildren.Add(new Node(NodeType.FIELD_TYPE, this.ExpectAndGet(IDENTIFIER).value,
                []));
        }

        return currentChildren;
    }

    private HashSet<Node> FieldAllowsDeclaration()
    {
        HashSet<Node> currentChildren =
        [
            new(NodeType.FIELD_ALLOWED_OBJECT, this.ExpectAndGet(IDENTIFIER).value, []),
        ];

        while (this.Accept(TYPE_SEPARATOR))
        {
            currentChildren.Add(new Node(NodeType.FIELD_ALLOWED_OBJECT,
                this.ExpectAndGet(IDENTIFIER).value, []));
        }

        return currentChildren;
    }

    private HashSet<Node> FieldBindDeclaration()
    {
        HashSet<Node> currentChildren = [];

        this.Expect(DECL_FIELD_BIND);

        currentChildren.Add(new Node(NodeType.FIELD_BIND_TARGET,
            this.ExpectAndGet(DESCENDING_IDENTIFIER).value, []));

        while (this.Accept(DECL_FIELD_BIND))
        {
            currentChildren.Add(new Node(NodeType.FIELD_BIND_TARGET,
                this.ExpectAndGet(DESCENDING_IDENTIFIER).value, []));
        }

        return currentChildren;
    }

    private HashSet<Node> FieldAttributeDeclaration()
    {
        HashSet<Node> currentChildren =
        [
            new(NodeType.FIELD_ATTRIBUTE, this.ExpectAndGet(IDENTIFIER).value, []),
        ];

        while (this.Has(IDENTIFIER))
        {
            currentChildren.Add(new Node(NodeType.FIELD_ATTRIBUTE, this.Consume().value,
                []));
        }

        return currentChildren;
    }

    private Node ObjectFieldDeclaration()
    {
        HashSet<Node> currentChildren =
        [
            new(NodeType.FIELD_ID, this.ExpectAndGet(IDENTIFIER).value, []),
        ];

        HashSet<Node> currentAttributes = [];
        HashSet<Node> currentAllows = [];
        HashSet<Node> currentBinds = [];
        HashSet<Node> currentTypes = [];
        // check for any of the possibilities

        while (!this.Has(STATEMENT_END) && !this.Has(BLOCK_START))
        {
            if (this.Accept(DECL_TYPE))
            {
                currentTypes.UnionWith(this.FieldTypeDeclaration());
            }
            else if (this.Accept(DECL_ALLOWS))
            {
                currentAllows.UnionWith(this.FieldAllowsDeclaration());
            }
            else if (this.Has(DECL_FIELD_BIND))
            {
                currentBinds.UnionWith(this.FieldBindDeclaration());
            }
            else if (this.Has(IDENTIFIER))
            {
                currentAttributes.UnionWith(this.FieldAttributeDeclaration());
            }
            else
            {
                ThrowUnexpectedSymbol(this.Peek());
            }
        }

        Symbol next = this.ExpectAndGet(STATEMENT_END, BLOCK_START);

        if (next.type == STATEMENT_END)
        {
            // set everything here before returning
            if (currentTypes.Count > 0)
                currentChildren.Add(new Node(NodeType.FIELD_TYPE_SET, currentTypes));

            if (currentAllows.Count > 0)
                currentChildren.Add(new Node(NodeType.FIELD_ALLOWS, currentAllows));

            if (currentBinds.Count > 0)
                currentChildren.Add(new Node(NodeType.FIELD_BIND, currentBinds));

            if (currentAttributes.Count > 0)
                currentChildren.Add(new Node(NodeType.FIELD_ATTRIBUTE_SET, currentAttributes));

            return new Node(NodeType.FIELD, currentChildren);
        }

        // long-form body
        while (!this.Has(BLOCK_END))
        {
            if (this.Accept(DECL_TYPE))
            {
                currentTypes.UnionWith(this.FieldTypeDeclaration());
                this.Expect(STATEMENT_END);
            }
            else if (this.Accept(DECL_ALLOWS))
            {
                currentAllows.UnionWith(this.FieldAllowsDeclaration());
                this.Expect(STATEMENT_END);
            }
            else if (this.Accept(DECL_BIND))
            {
                currentBinds.UnionWith(this.FieldBindDeclaration());
                this.Expect(STATEMENT_END);
            }
            else if (this.Accept(DECL_ATTRIB))
            {
                currentAttributes.UnionWith(this.FieldAttributeDeclaration());
                this.Expect(STATEMENT_END);
            }
            else
            {
                ThrowUnexpectedSymbol(this.Peek());
            }
        }

        this.Expect(BLOCK_END);

        // deduplicate, and don't bother adding an extra node if there are none
        if (currentTypes.Count > 0)
            currentChildren.Add(new Node(NodeType.FIELD_TYPE_SET, currentTypes));

        if (currentAllows.Count > 0)
            currentChildren.Add(new Node(NodeType.FIELD_ALLOWS, currentAllows));

        if (currentBinds.Count > 0)
            currentChildren.Add(new Node(NodeType.FIELD_BIND, currentBinds));

        if (currentAttributes.Count > 0)
            currentChildren.Add(new Node(NodeType.FIELD_ATTRIBUTE_SET, currentAttributes));

        return new Node(NodeType.FIELD, currentChildren);
    }

    private Node ObjectDeclaration()
    {
        HashSet<Node> currentNodes = [];
        HashSet<Node> currentAttributes = [];
        HashSet<Node> currentBinds = [];

        Symbol name = this.ExpectAndGet(IDENTIFIER);
        currentNodes.Add(new Node(NodeType.OBJECT_NAME, name.value, []));

        if (this.Accept(DECL_ID))
        {
            currentNodes.Add(new Node(NodeType.OBJECT_ID,
                this.ExpectAndGet(NUMBER).value, []));
        }

        this.Expect(BLOCK_START);

        while (true)
        {
            if (this.Accept(DECL_ATTRIB))
            {
                currentAttributes.UnionWith(this.ObjectAttributeDeclaration());
            }
            else if (this.Accept(DECL_BIND))
            {
                currentBinds.UnionWith(this.ObjectBindDeclaration());
            }
            else if (this.Accept(DECL_FIELD))
            {
                currentNodes.Add(this.ObjectFieldDeclaration()); // self-contained
            }
            else if (this.Accept(BLOCK_END))
            {
                if (currentAttributes.Count > 0)
                    currentNodes.Add(new Node(NodeType.OBJECT_ATTRIBUTE_SET, currentAttributes));

                if (currentBinds.Count > 0)
                    currentNodes.Add(new Node(NodeType.OBJECT_BIND, currentBinds));

                return new Node(NodeType.OBJECT, currentNodes);
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
            this._currentAttributeNodes.UnionWith(this.DocumentAttributeDeclaration());
        }
        else if (this.Accept(DECL_VERSION))
        {
            this._currentNodes.Add(this.VersionDeclaration());
        }
        else if (this.Accept(DECL_OBJECT))
        {
            this._currentNodes.Add(this.ObjectDeclaration());
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

        if (this._currentAttributeNodes.Count > 0)
            this._currentNodes.Add(new Node(NodeType.TOP_LEVEL_ATTRIBUTE_SET,
                this._currentAttributeNodes));

        return new Node(NodeType.ROOT, this._currentNodes);
    }

    public static Node Parse(Symbol[] symbols) { return new Parser(symbols).Parse(); }
}
