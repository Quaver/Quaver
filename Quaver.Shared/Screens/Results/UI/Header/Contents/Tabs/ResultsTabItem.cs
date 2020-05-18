using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Header.Contents.Tabs
{
    public class ResultsTabItem : SpriteTextPlus
    {
        /// <summary>
        /// </summary>
        private Bindable<ResultsScreenTabType> ActiveTab { get; }

        /// <summary>
        /// </summary>
        private ResultsScreenTabType Type { get; }

        /// <summary>
        /// </summary>
        private float SelectorHeight { get; }

        /// <summary>
        /// </summary>
        public ImageButton Button { get; private set; }

        /// <summary>
        ///     The color when <see cref="ActiveTab"/> is <see cref="Type"/>
        /// </summary>
        private static Color ActiveColor { get; } = ColorHelper.HexToColor("#00D1FF");

        /// <summary>
        ///     The color when the item is not hovered and isn't active
        /// </summary>
        private static Color InactiveColor { get; } = ColorHelper.HexToColor("#CACACA");

        /// <summary>
        ///     The color when the tab is disabled
        /// </summary>
        private static Color DisabledColor { get; } = ColorHelper.HexToColor("#5B5B5B");

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="activeTab"></param>
        /// <param name="type"></param>
        /// <param name="selectorHeight"></param>
        public ResultsTabItem(Bindable<ResultsScreenTabType> activeTab, ResultsScreenTabType type, float selectorHeight)
            : base(FontManager.GetWobbleFont(Fonts.LatoBlack), type.ToString(), 22)
        {
            ActiveTab = activeTab;
            Type = type;
            SelectorHeight = selectorHeight;

            CreateButton();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            SetTint();
            base.Update(gameTime);
        }

        public void SetTint()
        {
            if (!Button.IsClickable)
                Tint = DisabledColor;
            else if (ActiveTab.Value == Type)
                Tint = ActiveColor;
            else if (Button.IsHovered)
                Tint = Colors.MainAccent;
            else
                Tint = InactiveColor;
        }

        /// <summary>
        /// </summary>
        private void CreateButton()
        {
            Button = new ImageButton(UserInterface.BlankBox, (sender, args) =>
            {
                if (ActiveTab.Value != Type)
                    ActiveTab.Value = Type;
            })
            {
                Parent = this,
                Size = new ScalableVector2(Width, SelectorHeight),
                Y = -Height,
                Alpha = 0
            };

            SetTint();
        }
    }
}