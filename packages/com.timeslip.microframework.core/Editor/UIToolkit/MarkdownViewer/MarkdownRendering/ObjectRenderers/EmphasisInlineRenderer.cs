using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace MFramework.Core.Editor
{
    public class EmphasisInlineRenderer : MarkdownObjectRenderer<UIMarkdownRenderer, EmphasisInline>
    {
        protected override void Write(UIMarkdownRenderer renderer, EmphasisInline obj)
        {
            renderer.WriteText(obj.DelimiterCount == 2 ? "<b>" : "<i>");
            renderer.WriteChildren(obj);
            renderer.WriteText(obj.DelimiterCount == 2 ? "</b>" : "</i>");
        }
    }
}