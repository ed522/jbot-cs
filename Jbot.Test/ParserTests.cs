using Jbot.Nametable.Parse;

using static Jbot.Nametable.Parse.NodeType;

namespace Jbot.Test;

public class ParserTests
{
    /// <summary>
    ///     Run a parameterized parser test. The input string is lexed and parsed,
    ///     and the output of the parser (the children of the root node) is
    ///     compared to the expected output.
    /// </summary>
    /// <param name="input">the input to be parsed</param>
    /// <param name="expected">the expected output of the parser, in terms of the root's children</param>
    private static void TestValid(string input, Node[] expected)
    {
        Symbol[] lexerOut = Lexer.Parse(input);
        Node output = Parser.Parse(lexerOut);

        Assert.That(output.Children, Has.Count.EqualTo(expected.Length));
        Assert.That(output.Children, Is.EqualTo(expected));
    }

    private static void TestInvalid(string input)
    {
        Symbol[] lexerOut = Lexer.Parse(input);
        Assert.Throws<SyntaxException>(() => Parser.Parse(lexerOut));
    }

    // all test cases must be invocations since Node instantiations aren't compile-time constants

    #region Valid parser scenarios

    [Test]
    public void TestParserOutput_Empty() => TestValid("", []);

    [Test]
    public void TestParserOutput_Version() => TestValid(
        "version 10000;", [
            new Node(VERSION, "10000"),
        ]);

    [Test]
    public void TestParserOutput_DocumentAttrib() => TestValid(
        "attrib attr1 attr2;", [
            new Node(TOP_LEVEL_ATTRIBUTE_SET, [
                new Node(TOP_LEVEL_ATTRIBUTE, "attr1"),
                new Node(TOP_LEVEL_ATTRIBUTE, "attr2"),
            ]),
        ]);

    [Test]
    public void TestParserOutput_ObjectBasic() => TestValid(
        "object ObjName {}", [
            new Node(OBJECT, [
                new Node(OBJECT_NAME, "ObjName"),
            ]),
        ]);

    [Test]
    public void TestParserOutput_ObjectId() => TestValid(
        "object ObjName # 123 {}", [
            new Node(OBJECT, [
                new Node(OBJECT_NAME, "ObjName"),
                new Node(OBJECT_ID, "123"),
            ]),
        ]);

    [Test]
    public void TestParserOutput_ObjectAttributes() => TestValid(
        "object ObjName { attrib attr1 attr2; }", [
            new Node(OBJECT, [
                new Node(OBJECT_NAME, "ObjName"),
                new Node(OBJECT_ATTRIBUTE_SET, [
                    new Node(OBJECT_ATTRIBUTE, "attr1"),
                    new Node(OBJECT_ATTRIBUTE, "attr2"),
                ]),
            ]),
        ]);

    [Test]
    public void TestParserOutput_ObjectBind() => TestValid(
        "object ObjName { bind target.path.first target.path.second; }", [
            new Node(OBJECT, [
                new Node(OBJECT_NAME, "ObjName"),
                new Node(OBJECT_BIND, [
                    new Node(OBJECT_BIND_TARGET, "target.path.first"),
                    new Node(OBJECT_BIND_TARGET, "target.path.second"),
                ]),
            ]),
        ]);

    [Test]
    public void TestParserOutput_FieldShortForm_Basic() => TestValid(
        "object ObjName { field field1; }", [
            new Node(OBJECT, [
                new Node(OBJECT_NAME, "ObjName"),
                new Node(FIELD, [
                    new Node(FIELD_ID, "field1"),
                ]),
            ]),
        ]);

    [Test]
    public void TestParserOutput_FieldShortForm_Type() => TestValid(
        "object ObjName { field field1 type TYPEA,TYPEB; }", [
            new Node(OBJECT, [
                new Node(OBJECT_NAME, "ObjName"),
                new Node(FIELD, [
                    new Node(FIELD_ID, "field1"),
                    new Node(FIELD_TYPE_SET, [
                        new Node(FIELD_TYPE, "TYPEA"),
                        new Node(FIELD_TYPE, "TYPEB"),
                    ]),
                ]),
            ]),
        ]);

