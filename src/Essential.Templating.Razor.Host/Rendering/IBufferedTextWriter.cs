﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;

namespace Essential.Templating.Razor.Host.Rendering
{
    public interface IBufferedTextWriter
    {
        bool IsBuffering { get; }

        void CopyTo(TextWriter writer);

        Task CopyToAsync(TextWriter writer);
    }
}