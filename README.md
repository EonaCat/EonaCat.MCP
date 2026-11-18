# EonaCat.MCP Examples

- **BasicServer** registers and executes an echo skill.
- **FullServer** demonstrates greeting and counter skills.
- **ClientExample** lists skills in a new in-memory registry; it does not
  connect to a remote server.

All three are console examples; they do not host HTTP servers.

# EonaCat MCP Skill Server

A production-grade Model Context Protocol (MCP) server library for distributing, managing, and deploying reusable skills and workflows to AI agents.

## Overview

**EonaCat.MCP** provides a complete ecosystem for:

- **Skill Distribution**: Host skills (function libraries, workflows, integrations) on a central server
- **Agent Integration**: Any MCP-compatible agent can discover and install skills
- **Skill Versioning**: Semantic versioning with backward compatibility tracking
- **Dependency Management**: Skills can depend on other skills; resolution is automatic
- **Easy Extension**: Add new skills, workflows, and agent capabilities in minutes
- **Workflow Orchestration**: Chain skills together and define complex agent workflows
- **Configuration Management**: Per-agent configuration overrides and environment-specific settings

### Dependencies

This library uses **EonaCat.Json** for robust JSON serialization and deserialization

## Quick Start

The package is not currently published on nuget.org. For a local package
build and example restore, follow [the examples guide](examples/README.md).

Follow the [examples guide](examples/README.md) to build the package locally,
restore the examples against it, and run them.

## Running Examples

The [examples](examples/README.md) are separate console applications that
reference the NuGet package. BasicServer and FullServer demonstrate local
skill execution; ClientExample lists an in-memory registry. They do not host
or connect to an HTTP server.

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    MCP Skill Server                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────┐  │
│  │  Skill Registry  │  │  Workflow Engine │  │  Config Mgr  │  │
│  └──────────────────┘  └──────────────────┘  └──────────────┘  │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │              REST API & MCP Protocol                      │  │
│  │  GET  /api/skills                                         │  │
│  │  GET  /api/skills/{id}/manifest                           │  │
│  │  POST /api/skills/{id}/install                            │  │
│  │  POST /api/workflows                                      │  │
│  │  GET  /api/agents/{id}/capabilities                       │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │          Storage Layer (Files, Metadata, Cache)           │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
            ↓↓↓ MCP Protocol ↓↓↓
┌──────────────┬──────────────┬──────────────┬──────────────────┐
│   Agent 1    │   Agent 2    │   Agent N    │  CLI Tools       │
└──────────────┴──────────────┴──────────────┴──────────────────┘
```

## Project Structure

```
EonaCat.MCP/
├── Library/
│   ├── EonaCat.MCP.sln
│   ├── EonaCat.MCP.csproj
│   └── src/
│       ├── Core/
│       ├── Skills/
│       ├── Workflows/
│       ├── API/
│       ├── Configuration/
│       └── Samples/
├── examples/
│   ├── BasicServer/                    # Simple server example
│   ├── FullServer/                     # Multiple skills example
│   └── ClientExample/                  # Registry listing example
└── README.md
```

## Key Features

### 1. Skill Management
- Register, discover, and distribute skills
- Semantic versioning with compatibility metadata
- Automatic dependency resolution
- Skill enable/disable toggle
- Skill metadata (description, author, license, capabilities)

### 2. Workflow System
- Define multi-step workflows combining skills
- DAG-based execution with parallel execution where possible
- Error handling and retry policies
- Conditional logic and branching
- Workflow versioning and templates

### 3. Configuration
- Global server configuration
- Per-agent skill selection and configuration
- Environment-specific overrides
- Capability filtering by agent type
- Quota management per agent

### 4. Agent Integration
- Agent discovery and registration
- Capability negotiation
- Persistent agent sessions
- Usage tracking and quotas
- Agent-specific skill installation policies

### 5. API & Protocol
- REST endpoints for skill discovery and installation
- MCP protocol compliance
- GraphQL API for complex queries (optional)
- WebSocket support for real-time capability updates
- OpenAPI/Swagger documentation

## Usage Examples

### Register a Skill

```csharp
public class CodeAnalysisSkill : Skill
{
    public override SkillMetadata Metadata => new()
    {
        Id = "code-analysis",
        Name = "Code Analysis",
        Description = "Analyzes code for quality, security, and performance",
        Version = "1.0.0",
        Author = "EonaCat",
        Tags = new[] { "code", "analysis", "quality" },
        InputParameters = new[]
        {
            new SkillParameter("code", "string", "Code to analyze"),
            new SkillParameter("language", "string", "Programming language")
        },
        OutputParameters = new[]
        {
            new SkillParameter("issues", "array", "Found issues")
        }
    };

    public override async Task<SkillResult> ExecuteAsync(SkillInput input)
    {
        var code = input.GetParameter<string>("code");
        var language = input.GetParameter<string>("language");
        
        // Perform analysis
        var issues = AnalyzeCode(code, language);
        
        return SkillResult.Success(new { issues });
    }
}

// Register with server
var server = new SkillServer();
server.RegisterSkill(new CodeAnalysisSkill());
```

### Define a Workflow

```csharp
var workflow = new WorkflowDefinition
{
    Id = "pr-review",
    Name = "Pull Request Review",
    Description = "Complete PR review workflow",
    Steps = new[]
    {
        new WorkflowStep
        {
            Id = "1-analyze",
            SkillId = "code-analysis",
            Parameters = new { code = "${pr.diff}", language = "csharp" }
        },
        new WorkflowStep
        {
            Id = "2-security",
            SkillId = "security-review",
            DependsOn = "1-analyze",
            Parameters = new { code = "${pr.diff}" }
        },
        new WorkflowStep
        {
            Id = "3-summary",
            SkillId = "report-generator",
            DependsOn = new[] { "1-analyze", "2-security" },
            Parameters = new 
            { 
                analysis = "${1-analyze.output}",
                security = "${2-security.output}"
            }
        }
    }
};

server.RegisterWorkflow(workflow);
```

### Agent Discovers and Uses Skills

```csharp
// 1. Discover available skills
var skills = await client.GetSkillsAsync();

// 2. Install desired skills
await client.InstallSkillAsync("code-analysis", new SkillInstallOptions
{
    Version = "1.0.0",
    Configuration = new { language = "csharp" }
});

