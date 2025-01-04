using Antlr4.Runtime;
using System;
using System.IO;  // ATENȚIE: este necesar pentru TextWriter

public class MiniLangErrorListener : BaseErrorListener
{
    public override void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e
    )
    {
        Console.WriteLine($"[Eroare sintactică] Linia {line}, col {charPositionInLine}: {msg}");
    }
}
