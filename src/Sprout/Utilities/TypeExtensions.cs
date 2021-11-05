using System.Reflection;
using Sprout.Services;

namespace Sprout.Utilities;

public static class TypeExtensions
{
    public static IEnumerable<Type?> GetLoadableTypes(this Assembly assembly) 
    {
        if (assembly is null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }
        try
        {
            return assembly.GetTypes();
        } 
        catch (ReflectionTypeLoadException e) 
        {
            return e.Types.Where(t => t is not null);
        }
    }
    
    public static List<Type?> GetBlockGenerators(this Assembly asm) 
    {
        var it = typeof (IBlockGenerator<>);
        
        return asm.GetLoadableTypes()
            .Where(it.IsAssignableFrom)
            .ToList();
    }
}