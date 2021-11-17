using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Results.UI.Header.Contents.Tabs
{
    public class ResultsTabSelector : Sprite
    {
        /// <summary>
        /// </summary>
        private Bindable<ResultsScreenTabType> ActiveTab { get; }

        /// <summary>
        /// </summary>
        private Bindable<ScoreProcessor> Processor { get; }

        /// <summary>
        /// </summary>
        private List<Sprite> ModifierSprites { get; } = new List<Sprite>();

        /// <summary>
        /// </summary>
        /// <param name="activeTab"></param>
        /// <param name="processor"></param>
        /// <param name="size"></param>
        public ResultsTabSelector(Bindable<ResultsScreenTabType> activeTab, Bindable<ScoreProcessor> processor, ScalableVector2 size)
        {
            ActiveTab = activeTab;
            Processor = processor;
            Size = size;

            Image = SkinManager.Skin?.Results?.ResultsTabSelectorBackground ?? UserInterface.ResultsTabSelectorBackground;

            CreateTabs();
            CreateModifiers();
        }

        /// <summary>
        /// </summary>
        private void CreateTabs()
        {
            var values = Enum.GetValues(typeof(ResultsScreenTabType));

            var posX = 202f;

            for (var i = 0; i < values.Length; i++)
            {
                var item = new ResultsTabItem(ActiveTab, (ResultsScreenTabType) i, Height)
                {
                    Parent = this,
                    Alignment = Alignment.MidLeft,
                    X = posX
                };

                posX += item.Width + 44;

                if ((ResultsScreenTabType) i == ResultsScreenTabType.Multiplayer && OnlineManager.CurrentGame == null)
                    item.Button.IsClickable = false;

                item.SetTint();
            }
        }

        /// <summary>
        /// </summary>
        private void CreateModifiers()
        {
            ModifierSprites.ForEach(x => x.Destroy());
            ModifierSprites.Clear();

            var modsList = ModManager.GetModsList(Processor.Value.Mods);

            if (modsList.Count == 0)
                modsList.Add(ModIdentifier.None);

            var posX = -38f;

            for (var i = modsList.Count - 1; i >= 0; i--)
            {
                var mod = modsList[i];

                // HealthAdjust doesn't have a proper icon
                if (mod == ModIdentifier.HeatlthAdjust)
                    continue;

                var texture = ModManager.GetTexture(mod);
                const float width = 72f;

                var sprite = new Sprite()
                {
                    Parent = this,
                    Alignment = Alignment.MidRight,
                    Image = texture,
                    Size = new ScalableVector2(width, (float) texture.Height / texture.Width * width),
                    X = posX
                };

                posX -= sprite.Width - 4;
                ModifierSprites.Add(sprite);
            }
        }
    }
}