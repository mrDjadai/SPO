using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SPO_1
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

        private static GilbResult Compute(List<string> tokens)
        {
            int cl = 0;
            int depth = 0;
            int maxDepth = 0;
            int statements = 0;

            var ifStack = new Stack<IfFrame>();
            var switchStack = new Stack<SwitchFrame>();

            for (int i = 0; i < tokens.Count; i++)
            {
                string tok = tokens[i];

                switch (tok)
                {
                    case "if":
                        {
                            bool isElseIf = i > 0 && tokens[i - 1] == "else";
                            if (!isElseIf)
                            {
                                cl++;
                                depth++;
                                if (depth > maxDepth) maxDepth = depth;
                                ifStack.Push(new IfFrame { StartDepth = depth });
                            }
                            i = SkipParenGroup(tokens, i);
                            break;
                        }

                    case "for":
                    case "while":
                        {
                            cl++;
                            depth++;
                            if (depth > maxDepth) maxDepth = depth;
                            ifStack.Push(new IfFrame { StartDepth = depth });
                            i = SkipParenGroup(tokens, i);
                            break;
                        }

                    case "do":
                        {
                            cl++;
                            depth++;
                            if (depth > maxDepth) maxDepth = depth;
                            ifStack.Push(new IfFrame { StartDepth = depth });
                            break;
                        }

                    case "switch":
                        {
                            int caseCount = CountCases(tokens, i);
                            int addedIfs = Math.Max(0, caseCount - 1);
                            int addedDepth = Math.Max(0, caseCount - 2);

                            cl += addedIfs;
                            depth += addedDepth;
                            if (depth > maxDepth) maxDepth = depth;

                            switchStack.Push(new SwitchFrame
                            {
                                DepthBefore = depth - addedDepth
                            });

                            i = SkipParenGroup(tokens, i);
                            break;
                        }

                    case "?":
                        cl++;
                        break;

                    case "}":
                        {
                            if (ifStack.Count > 0)
                            {
                                var frame = ifStack.Pop();
                                depth = frame.StartDepth - 1;
                            }
                            else if (switchStack.Count > 0 &&
                                     IsSwitchClose(tokens, i))
                            {
                                var sf = switchStack.Pop();
                                depth = sf.DepthBefore;
                            }
                            break;
                        }

                    case ";":
                        statements++;
                        break;
                }
            }

            var result = new GilbResult
            {
                CL = cl,
                CLI = maxDepth,
                TotalStatements = statements
            };
            result.Cl = statements > 0 ? (double)cl / statements : 0.0;
            return result;
        }

        private static bool IsSwitchClose(List<string> tokens, int braceIndex)
        {
            if (braceIndex + 1 >= tokens.Count) return true;
            string next = tokens[braceIndex + 1];
            return next != "case" && next != "default";
        }

        private static int SkipParenGroup(List<string> tokens, int startIndex)
        {
            int i = startIndex;
            while (i < tokens.Count && tokens[i] != "(") i++;
            if (i >= tokens.Count) return startIndex;

            int depth = 0;
            for (; i < tokens.Count; i++)
            {
                if (tokens[i] == "(") depth++;
                else if (tokens[i] == ")")
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
            return tokens.Count - 1;
        }

        private static int CountCases(List<string> tokens, int switchIndex)
        {
            int i = switchIndex;
            while (i < tokens.Count && tokens[i] != "{") i++;
            if (i >= tokens.Count) return 0;

            int braceDepth = 1;
            int caseCount = 0;
            for (i++; i < tokens.Count; i++)
            {
                string t = tokens[i];
                if (t == "{") braceDepth++;
                else if (t == "}")
                {
                    braceDepth--;
                    if (braceDepth == 0) break;
                }
                else if (t == "case" || t == "default" && braceDepth == 1)
                {
                    caseCount++;
                }
            }
            return caseCount;
        }

        private class IfFrame { public int StartDepth; }
        private class SwitchFrame { public int DepthBefore; }
    }
}