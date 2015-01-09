using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KCoop
{
    // UnityEngine debug log warpper
    public class Logger
    {
        private const string fronter = "KCoop: ";
        public static void log(string info)
        {
            Debug.Log(fronter + info);
        }

        public static void error(string info)
        {
            Debug.LogError(fronter + info);
        }

        public static void exception(string info)
        {
            Debug.LogException(new Exception(fronter + info));
        }

        public static void warnning(string info)
        {
            Debug.LogWarning(fronter + info);
        }
    }
}