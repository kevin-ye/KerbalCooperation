/******************************************************************************
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
 * ***************************************************************************/
#define VERBOSE
using System;

namespace ReeperCommon
{
    internal class Log
    {
        [Flags]
        internal enum LogMask : int
        {
            Normal = 1 << 0,
            Debug = 1 << 1,
            Verbose = 1 << 2,
            Performance = 1 << 3,
            Warning = 1 << 4,
            Error = 1 << 5,

            None = 0,
            All = ~0
        }


        // default logging levels
#if DEBUG
        internal static LogMask Level = LogMask.All;
#else
        internal static LogMask Level = (LogMask.Normal | LogMask.Warning | LogMask.Error);
#endif

        #region Assembly/Class Information
        /// <summary>
        /// Name of the Assembly that is running this MonoBehaviour
        /// </summary>
        internal static String _AssemblyName
        { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name; } }

        /// <summary>
        /// Name of the Class - including Derivations
        /// </summary>
        internal String _ClassName
        { get { return this.GetType().Name; } }
        #endregion


        #region Main debug functions

        private static String FormatMessage(string msg)
        {
            return string.Format("{0}, {1}", _AssemblyName, msg);
        }

        private static bool ShouldLog(LogMask messageType)
        {
            return (Log.Level & messageType) != 0;
        }




        internal static void Write(string message, LogMask level)
        {
            if (ShouldLog(level))
            {
                string fmsg = FormatMessage(message);
                if ((level & LogMask.Error) != 0)
                {
                    UnityEngine.Debug.LogError(fmsg); 
                }
                else if ((level & LogMask.Warning) != 0)
                {
                    UnityEngine.Debug.LogWarning(fmsg); 
                }
                else if ((level & LogMask.Normal) != 0)
                {
                    UnityEngine.Debug.Log(fmsg);
                } 
                else if ((level & LogMask.Performance) != 0)
                {
                    UnityEngine.Debug.Log(FormatMessage(string.Format("[PERF] {0}", message)));
                }
                else UnityEngine.Debug.Log(fmsg); 
            }
        }


        internal static void Write(string message, LogMask level, params object[] strParams)
        {
            if (ShouldLog(level))
                Write(string.Format(message, strParams), level);
        }

        internal static void SaveInto(ConfigNode parentNode)
        {
            var node = parentNode.AddNode(new ConfigNode("LogSettings"));
            node.AddValue("LogMask", ((int)Log.Level));

            var levelNames = Enum.GetNames(typeof(LogMask));
            var levelValues = Enum.GetValues(typeof(LogMask));

            node.AddValue("// Bit index", "message type");

            for (int i = 0; i < levelNames.Length - 1 /* exclude "all" */; ++i)
            {
                node.AddValue(string.Format("// Bit {0}", i), levelValues.GetValue(i));
            }

            Debug("Log.SaveInto = {0}", node.ToString());
        }

        internal static void LoadFrom(ConfigNode parentNode)
        {
            if (parentNode == null || !parentNode.HasNode("LogSettings"))
            {
                Warning("Log.LoadFrom failed, did not find LogSettings in: {0}", parentNode != null ? parentNode.ToString() : "<null ConfigNode>");
            }
            else
            {
                var node = parentNode.GetNode("LogSettings");

                try
                {
                    if (!node.HasValue("LogMask")) throw new Exception("No LogMask value in ConfigNode");

                    string mask = node.GetValue("LogMask");
                    int maskResult = 0;

                    if (int.TryParse(mask, out maskResult))
                    {
                        if (maskResult == 0)
                            Warning("Log.LoadFrom: Log disabled");

                        Level = (LogMask)maskResult;

                        Log.Normal("Loaded LogMask = {0} from ConfigNode", Level.ToString());
                    }
                    else Error("Log.LoadFrom:  LogMask value '{0}' cannot be converted to LogMask", mask);
                } catch (Exception e)
                {
                    Warning("Log.LoadFrom failed with exception: {0}", e);
                }
            }
        }
        #endregion


        internal static void Debug(string message, params object[] strParams)
        {
            Write(message, LogMask.Debug, strParams);
        }

        internal static void Normal(string message, params object[] strParams)
        {
            Write(message, LogMask.Normal, strParams);
        }

        internal static void Warning(string message, params object[] strParams)
        {
            Write(message, LogMask.Warning, strParams);
        }

        internal static void Error(string message, params object[] strParams)
        {
            Write(message, LogMask.Error, strParams);
        }

        internal static void Verbose(string message, params object[] strParams)
        {
            Write(message, LogMask.Verbose, strParams);
        }

        internal static void Performance(string message, params object[] strParams)
        {
            Write(message, LogMask.Performance, strParams);
        }

        
        // Since I use Write a lot, Log.Write needs a non-critical version
        // it'll just be normal output
        internal static void Write(string message, params object[] strParams)
        {
            Write(message, LogMask.Normal, strParams);
        }
    }
}