// 3. Use skill in agent
var result = await agent.ExecuteSkillAsync("code-analysis", new
{
    code = sourceCode,
    language = "csharp"
});
```

### Configure Skills Per Agent

```json
{
  "agents": {
    "agent-review": {
      "allowedSkills": ["code-analysis", "security-review", "report-generator"],
      "skillConfigs": {
        "code-analysis": {
          "timeout": 30000,
          "maxParallelJobs": 5,
          "language": "csharp"
        },
        "security-review": {
          "severity": "high",
          "enabled": true
        }
      },
      "quotas": {
        "dailyExecutions": 1000,
        "requestsPerMinute": 100
      }
    }
  }
}
```

## Server Setup & Deployment

### 1. Build the Project

```bash
# From the repository root, build in Release mode
dotnet build Library/EonaCat.MCP.sln -c Release

# Verify build succeeded (0 errors, 0 warnings)
```

### 2. Create Your Server Instance

```csharp
// Program.cs - Create a basic MCP server
using EonaCat.MCP;

class Program
{
    static async Task Main()
    {
        // 1. Create server instance
        var server = new SkillServer();
        
        // 2. Register your skills
        await server.RegisterSkillAsync(new YourCustomSkill());
        await server.RegisterSkillAsync(new AnotherSkill());
        
        // 3. Configure agent access
        var agentConfig = new AgentConfiguration
        {
            AgentId = "my-agent",
            AllowedSkillIds = new[] { "skill-1", "skill-2" },
            Quotas = new AgentQuotas
            {
                MaxSkillExecutionsPerDay = 1000,
                MaxConcurrentExecutions = 10
            }
        };
        
        await server.AgentConfigurationManager.SaveAgentConfigurationAsync(agentConfig);
        
        // 4. Create and register workflows (optional)
        var workflow = new WorkflowDefinition { /* ... */ };
        await server.RegisterWorkflowAsync(workflow);
        
        // 5. Start listening for requests
        Console.WriteLine("MCP Skill Server is ready!");
        Console.WriteLine("📍 Agents can now connect and discover skills");
        
        // Keep server running
        await Task.Delay(Timeout.Infinite);
    }
}
```

### 3. Run the Examples

See the [examples guide](examples/README.md) for package restore and run commands.

### 4. Configure for Production

Create a `appsettings.json` file:

```json
{
  "SkillServer": {
    "MaxConcurrentExecutions": 50,
    "DefaultTimeoutMs": 30000,
    "EnableMetrics": true,
    "EnableLogging": true
  },
  "Agents": {
    "DefaultQuotas": {
      "MaxExecutionsPerDay": 10000,
      "MaxConcurrentExecutions": 20
    }
  }
}
```

### 5. Docker Deployment (Optional)

Create a `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY bin/Release/net8.0/publish/ .
EXPOSE 5000
ENTRYPOINT ["dotnet", "EonaCat.MCP.dll"]
```

Build and run:

```bash
docker build -t eonacat-mcp:latest .
docker run -p 5000:5000 eonacat-mcp:latest
```

---

## Asset Management System

The MCP server includes a comprehensive **asset management system** that allows you to host, discover, and download various types of AI capabilities including skills, documentation, agents, instructions, hooks, plugins, and more.

### Supported Asset Types

- **Skills** - Executable C# libraries and function definitions
- **Documentation** - Markdown files, guides, and API references
- **Agents** - Agent configuration files and definitions
- **Instructions** - System prompts, role definitions, and behavioral guidelines
- **Hooks** - Integration hooks and event handlers
- **Plugins** - Extensible plugin modules
- **Templates** - Workflow and skill templates
- **Configuration** - Server and application configuration files
- **Other** - Any additional asset files (JSON, YAML, XML, etc.)

### Asset API Endpoints

```
GET  /api/assets                    # List all assets
GET  /api/assets?type=Skill         # List assets by type
GET  /api/assets?tags=python,ai     # Filter by tags
GET  /api/assets/{id}/metadata      # Get asset metadata
GET  /api/assets/{id}/download      # Download asset binary/content
POST /api/assets                    # Upload/register asset
POST /api/assets/{id}/search        # Full-text search assets
DELETE /api/assets/{id}             # Delete asset
```

### Register an Asset

**Server-Side:**

```csharp
using EonaCat.MCP;

var server = new SkillServer();
var assetManager = server.AssetManager;

// Register a skill DLL as an asset
var skillAsset = new Asset
{
    Id = "my-skill-v1",
    Name = "My Custom Skill",
    Type = AssetType.Skill,
    Version = "1.0.0",
    Description = "A powerful skill for AI agents",
    Author = "Your Name",
    License = "MIT",
    Tags = new[] { "analysis", "python", "ai" },
    Dependencies = new[] { "base-skill-v1.0.0" },
    Metadata = new Dictionary<string, object>
    {
        { "language", "csharp" },
        { "platform", "dotnet8" },
        { "checksum", "sha256:..." }
    },
    FilePath = "skills/my-skill.dll",
    ContentType = "application/octet-stream"
};

await assetManager.RegisterAssetAsync(skillAsset, 
    contentProvider: async () => await File.ReadAllBytesAsync("skills/my-skill.dll")
);

// Register a markdown documentation asset
var docAsset = new Asset
{
    Id = "my-skill-docs",
    Name = "My Skill Documentation",
    Type = AssetType.Documentation,
    Version = "1.0.0",
    Description = "Complete documentation for My Custom Skill",
    Author = "Your Name",
    Tags = new[] { "documentation", "guide" },
    FilePath = "docs/my-skill.md",
    ContentType = "text/markdown"
};

await assetManager.RegisterAssetAsync(docAsset,
    contentProvider: async () => await File.ReadAllBytesAsync("docs/my-skill.md")
);

// Register agent configuration
var agentAsset = new Asset
{
    Id = "review-agent-config",
    Name = "Review Agent Configuration",
    Type = AssetType.Agent,
    Version = "1.0.0",
    Description = "Pre-configured review agent with optimal settings",
    Tags = new[] { "agent", "review", "config" },
    FilePath = "agents/review-agent.json",
    ContentType = "application/json"
};

