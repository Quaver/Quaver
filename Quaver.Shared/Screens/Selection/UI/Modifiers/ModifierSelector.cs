using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Selection.UI.Modifiers
{
    public class ModifierSelector : ScrollContainer
    {
        /// <summary>
        /// </summary>
        private const int ButtonBackgroundHeight = 83;

        /// <summary>
        /// </summary>
        private List<ModifierSection> Sections { get; }

        /// <summary>
        /// </summary>
        private Bindable<SelectContainerPanel> ActiveLeftPanel { get; }

        /// <summary>
        /// </summary>
        private Sprite ScrollbarBackground { get; set; }

        /// <summary>
        /// </summary>
        private Sprite ButtonBackground { get; set; }

        /// <summary>
        /// </summary>
        private ImageButton ResetModifiersButton { get; set; }

        /// <summary>
        /// </summary>
        private ImageButton ClosePanelButton { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="activeLeftPanel"></param>
        /// <param name="size"></param>
        /// <param name="sections"></param>
        public ModifierSelector(Bindable<SelectContainerPanel> activeLeftPanel, ScalableVector2 size, List<ModifierSection> sections) : base(size, size)
        {
            ActiveLeftPanel = activeLeftPanel;
            Sections = sections;
            AllowScrollbarDragging = true;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 220;
            IsMinScrollYEnabled = true;
            Alpha = 0;

            AlignAndContainSections();
            CreateScrollbar();
            CreateButtons();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && DialogManager.Dialogs.Count == 0
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt);
            ScrollbarBackground.Visible = ContentContainer.Height > Height;

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
                    // mod.OriginalColor = j % 2 == 0 ? ColorHelper.HexToColor("#363636") : ColorHelper.HexToColor("#242424");
                    mod.OriginalColor = ColorHelper.HexToColor("#242424");
                    AddContainedDrawable(mod);

                    mod.Y = totalY;
                    totalY += mod.Height;
                }
            }

            var contentHeight = totalY + ButtonBackgroundHeight;
            ContentContainer.Height = contentHeight > Height ? contentHeight : Height;
        }

        /// <summary>
        ///     Creates the scrollbar sprite and aligns it properly
        /// </summary>
        private void CreateScrollbar()
        {
            ScrollbarBackground = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = 30,
                Size = new ScalableVector2(4, Height),
                Tint = ColorHelper.HexToColor("#474747"),
                Visible = ContentContainer.Height > Height
            };

            MinScrollBarY = -(int) ScrollbarBackground.Height - (int) Scrollbar.Height / 2;
            Scrollbar.Width = ScrollbarBackground.Width;
            Scrollbar.Parent = ScrollbarBackground;
            Scrollbar.Alignment = Alignment.BotCenter;
            Scrollbar.Tint = Color.White;
        }

        /// <summary>
        /// </summary>
        private void CreateButtons()
        {
            ButtonBackground = new Sprite
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(Width, ButtonBackgroundHeight),
                Tint = ColorHelper.HexToColor("#181818")
            };

            ResetModifiersButton = new IconButton(UserInterface.EditPlayButton, (sender, args) =>
            {
                if (!PlaylistManager.CanEditSelectedTournamentModifiers())
                    return;

                if (OnlineManager.CurrentGame != null &&
                    (OnlineManager.CurrentGame.HostId != OnlineManager.Self?.OnlineUser?.Id && OnlineManager.CurrentGame.FreeModType == 0))
                {
                    return;
                }

                ModManager.RemoveAllMods();
            })
            {
                Parent = ButtonBackground,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(250, 38),
                X = 12,
                Image = UserInterface.ResetMods
            };

            ClosePanelButton = new IconButton(UserInterface.EditPlayButton, (sender, args) =>
            {
                if (ActiveLeftPanel == null)
                    return;

                var game = GameBase.Game as QuaverGame;

                switch (game?.CurrentScreen?.Type)
                {
                    case QuaverScreenType.Editor:
                    case QuaverScreenType.Select:
                        ActiveLeftPanel.Value = SelectContainerPanel.Leaderboard;
                        break;
                    case QuaverScreenType.Multiplayer:
                        ActiveLeftPanel.Value = SelectContainerPanel.MatchSettings;
                        break;
                }
            })
            {
                Parent = ButtonBackground,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(250, 38),
                X = -ResetModifiersButton.X,
                Image = UserInterface.ClosePanel
            };
        }

    }
}
