using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Microsoft.CodeAnalysis;
using Sprout.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sprout.Services;

public class GenerationService
{
    private readonly GeneratorExecutionContext _context;
    private readonly SproutConfig _config;
    
    private IDeserializer _yamlDeserializer;
    private MarkdownPipeline _markdownPipeline;

    public GenerationService(GeneratorExecutionContext context, SproutConfig config)
    {
        _context = context;
        _config = config;
        
        var yamlDeserializerBuilder = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance);

        if (config.FrontmatterMapping is not null)
        {
            var properties = typeof(FrontmatterMapping).GetProperties();

            foreach (var propertyInfo in properties)
            {
                var value = (string?) propertyInfo.GetValue(config.FrontmatterMapping);

                if (value is not null)
                {
                    // Actually configured a mapping
                    yamlDeserializerBuilder.WithAttributeOverride<FrontmatterMapping>(x => propertyInfo.GetValue(x),
                        new YamlMemberAttribute
                        {
                            Alias = value,
                            ApplyNamingConventions = false
                        });
                }
            }
        }
        
        _yamlDeserializer = yamlDeserializerBuilder.Build();
        
        _markdownPipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .Build();
    }

    public string GenerateFromPath(string filePath)
    {
        var document = ParseDocument(filePath);
        var frontmatterConfig = GetFrontmatterConfig(document);

        return Generate(document, frontmatterConfig);
    }

    public string Generate(MarkdownDocument document, FrontmatterConfig config)
    {
        var componentContext = new ComponentContext();

        var output = new StringBuilder();

        output.Append($@"
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Sprout.Web.Pages;

[Route(""{config.Route}"")]
    public class {"Scissor" + Guid.NewGuid().ToString().Replace("-", "")}Component : ComponentBase
    {{
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {{
            base.BuildRenderTree(builder);
");

        foreach (var block in document)
        {
            var generatedBlock = BlockService.Generate(block, componentContext);

            output.Append(generatedBlock);
        }

        output.Append(@"
        }
    }
");
        
        return output.ToString();
    }

    private MarkdownDocument ParseDocument(string filePath)
    {
        var rawMarkdown = File.ReadAllText(filePath);
        return Markdown.Parse(rawMarkdown, _markdownPipeline);
    }

    private FrontmatterConfig GetFrontmatterConfig(MarkdownDocument document)
    {
        var frontMatterBlock = (YamlFrontMatterBlock?)document.FirstOrDefault(x => x is YamlFrontMatterBlock);

        var frontmatterConfig = new FrontmatterConfig();

        if (frontMatterBlock is null) return frontmatterConfig;
        
        var yamlRaw = frontMatterBlock.Lines.ToString();
        frontmatterConfig = _yamlDeserializer.Deserialize<FrontmatterConfig>(yamlRaw);

        return frontmatterConfig;
    }
}