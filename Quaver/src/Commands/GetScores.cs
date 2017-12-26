using System;
using System.Text;
using System.Threading.Tasks;
using Quaver.Database;

namespace Quaver.Commands
{
    public class GetScores : ICommand
    {
        public string Name { get; set; } = "GETSCORES";

        public int Args { get; set; } = 2;

        public string Description { get; set; } = "Gets a list of local scores on a particular map";

        public string Usage { get; set; } = "getscores <map hash>";
        
        public void Execute(string[] args)
        {
            Task.Run(async () =>
            {
                var scores = await LocalScoreCache.SelectBeatmapScores(args[1]);

                var sb = new StringBuilder();

                sb.AppendLine("Id | Name | Date | Score | Accuracy | Max Combo | Mods");
                foreach (var score in scores)
                    sb.AppendLine($"{score.Id} | {score.Name} | {score.DateTime} | {score.Score} | {score.Accuracy}% | {score.MaxCombo}x | {score.Mods}");
                
                Console.WriteLine(sb.ToString());
            });
        }
    }
}