using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace MFramework.Core.Editor
{
    public class CodeInlineRenderer : MarkdownObjectRenderer<UIMarkdownRenderer, CodeInline>
    {
        protected override void Write(UIMarkdownRenderer renderer, CodeInline obj)
        {
            renderer.WriteText(obj.Content.ToString());
        }
    }
}