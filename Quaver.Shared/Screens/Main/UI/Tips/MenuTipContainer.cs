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
        private string[] Tips { get; } =
        {
            "Pressing CTRL+O will open the options menu.",
            "Drag map files (.qp) into the game to import them.",
            "Pressing F3 in song select displays a map preview.",
            "Pausing in gameplay prevents score submission.",
            "You can report a bug by opening a GitHub issue.",
            "Request new features by opening GitHub issues!",
            "Drag a .mp3 file in the game to create a new map!",
            "Pressing F2 in song select chooses a random song.",
            "Pressing F10 in the menu opens your playercard.",
            "Pressing F3/F4 in gameplay changes scroll speed.",
            "Your offset can be calibrated in the options menu.",
            "Standard* is the default judgement window preset.",
            "CTRL+ +/- changes the playback rate of the song."
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
        private void CreateLabel() => Label = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "TIP:", 20)
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
            const int spacing = 8;

            var size = new ScalableVector2(Width - Label.Width - Label.X - spacing - Label.X, Height);

            ScrollingContainer = new ScrollContainer(size, size)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Label.X + Label.Width + spacing,
                Alpha = 0
            };
        }

        /// <summary>
        /// </summary>
        private void CreateTextTip()
        {
            TextTip = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), Tips[12], Label.FontSize)
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

        private void PickRandomTip() => TextTip.Text = Tips[RNG.Next(0, Tips.Length)];
    }
}