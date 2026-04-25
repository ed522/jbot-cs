using Jbot.Nametable.Parse;

namespace Jbot.Test;

public class LexerTests
{
    [SetUp]
    public void Setup() { }

    private static string Space(string str) => "   " + str + "  ";

    [Test]
    public void TestLexer_InvalidSymbol()
    {
        string input = Space("valid symbols *****");
        Assert.Throws<SyntaxException>(() => Lexer.Parse(input));
    }

    #region Lexer tests - individual symbols

    [Test]
    public void TestLexer_Identifier()
    {
        string input = Space("Some_Identifier");
        Symbol[] output = Lexer.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(output, Is.Not.EqualTo(null));
            Assert.That(output, Has.Length.EqualTo(1));
            Assert.That(output[0].type, Is.EqualTo(SymbolType.IDENTIFIER));
            Assert.That(output[0].value, Is.EqualTo("Some_Identifier"));
        });
    }

    [Test]
    public void TestLexer_Number()
    {
        string input = Space("12345");
        Symbol[] output = Lexer.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(output, Is.Not.EqualTo(null));
            Assert.That(output, Has.Length.EqualTo(1));
            Assert.That(output[0].type, Is.EqualTo(SymbolType.NUMBER));
            Assert.That(output[0].value, Is.EqualTo("12345"));
        });
    }

    [Test]
    public void TestLexer_DescendingIdentifier()
    {
        string input = Space("some.descending.identifier");
        Symbol[] output = Lexer.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(output, Is.Not.EqualTo(null));
            Assert.That(output, Has.Length.EqualTo(1));
            Assert.That(output[0].type, Is.EqualTo(SymbolType.DESCENDING_IDENTIFIER));
            Assert.That(output[0].value, Is.EqualTo("some.descending.identifier"));
        });
    }

    [Test]
    public void TestLexer_DeclVersion()
    {
        string input = Space("version");
        Symbol[] output = Lexer.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(output, Is.Not.EqualTo(null));
            Assert.That(output, Has.Length.EqualTo(1));
            Assert.That(output[0].type, Is.EqualTo(SymbolType.DECL_VERSION));
            Assert.That(output[0].value, Is.EqualTo("version"));
        });
    }

    [Test]
    public void TestLexer_DeclAttrib()
    {
        string input = Space("attrib");
        Symbol[] output = Lexer.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(output, Is.Not.EqualTo(null));
            Assert.That(output, Has.Length.EqualTo(1));
            Assert.That(output[0].type, Is.EqualTo(SymbolType.DECL_ATTRIB));
            Assert.That(output[0].value, Is.EqualTo("attrib"));
        });
    }

    [Test]
    public void TestLexer_DeclObject()
    {
        string input = Space("object");
        Symbol[] output = Lexer.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(output, Is.Not.EqualTo(null));
            Assert.That(output, Has.Length.EqualTo(1));
            Assert.That(output[0].type, Is.EqualTo(SymbolType.DECL_OBJECT));
            Assert.That(output[0].value, Is.EqualTo("object"));
        });
    }

    [Test]
    public void TestLexer_DeclBind()
    {
        string input = Space("bind");
        Symbol[] output = Lexer.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(output, Is.Not.EqualTo(null));
            Assert.That(output, Has.Length.EqualTo(1));
            Assert.That(output[0].type, Is.EqualTo(SymbolType.DECL_BIND));
            Assert.That(output[0].value, Is.EqualTo("bind"));
        });
    }

    [Test]
    public void TestLexer_DeclField()
    {
        string input = Space("field");
        Symbol[] output = Lexer.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(output, Is.Not.EqualTo(null));
            Assert.That(output, Has.Length.EqualTo(1));
            Assert.That(output[0].type, Is.EqualTo(SymbolType.DECL_FIELD));
            Assert.That(output[0].value, Is.EqualTo("field"));
        });
    }

    [Test]
    public void TestLexer_DeclAllows()
    {
        string input = Space("allows");
        Symbol[] output = Lexer.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(output, Is.Not.EqualTo(null));
            Assert.That(output, Has.Length.EqualTo(1));
            Assert.That(output[0].type, Is.EqualTo(SymbolType.DECL_ALLOWS));
            Assert.That(output[0].value, Is.EqualTo("allows"));
        });
    }

    [Test]
    public void TestLexer_DeclType()
    {
        string input = Space("type");
        Symbol[] output = Lexer.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(output, Is.Not.EqualTo(null));
            Assert.That(output, Has.Length.EqualTo(1));
            Assert.That(output[0].type, Is.EqualTo(SymbolType.DECL_TYPE));
            Assert.That(output[0].value, Is.EqualTo("type"));
        });
    }

    [Test]
    public void TestLexer_BlockStart()
    {
        string input = Space("id1{id2");
        Symbol[] output = Lexer.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(output, Is.Not.EqualTo(null));
            Assert.That(output, Has.Length.EqualTo(3));
            Assert.That(output[0].type, Is.EqualTo(SymbolType.IDENTIFIER));
            Assert.That(output[0].value, Is.EqualTo("id1"));
            Assert.That(output[1].type, Is.EqualTo(SymbolType.BLOCK_START));
            Assert.That(output[1].value, Is.EqualTo("{"));
            Assert.That(output[2].type, Is.EqualTo(SymbolType.IDENTIFIER));
            Assert.That(output[2].value, Is.EqualTo("id2"));
        });
    }

    [Test]
    public void TestLexer_BlockEnd()
    {
        string input = Space("id1}id2");
        Symbol[] output = Lexer.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(output, Is.Not.EqualTo(null));
            Assert.That(output, Has.Length.EqualTo(3));
            Assert.That(output[0].type, Is.EqualTo(SymbolType.IDENTIFIER));
            Assert.That(output[0].value, Is.EqualTo("id1"));
            Assert.That(output[1].type, Is.EqualTo(SymbolType.BLOCK_END));
            Assert.That(output[1].value, Is.EqualTo("}"));
            Assert.That(output[2].type, Is.EqualTo(SymbolType.IDENTIFIER));
            Assert.That(output[2].value, Is.EqualTo("id2"));
        });
    }

    [Test]
    public void TestLexer_StatementEnd()
    {
        string input = Space("id1;id2");
        Symbol[] output = Lexer.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(output, Is.Not.EqualTo(null));
            Assert.That(output, Has.Length.EqualTo(3));
            Assert.That(output[0].type, Is.EqualTo(SymbolType.IDENTIFIER));
            Assert.That(output[0].value, Is.EqualTo("id1"));
            Assert.That(output[1].type, Is.EqualTo(SymbolType.STATEMENT_END));
            Assert.That(output[1].value, Is.EqualTo(";"));
            Assert.That(output[2].type, Is.EqualTo(SymbolType.IDENTIFIER));
            Assert.That(output[2].value, Is.EqualTo("id2"));
        });
    }

    [Test]
    public void TestLexer_DeclId()
    {
        string input = Space("id1#id2");
        Symbol[] output = Lexer.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(output, Is.Not.EqualTo(null));
            Assert.That(output, Has.Length.EqualTo(3));
            Assert.That(output[0].type, Is.EqualTo(SymbolType.IDENTIFIER));
            Assert.That(output[0].value, Is.EqualTo("id1"));
            Assert.That(output[1].type, Is.EqualTo(SymbolType.DECL_ID));
            Assert.That(output[1].value, Is.EqualTo("#"));
            Assert.That(output[2].type, Is.EqualTo(SymbolType.IDENTIFIER));
            Assert.That(output[2].value, Is.EqualTo("id2"));
        });
    }

    [Test]
    public void TestLexer_TypeSeparator()
    {
        const string input = "id1,id2";
        Symbol[] output = Lexer.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(output, Is.Not.EqualTo(null));
            Assert.That(output, Has.Length.EqualTo(3));
            Assert.That(output[0].type, Is.EqualTo(SymbolType.IDENTIFIER));
            Assert.That(output[0].value, Is.EqualTo("id1"));
            Assert.That(output[1].type, Is.EqualTo(SymbolType.TYPE_SEPARATOR));
            Assert.That(output[1].value, Is.EqualTo(","));
            Assert.That(output[2].type, Is.EqualTo(SymbolType.IDENTIFIER));
            Assert.That(output[2].value, Is.EqualTo("id2"));
        });
    }

    [Test]
    public void TestLexer_DeclFieldBind()
    {
        const string input = "id1$id2";
        Symbol[] output = Lexer.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(output, Is.Not.EqualTo(null));
            Assert.That(output, Has.Length.EqualTo(3));
            Assert.That(output[0].type, Is.EqualTo(SymbolType.IDENTIFIER));
            Assert.That(output[0].value, Is.EqualTo("id1"));
            Assert.That(output[1].type, Is.EqualTo(SymbolType.DECL_FIELD_BIND));
            Assert.That(output[1].value, Is.EqualTo("$"));
            Assert.That(output[2].type, Is.EqualTo(SymbolType.IDENTIFIER));
            Assert.That(output[2].value, Is.EqualTo("id2"));
        });
    }

    #endregion
}
