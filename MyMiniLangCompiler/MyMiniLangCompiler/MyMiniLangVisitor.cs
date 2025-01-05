using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyMiniLangCompiler
{
    public class MyMiniLangVisitor : MiniLangBaseVisitor<object>
    {
        // A. Lista de tokeni colectați (pentru cerința: salvare <token, lexemă, nr. linie>)
        private readonly List<(string TokenType, string Lexeme, int Line)> _tokens
            = new List<(string TokenType, string Lexeme, int Line)>();

        // B. Variabile globale: nume -> (tip, valoare_inicializare)
        private Dictionary<string, (string type, string? initValue)> _globalVars
            = new Dictionary<string, (string type, string? initValue)>();

        // C. Informații despre funcții
        private List<FunctionInfo> _functions
            = new List<FunctionInfo>();

        // ================== 1. Colectarea tokenilor ==================
        public override object VisitTerminal(ITerminalNode node)
        {
            var symbol = node.Symbol;
            string tokenName = MiniLangLexer.DefaultVocabulary.GetSymbolicName(symbol.Type);
            string lexeme = symbol.Text;
            int line = symbol.Line;

            // Ignorăm anumite token-uri de tip whitespace/comentariu
            if (tokenName != "WS" && tokenName != "LINE_COMMENT" && tokenName != "BLOCK_COMMENT")
            {
                _tokens.Add((tokenName, lexeme, line));
            }

            return base.VisitTerminal(node);
        }

        // ================== 2. Variabile globale ==================
        public override object VisitGlobalVarDecl([NotNull] MiniLangParser.GlobalVarDeclContext context)
        {
            string varType = context.type().GetText();
            string varName = context.ID().GetText();
            string? initValue = null;

            if (context.expression() != null)
            {
                initValue = context.expression().GetText();
            }

            // Verificare semantică: variabila globală există deja?
            if (_globalVars.ContainsKey(varName))
            {
                Console.WriteLine($"[Eroare semantica] Variabila globală '{varName}' este deja declarată.");
            }
            else
            {
                // Exemplu simplificat de verificare a compatibilității tipului:
                if (varType == "int" && initValue != null && initValue.StartsWith("\""))
                {
                    Console.WriteLine($"[Eroare semantica] Variabila '{varName}' (int) nu poate primi un string.");
                }
                // Puteți extinde această logică pentru float/double etc.

                _globalVars[varName] = (varType, initValue);
            }

            return base.VisitGlobalVarDecl(context);
        }

        // ================== 3. Funcții ==================
        public override object VisitFunctionDecl([NotNull] MiniLangParser.FunctionDeclContext context)
        {
            string returnType = context.type().GetText();
            string funcName = context.ID().GetText();

            // Colectăm parametrii
            List<ParameterInfo> parameters = new List<ParameterInfo>();
            if (context.paramList() != null)
            {
                foreach (var paramDecl in context.paramList().paramDecl())
                {
                    string pType = paramDecl.type().GetText();
                    string pName = paramDecl.ID().GetText();
                    parameters.Add(new ParameterInfo { ParamName = pName, ParamType = pType });
                }
            }

            // Verificare semantica: unicitatea funcțiilor (nume + semnătură)
            foreach (var existingFunc in _functions)
            {
                if (existingFunc.FuncName == funcName && existingFunc.HasSameParameters(parameters))
                {
                    Console.WriteLine($"[Eroare semantica] Funcția '{funcName}' cu această listă de parametri există deja.");
                }
            }

            // Creăm obiectul care descrie funcția
            var funcInfo = new FunctionInfo
            {
                FuncName = funcName,
                ReturnType = returnType,
                Parameters = parameters
            };

            // Vizităm corpul funcției (block) pentru a colecta eventualele variabile locale, structuri, etc.
            // => Apelăm visit ca să intre și în block
            var result = base.VisitFunctionDecl(context);

            // Adăugăm funcția în lista noastră
            _functions.Add(funcInfo);

            return result;
        }

        // EXEMPLED: Aici puteți suprascrie vizitarea block-ului, ifStatement, forStatement etc.
        //           pentru a colecta structuri de control (de ex. <if, nr. linie>).

        // ================== 4. Metode de scriere în fișiere ==================
        public void PrintTokens(string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            {
                foreach (var t in _tokens)
                {
                    writer.WriteLine($"<{t.TokenType}, {t.Lexeme}, {t.Line}>");
                }
            }
        }

        public void PrintGlobalVariables(string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            {
                foreach (var kvp in _globalVars)
                {
                    var (type, init) = kvp.Value;
                    string initVal = init == null ? "" : $"= {init}";
                    writer.WriteLine($"{type} {kvp.Key} {initVal}");
                }
            }
        }

        public void PrintFunctions(string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            {
                foreach (var f in _functions)
                {
                    // Simplu exemplu de afișare
                    writer.WriteLine($"Funcție: {f.FuncName} (return {f.ReturnType})");
                    writer.WriteLine($" - Parametri: {f.Parameters.Count}");
                    writer.WriteLine("--------------");
                }
            }
        }
        public override object VisitStructDecl([NotNull] MiniLangParser.StructDeclContext context)
        {
            string structName = context.ID().GetText();
            Console.WriteLine("Am găsit struct: " + structName);

            // ... colectați info, ex. cîmpuri, funcții ...
            return base.VisitStructDecl(context);
        }
    }

    // Clasă ajutătoare pentru parametrii unei funcții
    public class ParameterInfo
    {
        public string ParamName { get; set; } = "";
        public string ParamType { get; set; } = "";
    }

    // Clasă ajutătoare pentru descrierea unei funcții
    public class FunctionInfo
    {
        public string FuncName { get; set; } = "";
        public string ReturnType { get; set; } = "";
        public List<ParameterInfo> Parameters { get; set; } = new List<ParameterInfo>();

        public bool HasSameParameters(List<ParameterInfo> otherParams)
        {
            if (Parameters.Count != otherParams.Count) return false;
            for (int i = 0; i < Parameters.Count; i++)
            {
                // Comparam tipurile parametrelor (pentru a determina "semnătura")
                if (Parameters[i].ParamType != otherParams[i].ParamType)
                    return false;
            }
            return true;
        }
    }
}
