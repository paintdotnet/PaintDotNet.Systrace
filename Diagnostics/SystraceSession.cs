/////////////////////////////////////////////////////////////////////////////////
// paint.net                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, and contributors.                  //
// All Rights Reserved.                                                        //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace PaintDotNet.Diagnostics
{
    internal sealed class SystraceSession
        : Disposable
    {
        private FileStream stream;
        private TextWriter textWriter;
        private SystraceWriterMQ writer;
        private readonly Stopwatch stopwatch;
        private readonly long timestampNumerator;
        private readonly long timestampDenominator;
        private readonly int processID;

        public SystraceSession(FileStream stream)
        {
            this.stream = Validate.IsNotNull(stream, nameof(stream));
            this.textWriter = new StreamWriter(this.stream, Encoding.UTF8, 4096, false);
            this.writer = new SystraceWriterMQ(this.textWriter, false);

            this.processID = Process.GetCurrentProcess().Id;

            this.stopwatch = new Stopwatch();
            this.timestampDenominator = Stopwatch.Frequency;
            this.timestampNumerator = 1_000_000; // 1 million microseconds per second
            this.stopwatch.Start();
        }

        protected override void Dispose(bool disposing)
        {
            DisposableUtil.Free(ref this.writer, disposing);
            this.textWriter = null;
            this.stream = null;
            base.Dispose(disposing);
        }

        private long GetCurrentTimestampMicroseconds()
        {
            long elapsedTicks = this.stopwatch.ElapsedTicks;
            long timestampUS = checked(elapsedTicks * this.timestampNumerator) / this.timestampDenominator;
            return timestampUS;
        }

        public BeginEventScope BeginEvent(string name, string categories)
        {
            return new BeginEventScope(this, name, categories);
        }

        public struct BeginEventScope
            : IDisposable
        {
            private SystraceEvent systraceEvent;

            internal BeginEventScope(SystraceSession session, string name, string categories)
            {
                this.systraceEvent = new SystraceEvent(
                    session,
                    name,
                    categories,
                    SystraceEventTypes.DurationBegin,
                    session.GetCurrentTimestampMicroseconds(),
                    session.processID,
                    Thread.CurrentThread.ManagedThreadId);

                this.systraceEvent.Session.writer.WriteEvent(this.systraceEvent);
            }

            public void Dispose()
            {
                this.systraceEvent.EventType = SystraceEventTypes.DurationEnd;
                this.systraceEvent.TimestampMicroseconds = this.systraceEvent.Session.GetCurrentTimestampMicroseconds();
                this.systraceEvent.Session.writer.WriteEvent(this.systraceEvent);
            }
        }
    }
}
