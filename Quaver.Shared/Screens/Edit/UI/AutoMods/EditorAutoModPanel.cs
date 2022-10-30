using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.API.Maps.AutoMod;
using Quaver.API.Maps.AutoMod.Issues;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Edit.UI.AutoMods
{
    public class EditorAutoModPanel : DraggableButton
    {
        public Qua Map { get; }

        public List<Qua> Mapset { get; }

        public EditorAutoModPanelContainer Container { get; }

        public Bindable<AutoModMapset> AutoMod { get; }

        public Bindable<AutoModIssueCategory> FilterCategory { get; }

        public event EventHandler<EditorAutoModIssueClicked> IssueClicked;

        private EditorAutoModHeader Header { get; set; }

        private EditorAutoModDetails Details { get; set; }

        private EditorAutoModFilterDropdown FilterDropdown { get; set; }

        private TextKeyValue IssueCount { get; set; }

        private EditorAutoModScrollPanel ScrollPanel { get; set; }

        private const int SpacingY = 16;

        public EditorAutoModPanel(Qua map, List<Qua> mapset, EditorAutoModPanelContainer container) : base(UserInterface.AutoModPanel)
        {
            Map = map;
            Mapset = mapset;
            Container = container;

            FilterCategory = new Bindable<AutoModIssueCategory>(AutoModIssueCategory.None);
            AutoMod = new Bindable<AutoModMapset>(new AutoModMapset(Mapset));
            AutoMod.Value.Run();

            Size = new ScalableVector2(Image.Width, Image.Height);

            CreateHeader();
            CreateDetails();
            CreateIssueCount();
            CreateFilterDropdown();
            CreateScrollPanel();

            FilterDropdown.Parent = this;

            AutoMod.ValueChanged += OnAutoModUpdated;
        }

        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            AutoMod.ValueChanged -= OnAutoModUpdated;
            AutoMod.Dispose();

            FilterCategory.Dispose();

            IssueClicked = null;

            base.Destroy();
        }

        public void RunAutoMod()
        {
            AutoMod.Value.Run();
            AutoMod.TriggerChange();
        }

        public void TriggerEvent(AutoModIssue issue) => IssueClicked?.Invoke(this, new EditorAutoModIssueClicked(issue));

        private void CreateHeader() => Header = new EditorAutoModHeader(this) {Parent = this};

        private void CreateDetails() => Details = new EditorAutoModDetails(this)
        {
            Parent = this,
            Alignment = Alignment.TopCenter,
            Y = Header.Height + SpacingY
        };

        private void CreateFilterDropdown() => FilterDropdown = new EditorAutoModFilterDropdown(FilterCategory)
        {
            Parent = this,
            Alignment = Alignment.TopRight,
            X = -IssueCount.X,
            Y = IssueCount.Y - 6
        };

        private void CreateIssueCount()
        {
            IssueCount = new TextKeyValue("Issue Count:", "0", 22, Color.White)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                X = 22,
                Y = Details.Y + Details.Height + 20,
                Value = {Tint = ColorHelper.HexToColor($"#E9B736")}
            };

            UpdateIssueCount();
        }

        private void UpdateIssueCount() => ScheduleUpdate(() =>
        {
            var count = AutoMod.Value.Issues.Count + AutoMod.Value.Mods[Map].Issues.Count;
            IssueCount.ChangeValue($"{count:n0}");
        });

        private void CreateScrollPanel() => ScrollPanel = new EditorAutoModScrollPanel(Map, AutoMod,
            FilterCategory, this)
        {
            Parent = this,
            Alignment = Alignment.BotCenter,
            Y = -12
        };

        private void OnAutoModUpdated(object sender, BindableValueChangedEventArgs<AutoModMapset> e)
            => UpdateIssueCount();
    }
}