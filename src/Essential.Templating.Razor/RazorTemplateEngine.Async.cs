﻿using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Essential.Templating.Common;
using Essential.Templating.Common.Caching;
using Essential.Templating.Common.Rendering;
using Essential.Templating.Razor.Compilation;
using Essential.Templating.Razor.Rendering;
using Essential.Templating.Razor.Runtime;
using RazorEngine.Templating;

namespace Essential.Templating.Razor
{
    public partial class RazorTemplateEngine
    {
        public Task<string> RenderAsync(string path, object viewBag = null, CultureInfo culture = null)
        {
            return RenderAsync(path, renderer: new StringRenderer(), viewBag: viewBag, culture: culture);
        }

        public async Task<string> RenderAsync<T>(string path, T model, object viewBag = null, CultureInfo culture = null)
        {
            culture = culture ?? Thread.CurrentThread.CurrentCulture;

            var template = await ResolveTemplateAsync(path, culture, model);
            if (template == null)
            {
                return null;
            }
            try
            {
                var concreteTemplate = template as Template;
                if (concreteTemplate == null)
                {
                    throw new TypeMismatchException(template.GetType(), typeof (Template));
                }
                var renderer = new StringRenderer();
                return await Task.Run(() => renderer.Render(concreteTemplate, viewBag));
            }
            catch (Exception ex)
            {
                throw new TemplateEngineException(ex);
            }
        }

        public async Task<TResult> RenderAsync<TTemplate, TResult>(string path, IRenderer<TTemplate, TResult> renderer,
            object viewBag = null, CultureInfo culture = null)
            where TTemplate : class
            where TResult : class
        {
            culture = culture ?? Thread.CurrentThread.CurrentCulture;

            var template = await ResolveTemplateAsync(path, culture);
            if (template == null)
            {
                return null;
            }
            try
            {
                var concreteTemplate = template as TTemplate;
                if (concreteTemplate == null)
                {
                    throw new TypeMismatchException(template.GetType(), typeof (TTemplate));
                }
                return await Task.Run(() => renderer.Render(concreteTemplate, viewBag));
            }
            catch (Exception ex)
            {
                throw new TemplateEngineException(ex);
            }
        }

        public async Task<TResult> RenderAsync<TTemplate, TResult, T>(string path,
            IRenderer<TTemplate, TResult> renderer, T model, object viewBag = null, CultureInfo culture = null)
            where TTemplate : class
            where TResult : class
        {
            culture = culture ?? Thread.CurrentThread.CurrentCulture;

            var template = await ResolveTemplateAsync(path, culture, model);
            if (template == null)
            {
                return null;
            }
            try
            {
                var concreteTemplate = template as TTemplate;
                if (concreteTemplate == null)
                {
                    throw new TypeMismatchException(template.GetType(), typeof (TTemplate));
                }
                return await Task.Run(() => renderer.Render(concreteTemplate, viewBag));
            }
            catch (Exception ex)
            {
                throw new TemplateEngineException(ex);
            }
        }

        private async Task<Template> ResolveTemplateAsync(string path, CultureInfo culture)
        {
            var type = await ResolveTemplateTypeAsync(path, culture);
            if (type == null)
            {
                return null;
            }
            return ActivateTemplate(type,
                new TemplateContext(path, culture, _resourceProvider, new Tool(this)));
        }

        private async Task<ITemplate<T>> ResolveTemplateAsync<T>(string path, CultureInfo culture, T model)
        {
            var type = await ResolveTemplateTypeAsync<T>(path, culture);
            if (type == null)
            {
                return null;
            }
            return ActivateTemplate(type,
                new TemplateContext(path, culture, _resourceProvider, new Tool(this)), model);
        }

        private async Task<Type> ResolveTemplateTypeAsync(string path, CultureInfo culture)
        {
            var cacheKey = new TemplateCacheKey(path, culture);
            try
            {
                var cacheItem = _cache.Get(cacheKey);
                if (cacheItem != null)
                {
                    return cacheItem.TemplateInfo;
                }
                var templateStream = _resourceProvider.Get(path, culture);
                var type = templateStream == null ? null : await _compiler.CompileAsync(templateStream);
                if (type != null)
                {
                    _cache.Put(cacheKey, type, _cacheExpiration);
                }
                return type;
            }
            catch (CompilationException ex)
            {
                throw new TemplateEngineException(ex);
            }
            catch (Exception ex)
            {
                throw new TemplateEngineException("Can't resolve template type.", ex);
            }
        }

        private async Task<Type> ResolveTemplateTypeAsync<T>(string path, CultureInfo culture)
        {
            var cacheKey = new TemplateCacheKey(path, culture);
            try
            {
                var cacheItem = _cache.Get(cacheKey);
                if (cacheItem != null)
                {
                    return cacheItem.TemplateInfo;
                }
                var templateStream = _resourceProvider.Get(path, culture);
                var type = templateStream == null ? null : await _compiler.CompileAsync(templateStream, typeof (T));
                if (type != null)
                {
                    _cache.Put(cacheKey, type, _cacheExpiration);
                }
                return type;
            }
            catch (CompilationException ex)
            {
                throw new TemplateEngineException(ex);
            }
            catch (Exception ex)
            {
                throw new TemplateEngineException("Can't resolve template type.", ex);
            }
        }
    }
}