await assetManager.RegisterAssetAsync(agentAsset,
    contentProvider: async () => await File.ReadAllTextAsync("agents/review-agent.json")
);

// Register instruction/prompt
var instructionAsset = new Asset
{
    Id = "code-review-prompt",
    Name = "Code Review System Prompt",
    Type = AssetType.Instructions,
    Version = "1.0.0",
    Description = "Detailed instructions for conducting code reviews",
    Tags = new[] { "prompt", "instructions", "review" },
    FilePath = "prompts/code-review.md",
    ContentType = "text/markdown"
};

await assetManager.RegisterAssetAsync(instructionAsset,
    contentProvider: async () => await File.ReadAllTextAsync("prompts/code-review.md")
);
```

### Download Assets

**Client-Side (.NET):**

```csharp
using System.Net.Http;
using EonaCat.Json;

public class AssetClient
{
    private readonly HttpClient _httpClient;
    private readonly string _serverUrl;

    public AssetClient(string serverUrl = "http://localhost:5000")
    {
        _serverUrl = serverUrl;
        _httpClient = new HttpClient();
    }

    // List all available assets
    public async Task<List<AssetInfo>> ListAssetsAsync()
    {
        var response = await _httpClient.GetAsync($"{_serverUrl}/api/assets");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonHelper.ToObject<List<AssetInfo>>(json);
    }

    // List assets by type
    public async Task<List<AssetInfo>> ListAssetsByTypeAsync(AssetType type)
    {
        var response = await _httpClient.GetAsync(
            $"{_serverUrl}/api/assets?type={type}"
        );
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonHelper.ToObject<List<AssetInfo>>(json);
    }

    // Filter assets by tags
    public async Task<List<AssetInfo>> FilterAssetsByTagsAsync(params string[] tags)
    {
        var tagString = string.Join(",", tags);
        var response = await _httpClient.GetAsync(
            $"{_serverUrl}/api/assets?tags={tagString}"
        );
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonHelper.ToObject<List<AssetInfo>>(json);
    }

    // Get asset metadata
    public async Task<AssetMetadata> GetAssetMetadataAsync(string assetId)
    {
        var response = await _httpClient.GetAsync(
            $"{_serverUrl}/api/assets/{assetId}/metadata"
        );
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonHelper.ToObject<AssetMetadata>(json);
    }

    // Download asset content
    public async Task<byte[]> DownloadAssetAsync(string assetId)
    {
        var response = await _httpClient.GetAsync(
            $"{_serverUrl}/api/assets/{assetId}/download"
        );
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsByteArrayAsync();
    }

    // Download and save asset to disk
    public async Task<string> DownloadAssetToFileAsync(string assetId, string outputPath)
    {
        var content = await DownloadAssetAsync(assetId);
        
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        await File.WriteAllBytesAsync(outputPath, content);
        return outputPath;
    }

    // Download text-based asset
    public async Task<string> DownloadAssetAsTextAsync(string assetId)
    {
        var content = await DownloadAssetAsync(assetId);
        return System.Text.Encoding.UTF8.GetString(content);
    }

    // Search assets
    public async Task<List<AssetInfo>> SearchAssetsAsync(string query)
    {
        var response = await _httpClient.PostAsync(
            $"{_serverUrl}/api/assets/{query}/search",
            null
        );
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonHelper.ToObject<List<AssetInfo>>(json);
    }

    // Delete asset
    public async Task<bool> DeleteAssetAsync(string assetId)
    {
        var response = await _httpClient.DeleteAsync(
            $"{_serverUrl}/api/assets/{assetId}"
        );
        return response.IsSuccessStatusCode;
    }
}

// Usage example
var client = new AssetClient();

// List all skills
var skills = await client.ListAssetsByTypeAsync(AssetType.Skill);
Console.WriteLine($"Found {skills.Count} skills available");

// Get documentation for a skill
var metadata = await client.GetAssetMetadataAsync("my-skill-v1");
Console.WriteLine($"Skill version: {metadata.Version}");
Console.WriteLine($"Author: {metadata.Author}");
Console.WriteLine($"Dependencies: {string.Join(", ", metadata.Dependencies)}");

// Download skill DLL
await client.DownloadAssetToFileAsync("my-skill-v1", "./downloaded-skills/my-skill.dll");

// Download documentation as text
var docs = await client.DownloadAssetAsTextAsync("my-skill-docs");
Console.WriteLine(docs);

// Download and parse configuration
var configJson = await client.DownloadAssetAsTextAsync("review-agent-config");
var config = JsonHelper.ToObject<AgentConfiguration>(configJson);

// Get system prompt/instructions
var systemPrompt = await client.DownloadAssetAsTextAsync("code-review-prompt");
Console.WriteLine($"System Prompt: {systemPrompt}");

// Filter by tags
var aiSkills = await client.FilterAssetsByTagsAsync("ai", "analysis");
Console.WriteLine($"Found {aiSkills.Count} AI analysis skills");
```

### File-Based Asset Storage

For production, use file-based storage that persists assets to disk:

```csharp
var assetManager = new FileSystemAssetManager("./assets");
var server = new SkillServer(assetManager: assetManager);

// Assets are stored as:
// ./assets/
//   ├── metadata/
//   │   ├── my-skill-v1.json
//   │   ├── my-skill-docs.json
//   │   └── ...
//   └── content/
//       ├── my-skill-v1.bin
//       ├── my-skill-docs.md
//       └── ...
```

### Full Asset Download & Integration Example

**Complete agent setup with asset downloads:**

```csharp
using EonaCat.MCP;
using EonaCat.Json;

public class AgentSetup
{
    private readonly AssetClient _assetClient;
    private readonly string _agentDirectory;

    public AgentSetup(string serverUrl, string agentDirectory = "./agent-data")
    {
        _assetClient = new AssetClient(serverUrl);
        _agentDirectory = agentDirectory;
        Directory.CreateDirectory(agentDirectory);
    }

