using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EonaCat.MCP;
using EonaCat.MCP.Core;
using EonaCat.MCP.Skills;

namespace EonaCat.MCP.Examples
{
    // Simple skill implementation for demonstration
    public class EchoSkill : Skill
    {
        public override SkillMetadata Metadata => new()
        {
            Id = "echo",
            Name = "Echo",
            Description = "Echoes back the input text",
            Version = "1.0.0",
            Author = "EonaCat",
            License = "MIT",
            Tags = new[] { "example", "text", "utility" },
            IsStable = true,
            AverageExecutionTimeMs = 1,
            InputParameters = new[]
            {
                new SkillParameter("message", "string", "Message to echo")
            },
            OutputParameters = new[]
            {
                new SkillParameter("echo", "string", "Echoed message")
            }
        };

        protected override async Task<SkillResult> ExecuteImplAsync(SkillInput input)
        {
            var message = input.GetParameter<string>("message") ?? "Hello";
            return await Task.FromResult(SkillResult.Success(new { echo = message }));
        }
    }

    // Program entry point
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║   EonaCat MCP Skill Server - Basic     ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.WriteLine();

            // Create a skill server with default in-memory implementations
            var skillServer = new SkillServer();

            // Register a sample skill
            await skillServer.RegisterSkillAsync(new EchoSkill());

            var skills = await skillServer.SkillRegistry.GetAllSkillsAsync();
            Console.WriteLine($"Skills registered: {skills.Count()}");
            Console.WriteLine();

            // Execute a skill as an example
            var input = new SkillInput();
            input.SetParameter("message", "Hello from EonaCat MCP!");

            var result = await skillServer.ExecuteSkillAsync("echo", input);
            Console.WriteLine($"Skill execution result: {result.Data}");
            Console.WriteLine();

            Console.WriteLine("Basic example completed successfully!");
        }
    }
}
