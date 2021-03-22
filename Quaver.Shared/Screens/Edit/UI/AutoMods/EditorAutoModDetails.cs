using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.AutoMod;
using Quaver.API.Maps.AutoMod.Issues;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Edit.UI.AutoMods
{
    public class EditorAutoModDetails : Sprite
    {
        private EditorAutoModPanel Panel { get; }

        private TextKeyValue Difficulty { get; set; }

        private TextKeyValue Mode { get; set; }

        private TextKeyValue Status { get; set; }

        private IconButton RefreshMapButton { get; set; }

        private AutoModMapset AutoMod => Panel.AutoMod.Value;

        private AutoMod AutoModMap => Panel.AutoMod.Value.Mods[Panel.Map];

        private const int FontSize = 22;

        private const int PaddingX = 20;

        private const int SpacingY = 10;

        public EditorAutoModDetails(EditorAutoModPanel panel)
        {
            Panel = panel;

            Tint = ColorHelper.HexToColor("#242424");
            AddBorder(ColorHelper.HexToColor("#BEBEBE"), 2);
            Border.Alpha = 0.50f;

            Size = new ScalableVector2(720, 105);

            CreateTextDifficulty();
            CreateTextMode();
            CreateTextStatus();
            CreateRefreshMapButton();

            Panel.AutoMod.ValueChanged += OnAutoModUpdated;
        }

        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            Panel.AutoMod.ValueChanged -= OnAutoModUpdated;

            base.Destroy();
        }

        private void CreateTextDifficulty() => Difficulty = new TextKeyValue("Difficulty:",
            Panel.Map.DifficultyName, FontSize, ColorHelper.HexToColor("#5EC4FF"))
        {
            Parent = this,
            X = PaddingX,
            Y = 10
        };

        private void CreateTextMode()
        {
            Mode = new TextKeyValue("Mode:", ModeHelper.ToShortHand(Panel.Map.Mode), FontSize,
                Color.White)
            {
                Parent = this,
                X = PaddingX,
                Y = Difficulty.Y + Difficulty.Height + SpacingY,
            };

            switch (Panel.Map.Mode)
            {
                case GameMode.Keys4:
                    Mode.Value.Tint = ColorHelper.HexToColor("#0587E5");
                    break;
                case GameMode.Keys7:
                    Mode.Value.Tint = ColorHelper.HexToColor("#9B51E0");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CreateTextStatus()
        {
            Status = new TextKeyValue("Status:", "None", FontSize, Color.White)
            {
                Parent = this,
                X = PaddingX,
                Y = Mode.Y + Mode.Height + SpacingY
            };

            UpdateStatus();
        }

        private void CreateRefreshMapButton()
        {
            var img = UserInterface.AutoModRefreshButton;

            RefreshMapButton = new IconButton(img)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -PaddingX,
                Size = new ScalableVector2(img.Width, img.Height),
                Depth = -1
            };

            RefreshMapButton.Clicked += (sender, args) => Panel.RunAutoMod();
        }

        private void UpdateStatus()
        {
            var mapsetIssues = AutoMod.Issues.Any(x => x.Level > AutoModIssueLevel.Warning);
            var mapIssues = AutoModMap.Issues.Any(x => x.Level > AutoModIssueLevel.Warning);

            if (mapsetIssues || mapIssues)
            {
                Status.Value.Text = "Not ready for rank!";
                Status.Value.Tint = ColorHelper.HexToColor("#F9645D");
                return;
            }

            Status.Value.Text = $"Ready for rank!";
            Status.Value.Tint = ColorHelper.HexToColor("#5EFF75");
        }

        private void OnAutoModUpdated(object sender, BindableValueChangedEventArgs<AutoModMapset> e)
            => UpdateStatus();
    }
}