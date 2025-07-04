// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using System;
using System.IO;
using System.Text;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
#nullable enable
namespace Markdig
{
    /// <summary>
    /// This class is the Markdown pipeline build from a <see cref="MarkdownPipelineBuilder"/>.
    /// </summary>
    public sealed class MarkdownPipeline
    {
        // This class is immutable

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownPipeline" /> class.
        /// </summary>
        internal MarkdownPipeline(
            OrderedList<IMarkdownExtension> extensions,
            BlockParserList blockParsers,
            InlineParserList inlineParsers,
            TextWriter? debugLog,
            ProcessDocumentDelegate? documentProcessed)
        {
            if (blockParsers is null) ThrowHelper.ArgumentNullException(nameof(blockParsers));
            if (inlineParsers is null) ThrowHelper.ArgumentNullException(nameof(inlineParsers));
            // Add all default parsers
            Extensions = extensions;
            BlockParsers = blockParsers;
            InlineParsers = inlineParsers;
            DebugLog = debugLog;
            DocumentProcessed = documentProcessed;
        }

        internal bool PreciseSourceLocation { get; set; }

        /// <summary>
        /// The read-only list of extensions used to build this pipeline.
        /// </summary>
        public OrderedList<IMarkdownExtension> Extensions { get; }

        internal BlockParserList BlockParsers { get; }

        internal InlineParserList InlineParsers { get; }

        // TODO: Move the log to a better place
        internal TextWriter? DebugLog { get; }

        internal ProcessDocumentDelegate? DocumentProcessed;

        /// <summary>
        /// True to parse trivia such as whitespace, extra heading characters and unescaped
        /// string values.
        /// </summary>
        public bool TrackTrivia { get; internal set; }

        /// <summary>
        /// Allows to setup a <see cref="IMarkdownRenderer"/>.
        /// </summary>
        /// <param name="renderer">The markdown renderer to setup</param>
        public void Setup(IMarkdownRenderer renderer)
        {
            if (renderer is null) ThrowHelper.ArgumentNullException(nameof(renderer));
            foreach (var extension in Extensions)
            {
                extension.Setup(this, renderer);
            }
        }

        private HtmlRendererCache? _rendererCache;
        private HtmlRendererCache? _rendererCacheForCustomWriter;

        internal RentedHtmlRenderer RentHtmlRenderer(TextWriter? writer = null)
        {
            HtmlRendererCache cache = writer is null
                ? _rendererCache ??= new HtmlRendererCache(this, customWriter: false)
                : _rendererCacheForCustomWriter ??= new HtmlRendererCache(this, customWriter: true);

            HtmlRenderer renderer = cache.Get();

            if (writer is not null)
            {
                renderer.Writer = writer;
            }

            return new RentedHtmlRenderer(cache, renderer);
        }

        internal sealed class HtmlRendererCache : ObjectCache<HtmlRenderer>
        {
            private const int InitialCapacity = 1024;

            private static readonly StringWriter _dummyWriter = new();

            private readonly MarkdownPipeline _pipeline;
            private readonly bool _customWriter;

            public HtmlRendererCache(MarkdownPipeline pipeline, bool customWriter = false)
            {
                _pipeline = pipeline;
                _customWriter = customWriter;
            }

            protected override HtmlRenderer NewInstance()
            {
                var writer = _customWriter ? _dummyWriter : new StringWriter(new StringBuilder(InitialCapacity));
                var renderer = new HtmlRenderer(writer);
                _pipeline.Setup(renderer);
                return renderer;
            }

            protected override void Reset(HtmlRenderer instance)
            {
                instance.ResetInternal();

                if (_customWriter)
                {
                    instance.Writer = _dummyWriter;
                }
                else
                {
                    ((StringWriter)instance.Writer).GetStringBuilder().Length = 0;
                }
            }
        }

        internal readonly struct RentedHtmlRenderer : IDisposable
        {
            private readonly HtmlRendererCache _cache;
            public readonly HtmlRenderer Instance;

            internal RentedHtmlRenderer(HtmlRendererCache cache, HtmlRenderer renderer)
            {
                _cache = cache;
                Instance = renderer;
            }

            public void Dispose() => _cache.Release(Instance);
        }
    }
}