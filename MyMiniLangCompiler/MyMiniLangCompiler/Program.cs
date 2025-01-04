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
            // Verificăm dacă s-a dat un fișier ca argument
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
                // 1. Creăm un stream ANTLR din fișier
                AntlrFileStream inputStream = new AntlrFileStream(filePath);

                // 2. Creăm lexer-ul
                var lexer = new MiniLangLexer(inputStream);
                // Dacă vrem să ascultăm erori lexicale, putem face:
                // lexer.RemoveErrorListeners();
                // lexer.AddErrorListener(new MiniLangErrorListener());

                // 3. Creăm lista de tokeni
                CommonTokenStream tokens = new CommonTokenStream(lexer);

                // 4. Creăm parser-ul
                var parser = new MiniLangParser(tokens);
                parser.RemoveErrorListeners();                  // eliminăm listener-ul default
                parser.AddErrorListener(new MiniLangErrorListener());  // adăugăm unul personalizat

                // 5. Parsăm conform regulii de start 'program'
                IParseTree tree = parser.program();

                // 6. Creăm un visitor ce va face analiza suplimentară (semantică etc.)
                var visitor = new MyMiniLangVisitor();
                visitor.Visit(tree);

                // 7. Salvăm rezultatele: tokeni, variabile globale, funcții, etc.
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
