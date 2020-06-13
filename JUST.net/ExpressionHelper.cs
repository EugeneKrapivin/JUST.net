﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JUST
{
    internal class ExpressionHelper
    {
        internal const char EscapeChar = '/'; //do not use backslash, it is already the escape char in JSON
        private const string FunctionAndArgumentsRegex = "^#(.+?)[(](.*)[)]$";

        internal static bool TryParseFunctionNameAndArguments(string input, out string functionName, out string arguments)
        {
            var match = new Regex(FunctionAndArgumentsRegex).Match(input);
            functionName = match.Success ? match.Groups[1].Value : input;
            arguments = match.Success ? match.Groups[2].Value : null;
            return match.Success;
        }

        internal static string[] SplitArguments(string functionString)
        {
            bool brackettOpen = false;

            List<string> arguments = new List<string>();
            int index = 0;

            int openBrackettCount = 0;
            int closebrackettCount = 0;
            bool isEscapedChar = false;

            for (int i = 0; i < functionString.Length; i++)
            {
                char currentChar = functionString[i];
                if (currentChar == EscapeChar)
                {
                    isEscapedChar = !isEscapedChar;
                    continue;
                }
                if (currentChar == '(')
                {
                    if (!isEscapedChar) { openBrackettCount++; }
                    else { isEscapedChar = !isEscapedChar; }
                }
                else if (currentChar == ')')
                {
                    if (!isEscapedChar) { closebrackettCount++; }
                    else { isEscapedChar = !isEscapedChar; }
                }

                brackettOpen = openBrackettCount != closebrackettCount;
                if (currentChar == ',' && !brackettOpen)
                {
                    if (!isEscapedChar)
                    {
                        arguments.Add(Unescape(index != 0 ?
                            functionString.Substring(index + 1, i - index - 1) :
                            functionString.Substring(index, i)));
                        index = i;
                    }
                    else { isEscapedChar = !isEscapedChar; }
                }
                else { isEscapedChar = false; }
            }

            arguments.Add(index > 0 ? 
                Unescape(functionString.Substring(index + 1, functionString.Length - index - 1)) : 
                functionString);

            return arguments.ToArray();
        }

        private static string Unescape(string str)
        {
            /* TODO use Regex to unnescape to avoid sequencial replaces without back/forward lookups
            * Example: '\\)' -> replace '\\' = '\)' -> replace '\)' = ')' 
            */
            //Regex.Replace(commaSplit[i], $"(?<![\\(])\\{EscapeChar}\\((?![\\)])", expr => "(")
            //Regex.Replace(commaSplit[i], $"(?<![\\(])\\{EscapeChar}\\)(?![\\)])", expr => ")")
            return !IsFunction(str) ?
                str
                    .Replace($"{EscapeChar}{EscapeChar}", $"{EscapeChar}")
                    .Replace($"{EscapeChar}(", "(")
                    .Replace($"{EscapeChar})", ")")
                    .Replace($"{EscapeChar},", ",") : 
                str;
        }

        internal static bool IsFunction(string val)
        {
            return Regex.IsMatch(val, "^\\s*#");
        }

        internal static string UnescapeSharp(string val)
        {
            return Regex.Replace(val, "^\\s*\\/#", "#");
        }
    }
}
