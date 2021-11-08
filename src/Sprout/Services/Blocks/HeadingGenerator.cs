using Markdig.Syntax;
using Sprout.Models;

namespace Sprout.Services.Blocks;

public class HeadingGenerator : IBlockGenerator<HeadingBlock>
{
    public string Generate(HeadingBlock block, ComponentContext context)
    {
        return $@"
builder.OpenElement({context.CurrentIndex++}, ""h1"");
builder.AddContent({context.CurrentIndex++}, ""{string.Join(Environment.NewLine, block.Inline)}"");
builder.CloseElement();
";
    }

    public string GenerateBlock(Block block, ComponentContext context) => Generate((HeadingBlock) block, context);
}