    // Setup agent with all required assets
    public async Task SetupAgentAsync(string skillId, string docsId, string configId)
    {
        Console.WriteLine("Setting up agent...");

        // 1. Download skill DLL
        var skillPath = Path.Combine(_agentDirectory, "skills", $"{skillId}.dll");
        await _assetClient.DownloadAssetToFileAsync(skillId, skillPath);
        Console.WriteLine($"✓ Downloaded skill: {skillPath}");

        // 2. Download documentation
        var docsPath = Path.Combine(_agentDirectory, "docs", $"{docsId}.md");
        var docsContent = await _assetClient.DownloadAssetAsTextAsync(docsId);
        Directory.CreateDirectory(Path.GetDirectoryName(docsPath));
        await File.WriteAllTextAsync(docsPath, docsContent);
        Console.WriteLine($"✓ Downloaded documentation: {docsPath}");

        // 3. Download and parse configuration
        var configJson = await _assetClient.DownloadAssetAsTextAsync(configId);
        var config = JsonHelper.ToObject<AgentConfiguration>(configJson);
        var configPath = Path.Combine(_agentDirectory, "config.json");
        await File.WriteAllTextAsync(configPath, configJson);
        Console.WriteLine($"✓ Downloaded configuration: {configPath}");

        // 4. Load skill assembly
        var assembly = System.Reflection.Assembly.LoadFrom(skillPath);
        Console.WriteLine($"✓ Loaded skill assembly from: {skillPath}");

        Console.WriteLine("\nAgent setup complete!");
        Console.WriteLine($"Agent directory: {_agentDirectory}");
        Console.WriteLine($"Skills: {skillPath}");
        Console.WriteLine($"Docs: {docsPath}");
        Console.WriteLine($"Config: {configPath}");
    }

    // Discover and display available assets
    public async Task DiscoverAssetsAsync()
    {
        var allAssets = await _assetClient.ListAssetsAsync();

        Console.WriteLine("\nAvailable Assets on Server:");
        Console.WriteLine("================================\n");

        var groupedByType = allAssets.GroupBy(a => a.Type);
        foreach (var group in groupedByType)
        {
            Console.WriteLine($"\n{group.Key} Assets:");
            foreach (var asset in group)
            {
                Console.WriteLine($"  • {asset.Name} ({asset.Id})");
                Console.WriteLine($"    Version: {asset.Version}");
                Console.WriteLine($"    Author: {asset.Author}");
                Console.WriteLine($"    Tags: {string.Join(", ", asset.Tags)}");
            }
        }
    }

    // Download all documentation for offline use
    public async Task DownloadAllDocsAsync()
    {
        var docs = await _assetClient.ListAssetsByTypeAsync(AssetType.Documentation);
        
        var docsDir = Path.Combine(_agentDirectory, "docs-all");
        Directory.CreateDirectory(docsDir);

        foreach (var doc in docs)
        {
            var content = await _assetClient.DownloadAssetAsTextAsync(doc.Id);
            var filePath = Path.Combine(docsDir, $"{doc.Id}.md");
            await File.WriteAllTextAsync(filePath, content);
            Console.WriteLine($"✓ Downloaded: {doc.Name}");
        }

        Console.WriteLine($"\nAll documentation saved to: {docsDir}");
    }
}

// Usage
var setup = new AgentSetup("http://localhost:5000");
await setup.DiscoverAssetsAsync();
await setup.SetupAgentAsync("my-skill-v1", "my-skill-docs", "review-agent-config");
await setup.DownloadAllDocsAsync();
```

### Asset Metadata Example

Each asset has comprehensive metadata:

```json
{
  "id": "my-skill-v1",
  "name": "My Custom Skill",
  "type": "Skill",
  "version": "1.0.0",
  "description": "A powerful skill for AI agents",
  "author": "Your Name",
  "license": "MIT",
  "tags": ["analysis", "python", "ai"],
  "dependencies": ["base-skill-v1.0.0"],
  "fileSize": 2097152,
  "contentType": "application/octet-stream",
  "checksum": "sha256:abc123...",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z",
  "metadata": {
    "language": "csharp",
    "platform": "dotnet8",
    "targetFramework": "net8.0"
  }
}
```

---

## C# Client Integration

### HttpClient-Based Client (Generic HTTP)

**Simple HTTP client for any .NET application:**

```csharp
using System;
using System.Net.Http;
using EonaCat.Json;
using System.Threading.Tasks;

public class SkillServerClient
{
    private readonly HttpClient _httpClient;
    private readonly string _serverUrl;

    public SkillServerClient(string serverUrl = "http://localhost:5000")
    {
        _serverUrl = serverUrl;
        _httpClient = new HttpClient();
    }

    public async Task<List<SkillInfo>> DiscoverSkillsAsync()
    {
        var response = await _httpClient.GetAsync($"{_serverUrl}/api/skills");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<SkillInfo>>(content) ?? new();
    }

    public async Task<SkillInfo> GetSkillInfoAsync(string skillId)
    {
        var response = await _httpClient.GetAsync(
            $"{_serverUrl}/api/skills/{skillId}/manifest"
        );
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<SkillInfo>(content);
    }

    public async Task<SkillResult> ExecuteSkillAsync(string skillId, object input)
    {
        var json = JsonSerializer.Serialize(input);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(
            $"{_serverUrl}/api/skills/{skillId}/execute",
            content
        );
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<SkillResult>(responseContent);
    }

    public async Task<WorkflowResult> ExecuteWorkflowAsync(string workflowId, object input)
    {
        var json = JsonSerializer.Serialize(input);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(
            $"{_serverUrl}/api/workflows/{workflowId}/execute",
            content
        );
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<WorkflowResult>(responseContent);
    }
}

// Usage
var client = new SkillServerClient();
var skills = await client.DiscoverSkillsAsync();
Console.WriteLine($"Found {skills.Count} skills");

var result = await client.ExecuteSkillAsync("text-analysis", new { text = "Hello world" });
Console.WriteLine($"Result: {result.Data}");
```

### Refit-Based Client (Type-Safe REST)

**Using Refit for type-safe REST client:**

```bash
dotnet add package Refit
dotnet add package Refit.HttpClientFactory
```

```csharp
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ISkillServerApi
{
    [Get("/api/skills")]
    Task<List<SkillInfo>> GetSkillsAsync();

    [Get("/api/skills/{skillId}/manifest")]
    Task<SkillInfo> GetSkillInfoAsync(string skillId);

    [Post("/api/skills/{skillId}/execute")]
    Task<SkillResult> ExecuteSkillAsync(string skillId, [Body] object input);

    [Get("/api/skills/{skillId}/parameters")]
    Task<List<SkillParameter>> GetSkillParametersAsync(string skillId);

    [Post("/api/workflows/{workflowId}/execute")]
    Task<WorkflowResult> ExecuteWorkflowAsync(string workflowId, [Body] object input);

    [Get("/api/health")]
    Task<HealthStatus> GetHealthAsync();
}