    [Test]
    public void TestParserOutput_FieldShortForm_Allows() => TestValid(
        "object ObjName { field field1 allows Obj1, Obj2; }", [
            new Node(OBJECT, [
                new Node(OBJECT_NAME, "ObjName"),
                new Node(FIELD, [
                    new Node(FIELD_ID, "field1"),
                    new Node(FIELD_ALLOWS, [
                        new Node(FIELD_ALLOWED_OBJECT, "Obj1"),
                        new Node(FIELD_ALLOWED_OBJECT, "Obj2"),
                    ]),
                ]),
            ]),
        ]);

    [Test]
    public void TestParserOutput_FieldShortForm_Bind() => TestValid(
        "object ObjName { field field1 $bind.path1 $bind.path2; }", [
            new Node(OBJECT, [
                new Node(OBJECT_NAME, "ObjName"),
                new Node(FIELD, [
                    new Node(FIELD_ID, "field1"),
                    new Node(FIELD_BIND, [
                        new Node(FIELD_BIND_TARGET, "bind.path1"),
                        new Node(FIELD_BIND_TARGET, "bind.path2"),
                    ]),
                ]),
            ]),
        ]);

    [Test]
    public void TestParserOutput_FieldShortForm_Attribute() => TestValid(
        "object ObjName { field field1 attr1 attr2; }", [
            new Node(OBJECT, [
                new Node(OBJECT_NAME, "ObjName"),
                new Node(FIELD, [
                    new Node(FIELD_ID, "field1"),
                    new Node(FIELD_ATTRIBUTE_SET, [
                        new Node(FIELD_ATTRIBUTE, "attr1"),
                        new Node(FIELD_ATTRIBUTE, "attr2"),
                    ]),
                ]),
            ]),
        ]);

    [Test]
    public void TestParserOutput_FieldShortForm_Combined() => TestValid(
        "object ObjName { field field1 attr1 type type1 allows Obj1 $bind.path attr2; }", [
            new Node(OBJECT, [
                new Node(OBJECT_NAME, "ObjName"),
                new Node(FIELD, [
                    new Node(FIELD_ID, "field1"),
                    new Node(FIELD_TYPE_SET, [
                        new Node(FIELD_TYPE, "type1"),
                    ]),
                    new Node(FIELD_ALLOWS, [
                        new Node(FIELD_ALLOWED_OBJECT, "Obj1"),
                    ]),
                    new Node(FIELD_BIND, [
                        new Node(FIELD_BIND_TARGET, "bind.path"),
                    ]),
                    new Node(FIELD_ATTRIBUTE_SET, [
                        new Node(FIELD_ATTRIBUTE, "attr1"),
                        new Node(FIELD_ATTRIBUTE, "attr2"),
                    ]),
                ]),
            ]),
        ]);

    [Test]
    public void TestParserOutput_FieldLongForm() => TestValid(
        """
        object ObjName { 
            field field1 {
                type TYPEA,TYPEB;
                allows Obj1,Obj2;
                bind $bind.path1 $bind.path2;
                attrib attr1 attr2;
            }
        }
        """, [
            new Node(OBJECT, [
                new Node(OBJECT_NAME, "ObjName"),
                new Node(FIELD, [
                    new Node(FIELD_ID, "field1"),
                    new Node(FIELD_TYPE_SET, [
                        new Node(FIELD_TYPE, "TYPEA"),
                        new Node(FIELD_TYPE, "TYPEB"),
                    ]),
                    new Node(FIELD_ALLOWS, [
                        new Node(FIELD_ALLOWED_OBJECT, "Obj1"),
                        new Node(FIELD_ALLOWED_OBJECT, "Obj2"),
                    ]),
                    new Node(FIELD_BIND, [
                        new Node(FIELD_BIND_TARGET, "bind.path1"),
                        new Node(FIELD_BIND_TARGET, "bind.path2"),
                    ]),
                    new Node(FIELD_ATTRIBUTE_SET, [
                        new Node(FIELD_ATTRIBUTE, "attr1"),
                        new Node(FIELD_ATTRIBUTE, "attr2"),
                    ]),
                ]),
            ]),
        ]);

    #endregion

    #region Invalid parser scenarios

    // NOTE: every test that looks for a missing token should have an extra token at the end to
    // properly trigger the logic, instead of triggering a throw in the parser's advancement
    // logic (if there's no tokens during an accept it just throws an "unexpected file end" error)

    #region Top-level

    [Test]
    public void TestParserValidation_TopLevel_UnexpectedSymbol() =>
        TestInvalid("123");

