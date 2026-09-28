namespace Quaver.Shared.Screens.V2.Options.Model
{
    internal enum OptionCategory
    {
        Video,
        Audio,
        Gameplay,
        Skin,
        Input,
        Miscellaneous,
        Advanced
    }

    internal sealed class OptionsDefinition
    {
        public string Id { get; }

        public OptionCategory Category { get; }

        public string SectionName { get; }

        public string LabelName { get; }

        public int? LabelKeyCount { get; }

        public OptionsDefinition(string id, OptionCategory category, string sectionName, string labelName, int? labelKeyCount = null)
        {
            Id = id;
            Category = category;
            SectionName = sectionName;
            LabelName = labelName;
            LabelKeyCount = labelKeyCount;
        }
    }
}