// Setup in Startup.cs or Program.cs
services.AddRefitClient<ISkillServerApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5000"));

// Usage
public class MyAgent
{
    private readonly ISkillServerApi _skillApi;

    public MyAgent(ISkillServerApi skillApi)
    {
        _skillApi = skillApi;
    }

    public async Task ProcessAsync(string text)
    {
        // Discover available skills
        var skills = await _skillApi.GetSkillsAsync();
        
        // Execute a skill
        var result = await _skillApi.ExecuteSkillAsync("text-analysis", new { text });
        
        Console.WriteLine($"Analysis: {result.Data}");
    }
}
```

### Polly-Based Client with Retry Policy

**Resilient client with retry and circuit breaker:**

```bash
dotnet add package Polly
dotnet add package Polly.CircuitBreaker
```

```csharp
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Net.Http;
using EonaCat.Json;
using System.Threading.Tasks;

public class ResilientSkillServerClient
{
    private readonly HttpClient _httpClient;
    private readonly string _serverUrl;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreakerPolicy;

    public ResilientSkillServerClient(string serverUrl = "http://localhost:5000")
    {
        _serverUrl = serverUrl;
        _httpClient = new HttpClient();

        // Retry policy: 3 retries with exponential backoff
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timespan.TotalMilliseconds}ms");
                }
            );

        // Circuit breaker: open after 5 failures
        _circuitBreakerPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    Console.WriteLine($"Circuit breaker opened for {duration.TotalSeconds}s");
                }
            );
    }

    public async Task<SkillResult> ExecuteSkillAsync(string skillId, object input)
    {
        var policy = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy);

        var json = JsonSerializer.Serialize(input);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_serverUrl}/api/skills/{skillId}/execute")
        {
            Content = content
        };

        var response = await policy.ExecuteAsync(async () =>
            await _httpClient.SendAsync(request)
        );

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<SkillResult>(responseContent);
    }
}

// Usage with error handling
var client = new ResilientSkillServerClient();
try
{
    var result = await client.ExecuteSkillAsync("text-analysis", new { text = "Hello" });
    Console.WriteLine($"Success: {result.Data}");
}
catch (BrokenCircuitException)
{
    Console.WriteLine("Server is temporarily unavailable");
}
```

### ASP.NET Core Integration

**Integrate skill server into ASP.NET Core application:**

```bash
dotnet add package Refit
dotnet add package Polly
dotnet add package Microsoft.Extensions.Http.Polly
```

```csharp
// Startup.cs or Program.cs
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Refit;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register the skill server client
        services.AddRefitClient<ISkillServerApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5000"))
            .AddTransientHttpErrorPolicy(p =>
                p.WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
                )
            )
            .AddTransientHttpErrorPolicy(p =>
                p.CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                )
            );

        services.AddControllers();
    }
}

// Controller using the client
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly ISkillServerApi _skillApi;

    public AnalysisController(ISkillServerApi skillApi)
    {
        _skillApi = skillApi;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeAsync([FromBody] AnalysisRequest request)
    {
        try
        {
            var result = await _skillApi.ExecuteSkillAsync("text-analysis", new
            {
                text = request.Text,
                language = request.Language
            });

            return Ok(new { success = true, data = result.Data });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new { error = "Skill server unavailable", details = ex.Message });
        }
    }

    [HttpGet("skills")]
    public async Task<IActionResult> GetSkillsAsync()
    {
        var skills = await _skillApi.GetSkillsAsync();
        return Ok(skills);
    }

    [HttpPost("workflow/{workflowId}")]
    public async Task<IActionResult> ExecuteWorkflowAsync(
        string workflowId,
        [FromBody] object workflowInput)
    {
        var result = await _skillApi.ExecuteWorkflowAsync(workflowId, workflowInput);
        return Ok(result);
    }
}

public class AnalysisRequest
{
    public string Text { get; set; }
    public string Language { get; set; }
}
```

### Advanced Client with Caching

**Client with result caching and skill discovery caching:**

```bash
dotnet add package Microsoft.Extensions.Caching.Memory
```

```csharp
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Net.Http;
using EonaCat.Json;
using System.Threading.Tasks;

public class CachedSkillServerClient
{
    private readonly HttpClient _httpClient;
    private readonly string _serverUrl;
    private readonly IMemoryCache _cache;
    private const string SkillsCacheKey = "mcp_skills";
    private const int SkillsCacheDurationMinutes = 60;

    public CachedSkillServerClient(
        string serverUrl = "http://localhost:5000",
        IMemoryCache cache = null)
    {
        _serverUrl = serverUrl;
        _httpClient = new HttpClient();
        _cache = cache ?? new MemoryCache(new MemoryCacheOptions());
    }

    public async Task<List<SkillInfo>> DiscoverSkillsAsync(bool useCache = true)
    {
        if (useCache && _cache.TryGetValue(SkillsCacheKey, out List<SkillInfo> cachedSkills))
        {
            Console.WriteLine("Using cached skills");
            return cachedSkills;
        }

        var response = await _httpClient.GetAsync($"{_serverUrl}/api/skills");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var skills = JsonSerializer.Deserialize<List<SkillInfo>>(content) ?? new();

        // Cache the results
        _cache.Set(SkillsCacheKey, skills, TimeSpan.FromMinutes(SkillsCacheDurationMinutes));

        return skills;
    }

    public async Task<SkillResult> ExecuteSkillAsync(
        string skillId,
        object input,
        bool cacheResult = false,
        int cacheDurationSeconds = 300)
    {
        string cacheKey = $"skill_{skillId}_{JsonSerializer.Serialize(input)}";

        if (cacheResult && _cache.TryGetValue(cacheKey, out SkillResult cachedResult))
        {
            Console.WriteLine("Using cached skill result");
            return cachedResult;
        }

        var json = JsonSerializer.Serialize(input);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(
            $"{_serverUrl}/api/skills/{skillId}/execute",
            content
        );
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SkillResult>(responseContent);

        if (cacheResult)
        {
            _cache.Set(cacheKey, result, TimeSpan.FromSeconds(cacheDurationSeconds));
        }

        return result;
    }

