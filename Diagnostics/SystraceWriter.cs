/////////////////////////////////////////////////////////////////////////////////
// paint.net                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, and contributors.                  //
// All Rights Reserved.                                                        //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;

namespace PaintDotNet.Diagnostics
{
    internal sealed class SystraceWriter
        : Disposable
    {
        private readonly object sync = new object();
        private TextWriter textWriter;
        private bool leaveOpen;

        private bool isFirstEvent = true;
        private StringBuilder stringBuilder = new StringBuilder();

        public SystraceWriter(TextWriter textWriter, bool leaveOpen)
        {
            this.textWriter = Validate.IsNotNull(textWriter, nameof(textWriter));
            this.leaveOpen = leaveOpen;
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.leaveOpen)
            {
                DisposableUtil.Free(ref this.textWriter, disposing);
            }

            base.Dispose(disposing);
        }

        public void WriteEvent(SystraceEvent systraceEvent)
        {
            Validate.IsNotNull(systraceEvent, nameof(systraceEvent));

            lock (this.sync)
            {
                if (this.isFirstEvent)
                {
                    this.textWriter.Write("[ ");
                }
                else
                {
                    this.textWriter.WriteLine(",");
                    this.textWriter.Write("  ");
                }

                this.stringBuilder.Clear();
                systraceEvent.AppendTo(this.stringBuilder);
                string eventText = this.stringBuilder.ToString();
                this.textWriter.Write(eventText);

                this.textWriter.Flush();
                this.isFirstEvent = false;
            }
        }

        public void Close()
        {
            lock (this.sync)
            {
                this.textWriter.WriteLine(" ]");
                this.textWriter.Flush();
                this.textWriter = null;
            }
        }
    }
}