    [Test]
    public void TestParserValidation_DocumentAttrib_MissingAttributes() =>
        TestInvalid("attrib;");

    [Test]
    public void TestParserValidation_DocumentAttrib_MissingSemicolon() =>
        TestInvalid("attrib attr1 version");

    [Test]
    public void TestParserValidation_Version_MissingNumber() =>
        TestInvalid("version;");

    [Test]
    public void TestParserValidation_Version_MissingSemicolon() =>
        TestInvalid("version 1 attrib");

    #endregion

    #region Objects

    [Test]
    public void TestParserValidation_Object_MissingStartBrace() =>
        TestInvalid("object O }");

    [Test]
    public void TestParserValidation_Object_MissingEndBrace() =>
        TestInvalid("object O { object O2 { }");

    [Test]
    public void TestParserValidation_Object_UnexpectedSymbol() =>
        TestInvalid("object O { version }");

    [Test]
    public void TestParserValidation_Object_MissingName() =>
        TestInvalid("object #1 { }");

    [Test]
    public void TestParserValidation_ObjectId_MissingNumber() =>
        TestInvalid("object O # { }");

    [Test]
    public void TestParserValidation_ObjectAttrib_MissingSemicolon() =>
        TestInvalid("object O { attrib a }");

    [Test]
    public void TestParserValidation_ObjectBind_MissingTarget() =>
        TestInvalid("object O { bind ; }");

    [Test]
    public void TestParserValidation_ObjectBind_MissingSemicolon() =>
        TestInvalid("object O { bind a.b }");

    #endregion

    #region Fields (short-form)

    [Test]
    public void TestParserValidation_Field_MissingName() =>
        TestInvalid("object O { field ; }");

    [Test]
    public void TestParserValidation_Field_UnexpectedSymbol() =>
        TestInvalid("object O { field f version }");

    [Test]
    public void TestParserValidation_FieldId_MissingNumber() =>
        TestInvalid("object O { field f #; }");

    [Test]
    public void TestParserValidation_FieldType_MissingType() =>
        TestInvalid("object O { field f type ; }");

    [Test]
    public void TestParserValidation_FieldType_TrailingComma() =>
        TestInvalid("object O { field f type t, ; }");

    [Test]
    public void TestParserValidation_FieldAllows_MissingObject() =>
        TestInvalid("object O { field f allows ; }");

    [Test]
    public void TestParserValidation_FieldAllows_TrailingComma() =>
        TestInvalid("object O { field f allows t1,t2, ; }");

    [Test]
    public void TestParserValidation_FieldBind_MissingTarget() =>
        TestInvalid("object O { field f $ ; }");

    #endregion

    #region Fields (long-form)

    [Test]
    public void TestParserValidation_LongField_UnexpectedSymbol() =>
        TestInvalid("object O { field f { version } }");

    [Test]
    public void TestParserValidation_LongField_MissingBrace() =>
        TestInvalid("object O { field f { type t;");

    [Test]
    public void TestParserValidation_LongFieldBind_MissingDollar() =>
        TestInvalid("object O { field f { bind target; } }");

    [Test]
    public void TestParserValidation_LongFieldBind_MissingTarget() =>
        TestInvalid("object O { field f { bind $; } }");

    [Test]
    public void TestParserValidation_LongFieldBind_MissingSemicolon() =>
        TestInvalid("object O { field f { bind $b } }");

    [Test]
    public void TestParserValidation_LongFieldType_MissingSemicolon() =>
        TestInvalid("object O { field f { type t } }");

    [Test]
    public void TestParserValidation_LongFieldType_MissingTypeName() =>
        TestInvalid("object O { field f { type ; } }");

    [Test]
    public void TestParserValidation_LongFieldType_TrailingComma() =>
        TestInvalid("object O { field f { type t, ; } }");

    [Test]
    public void TestParserValidation_LongFieldAllows_MissingSemicolon() =>
        TestInvalid("object O { field f { allows o } }");

    [Test]
    public void TestParserValidation_LongFieldAllows_MissingObject() =>
        TestInvalid("object O { field f { allows ; } }");

    [Test]
    public void TestParserValidation_LongFieldAttrib_MissingSemicolon() =>
        TestInvalid("object O { field f { attrib a } }");

    [Test]
    public void TestParserValidation_LongFieldAttrib_MissingAttribute() =>
        TestInvalid("object O { field f { attrib ; } }");

    #endregion

    #endregion
}