    public void InvalidateCache()
    {
        _cache.Remove(SkillsCacheKey);
    }
}

// Usage
var cache = new MemoryCache(new MemoryCacheOptions());
var client = new CachedSkillServerClient(cache: cache);

// First call - hits the server
var skills1 = await client.DiscoverSkillsAsync();

// Second call - uses cache
var skills2 = await client.DiscoverSkillsAsync();

// Execute with caching
var result = await client.ExecuteSkillAsync(
    "text-analysis",
    new { text = "Hello" },
    cacheResult: true,
    cacheDurationSeconds: 300
);
```

### Console Application Example

**Complete console app demonstrating all features:**

```csharp
using System;
using System.Collections.Generic;
using EonaCat.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("EonaCat MCP Skill Server - C# Client Example");
        Console.WriteLine("================================================\n");

        var client = new SkillServerClient("http://localhost:5000");

        try
        {
            // 1. Discover skills
            Console.WriteLine("1. Discovering available skills...");
            var skills = await client.DiscoverSkillsAsync();
            Console.WriteLine($"   Found {skills.Count} skills:\n");
            
            foreach (var skill in skills)
            {
                Console.WriteLine($"   • {skill.Name} ({skill.Id})");
                Console.WriteLine($"     Description: {skill.Description}");
                Console.WriteLine($"     Version: {skill.Version}\n");
            }

            // 2. Get skill details
            if (skills.Count > 0)
            {
                Console.WriteLine("\n2. Getting skill details...");
                var skillId = skills[0].Id;
                var skillInfo = await client.GetSkillInfoAsync(skillId);
                Console.WriteLine($"   Skill: {skillInfo.Name}");
                Console.WriteLine($"   Parameters:");
                
                foreach (var param in skillInfo.Parameters)
                {
                    Console.WriteLine($"     - {param.Name} ({param.Type}): {param.Description}");
                }

                // 3. Execute skill
                Console.WriteLine("\n3. Executing skill...");
                var testInput = new { text = "Hello, MCP Server!" };
                Console.WriteLine($"   Input: {JsonSerializer.Serialize(testInput)}");
                
                var result = await client.ExecuteSkillAsync(skillId, testInput);
                Console.WriteLine($"   Success: {result.Success}");
                Console.WriteLine($"   Data: {result.Data}");
                Console.WriteLine($"   Execution Time: {result.ExecutionTimeMs}ms");
            }

            // 4. Execute workflow
            Console.WriteLine("\n4. Discovering workflows...");
            var workflows = await client.DiscoverWorkflowsAsync();
            Console.WriteLine($"   Found {workflows.Count} workflows");

            if (workflows.Count > 0)
            {
                Console.WriteLine("\n5. Executing workflow...");
                var workflowId = workflows[0].Id;
                var workflowResult = await client.ExecuteWorkflowAsync(
                    workflowId,
                    new { input = "Test data" }
                );
                Console.WriteLine($"   Success: {workflowResult.Success}");
                Console.WriteLine($"   Steps Executed: {workflowResult.StepsExecuted}");
            }

            Console.WriteLine("\nAll operations completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"   Details: {ex.InnerException?.Message}");
        }
    }
}

// Data models
public class SkillInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }
    public List<SkillParameter> Parameters { get; set; } = new();
}

public class SkillParameter
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
}

public class SkillResult
{
    public bool Success { get; set; }
    public object Data { get; set; }
    public int ExecutionTimeMs { get; set; }
    public string Error { get; set; }
}

public class WorkflowInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class WorkflowResult
{
    public bool Success { get; set; }
    public int StepsExecuted { get; set; }
    public object Data { get; set; }
    public string Error { get; set; }
}
```

### Unit Testing Example

**How to test your C# client:**

```bash
dotnet add package xunit
dotnet add package Moq
dotnet add package Moq.Contrib.HttpClientFactory
```

```csharp
using Moq;
using Moq.Contrib.HttpClientFactory;
using System.Net;
using System.Net.Http;
using EonaCat.Json;
using System.Threading.Tasks;
using Xunit;

public class SkillServerClientTests
{
    [Fact]
    public async Task DiscoverSkillsAsync_ShouldReturnSkills()
    {
        // Arrange
        var mockFactory = new Mock<IHttpClientFactory>();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        var skills = new[]
        {
            new { id = "skill-1", name = "Skill 1" },
            new { id = "skill-2", name = "Skill 2" }
        };
        
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(skills))
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var client = new SkillServerClient();
        var result = await client.DiscoverSkillsAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ExecuteSkillAsync_WithValidInput_ShouldReturnResult()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        var expectedResult = new { success = true, data = "processed" };
        
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResult))
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var client = new SkillServerClient();

        // Act
        var result = await client.ExecuteSkillAsync("test-skill", new { input = "test" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }
}
```

---

## Connecting from LLM Clients

The MCP Skill Server exposes an interface that any LLM client can connect to discover and use skills. Below are examples for popular platforms:

### Claude (Anthropic)

**Setup Claude to connect to your MCP server:**

1. Install Claude Desktop or use Claude API
2. Configure MCP connection in Claude settings
3. Connect and discover skills

**Example: Python client for Claude integration**

```python
import anthropic
import httpx
import json

class ClaudeSkillClient:
    def __init__(self, server_url: str = "http://localhost:5000"):
        self.server_url = server_url
        self.client = anthropic.Anthropic()
        self.http_client = httpx.Client()
    
    def discover_skills(self) -> list:
        """Discover available skills from MCP server"""
        response = self.http_client.get(f"{self.server_url}/api/skills")
        return response.json()
    
    def get_skill_info(self, skill_id: str) -> dict:
        """Get detailed skill information"""
        response = self.http_client.get(
            f"{self.server_url}/api/skills/{skill_id}/manifest"
        )
        return response.json()
    
    def execute_skill_in_claude(self, skill_id: str, input_data: dict):
        """Execute a skill and use result in Claude"""
        # Execute skill via MCP server
        skill_response = self.http_client.post(
            f"{self.server_url}/api/skills/{skill_id}/execute",
            json=input_data
        )
        
        # Use result in Claude conversation
        result = skill_response.json()
        
        message = self.client.messages.create(
            model="claude-3-5-sonnet-20241022",
            max_tokens=1024,
            system="You are an AI assistant with access to various skills.",
            messages=[
                {
                    "role": "user",
                    "content": f"Process this using available skills: {json.dumps(input_data)}"
                },
                {
                    "role": "assistant",
                    "content": f"I executed the skill and got: {json.dumps(result)}"
                }
            ]
        )
        
        return message.content[0].text

