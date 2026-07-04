using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Main.UI.Panels
{
    public class MenuPanelContainer : Sprite
    {
        private static readonly string[] TitleKeys =
        {
            "Screen_Main_SinglePlayer",
            "Screen_Main_Multiplayer",
            "Screen_Main_Competitive",
            "Screen_Main_MapEditor",
            "Screen_Main_DownloadMaps"
        };

        private static readonly string[] SubtitleKeys =
        {
            "Screen_Main_SinglePlayerDescription",
            "Screen_Main_MultiplayerDescription",
            "Screen_Main_CompetitiveDescription",
            "Screen_Main_MapEditorDescription",
            "Screen_Main_DownloadMapsDescription"
        };

        /// <summary>
        /// </summary>
        public List<MenuPanel> Panels { get; }

        /// <summary>
        /// </summary>
        public MenuPanelContainer(MainMenuScreen menuScreen)
        {
            Size = new ScalableVector2(1296, MenuPanel.PanelSize.Y.Value);
            Alpha = 0;
            AutoScaleHeight = true;

            Panels = new List<MenuPanel>
            {
                new MenuPanel(this, UserInterface.MenuBackgroundClear,
                    GetPanelTitle(0), GetPanelSubtitle(0), menuScreen.ExitToSinglePlayer)
                {
                    Background = { X = -450 }
                },
                new MenuPanel(this, UserInterface.BlankBox,
                    GetPanelTitle(1), GetPanelSubtitle(1), menuScreen.ExitToMultiplayer)
                {
                    Background = { Tint = Color.Purple }
                },
                new MenuPanel(this, UserInterface.BlankBox,
                    GetPanelTitle(2), GetPanelSubtitle(2), menuScreen.ExitToCompetitive)
                {
                    Background = { Tint = Color.Green }
                },
                new MenuPanel(this, UserInterface.BlankBox,
                    GetPanelTitle(3), GetPanelSubtitle(3), menuScreen.ExitToEditor)
                {
                    Background = { Tint = Color.Orange }
                },
                new MenuPanel(this, UserInterface.BlankBox,
                    GetPanelTitle(4), GetPanelSubtitle(4), menuScreen.ExitToDownload)
                {
                    Background = { Tint = Color.PaleVioletRed }
                }
            };

            Initialize();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var buttonsHovered = false;

            for (var i = 0; i < Panels.Count; i++)
            {
                if (Panels[i].Button.IsHovered)
                {
                    buttonsHovered = true;
                    break;
                }
            }

            if (buttonsHovered)
            {
                for (var i = 0; i < Panels.Count; i++)
                {
                    if (Panels[i].Button.IsHovered)
                        Panels[i].Expand(gameTime);
                    else
                        Panels[i].Condense(gameTime);
                }
            }
            else
            {
                for (var i = 0; i < Panels.Count; i++)
                    Panels[i].CondenseToOriginalSize(gameTime);
            }

            SetPositions();
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void Initialize()
        {
            for (var i = 0; i < Panels.Count; i++)
            {
                var panel = Panels[i];
                panel.Parent = this;
            }

            SetPositions();
        }

        /// <summary>
        /// </summary>
        private void SetPositions()
        {
            for (var i = 0; i < Panels.Count; i++)
            {
                var panel = Panels[i];

                if (i == 0)
                    continue;

                panel.X = Panels[i - 1].X + Panels[i - 1].Width;
            }
        }

        private static string GetPanelTitle(int index) => LocalizationManager.Get(TitleKeys[index]).ToUpper();

        private static string GetPanelSubtitle(int index) => LocalizationManager.Get(SubtitleKeys[index]).ToUpper();
    }
}
