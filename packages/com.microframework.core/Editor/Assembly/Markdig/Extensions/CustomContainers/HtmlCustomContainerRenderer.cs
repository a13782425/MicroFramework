// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using Markdig.Renderers;
using Markdig.Renderers.Html;
#nullable enable
namespace Markdig.Extensions.CustomContainers
{
    /// <summary>
    /// A HTML renderer for a <see cref="CustomContainer"/>.
    /// </summary>
    /// <seealso cref="HtmlObjectRenderer{CustomContainer}" />
    public class HtmlCustomContainerRenderer : HtmlObjectRenderer<CustomContainer>
    {
        protected override void Write(HtmlRenderer renderer, CustomContainer obj)
        {
            renderer.EnsureLine();
            if (renderer.EnableHtmlForBlock)
            {
                renderer.Write("<div").WriteAttributes(obj).Write('>');
            }
            // We don't escape a CustomContainer
            renderer.WriteChildren(obj);
            if (renderer.EnableHtmlForBlock)
            {
                renderer.WriteLine("</div>");
            }
        }
    }
}