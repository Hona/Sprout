using Markdig.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sprout.Models;
using Sprout.Utilities;

namespace Sprout.Services;

public static class BlockService
{
    private static readonly Dictionary<Type, IBlockGenerator> BlockGenerators = new();

    static BlockService()
    {
        var blockGenerators = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetBlockGenerators())
            .ToList();

        foreach (var blockGenerator in blockGenerators)
        {
            if (blockGenerator.GetInterfaces().All(x => x != typeof(IBlockGenerator)))
            {
                continue;
            }
            
            var blockGeneratorInterface = blockGenerator.GetInterfaces().Single(x => x.GenericTypeArguments.Any());

            var typeConstraint = blockGeneratorInterface.GenericTypeArguments.Single();
            
            BlockGenerators[typeConstraint] = (IBlockGenerator)Activator.CreateInstance(blockGenerator);
        }
    }

    public static string Generate(Block block, ComponentContext context)
    {
        if (BlockGenerators.TryGetValue(block.GetType(), out var generator))
        {
            return generator.GenerateBlock(block, context);
        }

        // For now, silently fail on unsupported blocks
        return string.Empty;
    }
}