using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace MFramework.Core.Editor
{
    public class LiteralInlineRenderer : MarkdownObjectRenderer<UIMarkdownRenderer, LiteralInline>
    {
        protected override void Write(UIMarkdownRenderer renderer, LiteralInline obj)
        {
            renderer.WriteText(obj.Content.ToString());
        }
    }
}