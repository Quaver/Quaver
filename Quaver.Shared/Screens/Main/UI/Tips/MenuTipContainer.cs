using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Main.UI.Tips
{
    public class MenuTipContainer : ImageButton
    {
        /// <summary>
        /// </summary>
        private SpriteTextPlus Label { get; set; }

        /// <summary>
        /// </summary>
        private ScrollContainer ScrollingContainer { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus TextTip { get; set; }

        /// <summary>
        /// </summary>
        private Sprite HoverEffect { get; set; }

        /// <summary>\
        /// </summary>
        private string[] TipKeys { get; } =
        {
            "Screen_Main_TipOpenOptions",
            "Screen_Main_TipImportMapFiles",
            "Screen_Main_TipMapPreview",
            "Screen_Main_TipPauseSubmission",
            "Screen_Main_TipReportBug",
            "Screen_Main_TipRequestFeatures",
            "Screen_Main_TipCreateMap",
            "Screen_Main_TipRandomSong",
            "Screen_Main_TipPlayercard",
            "Screen_Main_TipScrollSpeed",
            "Screen_Main_TipCalibrateOffset",
            "Screen_Main_TipDefaultJudgement",
            "Screen_Main_TipPlaybackRate"
        };

        /// <summary>
        /// </summary>
        private Random RNG { get; } = new Random();

        /// <summary>
        /// </summary>
        public MenuTipContainer(): base(UserInterface.BlankBox)
        {
            var tex =  SkinManager.Skin?.MainMenu?.TipPanel ?? UserInterface.MenuTipPanel;

            Image = tex;
            Size = new ScalableVector2(484, 52);

            CreateLabel();
            CreateScrollContainer();
            CreateTextTip();
            CreateHoverEffect();
            PickRandomTip();

            Clicked += (sender, args) => PickRandomTip();
            Hovered += (sender, args) => SkinManager.Skin?.SoundHover.CreateChannel().Play();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Label.Alpha = Alpha;
            TextTip.Alpha = Alpha;

            HoverEffect.Size = new ScalableVector2(Width - 4, Height - 4);
            HoverEffect.Alpha = IsHovered ? 0.35f : 0;

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateLabel() => Label = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold),
            GetTipLabel(), 18)
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            X = 16,
            Tint = SkinManager.Skin?.MainMenu?.TipTitleColor ?? ColorHelper.HexToColor("#45D6F5")
        };

        /// <summary>
        /// </summary>
        private void CreateScrollContainer()
        {
            var size = GetScrollingContainerSize();

            ScrollingContainer = new ScrollContainer(size, size)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = GetScrollingContainerX(),
                Alpha = 0
            };
        }

        /// <summary>
        /// </summary>
        private void CreateTextTip()
        {
            TextTip = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold),
                LocalizationManager.Get(TipKeys[12]), 16)
            {
                Alignment = Alignment.MidLeft,
                Tint = SkinManager.Skin?.MainMenu?.TipTextColor ?? Color.White
            };

            ScrollingContainer.AddContainedDrawable(TextTip);
        }

        private void CreateHoverEffect() => HoverEffect = new Sprite()
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(Width - 4, Height - 4),
            Alpha = 0
        };

        private void PickRandomTip()
        {
            TextTip.Text = LocalizationManager.Get(TipKeys[RNG.Next(0, TipKeys.Length)]);
        }

        private static string GetTipLabel() => LocalizationManager.Get("Screen_Main_TipLabel");

        private float GetScrollingContainerX()
        {
            const int spacing = 8;
            return Label.X + Label.Width + spacing;
        }

        private ScalableVector2 GetScrollingContainerSize()
        {
            const int spacing = 8;
            return new ScalableVector2(Width - Label.Width - Label.X - spacing - Label.X, Height);
        }
    }
}
