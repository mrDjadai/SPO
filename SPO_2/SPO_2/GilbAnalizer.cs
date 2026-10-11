using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SPO_2
{
    public class GilbResult
    {
        public int CL { get; set; }              // абсолютная сложность
        public double Cl { get; set; }           // относительная сложность
        public int CLI { get; set; }             // максимальный уровень вложенности
        public int TotalStatements { get; set; }
    }

    public static class JavaGilbAnalyzer
    {
        private static readonly Regex TokenRegex = new(
            @"//[^\n]*" +
            @"|/\*.*?\*/" +
            @"|""(?:\\.|[^""\\])*""" +
            @"|'(?:\\.|[^'\\])*'" +
            @"|\b[A-Za-z_$][A-Za-z0-9_$]*\b" +
            @"|\d+(?:\.\d+)?" +
            @"|\S",
            RegexOptions.Singleline | RegexOptions.Compiled);

        public static GilbResult Analyze(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return new GilbResult();

            var tokens = Tokenize(source);
            return Compute(tokens);
        }

        // ---------------- Токенизация ----------------
        private static List<string> Tokenize(string src)
        {
            var result = new List<string>();
            foreach (Match m in TokenRegex.Matches(src))
            {
                string t = m.Value;
                if (t.StartsWith("//") || t.StartsWith("/*") ||
                    t.StartsWith("\"") || t.StartsWith("'"))
                    continue;
                result.Add(t);
            }
            return result;
        }

        // ---------------- Основной расчёт ----------------
        private static GilbResult Compute(List<string> tokens)
        {
            int cl = 0;
            int statements = 0;

            var chainStack = new Stack<Chain>();
            int ternaryDepth = 0;       // открытые ?:
            int openIfs = 0;
            int maxOpenIfs = 0;

            void RecalcOpenIfs()
            {
                openIfs = ternaryDepth;
                foreach (var c in chainStack) openIfs += c.Depth;
                if (openIfs > maxOpenIfs) maxOpenIfs = openIfs;
            }

            for (int i = 0; i < tokens.Count; i++)
            {
                string tok = tokens[i];

                switch (tok)
                {
                    case "if":
                        {
                            bool isElseIf = i > 0 && tokens[i - 1] == "else";
                            cl++;

                            if (isElseIf && chainStack.Count > 0 &&
                                chainStack.Peek().Kind == ChainKind.If &&
                                !chainStack.Peek().HasElse)
                            {
                                chainStack.Peek().Depth++;
                                chainStack.Peek().HasElse = false;
                            }
                            else
                            {
                                chainStack.Push(new Chain
                                {
                                    Kind = ChainKind.If,
                                    Depth = 1,
                                    HasElse = false,
                                    BracesOpened = 0
                                });
                            }

                            RecalcOpenIfs();
                            i = SkipParenGroup(tokens, i);
                            break;
                        }

                    case "else":
                        {
                            bool isElseIf = i + 1 < tokens.Count && tokens[i + 1] == "if";
                            if (!isElseIf && chainStack.Count > 0)
                                chainStack.Peek().HasElse = true;
                            break;
                        }

                    case "switch":
                        {
                            chainStack.Push(new Chain
                            {
                                Kind = ChainKind.Switch,
                                Depth = 0,
                                HasElse = false,
                                BracesOpened = 0
                            });
                            RecalcOpenIfs();
                            i = SkipParenGroup(tokens, i);
                            break;
                        }

                    case "case":
                        {
                            cl++;
                            if (chainStack.Count > 0 &&
                                chainStack.Peek().Kind == ChainKind.Switch)
                            {
                                chainStack.Peek().Depth++;
                            }
                            ternaryDepth = 0;   // между case ?: не переносятся
                            RecalcOpenIfs();
                            break;
                        }

                    case "default":
                        {
                            if (chainStack.Count > 0 &&
                                chainStack.Peek().Kind == ChainKind.Switch)
                            {
                                chainStack.Peek().HasElse = true;
                            }
                            ternaryDepth = 0;
                            RecalcOpenIfs();
                            break;
                        }

                    case "for":
                    case "while":
                    case "do":
                        {
                            cl++;
                            chainStack.Push(new Chain
                            {
                                Kind = ChainKind.Loop,
                                Depth = 1,
                                HasElse = false,
                                BracesOpened = 0
                            });
                            RecalcOpenIfs();
                            if (tok != "do") i = SkipParenGroup(tokens, i);
                            break;
                        }

                    // -------- тернарный оператор --------
                    case "?":
                        {
                            cl++;
                            ternaryDepth++;
                            RecalcOpenIfs();
                            break;
                        }

                    case ":":
                        {
                            if (ternaryDepth > 0)
                            {
                                ternaryDepth--;
                                RecalcOpenIfs();
                            }
                            // иначе — метка или for-each, игнорируем
                            break;
                        }

                    case "{":
                        {
                            if (!IsChainBodyOpener(tokens, i) && chainStack.Count > 0)
                                chainStack.Peek().BracesOpened++;
                            break;
                        }

                    case "}":
                        {
                            if (chainStack.Count > 0)
                            {
                                var top = chainStack.Peek();

                                if (top.BracesOpened > 0)
                                {
                                    top.BracesOpened--;
                                }
                                else
                                {
                                    bool nextIsElse = i + 1 < tokens.Count &&
                                                      tokens[i + 1] == "else";

                                    if (!nextIsElse || top.HasElse)
                                    {
                                        chainStack.Pop();
                                        if (ternaryDepth > 0) ternaryDepth = 0;
                                    }
                                }
                                RecalcOpenIfs();
                            }
                            break;
                        }

                    case ";":
                        statements++;
                        break;
                }
            }
            statements += cl;

            var result = new GilbResult
            {
                CL = cl,
                CLI = maxOpenIfs > 0 ? maxOpenIfs - 1 : 0,
                TotalStatements = statements
            };
            result.Cl = statements > 0 ? (double)cl / statements : 0.0;
            return result;
        }
        // ---------------- Вспомогательные ----------------

        private enum ChainKind { If, Loop, Switch }

        private class Chain
        {
            public ChainKind Kind;
            public int Depth;          // число if / case в цепочке
            public bool HasElse;       // встретился ли else / default
            public int BracesOpened;   // вложенные { внутри текущей ветки
        }

        /// <summary>
        /// Возвращает true, если '{' открывает тело if / else / case /
        /// for / while / do / switch / default, а не отдельный вложенный блок.
        /// </summary>
        private static bool IsChainBodyOpener(List<string> tokens, int braceIndex)
        {
            int j = braceIndex - 1;
            if (j < 0) return false;

            // Пропускаем ')' закрытия условия if / for / while / switch
            if (tokens[j] == ")")
            {
                int d = 0;
                for (; j >= 0; j--)
                {
                    if (tokens[j] == ")") d++;
                    else if (tokens[j] == "(")
                    {
                        d--;
                        if (d == 0) { j--; break; }
                    }
                }
            }

            if (j < 0) return false;
            string prev = tokens[j];
            return prev == "if" || prev == "else" || prev == "case" ||
                   prev == "for" || prev == "while" || prev == "do" ||
                   prev == "switch" || prev == "default";
        }

        /// <summary>
        /// Пропускает группу в круглых скобках, начиная с текущего токена,
        /// и возвращает индекс закрывающей ')'.
        /// </summary>
        private static int SkipParenGroup(List<string> tokens, int startIndex)
        {
            int i = startIndex;
            while (i < tokens.Count && tokens[i] != "(") i++;
            if (i >= tokens.Count) return startIndex;

            int d = 0;
            for (; i < tokens.Count; i++)
            {
                if (tokens[i] == "(") d++;
                else if (tokens[i] == ")")
                {
                    d--;
                    if (d == 0) return i;
                }
            }
            return tokens.Count - 1;
        }
    }
}