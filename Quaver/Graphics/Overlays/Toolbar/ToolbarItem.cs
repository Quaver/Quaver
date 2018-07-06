using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.Graphics.Overlays.Toolbar
{
    internal class ToolbarItem : Button
    {
        /// <summary>
        ///     The bottom line sprite, used to visually show if it's highlighted.
        /// </summary>
        private Sprite BottomLine { get; set; }

        /// <summary>
        ///     If the toolbar item is actually selected.
        /// </summary>
        internal bool IsSelected { get; set; }

        /// <summary>
        ///     Keeps track of if a sound has played when hovering over the button.
        /// </summary>
        private bool HoverSoundPlayed { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor - Creates a left-aligned one w/ a name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="onClick"></param>
        /// <param name="selected"></param>
        internal ToolbarItem(string name, Action onClick, bool selected = false)
        {
            Size = new UDim2D(165, 45);
            Initialize(onClick, selected);

            // Create the text in the middle of the button.
            new SpriteText
            {
                Parent = this,
                Text = name,
                Alignment = Alignment.MidCenter,
                TextAlignment = Alignment.MidCenter,
                Font = Fonts.AssistantRegular16,
                TextScale = 0.75f
            };
        }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor - Creates toolbar item with an icon. 
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="onClick"></param>
        /// <param name="selected"></param>
        internal ToolbarItem(Texture2D icon, Action onClick, bool selected = false)
        {
            Size = new UDim2D(75, 45);
            Initialize(onClick, selected);

            new Sprite
            {
                Parent = this,
                Image = icon,
                Size = new UDim2D(20, 20),
                Alignment = Alignment.MidCenter
            };
        }
        
        /// <summary>
        ///     Initializes the toolbar item, used in the constructors.
        /// </summary>
        /// <param name="onClick"></param>
        /// <param name="selected"></param>
        private void Initialize(Action onClick, bool selected = false)
        {
            Clicked += (o, e) => onClick();
            IsSelected = selected;
            
            Tint = Color.Black;
            Alpha = IsSelected ? 0.1f : 0;
            
            BottomLine = new Sprite
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Size = new UDim2D(SizeX, 1),
                Tint = Color.White,
                SizeX = IsSelected ? SizeX : 0,
            };
        }
                
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            // Change the size of the line based on if it's hovered/already selected.
            if (IsHovered)
                BottomLine.SizeX = GraphicsHelper.Lerp(SizeX, BottomLine.SizeX, Math.Min(dt / 60f, 1));
            else if (!IsSelected)
                BottomLine.SizeX = GraphicsHelper.Lerp(0, BottomLine.SizeX, Math.Min(dt / 60f, 1));
            
            base.Update(dt);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void MouseOut()
        {
            Alpha = IsSelected ? 0.1f : 0;
            HoverSoundPlayed = false;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void MouseOver()
        {
            Alpha = 0.05f;

            // Make sure the hover sound only plays one time.
            if (HoverSoundPlayed) 
                return;
            
            GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundHover);
            HoverSoundPlayed = true;
        }
    }
}