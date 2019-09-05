﻿/////////////////////////////////////////////////////////////////////////////////
// paint.net                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, and contributors.                  //
// All Rights Reserved.                                                        //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Threading;

namespace PaintDotNet
{
    [Serializable]
    public abstract class Disposable
        : IDisposable,
          IIsDisposed
    {
        private int isDisposed; // 0 for false, 1 for true

        public bool IsDisposed
        {
            get
            {
                return Thread.VolatileRead(ref this.isDisposed) == 1;
            }
        }

        protected Disposable()
        {
        }

        ~Disposable()
        {
            Debug.Assert(!this.IsDisposed);
            Dispose(false);
        }

        public void Dispose()
        {
            int oldIsDisposed = Interlocked.Exchange(ref this.isDisposed, 1);
            if (oldIsDisposed == 0)
            {
                try
                {
                    Dispose(true);
                }

                finally
                {
                    GC.SuppressFinalize(this);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
