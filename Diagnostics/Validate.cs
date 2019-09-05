/////////////////////////////////////////////////////////////////////////////////
// paint.net                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, and contributors.                  //
// All Rights Reserved.                                                        //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;

namespace PaintDotNet.Diagnostics
{
    public static class Validate
    {
        private static void ThrowArgumentNullException(string paramName)
        {
            throw new ArgumentNullException(paramName);
        }

        private static void ThrowArgumentException(string argName)
        {
            throw new ArgumentException(argName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T IsNotNull<T>(T param, string paramName)
            where T : class
        {
            if (param == null)
            {
                ThrowArgumentNullException(paramName);
            }

            return param;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string IsNotNullOrWhiteSpace(string param, string paramName)
        {
            if (param == null)
            {
                ThrowArgumentNullException(paramName);
            }
            else if (string.IsNullOrWhiteSpace(param))
            {
                ThrowArgumentException(paramName);
            }

            return param;
        }
    }
}