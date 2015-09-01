﻿using System;
using System.Diagnostics.Contracts;
using RazorEngine.Templating;

namespace Essential.Templating.Razor
{
    public class Template<T> : Template, ITemplate<T>
    {
        public Template(TemplateContext templateContext) : base(templateContext)
        {
            Contract.Requires<ArgumentNullException>(templateContext != null);
        }

        public T Model { get; set; }

        protected override object GetModel()
        {
            return Model;
        }
    }
}