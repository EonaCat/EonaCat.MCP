using System;
using System.Linq;
using System.Threading.Tasks;
using EonaCat.MCP;
using EonaCat.MCP.Core;

namespace EonaCat.MCP.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║   EonaCat MCP Client Example           ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.WriteLine();

            Console.WriteLine("This is a client example that demonstrates how to");
            Console.WriteLine("interact with an EonaCat MCP Skill Server.");
            Console.WriteLine();

            // Create a skill server for demonstration
            var skillServer = new SkillServer();

            var skills = await skillServer.SkillRegistry.GetAllSkillsAsync();
            Console.WriteLine($"Available Skills: {skills.Count()}");
            Console.WriteLine();

            // List all available skills
            if (skills.Any())
            {
                Console.WriteLine("Registered Skills:");
                foreach (var skill in skills)
                {
                    Console.WriteLine($"  - {skill.Metadata.Name} ({skill.Metadata.Id})");
                    Console.WriteLine($"    Description: {skill.Metadata.Description}");
                }
            }
            else
            {
                Console.WriteLine("No skills are currently registered on the server.");
            }

            Console.WriteLine();
            Console.WriteLine("Client example completed!");
        }
    }
}
