using Markdig.Renderers;
using Markdig.Syntax;

namespace MFramework.Core.Editor
{
    public class ThematiceBreakBlockRenderer : MarkdownObjectRenderer<UIMarkdownRenderer, ThematicBreakBlock>
    {
        protected override void Write(UIMarkdownRenderer renderer, ThematicBreakBlock obj)
        {
            renderer.StartBlock("line");
            renderer.FinishBlock();
        }
    }
}