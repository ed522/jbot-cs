namespace Jbot.Nametable.Parse;

public static class NametableCompiler
{
    public static Nametable CompileFile(string path)
    {
        string contents = File.ReadAllText(path);
        return CompileString(contents);
    }

    public static Nametable CompileString(string str)
    {
        Symbol[] symbols = Lexer.Parse(str);
        Node tree = new Parser(symbols).Parse();
        return Compiler.Compile(tree);
    }
}
