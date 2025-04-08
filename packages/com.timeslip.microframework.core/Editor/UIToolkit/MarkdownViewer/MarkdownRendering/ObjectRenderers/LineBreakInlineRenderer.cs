using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace MFramework.Core.Editor
{
    public class LineBreakInlineRenderer : MarkdownObjectRenderer<UIMarkdownRenderer, LineBreakInline>
    {
        protected override void Write(UIMarkdownRenderer renderer, LineBreakInline obj)
        {
            renderer.WriteText(obj.IsHard ? "\n" : " ");
        }
    }
}