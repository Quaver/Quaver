using System;
using Wobble.Graphics;

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

        public Func<Container, Drawable> ControlFactory { get; }

        public OptionsDefinition(string id, OptionCategory category, string sectionName, string labelName, int? labelKeyCount = null, Func<Container, Drawable> controlFactory = null)
        {
            Id = id;
            Category = category;
            SectionName = sectionName;
            LabelName = labelName;
            LabelKeyCount = labelKeyCount;
            ControlFactory = controlFactory;
        }
    }
}
