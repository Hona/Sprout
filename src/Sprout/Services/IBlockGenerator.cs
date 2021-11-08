using Markdig.Syntax;
using Sprout.Models;

namespace Sprout.Services;

public interface IBlockGenerator<in T> : IBlockGenerator where T : Block
{
    public string Generate(T block, ComponentContext context);
}

public interface IBlockGenerator
{
    public string GenerateBlock(Block block, ComponentContext context);
}