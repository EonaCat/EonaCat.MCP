using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EonaCat.MCP;
using EonaCat.MCP.Core;
using EonaCat.MCP.Skills;

namespace EonaCat.MCP.Examples
{
    // Simple example skills
    public class GreetingSkill : Skill
    {
        public override SkillMetadata Metadata => new()
        {
            Id = "greeting",
            Name = "Greeting",
            Description = "Provides personalized greetings",
            Version = "1.0.0",
            Author = "EonaCat",
            License = "MIT",
            Tags = new[] { "example", "greeting" },
            IsStable = true,
            AverageExecutionTimeMs = 1,
            InputParameters = new[]
            {
                new SkillParameter("name", "string", "Name to greet")
            },
            OutputParameters = new[]
            {
                new SkillParameter("greeting", "string", "Personalized greeting")
            }
        };

        protected override async Task<SkillResult> ExecuteImplAsync(SkillInput input)
        {
            var name = input.GetParameter<string>("name") ?? "World";
            return await Task.FromResult(SkillResult.Success(new 
            { 
                greeting = $"Hello, {name}! Welcome to EonaCat MCP." 
            }));
        }
    }

    public class CounterSkill : Skill
    {
        private int _count = 0;

        public override SkillMetadata Metadata => new()
        {
            Id = "counter",
            Name = "Counter",
            Description = "Maintains and returns a counter value",
            Version = "1.0.0",
            Author = "EonaCat",
            License = "MIT",
            Tags = new[] { "example", "counter" },
            IsStable = true,
            AverageExecutionTimeMs = 1,
            InputParameters = new[]
            {
                new SkillParameter("action", "string", "Action: increment, decrement, or reset")
            },
            OutputParameters = new[]
            {
                new SkillParameter("count", "int", "Current counter value")
            }
        };

        protected override async Task<SkillResult> ExecuteImplAsync(SkillInput input)
        {
            var action = (input.GetParameter<string>("action") ?? "get").ToLower();

            switch (action)
            {
                case "increment":
                    _count++;
                    break;
                case "decrement":
                    _count--;
                    break;
                case "reset":
                    _count = 0;
                    break;
            }

            return await Task.FromResult(SkillResult.Success(new { count = _count }));
        }
    }

    // Program entry point
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║   EonaCat MCP Skill Server - Full      ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.WriteLine();

            // Create a skill server
            var skillServer = new SkillServer();

            // Register multiple skills
            await skillServer.RegisterSkillAsync(new GreetingSkill());
            await skillServer.RegisterSkillAsync(new CounterSkill());

            var skills = await skillServer.SkillRegistry.GetAllSkillsAsync();
            Console.WriteLine($"Skills registered: {skills.Count()}");
            Console.WriteLine();

            // Demonstrate skill execution
            await DemonstrateSkills(skillServer);

            Console.WriteLine();
            Console.WriteLine("Full example completed successfully!");
        }

        private static async Task DemonstrateSkills(SkillServer skillServer)
        {
            // Execute Greeting Skill
            Console.WriteLine("--- Greeting Skill ---");
            var greetingInput = new SkillInput();
            greetingInput.SetParameter("name", "Alice");
            var greetingResult = await skillServer.ExecuteSkillAsync("greeting", greetingInput);
            Console.WriteLine($"Result: {greetingResult.Data}");
            Console.WriteLine();

            // Execute Counter Skill
            Console.WriteLine("--- Counter Skill ---");
            for (int i = 0; i < 3; i++)
            {
                var counterInput = new SkillInput();
                counterInput.SetParameter("action", "increment");
                var counterResult = await skillServer.ExecuteSkillAsync("counter", counterInput);
                Console.WriteLine($"After increment: {counterResult.Data}");
            }
        }
    }
}
