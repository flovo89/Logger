using System;
using System.Collections.Generic;
using System.Text;

using Logger.Common.DataTypes;
using Logger.Common.ObjectModel.Exceptions;




namespace Logger.Common.Runtime
{
    public static class CommandLine
    {
        #region Static Properties/Indexer

        public static string ProcessCommandLine
        {
            get
            {
                return Environment.CommandLine;
            }
        }

        #endregion




        #region Static Methods

        public static string Build (IList<string> literals)
        {
            return CommandLine.Build(null, null, literals);
        }

        public static string Build (IDictionary<string, IList<string>> parameters)
        {
            return CommandLine.Build(null, parameters, null);
        }

        public static string Build (IDictionary<string, IList<string>> parameters, IList<string> literals)
        {
            return CommandLine.Build(null, parameters, literals);
        }

        public static string Build (string executable)
        {
            return CommandLine.Build(executable, null, null);
        }

        public static string Build (string executable, IList<string> literals)
        {
            return CommandLine.Build(executable, null, literals);
        }

        public static string Build (string executable, IDictionary<string, IList<string>> parameters)
        {
            return CommandLine.Build(executable, parameters, null);
        }

        public static string Build (string executable, IDictionary<string, IList<string>> parameters, IList<string> literals)
        {
            if (executable != null)
            {
                if (executable.IsEmpty())
                {
                    throw new EmptyStringArgumentException(nameof(executable));
                }
            }

            if (parameters != null)
            {
                if (parameters.Count == 0)
                {
                    parameters = null;
                }
            }

            if (literals != null)
            {
                if (literals.Count == 0)
                {
                    literals = null;
                }
            }

            StringBuilder commandLine = new StringBuilder();

            if (executable != null)
            {
                commandLine.Append("\"");
                commandLine.Append(executable);
                commandLine.Append("\"");
            }

            if (parameters != null)
            {
                foreach (KeyValuePair<string, IList<string>> parameter in parameters)
                {
                    if (parameter.Value.Count == 0)
                    {
                        commandLine.Append(" -");
                        commandLine.Append(parameter.Key);
                    }
                    else
                    {
                        foreach (string value in parameter.Value)
                        {
                            commandLine.Append(" -");
                            commandLine.Append(parameter.Key);
                            commandLine.Append("=\"");

                            commandLine.Append(value.Replace("\"", "\\\""));

                            commandLine.Append("\"");
                        }
                    }
                }
            }

            if (literals != null)
            {
                foreach (string literal in literals)
                {
                    commandLine.Append(" \"");

                    commandLine.Append(literal.Replace("\"", "\\\""));

                    commandLine.Append("\"");
                }
            }

            return commandLine.ToString().Trim();
        }

        public static string Parse ()
        {
            IDictionary<string, IList<string>> parameters = null;
            IList<string> literals = null;

            return CommandLine.Parse(CommandLine.ProcessCommandLine, out parameters, out literals);
        }

        public static string Parse (string commandLine)
        {
            IDictionary<string, IList<string>> parameters = null;
            IList<string> literals = null;

            return CommandLine.Parse(commandLine, out parameters, out literals);
        }

        public static string Parse (out IList<string> literals)
        {
            IDictionary<string, IList<string>> parameters = null;

            return CommandLine.Parse(CommandLine.ProcessCommandLine, out parameters, out literals);
        }

        public static string Parse (string commandLine, out IList<string> literals)
        {
            IDictionary<string, IList<string>> parameters = null;

            return CommandLine.Parse(commandLine, out parameters, out literals);
        }

        public static string Parse (out IDictionary<string, IList<string>> parameters)
        {
            IList<string> literals = null;

            return CommandLine.Parse(CommandLine.ProcessCommandLine, out parameters, out literals);
        }

        public static string Parse (string commandLine, out IDictionary<string, IList<string>> parameters)
        {
            IList<string> literals = null;

            return CommandLine.Parse(commandLine, out parameters, out literals);
        }

