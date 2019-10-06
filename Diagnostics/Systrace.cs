/////////////////////////////////////////////////////////////////////////////////
// paint.net                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, and contributors.                  //
// All Rights Reserved.                                                        //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace PaintDotNet.Diagnostics
{
    // Resources:
    // * Trace Event Format, https://docs.google.com/document/d/1CvAClvFfyA5R-PhYUmn5OOQtYMH4h6I0nSsKchNAySU/preview

    public static class Systrace
    {
        private static SystraceSession session;

        public static bool IsTracing
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return session != null;
            }
        }

        public static void Initialize(string traceFilePath)
        {
            if (session != null)
            {
                throw new InvalidOperationException("Already initialized");
            }

            FileStream stream = new FileStream(traceFilePath, FileMode.Create, FileAccess.ReadWrite);
            session = new SystraceSession(stream);
        }

        public static void Uninitialize()
        {
            DisposableUtil.Free(ref session);
            // The end of the trace json is not correct, but it's ok. 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BeginEventScope BeginEvent(string name)
        {
            if (session == null)
            {
                return default;
            }
            else
            {
                return new BeginEventScope(session.BeginEvent(name, SystraceCategories.Default));
            }
        }

        public static BeginEventScope BeginEvent(string name, string categories)
        {
            if (session == null)
            {
                return default;
            }
            else
            {
                return new BeginEventScope(session.BeginEvent(name, categories));
            }
        }

        public struct BeginEventScope
            : IDisposable
        {
            private SystraceSession.BeginEventScope scope;

            internal BeginEventScope(SystraceSession.BeginEventScope scope)
            {
                this.scope = scope;
            }

            public void Dispose()
            {
                this.scope.Dispose();
            }

            // This method is provided for use by C++/CLI code, which otherwise can't call Dispose() because
            // the delete operator is required instead, and that causes a boxing allocation. For some reason.
            public void EndEvent()
            {
                Dispose();
            }
        }
    }
}
