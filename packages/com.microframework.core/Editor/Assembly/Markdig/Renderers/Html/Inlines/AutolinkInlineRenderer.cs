// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using Markdig.Syntax.Inlines;
using System;
#nullable enable
namespace Markdig.Renderers.Html.Inlines
{
    /// <summary>
    /// A HTML renderer for an <see cref="AutolinkInline"/>.
    /// </summary>
    /// <seealso cref="HtmlObjectRenderer{AutolinkInline}" />
    public class AutolinkInlineRenderer : HtmlObjectRenderer<AutolinkInline>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to always add rel="nofollow" for links or not.
        /// </summary>
        [Obsolete("AutoRelNoFollow is obsolete. Please write \"nofollow\" into Property Rel.")]
        public bool AutoRelNoFollow
        {
            get
            {
                return Rel is not null && Rel.Contains("nofollow");
            }
            set
            {
                const string NoFollow = "nofollow";

                if (value)
                {
                    if (string.IsNullOrEmpty(Rel))
                    {
                        Rel = NoFollow;
                    }
                    else if (!Rel!.Contains(NoFollow))
                    {
                        Rel += $" {NoFollow}";
                    }
                }
                else
                {
                    Rel = Rel?.Replace(NoFollow, string.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the literal string in property rel for links
        /// </summary>
        public string? Rel { get; set; }

        protected override void Write(HtmlRenderer renderer, AutolinkInline obj)
        {
            if (renderer.EnableHtmlForInline)
            {
                renderer.Write("<a href=\"");
                if (obj.IsEmail)
                {
                    renderer.Write("mailto:");
                }
                renderer.WriteEscapeUrl(obj.Url);
                renderer.Write('"');
                renderer.WriteAttributes(obj);

                if (!obj.IsEmail && !string.IsNullOrWhiteSpace(Rel))
                {
                    renderer.Write($" rel=\"{Rel}\"");
                }

                renderer.Write('>');
            }

            renderer.WriteEscape(obj.Url);

            if (renderer.EnableHtmlForInline)
            {
                renderer.Write("</a>");
            }
        }
    }
}