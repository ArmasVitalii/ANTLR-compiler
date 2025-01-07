using System;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace MyMiniLangCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Utilizare: MyMiniLangCompiler <fisier_sursa>");
                return;
            }

            string filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Fișierul {filePath} nu există.");
                return;
            }

            try
            {
                AntlrFileStream inputStream = new AntlrFileStream(filePath);

                var lexer = new MiniLangLexer(inputStream);

                CommonTokenStream tokens = new CommonTokenStream(lexer);

                var parser = new MiniLangParser(tokens);
                parser.RemoveErrorListeners();                  
                parser.AddErrorListener(new MiniLangErrorListener());  

                IParseTree tree = parser.program();

                var visitor = new MyMiniLangVisitor();
                visitor.Visit(tree);

                visitor.PrintTokens("tokens.txt");
                visitor.PrintGlobalVariables("global_vars.txt");
                visitor.PrintFunctions("functions.txt");

                Console.WriteLine("Analiza s-a încheiat cu succes!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la parsare: {ex.Message}");
            }
        }
    }
}
