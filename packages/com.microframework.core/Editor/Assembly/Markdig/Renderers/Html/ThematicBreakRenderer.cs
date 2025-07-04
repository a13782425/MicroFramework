// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using Markdig.Syntax;
#nullable enable
namespace Markdig.Renderers.Html
{
    /// <summary>
    /// A HTML renderer for a <see cref="ThematicBreakBlock"/>.
    /// </summary>
    /// <seealso cref="HtmlObjectRenderer{ThematicBreakBlock}" />
    public class ThematicBreakRenderer : HtmlObjectRenderer<ThematicBreakBlock>
    {
        protected override void Write(HtmlRenderer renderer, ThematicBreakBlock obj)
        {
            if (renderer.EnableHtmlForBlock)
            {
                renderer.Write("<hr").WriteAttributes(obj).WriteLine(" />");
            }
        }
    }
}