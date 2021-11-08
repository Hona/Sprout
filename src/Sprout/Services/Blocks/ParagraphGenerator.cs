using Markdig.Syntax;
using Sprout.Models;

namespace Sprout.Services.Blocks;

public class ParagraphGenerator : IBlockGenerator<ParagraphBlock>
{
    public string Generate(ParagraphBlock block, ComponentContext context)
    {
        return $@"
builder.OpenElement({context.CurrentIndex++}, ""p"");
builder.AddContent({context.CurrentIndex++}, ""{string.Join(Environment.NewLine, block.Inline)}"");
builder.CloseElement();
";
    }

    public string GenerateBlock(Block block, ComponentContext context) => Generate((ParagraphBlock) block, context);
}