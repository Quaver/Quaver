using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers.Mods;
using Quaver.Shared.Screens.Selection.UI.Modifiers.Components;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Modifiers
{
    public class ModifierSelectorContainer : Sprite
    {
        /// <summary>
        /// </summary>
        private Bindable<SelectContainerPanel> ActiveLeftPanel { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Header { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus SubHeader { get; set; }

        /// <summary>
        /// </summary>
        private Sprite ModifierSelectorBackground { get; set; }

        /// <summary>
        /// </summary>
        private ModifierSelector Selector { get; set; }

        /// <summary>
        /// </summary>
        public ModifierSelectorContainer(Bindable<SelectContainerPanel> activeLeftPanel)
        {
            ActiveLeftPanel = activeLeftPanel;

            Size = new ScalableVector2(564, 838);
            Alpha = 0f;
            AutoScaleHeight = true;

            CreateHeaderText();
            CreateSubHeaderText();
            CreateModifierSelectorBackground();
        }

        /// <summary>
        ///    Creates <see cref="Header"/>
        /// </summary>
        private void CreateHeaderText()
        {
            Header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoHeavy), "MODIFIERS", 30)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
            };
        }

        /// <summary>
        ///    Creates <see cref="SubHeader"/>
        /// </summary>
        private void CreateSubHeaderText()
        {
            SubHeader = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Customize gameplay to your heart's desire".ToUpper(), 18)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Tint = ColorHelper.HexToColor("#808080")
            };

            SubHeader.Y = Header.Y + Header.Height - SubHeader.Height - 3;
        }

        /// <summary>
        ///     Creates <see cref="ModifierSelectorBackground"/>
        /// </summary>
        private void CreateModifierSelectorBackground()
        {
            ModifierSelectorBackground = new Sprite
            {
                Parent = this,
                Image = UserInterface.ModifierSelectorBackground,
                Size = new ScalableVector2(Width, Height - Header.Height - 8),
                Y = Header.Y + Header.Height + 8,
            };

            var width = (int) (Width - 4);

            Selector = new ModifierSelector(ActiveLeftPanel,
                new ScalableVector2(width, ModifierSelectorBackground.Height - 4), new List<ModifierSection>
                {
                    new ModifierSection(width, FontAwesome.Get(FontAwesomeIcon.fa_check_mark),"Ranked",
                        "These mods can be used for ranked scores", ColorHelper.HexToColor("#27B06E"), new List<SelectableModifier>()
                        {
                            new SelectableModifierSpeed(width),
                            new SelectableModifierJudgementWindows(width),
                            new SelectableModifierBool(width, new ModMirror()),
                            new SelectableModifierBool(width, new ModNoMiss())
                        }),

                    new ModifierSection(width, FontAwesome.Get(FontAwesomeIcon.fa_warning_sign_on_a_triangular_background), "Unranked",
                            "Scores will not be submitted while using these", ColorHelper.HexToColor("#F2C94C"), new List<SelectableModifier>()
                        {
                            new SelectableModifierBool(width, new ModAutoplay()),
                            new SelectableModifierBool(width, new ModCoop()),
                            new SelectableModifierBool(width, new ModNoFail()),
                            new SelectableModifierBool(width, new ModNoSliderVelocities()),
                            new SelectableModifierBool(width, new ModNoLongNotes()),
                            new SelectableModifierBool(width, new ModFullLN()),
                            new SelectableModifierBool(width, new ModInverse()),
                            new SelectableModifierBool(width, new ModRandomize()),
                        }),
                })
            {
                Parent = ModifierSelectorBackground,
                Alignment = Alignment.MidCenter
            };
        }
    }
}