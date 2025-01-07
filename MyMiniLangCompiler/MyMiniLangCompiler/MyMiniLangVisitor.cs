using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyMiniLangCompiler
{
    public class MyMiniLangVisitor : MiniLangBaseVisitor<object>
    {
        private readonly List<(string TokenType, string Lexeme, int Line)> _tokens
            = new List<(string TokenType, string Lexeme, int Line)>();

        private Dictionary<string, (string type, string? initValue)> _globalVars
            = new Dictionary<string, (string type, string? initValue)>();

        private List<FunctionInfo> _functions
            = new List<FunctionInfo>();

        // ================== 1. Colectarea tokenilor ==================
        public override object VisitTerminal(ITerminalNode node)
        {
            var symbol = node.Symbol;
            string tokenName = MiniLangLexer.DefaultVocabulary.GetSymbolicName(symbol.Type);
            string lexeme = symbol.Text;
            int line = symbol.Line;

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

            if (_globalVars.ContainsKey(varName))
            {
                Console.WriteLine($"[Eroare semantica] Variabila globală '{varName}' este deja declarată.");
            }
            else
            {
                if (varType == "int" && initValue != null && initValue.StartsWith("\""))
                {
                    Console.WriteLine($"[Eroare semantica] Variabila '{varName}' (int) nu poate primi un string.");
                }

                _globalVars[varName] = (varType, initValue);
            }

            return base.VisitGlobalVarDecl(context);
        }

        // ================== 3. Funcții ==================
        public override object VisitFunctionDecl([NotNull] MiniLangParser.FunctionDeclContext context)
        {
            string returnType = context.type().GetText();
            string funcName = context.ID().GetText();

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

            foreach (var existingFunc in _functions)
            {
                if (existingFunc.FuncName == funcName && existingFunc.HasSameParameters(parameters))
                {
                    Console.WriteLine($"[Eroare semantica] Funcția '{funcName}' cu această listă de parametri există deja.");
                }
            }

            var funcInfo = new FunctionInfo
            {
                FuncName = funcName,
                ReturnType = returnType,
                Parameters = parameters
            };

            var result = base.VisitFunctionDecl(context);

            _functions.Add(funcInfo);

            return result;
        }


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

            return base.VisitStructDecl(context);
        }
    }

    public class ParameterInfo
    {
        public string ParamName { get; set; } = "";
        public string ParamType { get; set; } = "";
    }

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
                if (Parameters[i].ParamType != otherParams[i].ParamType)
                    return false;
            }
            return true;
        }
    }
}
