using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Microsoft.CodeAnalysis;
using Sprout.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sprout;

[Generator]
public class SproutGenerator : ISourceGenerator
{


    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var projectDirectory = GetProjectRootDirectory(context);

        var configPath = Path.Join(projectDirectory, Constants.SproutConfigFileName);
        var config = SproutConfig.Load(configPath);

        var markdownFilePaths = GetMarkdownFilePaths(projectDirectory, config);
    }

    private string GetProjectRootDirectory(GeneratorExecutionContext context)
    {
        if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory", 
                out var projectDirectory)) return projectDirectory;
        
        // Hack
        var objFolderQuery = $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"; 
        
        // Currently no easy way to do this, we can look behind the obj/ folder for now
        var objSyntaxTree = context.Compilation.SyntaxTrees
            .FirstOrDefault(x => x.HasCompilationUnitRoot &&
                        x.FilePath.Contains(objFolderQuery));

        if (objSyntaxTree is null)
        {
            throw new Exception("Could not find project root directory from syntax tree");
        }
        
        var directoryName = Path.GetDirectoryName(objSyntaxTree.FilePath);
        
        if (directoryName is null)
        {
            throw new Exception("Could not find project root directory");
        }

        return directoryName;
    }

    private ReadOnlyCollection<string> GetMarkdownFilePaths(string projectDirectory, SproutConfig config)
    {
        var folderPath = Path.Join(projectDirectory, config.MarkdownPath);
        var markdownFiles = Directory.GetFiles(folderPath, "**.md", SearchOption.AllDirectories);

        return Array.AsReadOnly(markdownFiles);
    }
}