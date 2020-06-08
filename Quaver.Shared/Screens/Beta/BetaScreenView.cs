/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Beta
{
    public class BetaScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        /// </summary>
        private Sprite TextBackground { get; }
        
        /// <summary>
        /// </summary>
        private SpriteTextPlus WelcomeText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus DiscordJoinText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus ThanksText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus ThingsWontBePerfectText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus PressToSkipText { get; set; }

        private static Color DisclaimerColor { get; } = ColorHelper.HexToColor("#ffcc00"); 

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public BetaScreenView(Screen screen) : base(screen)
        {
            GameBase.Game.GlobalUserInterface.Cursor.Hide(0);

            CreateBackgroundImage();

            TextBackground = new Sprite
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(WindowManager.Width, 175),
                Alpha = 0.45f,
                Tint = Color.Black
            };

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite()
            {
                Parent = TextBackground,
                Alignment = Alignment.TopCenter,
                Size = new ScalableVector2(TextBackground.Width, 2),
                Tint = DisclaimerColor,
                Alpha = 0.75f,
            };
            
            // ReSharper disable once ObjectCreationAsStatement
            new Sprite()
            {
                Parent = TextBackground,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(TextBackground.Width, 2),
                Tint = DisclaimerColor,
                Alpha = 0.75f,
            };
            
            CreateWarningText();
            CreateBetaText();
            CreateThanksText();
            CreatePressToSkipText();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        /// </summary>
        private void CreateBackgroundImage() => Background = new BackgroundImage(UserInterface.TrianglesWallpaper, 20, false) { Parent = Container };

        /// <summary>
        /// </summary>
        private void CreateWarningText() => WelcomeText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
            "WARNING", 30)
        {
            Parent = TextBackground,
            Alignment = Alignment.TopCenter,
            Y = 34,
            Alpha = 0,
            Tint = DisclaimerColor,
            Animations = { new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 600) }
        };

        /// <summary>
        /// </summary>
        private void CreateBetaText() => DiscordJoinText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
            "This is an early access version of Quaver for Beta testing.".ToUpper(), 20)
        {
            Parent = TextBackground,
            Alignment = Alignment.TopCenter,
            Y = WelcomeText.Y + WelcomeText.Height + 20,
            Alpha = 0,
            Animations = { new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 1200) }
        };

        /// <summary>
        /// </summary>
        private void CreateThanksText() => ThanksText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
            "BE SURE TO REPORT ANY BUGS, AND REQUEST NEW FEATURES ON GITHUB!", 20)
        {
            Parent = TextBackground,
            Alignment = Alignment.TopCenter,
            Y = DiscordJoinText.Y + DiscordJoinText.Height + 20,
            Alpha = 0,
            Animations = { new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 1800) }
        };
        
        /// <summary>
        /// </summary>
        private void CreatePressToSkipText() => PressToSkipText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
            "PRESS ENTER TO SKIP", 20)
        {
            Parent = Container,
            Alignment = Alignment.BotCenter,
            Y = -32,
            Alpha = 0,
            Animations = { new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 800) }
        };
    }
}
