using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sprout.Models;

public class SproutConfig
{
    public string? MarkdownPath { get; set; }
    public FrontmatterMapping? FrontmatterMapping { get; set; }

    public static SproutConfig Load(string filePath)
    {
        var configString = File.ReadAllText(filePath)
            .AsSpan();
        var config = JsonSerializer.Deserialize<SproutConfig>(configString);

        if (config is null)
        {
            throw new Exception("Scissor config can't be null");
        }

        return config;
    }
}

public class FrontmatterMapping
{
    public string? Route { get; set; }
    public string? Title { get; set; }
}