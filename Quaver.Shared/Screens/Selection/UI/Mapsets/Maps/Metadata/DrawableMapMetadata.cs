using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Wobble.Graphics.Animations;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets.Maps.Metadata
{
    public class DrawableMapMetadata : IconTextButton, IDrawableMapMetadata
    {
        /// <summary>
        /// </summary>
        protected Map Map { get; }

        /// <summary>
        /// </summary>
        private bool IsOpening { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="icon"></param>
        public DrawableMapMetadata(Map map, Texture2D icon) : base(icon, FontManager.GetWobbleFont(Fonts.LatoBlack), "00:00")
        {
            Map = map;
            IsClickable = false;

            Icon.Alpha = 0;

            Text.Visible = false;
            Icon.Visible = false;
            SetTextTint = false;

            Text.SetChildrenAlpha = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (Text.Animations.Count == 0)
                Text.Alpha = MathHelper.Lerp(Text.Alpha, IsOpening ? 1 : 0, (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 60, 1));

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Open()
        {
            IsOpening = true;

            Icon.ClearAnimations();
            Icon.Visible = true;
            Icon.Wait(200);
            Icon.FadeTo(1, Easing.Linear, 250);

            Text.Visible = true;
            Text.Wait(200);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Close()
        {
            IsOpening = false;

            // Make difficulty text invisible
            ClearAnimations();

            Text.Visible = false;
            Text.ClearAnimations();
            Text.Alpha = 0;

            Icon.ClearAnimations();
            Icon.Visible = false;
            Icon.Alpha = 0;
        }
    }
}