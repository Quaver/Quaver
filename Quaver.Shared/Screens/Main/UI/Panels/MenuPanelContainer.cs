using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Main.UI.Panels
{
    public class MenuPanelContainer : Sprite
    {
        /// <summary>
        /// </summary>
        public List<MenuPanel> Panels { get; }

        /// <summary>
        /// </summary>
        public MenuPanelContainer(MainMenuScreen menuScreen)
        {
            Panels = new List<MenuPanel>
            {
                new MenuPanel(this, UserInterface.MenuBackgroundClear,
                    "Single Player".ToUpper(), "Compete for leaderboard ranks".ToUpper(), menuScreen.ExitToSinglePlayer)
                {
                    Background = { X = -450 }
                },
                new MenuPanel(this, UserInterface.BlankBox,
                    "Competitive".ToUpper(), "Compete against players all over the world".ToUpper(), menuScreen.ExitToCompetitive)
                {
                    Background = { Tint = Color.LimeGreen }
                },
                new MenuPanel(this, UserInterface.BlankBox,
                    "Multiplayer".ToUpper(), "Play custom matches online with others".ToUpper(), menuScreen.ExitToMultiplayer)
                {
                    Background = { Tint = Color.MediumPurple }
                },
                new MenuPanel(this, UserInterface.BlankBox,
                    "Editor".ToUpper(), "Create or edit a map to any song you'd like".ToUpper(), menuScreen.ExitToEditor)
                {
                    Background = { Tint = Color.Orange }
                }
            };

            Size = new ScalableVector2(1296, MenuPanel.PanelSize.Y.Value);
            Alpha = 0;
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
    }
}