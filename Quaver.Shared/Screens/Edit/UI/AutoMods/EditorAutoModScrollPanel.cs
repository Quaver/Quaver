using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Maps;
using Quaver.API.Maps.AutoMod;
using Quaver.API.Maps.AutoMod.Issues;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Edit.UI.AutoMods
{
    public sealed class EditorAutoModScrollPanel : PoolableScrollContainer<AutoModIssue>
    {
        private Qua Map { get; }

        private Bindable<AutoModMapset> AutoMod { get; }

        private Bindable<AutoModIssueCategory> FilterCategory { get; }

        public EditorAutoModPanel Panel { get; }

        private static ScalableVector2 DefaultSize { get; } = new ScalableVector2(720, 600);

        public EditorAutoModScrollPanel(Qua map, Bindable<AutoModMapset> autoMod,
            Bindable<AutoModIssueCategory> category, EditorAutoModPanel panel)
            : base(new List<AutoModIssue>(), 16, 0, DefaultSize, DefaultSize)
        {
            Map = map;
            AutoMod = autoMod;
            FilterCategory = category;
            Panel = panel;

            Tint = ColorHelper.HexToColor("#242424");
            AddBorder(ColorHelper.HexToColor("#BEBEBE"), 2);
            Border.Alpha = 0.50f;

            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 320;
            InputEnabled = true;

            Scrollbar.Tint = ColorHelper.HexToColor("#656565");
            Scrollbar.Width = 4;
            Scrollbar.X = 8;

            FilterIssues();

            CreatePool();
            RecalculateContainerHeight();

            AutoMod.ValueChanged += OnAutoModUpdated;
            FilterCategory.ValueChanged += OnFilterCategoryChanged;
        }

        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && DialogManager.Dialogs.Count == 0
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt);

            base.Update(gameTime);
        }

        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            AutoMod.ValueChanged -= OnAutoModUpdated;

            base.Destroy();
        }

        protected override PoolableSprite<AutoModIssue> CreateObject(AutoModIssue item, int index)
            => new EditorAutoModIssue(this, item, index);

        private void FilterIssues()
        {
            AvailableItems = new List<AutoModIssue>();

            foreach (var issue in AutoMod.Value.Issues)
            {
                if (FilterCategory.Value == AutoModIssueCategory.None || issue.Category == FilterCategory.Value)
                    AvailableItems.Add(issue);
            }

            foreach (var issue in AutoMod.Value.Mods[Map].Issues)
            {
                if (FilterCategory.Value == AutoModIssueCategory.None || issue.Category == FilterCategory.Value)
                    AvailableItems.Add(issue);
            }
        }

        private void Refresh() => ScheduleUpdate(() =>
        {
            Pool.ForEach(x => x.Destroy());
            Pool = new List<PoolableSprite<AutoModIssue>>();

            PoolStartingIndex = 0;
            ContentContainer.Y = 0;
            PreviousContentContainerY = ContentContainer.Y;
            TargetY = PreviousContentContainerY;
            PreviousTargetY = PreviousContentContainerY;

            FilterIssues();

            CreatePool();
            RecalculateContainerHeight();
        });

        private void OnAutoModUpdated(object sender, BindableValueChangedEventArgs<AutoModMapset> e)
            => Refresh();

        private void OnFilterCategoryChanged(object sender, BindableValueChangedEventArgs<AutoModIssueCategory> e)
            => Refresh();
    }
}