﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Essential.Templating.Razor.Host.Rendering
{
    /// <summary>
    /// A <see cref="TextWriter"/> that represents individual write operations as a sequence of strings.
    /// </summary>
    /// <remarks>
    /// This is primarily designed to avoid creating large in-memory strings.
    /// Refer to https://aspnetwebstack.codeplex.com/workitem/585 for more details.
    /// </remarks>
    public class StringCollectionTextWriter : TextWriter
    {
        private static readonly Task _completedTask = Task.FromResult(0);
        private readonly Encoding _encoding;

        /// <summary>
        /// Creates a new instance of <see cref="StringCollectionTextWriter"/>.
        /// </summary>
        /// <param name="encoding">The character <see cref="Encoding"/> in which the output is written.</param>
        public StringCollectionTextWriter(Encoding encoding)
        {
            _encoding = encoding;
            Buffer = new BufferEntryCollection();
        }

        
        public override Encoding Encoding
        {
            get { return _encoding; }
        }

        /// <summary>
        /// A collection of entries buffered by this instance of <see cref="StringCollectionTextWriter"/>.
        /// </summary>
        // internal for testing purposes.
        internal BufferEntryCollection Buffer { get; private set; }

        
        public override void Write(char value)
        {
            Buffer.Add(value.ToString());
        }

        
        public override void Write(char[] buffer, int index, int count)
        {
            Contract.Requires<ArgumentNullException>(buffer != null);

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count < 0 || (buffer.Length - index < count))
            {
                throw new ArgumentOutOfRangeException("count");
            }

            Buffer.Add(buffer, index, count);
        }

        
        public override void Write(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            Buffer.Add(value);
        }

        
        public override Task WriteAsync(char value)
        {
            Write(value);
            return _completedTask;
        }

        
        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            Write(buffer, index, count);
            return _completedTask;
        }

        
        public override Task WriteAsync(string value)
        {
            Write(value);
            return _completedTask;
        }

        
        public override void WriteLine()
        {
            Buffer.Add(Environment.NewLine);
        }

        
        public override void WriteLine(string value)
        {
            Write(value);
            WriteLine();
        }

        
        public override Task WriteLineAsync(char value)
        {
            WriteLine(value);
            return _completedTask;
        }

        
        public override Task WriteLineAsync(char[] value, int start, int offset)
        {
            WriteLine(value, start, offset);
            return _completedTask;
        }

        
        public override Task WriteLineAsync(string value)
        {
            WriteLine(value);
            return _completedTask;
        }

        
        public override Task WriteLineAsync()
        {
            WriteLine();
            return _completedTask;
        }

        
        public void CopyTo(TextWriter writer)
        {
            var targetStringCollectionWriter = writer as StringCollectionTextWriter;
            if (targetStringCollectionWriter != null)
            {
                targetStringCollectionWriter.Buffer.Add(Buffer);
            }
            else
            {
                WriteList(writer, Buffer);
            }
        }

        
        public Task CopyToAsync(TextWriter writer)
        {
            var targetStringCollectionWriter = writer as StringCollectionTextWriter;
            if (targetStringCollectionWriter != null)
            {
                targetStringCollectionWriter.Buffer.Add(Buffer);
            }
            else
            {
                return WriteListAsync(writer, Buffer);
            }

            return _completedTask;
        }

        
        public override string ToString()
        {
            return string.Join(string.Empty, Buffer);
        }

        private static void WriteList(TextWriter writer, BufferEntryCollection values)
        {
            foreach (var value in values)
            {
                writer.Write(value);
            }
        }

        private static async Task WriteListAsync(TextWriter writer, BufferEntryCollection values)
        {
            foreach (var value in values)
            {
                await writer.WriteAsync(value);
            }
        }
    }
}