using System.Text;
using System.Text.RegularExpressions;

namespace Jbot.Model.Parse;

internal static partial class Lexer
{
    private const string BLOCK_START = "{";
    private const string BLOCK_END = "}";
    private const string STATEMENT_END = ";";
    private const string DECL_ID = "#";
    private const string TYPE_SEPARATOR = ",";
    private const string DECL_FIELD_BIND = "$";

    private static readonly string[] BREAKING_SYMBOLS =
        [BLOCK_START, BLOCK_END, STATEMENT_END, DECL_ID, TYPE_SEPARATOR, DECL_FIELD_BIND];

    private static string[] Scan(string input)
    {
        List<string> tokens = [];
        StringBuilder currentToken = new();
        int currentIndex = 0;

        while (currentIndex < input.Length)
        {
            char currentChar = input[currentIndex];

            if (char.IsWhiteSpace(currentChar) || BREAKING_SYMBOLS.Contains($"{currentChar}"))
            {
                // finalize token if there already is one
                if (currentToken.Length > 0)
                {
                    tokens.Add(currentToken.ToString());
                    currentToken = new StringBuilder();
                }

                if (BREAKING_SYMBOLS.Contains($"{currentChar}"))
                {
                    // directly add the token since it's one character long
                    // lets us move on to a new token without a space
                    tokens.Add(currentChar.ToString());
                }
            }
            else
            {
                currentToken.Append(currentChar);
            }

            currentIndex++;
        }

        // if there's an extra token add it
        if (currentToken.Length > 0) tokens.Add(currentToken.ToString());

        return [..tokens];
    }

    private static Symbol Evaluate(string token)
    {
        switch (token)
        {
            case "version": return new Symbol(SymbolType.DECL_VERSION, token);
            case "attrib": return new Symbol(SymbolType.DECL_ATTRIB, token);
            case "object": return new Symbol(SymbolType.DECL_OBJECT, token);
            case "bind": return new Symbol(SymbolType.DECL_BIND, token);
            case "field": return new Symbol(SymbolType.DECL_FIELD, token);
            case "allows": return new Symbol(SymbolType.DECL_ALLOWS, token);
            case "type": return new Symbol(SymbolType.DECL_TYPE, token);
            case BLOCK_START: return new Symbol(SymbolType.BLOCK_START, token);
            case BLOCK_END: return new Symbol(SymbolType.BLOCK_END, token);
            case STATEMENT_END: return new Symbol(SymbolType.STATEMENT_END, token);
            case DECL_ID: return new Symbol(SymbolType.DECL_ID, token);
            case TYPE_SEPARATOR: return new Symbol(SymbolType.TYPE_SEPARATOR, token);
            case DECL_FIELD_BIND: return new Symbol(SymbolType.DECL_FIELD_BIND, token);

            default:
                if (NumberMatchRegex().Match(token).Success)
                {
                    return new Symbol(SymbolType.NUMBER, token);
                }

                if (IdentifierMatchRegex().Match(token).Success)
                {
                    return new Symbol(SymbolType.IDENTIFIER, token);
                }

                if (DescendingIdentifierMatchRegex().Match(token).Success)
                {
                    return new Symbol(SymbolType.DESCENDING_IDENTIFIER, token);
                }

                throw new SyntaxException("invalid symbol " + token);
        }
    }

    public static Symbol[] Parse(string str) =>
        // tersely: splits the string, then for each token, evaluates it (makes it a symbol)
        [..Scan(str).Select(Evaluate)];

    [GeneratedRegex(@"\A[a-zA-Z_][a-zA-Z0-9_]*\z")]
    private static partial Regex IdentifierMatchRegex();

    [GeneratedRegex(@"\A[a-zA-Z_\.][a-zA-Z0-9_\.]*\z")]
    private static partial Regex DescendingIdentifierMatchRegex();

    [GeneratedRegex(@"\A[0-9]*\z")]
    private static partial Regex NumberMatchRegex();
}