        public static string Parse (out IDictionary<string, IList<string>> parameters, out IList<string> literals)
        {
            return CommandLine.Parse(CommandLine.ProcessCommandLine, out parameters, out literals);
        }

        public static string Parse (string commandLine, out IDictionary<string, IList<string>> parameters, out IList<string> literals)
        {
            if (commandLine == null)
            {
                throw new ArgumentNullException(nameof(commandLine));
            }

            parameters = null;
            literals = null;

            IDictionary<string, IList<string>> parameterDictionary = new Dictionary<string, IList<string>>(StringComparer.InvariantCulture);
            IList<string> literalList = new List<string>();

            bool eos = false;

            for (int i1 = 0; i1 < commandLine.Length; i1++)
            {
                string parameterKeyToAdd = null;
                string parameterValueToAdd = null;

                if (!char.IsWhiteSpace(commandLine[i1]))
                {
                    if (commandLine[i1] == '-')
                    {
                        i1++;

                        if (i1 < commandLine.Length)
                        {
                            string key = CommandLine.ReadUntil(commandLine, ref i1, out eos, (v, p) => ( v[p] == '=' ) || char.IsWhiteSpace(v[p]));

                            if (eos)
                            {
                                parameterKeyToAdd = key;
                                parameterValueToAdd = null;
                            }
                            else
                            {
                                i1++;

                                if (i1 >= commandLine.Length)
                                {
                                    parameterKeyToAdd = key;
                                    parameterValueToAdd = null;
                                }
                                else
                                {
                                    string value = null;

                                    if (commandLine[i1] == '\"')
                                    {
                                        i1++;

                                        if (i1 >= commandLine.Length)
                                        {
                                            value = null;
                                        }
                                        else
                                        {
                                            value = CommandLine.ReadUntil(commandLine, ref i1, out eos, (v, p) => ( v[p] == '\"' ) && ( v[p - 1] != '\\' ));
                                        }
                                    }
                                    else
                                    {
                                        value = CommandLine.ReadUntil(commandLine, ref i1, out eos, (v, p) => char.IsWhiteSpace(v[p]));
                                    }

                                    parameterKeyToAdd = key;
                                    parameterValueToAdd = value;
                                }
                            }
                        }
                    }
                    else if (commandLine[i1] == '\"')
                    {
                        i1++;

                        if (i1 < commandLine.Length)
                        {
                            string literal = CommandLine.ReadUntil(commandLine, ref i1, out eos, (v, p) => ( v[p] == '\"' ) && ( v[p - 1] != '\\' ));

                            literalList.Add(literal);
                        }
                    }
                    else
                    {
                        string literal = CommandLine.ReadUntil(commandLine, ref i1, out eos, (v, p) => char.IsWhiteSpace(v[p]));

                        if (!literal.IsEmpty(true))
                        {
                            literalList.Add(literal);
                        }
                    }
                }

                if (parameterKeyToAdd != null)
                {
                    parameterKeyToAdd = parameterKeyToAdd.Trim();

                    if (!parameterDictionary.ContainsKey(parameterKeyToAdd))
                    {
                        parameterDictionary.Add(parameterKeyToAdd, new List<string>());
                    }

                    if (parameterValueToAdd != null)
                    {
                        parameterDictionary[parameterKeyToAdd].Add(parameterValueToAdd);
                    }
                }
            }

            parameters = parameterDictionary;
            literals = literalList;

            if (literalList.Count == 0)
            {
                return null;
            }

            string executable = literalList[0];
            literalList.RemoveAt(0);

            return executable;
        }

        private static string ReadUntil (string value, ref int position, out bool eos, Func<string, int, bool> condition)
        {
            StringBuilder result = new StringBuilder();

            while (true)
            {
                if (position >= value.Length)
                {
                    eos = true;
                    return result.ToString();
                }

                if (condition(value, position))
                {
                    eos = false;
                    return result.ToString();
                }

                result.Append(value[position]);

                position++;
            }
        }

        #endregion
    }
}
