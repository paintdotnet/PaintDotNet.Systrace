using System;
using System.Text;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace PaintDotNet.Diagnostics
{
    internal sealed class SystraceWriterMQ
        : Disposable
    {
        private TextWriter textWriter;

        private ConcurrentQueue<SystraceEvent> queue = new ConcurrentQueue<SystraceEvent>();

        private bool leaveOpen;

        private Task writingTask;

        public SystraceWriterMQ(TextWriter textWriter, bool leaveOpen)
        {
            this.textWriter = Validate.IsNotNull(textWriter, nameof(textWriter));
            this.leaveOpen = leaveOpen;
            textWriter.WriteLine("[ ");
            textWriter.Flush();
            writingTask = Task.Factory.StartNew(() => {
                while (true)
                {
                    Thread.Sleep(100);
                    if (queue == null) return;
                    Flush();
                }
            });
        }

        public void WriteEvent(SystraceEvent systraceEvent)
        {
            queue.Enqueue(systraceEvent);            
        }

        public void Flush()
        {
            while (true)
            {
                if(!queue.TryDequeue(out SystraceEvent systraceEvent)) return;
                WriteEventToDisk(systraceEvent);
            }
        }

        private void WriteEventToDisk(SystraceEvent systraceEvent)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("  ");
            systraceEvent.AppendTo(sb);
            sb.Append(",");
            textWriter.WriteLine(sb);
            textWriter.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            Flush();
            queue = null;
            if (!leaveOpen)
            {                
                DisposableUtil.Free(ref textWriter);
            }
            base.Dispose(disposing);
        }
    }
}