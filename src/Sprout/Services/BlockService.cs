using Markdig.Syntax;
using Sprout.Models;
using Sprout.Utilities;

namespace Sprout.Services;

public static class BlockService
{
    private static readonly Dictionary<Type, IBlockGenerator<Block>> BlockGenerators = new();

    static BlockService()
    {
        var blockGenerators = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetBlockGenerators());

        foreach (var blockGenerator in blockGenerators)
        {
            var blockGeneratorInterface = blockGenerator.GetInterfaces().Single();

            var typeConstraint = blockGeneratorInterface.GenericTypeArguments.Single();
            
            BlockGenerators[typeConstraint] = (IBlockGenerator<Block>)Activator.CreateInstance(blockGeneratorInterface);
        }
    }

    public static string Generate(Block block, ComponentContext context)
    {
        if (BlockGenerators.TryGetValue(block.GetType(), out var generator))
        {
            return generator.Generate(block, context);
        }

        // For now, silently fail on unsupported blocks
        return string.Empty;
    }
}