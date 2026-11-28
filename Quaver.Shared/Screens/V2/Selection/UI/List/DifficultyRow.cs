using System;
using System.Globalization;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Selection.UI.List
{
    /// <summary>
    ///     Displays one difficulty row with its gameplay statistics
    /// </summary>
    internal sealed class DifficultyRow : SelectionListRow
    {
        protected override bool ChangesWidthWhenSelected => false;
        protected override float RightInset => 10;

        private FlexContainer Layout { get; }
        private FlexContainer MainLayout { get; }
        private FlexContainer BadgeLayout { get; }
        private FlexContainer StatsLayout { get; }
        private FlexContainer LongNoteStatLayout { get; }
        private FlexContainer NotesPerSecondStatLayout { get; }
        private FlexContainer DurationStatLayout { get; }
        private FlexContainer BpmStatLayout { get; }
        private Sprite GradeSprite { get; }
        private RoundedButton DifficultyPill { get; }
        private MarqueeSpriteText Title { get; }
        private Sprite LongNoteIcon { get; }
        private Sprite NotesPerSecondIcon { get; }
        private Sprite DurationIcon { get; }
        private Sprite BpmIcon { get; }
        private SpriteTextPlus LongNoteValue { get; }
        private SpriteTextPlus NotesPerSecondValue { get; }
        private SpriteTextPlus DurationValue { get; }
        private SpriteTextPlus BpmValue { get; }

        internal DifficultyRow(MapsetList list, SelectionListEntry item, int index) : base(list, item, index, list.DifficultyRowConfig)
        {
            Layout = new FlexContainer
            {
                Parent = this,
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center,
                JustifyContent = FlexJustifyContent.SpaceBetween,
                ColumnGap = Config.DifficultyStatsGap
            };

            MainLayout = new FlexContainer
            {
                Parent = Layout,
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center,
                ColumnGap = Config.DifficultyContentGap
            };

            BadgeLayout = new FlexContainer
            {
                Parent = MainLayout,
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center,
                ColumnGap = Config.DifficultyGradeGap
            };

            StatsLayout = new FlexContainer
            {
                Parent = Layout,
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center,
                ColumnGap = Config.DifficultyStatGroupGap
            };

            LongNoteStatLayout = CreateStatLayout();
            NotesPerSecondStatLayout = CreateStatLayout();
            DurationStatLayout = CreateStatLayout();
            BpmStatLayout = CreateStatLayout();

            DurationStatLayout.Parent = this;
            BpmStatLayout.Parent = this;
            DurationStatLayout.Visible = BpmStatLayout.Visible = false;

            GradeSprite = new Sprite { Parent = this, Visible = false, UsePreviousSpriteBatchOptions = true };

            DifficultyPill = new RoundedButton
            {
                Parent = BadgeLayout,
                WidthMode = ButtonSizeMode.Fixed,
                Height = Config.DifficultyPillHeight,
                CornerRadius = Config.DifficultyPillHeight / 2f,
                Tint = SkinV2Color.Parse(Config.DifficultyPillColor),
                IsClickable = false,
                IsInteractionEnabled = false,
                PerformHoverFade = false,
                UsePreviousSpriteBatchOptions = true
            };

            DifficultyPill.SetIcon(CapsuleIcons.Get(CapsuleIcon.AreaChart), CapsuleIcons.GetDisplaySize(CapsuleIcon.AreaChart, CapsuleIcons.IconHeight));
            DifficultyPill.Icon.Tint = SkinV2Color.Parse(Config.DifficultyStatColor);

            Title = CreateText(Config.TitleFont, Config.TitleFontSize, Config.TextColor);
            Title.Parent = MainLayout;

            LongNoteIcon = CreateStatIcon(LongNoteStatLayout, UserInterface.SelectionLongNotePercentage);
            LongNoteValue = CreateStatValue(LongNoteStatLayout);

            NotesPerSecondIcon = CreateStatIcon(NotesPerSecondStatLayout, UserInterface.SelectionNotesPerSecond);
            NotesPerSecondValue = CreateStatValue(NotesPerSecondStatLayout);

            DurationIcon = CreateStatIcon(DurationStatLayout, CapsuleIcons.Get(CapsuleIcon.Time));
            DurationValue = CreateStatValue(DurationStatLayout);

            BpmIcon = CreateStatIcon(BpmStatLayout, UserInterface.SelectionBpm);
            BpmValue = CreateStatValue(BpmStatLayout);

            Layout.SetItemOptions(MainLayout, new FlexItemOptions { Basis = 0, Grow = 1, Shrink = 1 });
            Layout.SetItemOptions(StatsLayout, new FlexItemOptions { Shrink = 0 });

            MainLayout.SetItemOptions(BadgeLayout, new FlexItemOptions { Shrink = 0 });
            MainLayout.SetItemOptions(Title, new FlexItemOptions { Basis = 0, Grow = 1, Shrink = 1 });

            BadgeLayout.SetItemOptions(DifficultyPill, new FlexItemOptions { Shrink = 0 });

            StatsLayout.SetItemOptions(LongNoteStatLayout, new FlexItemOptions { Shrink = 0 });
            StatsLayout.SetItemOptions(NotesPerSecondStatLayout, new FlexItemOptions { Shrink = 0 });
        }

        protected override void UpdateRowContent(bool sameItem)
        {
            Title.TextSprite.Font = FontManager.GetWobbleFont(Config.TitleFont);
            Title.TextSprite.FontSize = Config.TitleFontSize;

            UpdateDifficultyContent();
        }

        protected override void UpdateRowLayout(float rowWidth)
        {
            var statColor = SkinV2Color.Parse(Config.DifficultyStatColor);
            var iconSize = Config.DifficultyStatIconSize;
            LongNoteIcon.Size = NotesPerSecondIcon.Size = DurationIcon.Size = BpmIcon.Size = new ScalableVector2(iconSize, iconSize);
            LongNoteIcon.Tint = NotesPerSecondIcon.Tint = DurationIcon.Tint = BpmIcon.Tint = statColor;

            LongNoteValue.FontSize = NotesPerSecondValue.FontSize = DurationValue.FontSize = BpmValue.FontSize = Config.DifficultyStatFontSize;
            LongNoteValue.Tint = NotesPerSecondValue.Tint = DurationValue.Tint = BpmValue.Tint = statColor;

            LongNoteStatLayout.ColumnGap = NotesPerSecondStatLayout.ColumnGap = DurationStatLayout.ColumnGap = BpmStatLayout.ColumnGap = Config.DifficultyStatItemGap;

            ConfigureStatColumn(LongNoteStatLayout, List.DifficultyLongNoteStatWidth, 0);
            ConfigureStatColumn(NotesPerSecondStatLayout, List.DifficultyNotesPerSecondStatWidth, 0);

            if (List.ShowDifficultyTimingStats)
            {
                ConfigureStatColumn(DurationStatLayout, List.DifficultyDurationStatWidth, -2);
                ConfigureStatColumn(BpmStatLayout, List.DifficultyBpmStatWidth, -1);
            }

            StatsLayout.ColumnGap = Config.DifficultyStatGroupGap;

            var statsWidth = 0f;
            var statCount = 0;
            foreach (var stat in StatsLayout.Children)
            {
                if (stat is not FlexContainer)
                    continue;
                statsWidth += stat.Width;
                statCount++;
            }
            statsWidth += Config.DifficultyStatGroupGap * System.Math.Max(0, statCount - 1);

            StatsLayout.Size = new ScalableVector2(statsWidth, Height);

            BadgeLayout.ColumnGap = Config.DifficultyGradeGap;
            BadgeLayout.Size = new ScalableVector2(
                DifficultyPill.Width + (GradeSprite.Visible ? Config.DifficultyGradeSize + Config.DifficultyGradeGap : 0), 
                Height);

            MainLayout.ColumnGap = Config.DifficultyContentGap;
            MainLayout.Size = new ScalableVector2(
                Math.Max(1,rowWidth - Config.DifficultyLeftPadding - Config.DifficultyRightPadding - Config.DifficultyStatsGap - StatsLayout.Width), 
                Height);

            Layout.ColumnGap = Config.DifficultyStatsGap;
            Layout.Position = new ScalableVector2(Config.DifficultyLeftPadding, 0);
            Layout.Size = new ScalableVector2(
                Math.Max(1, rowWidth - Config.DifficultyLeftPadding - Config.DifficultyRightPadding), 
                Height);

            foreach (var stat in StatsLayout.Children)
            {
                if (stat is FlexContainer layout)
                    layout.RefreshLayout();
            }
            StatsLayout.RefreshLayout();
            BadgeLayout.RefreshLayout();
            MainLayout.RefreshLayout();
            Layout.RefreshLayout();
        }

        internal override void RefreshDynamicContent()
        {
            UpdateDifficultyContent();
            UpdateRowLayout(Width);
        }

        private void UpdateDifficultyContent()
        {
            var gradeTexture = GetGradeTexture(Item.Difficulty.OnlineGrade);
            if (gradeTexture != null)
            {
                GradeSprite.Parent = BadgeLayout;
                GradeSprite.Image = gradeTexture;
                GradeSprite.Size = new ScalableVector2(Config.DifficultyGradeSize, Config.DifficultyGradeSize);
                GradeSprite.Alpha = 1;
                GradeSprite.Visible = true;
                BadgeLayout.SetItemOptions(GradeSprite, new FlexItemOptions { Order = -1, Shrink = 0 });
            }
            else
            {
                GradeSprite.Visible = false;
                GradeSprite.Parent = this;
            }

            var color = ColorHelper.DifficultyToColor((float) Item.Difficulty.DifficultyFromMods(ModManager.Mods));
            Title.TextSprite.Tint = color;
            if (Title.TextSprite.Text != (Item.Difficulty.DifficultyName ?? ""))
            {
                Title.TextSprite.Text = Item.Difficulty.DifficultyName ?? "";
                Title.ResetPosition();
            }

            DifficultyPill.SetLabel(FontManager.GetWobbleFont(Config.TitleFont), Item.Difficulty.DifficultyFromMods(ModManager.Mods).ToString("0.00", CultureInfo.InvariantCulture), Config.DifficultyPillFontSize, color);
            DifficultyPill.Width = Math.Max(Config.DifficultyPillMinimumWidth, DifficultyPill.Icon.Width + 8 + DifficultyPill.Label.Width + Config.DifficultyPillHorizontalPadding);

            LongNoteValue.Text = GetLongNoteStatText(Item.Difficulty);
            NotesPerSecondValue.Text = GetNotesPerSecondStatText(Item.Difficulty);

            if (List.ShowDifficultyTimingStats)
            {
                DurationStatLayout.Parent = BpmStatLayout.Parent = StatsLayout;
                DurationStatLayout.Visible = BpmStatLayout.Visible = true;
                StatsLayout.SetItemOptions(DurationStatLayout, new FlexItemOptions { Order = -2, Shrink = 0 });
                StatsLayout.SetItemOptions(BpmStatLayout, new FlexItemOptions { Order = -1, Shrink = 0 });

                DurationValue.Text = GetDurationStatText(Item.Difficulty);
                BpmValue.Text = GetBpmStatText(Item.Difficulty);
            }
            else
            {
                DurationStatLayout.Visible = BpmStatLayout.Visible = false;
                DurationStatLayout.Parent = BpmStatLayout.Parent = this;
            }
        }

        private void ConfigureStatColumn(FlexContainer layout, float width, int order)
        {
            layout.Size = new ScalableVector2(width, Height);
            StatsLayout.SetItemOptions(layout, new FlexItemOptions { Basis = width, Order = order, Shrink = 0 });
        }

        private FlexContainer CreateStatLayout() => new()
        {
            Parent = StatsLayout,
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            ColumnGap = Config.DifficultyStatItemGap
        };

        private Sprite CreateStatIcon(FlexContainer parent, TextureRegion image)
        {
            var icon = new Sprite
            {
                Parent = parent,
                Region = image,
                Size = new ScalableVector2(Config.DifficultyStatIconSize, Config.DifficultyStatIconSize),
                Tint = SkinV2Color.Parse(Config.DifficultyStatColor),
                UsePreviousSpriteBatchOptions = true
            };
            parent.SetItemOptions(icon, new FlexItemOptions { Basis = icon.Width, Shrink = 0 });
            return icon;
        }

        private SpriteTextPlus CreateStatValue(FlexContainer parent)
        {
            var value = new SpriteTextPlus(FontManager.GetWobbleFont(Config.Font), "", Config.DifficultyStatFontSize)
            {
                Parent = parent,
                Tint = SkinV2Color.Parse(Config.DifficultyStatColor),
                UsePreviousSpriteBatchOptions = true
            };
            parent.SetItemOptions(value, new FlexItemOptions { Shrink = 0 });
            return value;
        }

        internal static string GetLongNoteStatText(Map map) => $"{map.LNPercentage.ToString("0.#", CultureInfo.InvariantCulture)}%";

        internal static string GetNotesPerSecondStatText(Map map) => $"{map.NotesPerSecond.ToString("0", CultureInfo.InvariantCulture)} NPS";

        internal static string GetDurationStatText(Map map)
        {
            var rate = Math.Max(0.01f, ModHelper.GetRateFromMods(ModManager.Mods));
            var duration = TimeSpan.FromMilliseconds(map.SongLength / rate);
            return duration.TotalHours >= 1
                ? $"{(int) duration.TotalHours}:{duration.Minutes:00}:{duration.Seconds:00}"
                : $"{duration.Minutes}:{duration.Seconds:00}";
        }

        internal static string GetBpmStatText(Map map) =>
            (map.Bpm * ModHelper.GetRateFromMods(ModManager.Mods)).ToString("0", CultureInfo.InvariantCulture);

        internal static Texture2D GetGradeTexture(Grade grade) => grade switch
        {
            Grade.X => UserInterface.SelectionGradeX,
            Grade.SS => UserInterface.SelectionGradeSs,
            Grade.S => UserInterface.SelectionGradeS,
            Grade.A => UserInterface.SelectionGradeA,
            Grade.B => UserInterface.SelectionGradeB,
            Grade.C => UserInterface.SelectionGradeC,
            Grade.D => UserInterface.SelectionGradeD,
            Grade.F => UserInterface.SelectionGradeF,
            _ => null
        };
    }
}
