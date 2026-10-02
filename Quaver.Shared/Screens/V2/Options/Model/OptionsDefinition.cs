using System;
using System.Collections.Generic;
using Quaver.Shared.Input.Global;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Input;

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

        public Func<string> ReadPresetValue { get; private set; }

        public Func<string, bool> IsPresetValueValid { get; private set; }

        public Func<string, bool> TryApplyPresetValue { get; private set; }

        public GlobalKeybindActions? PresetKeybindAction { get; private set; }

        public Func<List<Bindable<GenericKey>>> PresetKeyLayout { get; private set; }

        public OptionsDefinition(string id, OptionCategory category, string sectionName, string labelName, int? labelKeyCount = null, Func<Container, Drawable> controlFactory = null)
        {
            Id = id;
            Category = category;
            SectionName = sectionName;
            LabelName = labelName;
            LabelKeyCount = labelKeyCount;
            ControlFactory = controlFactory;
        }

        public void SetPresetValueAccessors(Func<string> read, Func<string, bool> isValid, Func<string, bool> tryApply)
        {
            ReadPresetValue = read ?? throw new ArgumentNullException(nameof(read));
            IsPresetValueValid = isValid ?? throw new ArgumentNullException(nameof(isValid));
            TryApplyPresetValue = tryApply ?? throw new ArgumentNullException(nameof(tryApply));
        }

        public void SetPresetKeybindAction(GlobalKeybindActions action) => PresetKeybindAction = action;

        public void SetPresetKeyLayout(Func<List<Bindable<GenericKey>>> keys) => PresetKeyLayout = keys ?? throw new ArgumentNullException(nameof(keys));
    }
}
