using System;
using Microsoft.Xna.Framework;
using Quaver.Database.Beatmaps;
using Quaver.Graphics.Text;
using Quaver.Helpers;

namespace Quaver.Graphics.Buttons
{
    /// <inheritdoc />
    /// <summary>
    ///     This type of button is used for simple buttons that only require a single image + text, but also includes a tint animation.
    /// </summary>
    internal class QuaverSongSelectButton : QuaverButton
    {
        internal bool Selected { get; set; }

        internal Beatmap Map { get; set; }

        private TextBoxSprite TitleText { get; set; }

        private TextBoxSprite ArtistText { get; set; }

        private TextBoxSprite DiffText { get; set; }

        private Sprite.Sprite UnderlayImage { get; set; }

        private Sprite.Sprite GameModeImage { get; set; }

        private Sprite.Sprite GradeImage { get; set; }

        /// <summary>
        ///     Current tween value of the object. Used for animation.
        /// </summary>
        private float HoverCurrentTween { get; set; }

        /// <summary>
        ///     Target tween value of the object. Used for animation.
        /// </summary>
        private float HoverTargetTween { get; set; } = 0.6f;

        /// <summary>
        ///     Current Color/Tint of the object.
        /// </summary>
        private Color CurrentTint = Color.White;

        //Constructor
        internal QuaverSongSelectButton(Beatmap map, float ButtonScale)
        {
            var ButtonSizeY = 40 * ButtonScale;
            var mapText = map.Artist + " - " + map.Title + " [" + map.DifficultyName + "]";

            Size.Y.Offset = ButtonSizeY;
            Size.X.Offset = ButtonSizeY * 8;

            //Load and set BG Image
            /*
            Task.Run(() => {
                try
                {
                    Image = ImageLoader.Load(ConfigManager.SongDirectory + "/" + map.Directory + "/" + map.BackgroundPath);
                }
                catch
                {
                    Exception ex;
                }
            });*/

            TitleText = new TextBoxSprite()
            {
                Text = map.Title,
                Font = Fonts.Medium48,
                Size = new UDim2(-5 * ButtonScale, -2 * ButtonScale, 0.825f, 0.5f),
                Position = new UDim2(-5 * ButtonScale, 2 * ButtonScale),
                Alignment = Alignment.TopRight,
                TextAlignment = Alignment.BotLeft,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.Black,
                Parent = this
            };

            ArtistText = new TextBoxSprite()
            {
                Text = map.Artist + " | "+ map.Creator,
                Font = Fonts.Medium48,
                Position = new UDim2(-5 * ButtonScale, -5 * ButtonScale),
                Size = new UDim2(-5 * ButtonScale, -5 * ButtonScale, 0.825f, 0.5f),
                Alignment = Alignment.BotRight,
                TextAlignment = Alignment.TopLeft,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.Black,
                Parent = this
            };

            DiffText = new TextBoxSprite()
            {
                Text = string.Format("{0:f2}", map.DifficultyRating),
                Font = Fonts.Bold12,
                Position = new UDim2(2 * ButtonScale, 5 * ButtonScale),
                Size = new UDim2(-6 * ButtonScale, -5 * ButtonScale, 0.175f, 0.5f),
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.BotRight,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.Red,
                Parent = this
            };

            /*
            ModeAndGradeBoundaryInner = new Boundary()
            {
                SizeX = 35 * ButtonScale,
                ScaleY = 1,
                Alignment = Alignment.MidCenter,
                Parent = ModeAndGradeBoundaryOutter
            };*/

            UnderlayImage = new Sprite.Sprite
            {
                Position = new UDim2(2 * ButtonScale, -5 * ButtonScale),
                Size = new UDim2(-6 * ButtonScale, -5 * ButtonScale, 0.175f, 0.5f),
                Alignment = Alignment.BotLeft,
                Alpha = 0,
                Parent = this
            };
            
            GradeImage = new Sprite.Sprite()
            {
                Position = new UDim2(-16 * ButtonScale, 0),
                Size = new UDim2(14 * ButtonScale, 14 * ButtonScale),
                Alpha = 1f,
                Image = GameBase.LoadedSkin.GradeSmallA,
                Alignment = Alignment.MidRight,
                Parent = UnderlayImage
            };

            GameModeImage = new Sprite.Sprite()
            {
                Size = new UDim2(14 * ButtonScale, 14 * ButtonScale),
                Image = GameBase.LoadedSkin.Cursor,
                Alpha = 0.5f,
                Alignment = Alignment.MidRight,
                Parent = UnderlayImage
            };
        }

        /// <summary>
        ///     This method is called when the mouse hovers over the button
        /// </summary>
        internal override void MouseOver()
        {
            HoverTargetTween = 0.85f;
        }

        /// <summary>
        ///     This method is called when the Mouse hovers out of the button
        /// </summary>
        internal override void MouseOut()
        {
            HoverTargetTween = 0.6f;
        }

        /// <summary>
        ///     This method will be used for button logic and animation
        /// </summary>
        internal override void Update(double dt)
        {
            if (Selected)
                HoverCurrentTween = GraphicsHelper.Tween(1, HoverCurrentTween, Math.Min(dt / 40, 1));
            else
                HoverCurrentTween = GraphicsHelper.Tween(HoverTargetTween, HoverCurrentTween, Math.Min(dt / 40, 1));

            CurrentTint.R = (byte)(HoverCurrentTween * 255);
            CurrentTint.G = (byte)(HoverCurrentTween * 255);
            CurrentTint.B = (byte)(HoverCurrentTween * 255);

            Tint = CurrentTint;
            GradeImage.Tint = Tint;
            GameModeImage.Tint = Tint;
            
            //TextSprite.Update(dt);
            base.Update(dt);
        }
    }
}
