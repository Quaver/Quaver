using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons.Selection;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Sprites;
using Quaver.Main;
using Quaver.Skinning;

namespace Quaver.States.Tests
{
    internal class DropdownTestState : IGameState
    {
        /// <summary>
        ///     Test Screen
        /// </summary>
        public State CurrentState { get; set; } = State.Test;
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UpdateReady { get; set; }

        private Container Container { get; set; }

        /// <summary>
        ///     Navbar sprite
        /// </summary>
        private Nav Nav { get; set; }

        public void Initialize()
        {
            Container = new Container();
            Nav = Nav.CreateGlobalNavbar();
            Nav.Initialize(this);

            // Get all skins.
            var skins = Directory.GetDirectories(ConfigManager.SkinDirectory.Value).ToList();
            for (var i = 0; i < skins.Count; i++)
                skins[i] = new DirectoryInfo(skins[i]).Name;
            
            skins.Insert(0, "None");

            var defaultSkinSelect = new Dropdown(new List<string>() {"Default Arrow Skin", "Default Bar Skin"},
                OnDefaultSkinDropdownButtonClicked)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosY = -200
            };
            
            var skinSelect = new Dropdown(skins, OnSkinDropdownButtonClicked)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };
            
            UpdateReady = true;
        }

        public void UnloadContent()
        {
        }

        public void Update(double dt)
        {
            Nav.Update(dt);
            Container.Update(dt);
        }

        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.DarkOliveGreen);
            GameBase.SpriteBatch.Begin();
            
            Nav.Draw();
            Container.Draw();
            
            GameBase.SpriteBatch.End();
        }

        private void OnDefaultSkinDropdownButtonClicked(object sender, DropdownButtonClickedEventArgs e)
        {
            switch (e.ButtonText)
            {
                case "Default Arrow Skin":
                    ConfigManager.DefaultSkin.Value = DefaultSkins.Arrow;
                    break;
                case "Default Bar Skin":
                    ConfigManager.DefaultSkin.Value = DefaultSkins.Bar;
                    break;
                default:
                    break;
            }
            
            GameBase.Skin = new SkinStore();
        }
        
        /// <summary>
        ///     
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSkinDropdownButtonClicked(object sender, DropdownButtonClickedEventArgs e)
        {
            ConfigManager.Skin.Value = e.ButtonText == "None" ? "" : e.ButtonText;
            GameBase.Skin = new SkinStore();
        }
    }
}