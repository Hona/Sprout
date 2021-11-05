using Markdig.Syntax;
using Sprout.Models;

namespace Sprout.Services;

public interface IBlockGenerator<in T> where T : Block
{
    public string Generate(T block, ComponentContext context);
}