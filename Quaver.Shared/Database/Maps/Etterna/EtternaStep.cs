using SQLite;

namespace Quaver.Shared.Database.Maps.Etterna
{
    [Table("steps")]
    public class EtternaStep
    {
        [Column("STEPSTYPE")]
        public string StepsType { get; set; }

        [Column("CHARTKEY")]
        public string ChartKey { get; set; }

        [Column("DIFFICULTY")]
        public StepManiaDifficulty Difficulty { get; set; }

        [Column("CREDIT")]
        public string Credit { get; set; }

        [Column("STEPFILENAME")]
        public string StepFileName { get; set; }
    }
}