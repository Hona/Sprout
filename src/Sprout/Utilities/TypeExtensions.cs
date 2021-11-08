using System.Reflection;
using Sprout.Services;

namespace Sprout.Utilities;

public static class TypeExtensions
{
    public static List<Type?> GetBlockGenerators(this Assembly assembly) 
    {
        var it = typeof (IBlockGenerator<>);

        return GetClassesImplementingAnInterface(assembly, it);
    }
    
    public static List<Type?> GetClassesImplementingAnInterface(Assembly? assemblyToScan, Type? implementedInterface)
    {
        if (assemblyToScan == null)
        {
            return new List<Type?>();
        }

        if (implementedInterface is not {IsInterface: true})
        {
            return new List<Type?>();
        }

        IEnumerable<Type> typesInTheAssembly;

        try
        {
            typesInTheAssembly = assemblyToScan.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            typesInTheAssembly = e.Types.Where(t => t != null);
        }

        var classesImplementingInterface = new List<Type>();

        // if the interface is a generic interface
        if (implementedInterface.IsGenericType)
        {
            foreach (var typeInTheAssembly in typesInTheAssembly)
            {
                if (!typeInTheAssembly.IsClass) continue;
                
                var typeInterfaces = typeInTheAssembly.GetInterfaces();
                foreach (var typeInterface in typeInterfaces)
                {
                    if (!typeInterface.IsGenericType) continue;
                    
                    var typeGenericInterface = typeInterface.GetGenericTypeDefinition();
                    var implementedGenericInterface = implementedInterface.GetGenericTypeDefinition();

                    if (typeGenericInterface == implementedGenericInterface)
                    {
                        classesImplementingInterface.Add(typeInTheAssembly);
                    }
                }
            }
        }
        else
        {
            foreach (var typeInTheAssembly in typesInTheAssembly)
            {
                if (!typeInTheAssembly.IsClass) continue;
                
                // if the interface is a non-generic interface
                if (implementedInterface.IsAssignableFrom(typeInTheAssembly))
                {
                    classesImplementingInterface.Add(typeInTheAssembly);
                }
            }
        }
        return classesImplementingInterface;
    }
}