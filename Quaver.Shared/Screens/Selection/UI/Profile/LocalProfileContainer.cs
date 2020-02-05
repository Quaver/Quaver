using Quaver.Shared.Assets;
using Quaver.Shared.Database.Profiles;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Profile
{
    public class LocalProfileContainer : Sprite
    {
        /// <summary>
        /// </summary>
        public Bindable<UserProfile> Profile { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Header { get; set; }

        /// <summary>
        /// </summary>
        private Sprite BackgroundContainer { get; set; }

        /// <summary>
        /// </summary>
        public LocalProfileContainer(Bindable<UserProfile> profile)
        {
            Profile = profile;
            Size = new ScalableVector2(564, 838);
            Alpha = 0;

            CreateHeaderText();
            CreateContainer();
        }

        /// <summary>
        ///    Creates <see cref="Header"/>
        /// </summary>
        private void CreateHeaderText()
        {
            Header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoHeavy), "USER PROFILE", 30)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
            };
        }

        /// <summary>
        /// </summary>
        private void CreateContainer()
        {
            BackgroundContainer = new Sprite
            {
                Parent = this,
                Image = UserInterface.ModifierSelectorBackground,
                Size = new ScalableVector2(Width, Height - Header.Height - 8),
                Y = Header.Y + Header.Height + 8,
            };
        }
    }
}