﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldursLab.Essentials.Extensions
{
    public static class ActionExtensions
    {
        public static string MethodInformationToString(this Action eventHandler)
        {
            return string.Format("{0}.{1}",
                eventHandler.Method.DeclaringType != null
                    ? eventHandler.Method.DeclaringType.FullName
                    : string.Empty,
                eventHandler.Method.Name);
        }

        public static string MethodInformationToString<T>(this Action<T> eventHandler) where T : EventArgs
        {
            return string.Format("{0}.{1}",
                eventHandler.Method.DeclaringType != null
                    ? eventHandler.Method.DeclaringType.FullName
                    : string.Empty,
                eventHandler.Method.Name);
        }
    }
}
