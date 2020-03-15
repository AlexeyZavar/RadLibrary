﻿#region

using System;
using System.Collections;

#endregion

namespace RadLibrary.Logging
{
    /// <summary>Defines the logger</summary>
    public class Logger
    {
        /// <summary>Initializes a new instance of the <see cref="Logger"/> class.</summary>
        /// <param name="name">The name.</param>
        /// <param name="settings">The settings.</param>
        public Logger(string name, LoggerSettings settings)
        {
            Name = name;
            Settings = settings;
        }

        /// <summary>Gets or sets the settings.</summary>
        /// <value>The settings.</value>
        public LoggerSettings Settings { get; set; }

        /// <summary>Gets the name.</summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>Logs the specified type.</summary>
        /// <param name="type">The type.</param>
        /// <param name="args">The arguments.</param>
        /// <exception cref="ArgumentOutOfRangeException">type is null</exception>
        public void Log(LogType type, params object[] args)
        {
            if (type < Settings.LogLevel)
                return;
            switch (type)
            {
                case LogType.Verbose:
                    Console.ForegroundColor = Settings.VerboseColor;
                    break;
                case LogType.Information:
                    Console.ForegroundColor = Settings.InformationColor;
                    break;
                case LogType.Warning:
                    Console.ForegroundColor = Settings.WarningColor;
                    break;
                case LogType.Error:
                    Console.ForegroundColor = Settings.ErrorColor;
                    break;
                case LogType.Exception:
                    Console.ForegroundColor = Settings.ExceptionColor;
                    break;
                case LogType.Deprecated:
                    Console.ForegroundColor = Settings.DeprecatedColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            Console.Write(Settings.LoggerPrefix + " ", DateTime.Now.ToString(Settings.TimeFormat), Name,
                type.ToString());

            var str = args[0]?.ToString();

            if (str?.Contains("{0}") == true)
            {
                for (var i = 1; i < args.Length; i++)
                    if (str.Contains("{" + (i - 1) + "}"))
                        str = str.Replace("{" + (i - 1) + "}", args[i].ToString());


                Console.WriteLine(str);
            }
            else
            {
                foreach (var arg in args)
                {
                    var handledArgument = HandleArgument(arg);
                    Console.Write(handledArgument);
                    Console.Write(" ");
                }

                Console.Write(Environment.NewLine);
            }

            Console.ResetColor();
        }

        /// <summary>Handles the argument.</summary>
        /// <param name="arg">The argument.</param>
        /// <returns>Returns formatted string</returns>
        private string HandleArgument(object arg)
        {
            switch (arg)
            {
                case DateTime date:
                    return date.ToString(Settings.TimeFormat);
                // Python styled list output
                case IList list:
                {
                    var str = "[";
                    foreach (var o in list) str += HandleArgument(o) + ", ";

                    return str.Remove(str.Length - 2) + "]";
                }
                // Python styled dictionary output
                case IDictionary dictionary:
                {
                    var str = "[";
                    foreach (var o in dictionary) str += HandleArgument(o) + ", ";

                    return str.Remove(str.Length - 2) + "]";
                }
                case DictionaryEntry pair:
                    return HandleArgument(pair.Key) + ": " + HandleArgument(pair.Value);
                default:
                    // @TODO: Custom handler
                    // CustomHandler(arg);

                    return arg.ToString();
            }
        }

        /// <summary>Alias for <see cref="Log"/> (Verbose)</summary>
        /// <param name="args">The arguments.</param>
        public void Verbose(params object[] args)
        {
            Log(LogType.Verbose, args);
        }

        /// <summary>Alias for <see cref="Log"/> (Information)</summary>
        /// <param name="args">The arguments.</param>
        public void Info(params object[] args)
        {
            Log(LogType.Information, args);
        }

        /// <summary>Alias for <see cref="Log"/> (Warning)</summary>
        /// <param name="args">The arguments.</param>
        public void Warn(params object[] args)
        {
            Log(LogType.Warning, args);
        }

        /// <summary>Alias for <see cref="Log"/> (Error)</summary>
        /// <param name="args">The arguments.</param>
        public void Error(params object[] args)
        {
            Log(LogType.Error, args);
        }

        /// <summary>Logs the specified exception</summary>
        /// <param name="ex">The exception</param>
        /// <exception cref="FormatException"><see cref="LoggerSettings.ExceptionString"/> doesn't contains {0}</exception>
        public void Exception(Exception ex)
        {
            if (!Settings.ExceptionString.Contains("{0}"))
                throw new FormatException();
            Log(LogType.Exception, Settings.ExceptionString, ex.GetType(), Name);
        }

        /// <summary>Logs the deprecated part of code</summary>
        /// <param name="old">Deprecated object</param>
        /// <param name="replacement">Replacement</param>
        /// <exception cref="FormatException"><see cref="LoggerSettings.ExceptionString"/> doesn't contains {0}</exception>
        public void Deprecated(object old, object replacement)
        {
            if (!Settings.DeprecatedString.Contains("{0}"))
                throw new FormatException();
            Log(LogType.Deprecated, Settings.DeprecatedString, old, replacement);
        }
    }
}