# Usage
client = ClaudeSkillClient()
skills = client.discover_skills()
print(f"Available skills: {[s['name'] for s in skills]}")

result = client.execute_skill_in_claude(
    "text-analysis",
    {"text": "Analyze this document"}
)
print(result)
```

**Claude Desktop Configuration** (`.claude/config.json`):

```json
{
  "mcpServers": {
    "eonacat-skills": {
      "command": "http",
      "url": "http://localhost:5000",
      "type": "mcp"
    }
  }
}
```

### ChatGPT / OpenAI API

**Setup ChatGPT to connect to your MCP server:**

```python
from openai import OpenAI
import httpx
import json

class ChatGPTSkillClient:
    def __init__(self, server_url: str = "http://localhost:5000"):
        self.server_url = server_url
        self.client = OpenAI()  # Uses OPENAI_API_KEY env var
        self.http_client = httpx.Client()
    
    def get_available_tools(self) -> list:
        """Get all available skills as OpenAI tools"""
        response = self.http_client.get(f"{self.server_url}/api/skills")
        skills = response.json()
        
        tools = []
        for skill in skills:
            tool = {
                "type": "function",
                "function": {
                    "name": skill["id"],
                    "description": skill["description"],
                    "parameters": {
                        "type": "object",
                        "properties": {
                            param["name"]: {
                                "type": param["type"],
                                "description": param["description"]
                            }
                            for param in skill.get("parameters", [])
                        }
                    }
                }
            }
            tools.append(tool)
        
        return tools
    
    def execute_skill(self, skill_id: str, **kwargs) -> dict:
        """Execute a skill on the MCP server"""
        response = self.http_client.post(
            f"{self.server_url}/api/skills/{skill_id}/execute",
            json=kwargs
        )
        return response.json()
    
    def chat_with_skills(self, user_message: str) -> str:
        """Chat with ChatGPT using available skills"""
        tools = self.get_available_tools()
        
        messages = [{"role": "user", "content": user_message}]
        
        response = self.client.chat.completions.create(
            model="gpt-4-turbo",
            messages=messages,
            tools=tools,
            tool_choice="auto"
        )
        
        # Handle tool calls if ChatGPT requests them
        if response.choices[0].message.tool_calls:
            for tool_call in response.choices[0].message.tool_calls:
                skill_id = tool_call.function.name
                args = json.loads(tool_call.function.arguments)
                
                result = self.execute_skill(skill_id, **args)
                
                messages.append({
                    "role": "assistant",
                    "content": response.choices[0].message.content
                })
                messages.append({
                    "role": "tool",
                    "tool_call_id": tool_call.id,
                    "content": json.dumps(result)
                })
                
                # Get final response
                final_response = self.client.chat.completions.create(
                    model="gpt-4-turbo",
                    messages=messages
                )
                
                return final_response.choices[0].message.content
        
        return response.choices[0].message.content

# Usage
client = ChatGPTSkillClient()
response = client.chat_with_skills(
    "Analyze this text and tell me what it's about"
)
print(response)
```

### GitHub Copilot (Codex)

**Setup GitHub Copilot to use MCP skills:**

```python
import requests
import json
from typing import Optional

class CopilotSkillClient:
    def __init__(self, server_url: str = "http://localhost:5000"):
        self.server_url = server_url
        self.session = requests.Session()
    
    def get_copilot_manifest(self) -> dict:
        """Get manifest that Copilot can understand"""
        skills = self.session.get(f"{self.server_url}/api/skills").json()
        
        return {
            "version": "1.0",
            "name": "EonaCat MCP Skills",
            "description": "Extended capabilities for GitHub Copilot",
            "capabilities": [
                {
                    "id": skill["id"],
                    "name": skill["name"],
                    "description": skill["description"],
                    "invoke": f"{self.server_url}/api/skills/{skill['id']}/execute",
                    "parameters": skill.get("parameters", [])
                }
                for skill in skills
            ]
        }
    
    def execute_via_copilot(self, skill_id: str, **params) -> dict:
        """Execute skill in Copilot context"""
        response = self.session.post(
            f"{self.server_url}/api/skills/{skill_id}/execute",
            json=params,
            headers={"X-Client": "github-copilot"}
        )
        return response.json()

# Usage in VS Code extension or Copilot
client = CopilotSkillClient()
manifest = client.get_copilot_manifest()
print(f"Copilot can use {len(manifest['capabilities'])} skills")
```

**VS Code Copilot Extension Integration:**

```javascript
// extension.js - VS Code extension for EonaCat skills
import * as vscode from 'vscode';
import axios from 'axios';

export async function activate(context: vscode.ExtensionContext) {
    const serverUrl = 'http://localhost:5000';
    
    // Get available skills
    const response = await axios.get(`${serverUrl}/api/skills`);
    const skills = response.data;
    
    // Register command for each skill
    for (const skill of skills) {
        const commandId = `eonacat.${skill.id}`;
        
        const command = vscode.commands.registerCommand(commandId, async () => {
            const input = await vscode.window.showInputBox({
                prompt: `Enter parameters for ${skill.name}`
            });
            
            if (input) {
                const result = await axios.post(
                    `${serverUrl}/api/skills/${skill.id}/execute`,
                    JSON.parse(input)
                );
                
                vscode.window.showInformationMessage(
                    `${skill.name}: ${JSON.stringify(result.data)}`
                );
            }
        });
        
        context.subscriptions.push(command);
    }
}
```

### Kimi (智谱清言)

**Setup Kimi to connect to MCP server:**

```python
import requests
import json

