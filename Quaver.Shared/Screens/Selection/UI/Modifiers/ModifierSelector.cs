using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;

namespace Quaver.Shared.Screens.Selection.UI.Modifiers
{
    public class ModifierSelector : ScrollContainer
    {
        /// <summary>
        /// </summary>
        private List<ModifierSection> Sections { get; }

        /// <summary>
        /// </summary>
        private Sprite ButtonBackground { get; set; }

        /// <summary>
        ///     The currently active tooltip that is displayed on top of the container
        /// </summary>
        public Tooltip ActiveTooltip { get; set; }

        /// <summary>
        /// </summary>
        private ImageButton ResetModifiersButton { get; set; }

        /// <summary>
        /// </summary>
        private ImageButton ClosePanelButton { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        /// <param name="sections"></param>
        public ModifierSelector(ScalableVector2 size, List<ModifierSection> sections) : base(size, size)
        {
            Sections = sections;
            Alpha = 0;

            AlignAndContainSections();
            CreateButtons();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleTooltipAnimation();

            base.Update(gameTime);
        }

        /// <summary>
        ///     Makes sure each section is contained and aligned properly
        /// </summary>
        private void AlignAndContainSections()
        {
            var totalY = 0f;

            for (var i = 0; i < Sections.Count; i++)
            {
                var section = Sections[i];

                AddContainedDrawable(section.Header);
                section.Header.Y = totalY;
                totalY += section.Header.Height;

                // Contain & Position Modifiers
                for (var j = 0; j < section.Modifiers.Count; j++)
                {
                    var mod = section.Modifiers[j];
                    mod.Selector = this;

                    // mod.OriginalColor = j % 2 == 0 ? ColorHelper.HexToColor("#363636") : ColorHelper.HexToColor("#242424");
                    mod.OriginalColor = ColorHelper.HexToColor("#242424");
                    AddContainedDrawable(mod);

                    mod.Y = totalY;
                    totalY += mod.Height;
                }
            }
        }

        /// <summary>
        /// </summary>
        private void CreateButtons()
        {
            ButtonBackground = new Sprite
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(Width, 70),
                Tint = ColorHelper.HexToColor("#181818")
            };

            ResetModifiersButton = new IconButton(UserInterface.EditPlayButton, (sender, args) => ModManager.RemoveAllMods())
            {
                Parent = ButtonBackground,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(250, 38),
                X = 12,
                Image = UserInterface.ResetMods
            };

            ClosePanelButton = new IconButton(UserInterface.EditPlayButton)
            {
                Parent = ButtonBackground,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(250, 38),
                X = -ResetModifiersButton.X,
                Image = UserInterface.ClosePanel
            };
        }

        /// <summary>
        ///     Sets the active tooltip
        /// </summary>
        /// <param name="tooltip"></param>
        public void ActivateTooltip(Tooltip tooltip)
        {
            if (ActiveTooltip != null)
                ActiveTooltip.Parent = null;

            ActiveTooltip = tooltip;

            if (ActiveTooltip == null)
                return;

            ActiveTooltip.Parent = this;

            ActiveTooltip.Alpha = 0;
            ActiveTooltip.ClearAnimations();
            ActiveTooltip.FadeTo(1, Easing.Linear, 150);
        }

        /// <summary>
        /// </summary>
        private void HandleTooltipAnimation()
        {
            if (ActiveTooltip == null)
                return;

            ActiveTooltip.X = MouseManager.CurrentState.X - AbsolutePosition.X - ActiveTooltip.Width / 2f;
            ActiveTooltip.Y = MouseManager.CurrentState.Y - AbsolutePosition.Y - ActiveTooltip.Height - 2;
        }
    }
}