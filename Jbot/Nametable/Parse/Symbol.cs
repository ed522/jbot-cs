namespace Jbot.Nametable.Parse;

internal enum SymbolType
{
    BLOCK_START, BLOCK_END, STATEMENT_END,
    DECL_FIELD, DECL_OBJECT, DECL_ATTRIB, DECL_BIND, DECL_VERSION,
    DECL_FIELD_BIND, DECL_TYPE, DECL_ALLOWS, DECL_ID, 
    IDENTIFIER, NUMBER, DESCENDING_IDENTIFIER, TYPE_SEPARATOR
}

internal readonly struct Symbol(SymbolType type, string value)
{
    public readonly SymbolType type = type;
    public readonly string value = value;
}