class KimiSkillClient:
    def __init__(self, api_key: str, server_url: str = "http://localhost:5000"):
        self.api_key = api_key
        self.server_url = server_url
        self.base_url = "https://api.kimi.com/v1"
        self.session = requests.Session()
        self.session.headers.update({"Authorization": f"Bearer {api_key}"})
    
    def get_skills_for_kimi(self) -> list:
        """Get skills formatted for Kimi API"""
        response = self.session.get(f"{self.server_url}/api/skills")
        skills = response.json()
        
        tools = []
        for skill in skills:
            tools.append({
                "type": "function",
                "function": {
                    "name": skill["id"],
                    "description": skill["description"],
                    "parameters": {
                        "type": "object",
                        "properties": {
                            param["name"]: {
                                "type": param.get("type", "string"),
                                "description": param.get("description", "")
                            }
                            for param in skill.get("parameters", [])
                        },
                        "required": [param["name"] for param in skill.get("parameters", [])]
                    }
                }
            })
        
        return tools
    
    def chat_with_kimi(self, user_message: str) -> str:
        """Send message to Kimi with available skills"""
        tools = self.get_skills_for_kimi()
        
        response = self.session.post(
            f"{self.base_url}/chat/completions",
            json={
                "model": "kimi-9b-latest",
                "messages": [
                    {"role": "user", "content": user_message}
                ],
                "tools": tools,
                "tool_choice": "auto"
            }
        )
        
        return response.json()

# Usage
client = KimiSkillClient(api_key="your-kimi-api-key")
response = client.chat_with_kimi("Please analyze this code")
print(response)
```

### DeepSeek

**Setup DeepSeek to use MCP skills:**

```python
import requests
import json

class DeepSeekSkillClient:
    def __init__(self, api_key: str, server_url: str = "http://localhost:5000"):
        self.api_key = api_key
        self.server_url = server_url
        self.base_url = "https://api.deepseek.com/v1"
        self.session = requests.Session()
        self.session.headers.update({
            "Authorization": f"Bearer {api_key}",
            "Content-Type": "application/json"
        })
    
    def execute_skill(self, skill_id: str, **params) -> dict:
        """Execute a skill on MCP server"""
        response = self.session.post(
            f"{self.server_url}/api/skills/{skill_id}/execute",
            json=params
        )
        return response.json()
    
    def process_with_deepseek(self, user_message: str) -> str:
        """Process message with DeepSeek using MCP skills"""
        
        # Get available skills
        skills_response = self.session.get(f"{self.server_url}/api/skills")
        skills = skills_response.json()
        
        # Format as function calls for DeepSeek
        functions = []
        for skill in skills:
            functions.append({
                "name": skill["id"],
                "description": skill["description"],
                "parameters": {
                    "type": "object",
                    "properties": {
                        param["name"]: {
                            "type": param.get("type", "string"),
                            "description": param.get("description", "")
                        }
                        for param in skill.get("parameters", [])
                    }
                }
            })
        
        # Call DeepSeek API
        response = self.session.post(
            f"{self.base_url}/chat/completions",
            json={
                "model": "deepseek-chat",
                "messages": [{"role": "user", "content": user_message}],
                "functions": functions
            }
        )
        
        return response.json()

# Usage
client = DeepSeekSkillClient(api_key="your-deepseek-api-key")
result = client.process_with_deepseek("Summarize this document")
print(result)
```

### Generic LLM Client Pattern

**Universal pattern for any LLM:**

```python
import requests
import json
from typing import Callable, Dict, Any

class GenericLLMSkillClient:
    """Universal client pattern for connecting any LLM to MCP skills"""
    
    def __init__(self, server_url: str = "http://localhost:5000"):
        self.server_url = server_url
        self.session = requests.Session()
        self.skill_cache = {}
    
    def discover_skills(self) -> Dict[str, Any]:
        """Discover all available skills"""
        if not self.skill_cache:
            response = self.session.get(f"{self.server_url}/api/skills")
            self.skill_cache = {skill["id"]: skill for skill in response.json()}
        return self.skill_cache
    
    def get_skill_definition(self, skill_id: str) -> Dict[str, Any]:
        """Get skill definition in LLM-compatible format"""
        skill = self.discover_skills().get(skill_id)
        if not skill:
            return None
        
        return {
            "id": skill["id"],
            "name": skill["name"],
            "description": skill["description"],
            "parameters": skill.get("parameters", []),
            "returns": skill.get("returns", {})
        }
    
    def execute_skill(self, skill_id: str, **kwargs) -> Dict[str, Any]:
        """Execute a skill on the server"""
        response = self.session.post(
            f"{self.server_url}/api/skills/{skill_id}/execute",
            json=kwargs
        )
        return response.json()
    
    def format_as_tools(self) -> list:
        """Format all skills as tools for LLM function calling"""
        tools = []
        for skill_id, skill in self.discover_skills().items():
            tools.append({
                "id": skill["id"],
                "name": skill["name"],
                "description": skill["description"],
                "parameters": self._build_parameters_schema(skill.get("parameters", []))
            })
        return tools
    
    def _build_parameters_schema(self, params: list) -> Dict:
        """Build JSON schema for parameters"""
        return {
            "type": "object",
            "properties": {
                param["name"]: {
                    "type": param.get("type", "string"),
                    "description": param.get("description", "")
                }
                for param in params
            }
        }

# Usage - works with any LLM
client = GenericLLMSkillClient()

# Get available tools
tools = client.format_as_tools()
print(f"Available tools: {[t['name'] for t in tools]}")

# Execute a skill
result = client.execute_skill("text-analysis", text="Hello world")
print(f"Result: {result}")
```

---

## Building and Testing

```bash
# Build the library and generate the package
dotnet build Library/EonaCat.MCP.sln -c Release
```

See the [examples guide](README.md) for restoring and running examples
against the generated package.

## Adding a New Skill

1. Create a class inheriting from `Skill`
2. Implement `Metadata` property with skill information
3. Implement `ExecuteAsync` method with skill logic
4. Register with server: `server.RegisterSkill(new YourSkill())`
5. Write tests in `tests/Skills.Tests/`
6. Document in skill's README

That's it! Agents can immediately discover and use the new skill.

## License

MIT License. See LICENSE file for details.

