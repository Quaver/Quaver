using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemSuggestDifficulty : OptionsItem
    {
        /// <summary>
        /// </summary>
        private IconButton Button { get; }

        /// <summary>
        /// </summary>
        /// <param name="containerWidth"></param>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        public OptionsItemSuggestDifficulty(RectangleF containerRect, string name) : base(containerRect, name)
        {
            const float scale = 0.85f;

            Button = new IconButton(UserInterface.OptionsCalibrateOffsetButton)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(215 * scale, 36 * scale),
                UsePreviousSpriteBatchOptions = true
            };

            Button.Clicked += (sender, args) =>
            {
                if (!OnlineManager.Connected || OnlineManager.Self == null)
					return;

                var rating = OnlineManager.Self.Stats[GameMode.Keys4].OverallPerformanceRating;
                var diff = rating / 20f;
				ConfigManager.PrioritizedMapDifficulty4K.Value = (int)(diff * 10);

                rating = OnlineManager.Self.Stats[GameMode.Keys7].OverallPerformanceRating;
                diff = rating / 20f;
                ConfigManager.PrioritizedMapDifficulty7K.Value = (int)(diff * 10);
            };
        }
    }
}
