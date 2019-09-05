/////////////////////////////////////////////////////////////////////////////////
// paint.net                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, and contributors.                  //
// All Rights Reserved.                                                        //
/////////////////////////////////////////////////////////////////////////////////

using System.Globalization;
using System.Text;
using System;

namespace PaintDotNet.Diagnostics
{
    internal sealed class SystraceEvent
    {
        public SystraceSession Session
        {
            get;
        }

        public string Name
        {
            get;
        }

        public string Categories
        {
            get;
        }

        public char EventType
        {
            get;
            set;
        }

        public long TimestampMicroseconds
        {
            get;
            set;
        }

        public int ProcessID
        {
            get;
        }

        public int ThreadID
        {
            get;
        }

        // TODO: Arguments
        // TODO: cname (color name)

        public SystraceEvent(
            SystraceSession session,
            string name,
            string categories,
            char eventType,
            long timestampMicroseconds,
            int processID,
            int threadID)
        {
            this.Session = Validate.IsNotNull(session, nameof(session));
            this.Name = Validate.IsNotNull(name, nameof(name));
            this.Categories = Validate.IsNotNull(categories, nameof(categories));
            this.EventType = eventType;
            this.TimestampMicroseconds = timestampMicroseconds;
            this.ProcessID = processID;
            this.ThreadID = threadID;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            AppendTo(builder);
            return builder.ToString();
        }

        public void AppendTo(StringBuilder builder)
        {
            builder.Append("{ ");
            builder.Append($"\"name\": \"{JsonEscapify(this.Name)}\"");
            builder.Append($", \"cat\": \"{JsonEscapify(this.Categories)}\"");
            builder.Append($", \"ph\": \"{this.EventType}\"");
            builder.Append($", \"ts\": {this.TimestampMicroseconds.ToString(CultureInfo.InvariantCulture)}");
            builder.Append($", \"pid\": {this.ProcessID.ToString(CultureInfo.InvariantCulture)}");
            builder.Append($", \"tid\": {this.ThreadID.ToString(CultureInfo.InvariantCulture)}");
            builder.Append(" }");
        }

        private string JsonEscapify(string text)
        {
            // TODO: do all proper escaping, and efficiently, e.g. https://www.freeformatter.com/json-escape.html
            return text
                .Replace("\\", "\\\\") // backslash
                .Replace("\"", "\\\"") // double quotes
                ;
        }
    }
}
