using Quaver.Server.Client.Handlers;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsDifficultyRange : MultiplayerSettingsText
    {
        public MultiplayerSettingsDifficultyRange(string name, string value) : base(name, value)
        {
            OnlineManager.Client.OnDifficultyRangeChanged += OnGameDifficultyRangeChanged;
        }

        public override void Destroy()
        {
            OnlineManager.Client.OnDifficultyRangeChanged -= OnGameDifficultyRangeChanged;
            base.Destroy();
        }

        private void OnGameDifficultyRangeChanged(object sender, DifficultyRangeChangedEventArgs e)
            => Value.Text = ToString(e.MinimumDifficulty, e.MaximumDifficulty);

        public static string ToString(double min, double max)
            => $"{ToString(min)} - {ToString(max)}";

        private static string ToString(double num)
        {
            if (num > 100)
                return "Any";

            return $"{num:00.00}";
        }
    }
}