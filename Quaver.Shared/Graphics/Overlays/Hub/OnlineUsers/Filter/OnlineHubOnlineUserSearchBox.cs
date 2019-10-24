using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers.Filter
{
    public class OnlineHubOnlineUserSearchBox : Textbox
    {
        /// <summary>
        /// </summary>
        private Sprite MagnifyingGlass { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="bindable"></param>
        /// <param name="size"></param>
        public OnlineHubOnlineUserSearchBox(Bindable<string> bindable, ScalableVector2 size) : base(size, FontManager.GetWobbleFont(Fonts.LatoBlack),
            20, "", "Search for users...")
        {
            Tint = ColorHelper.HexToColor("#131313");
            Image = UserInterface.DropdownClosed;

            CreateMagnifyingGlass();

            StoppedTypingActionCalltime = 250;
            OnStoppedTyping += s => bindable.Value = s;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleMagnifyingGlassAnimations(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateMagnifyingGlass()
        {
            MagnifyingGlass = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -12,
                Size = new ScalableVector2(Height * 0.50f, Height * 0.50f),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_magnifying_glass)
            };
        }

        /// <summary>
        ///     Animates the search icon when the text gets too long
        /// </summary>
        private void HandleMagnifyingGlassAnimations(GameTime gameTime)
        {
            var target = InputText.Width < Width - 20 - MagnifyingGlass.Width ? 1 : 0;

            MagnifyingGlass.Alpha = MathHelper.Lerp(MagnifyingGlass.Alpha, target,
                (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 120, 1));
        }
    }
}