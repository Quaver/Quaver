using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Rating;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Selection.UI.Leaderboard;
using Quaver.Shared.Screens.V2.Selection.UI.List;
using Quaver.Shared.Skinning.V2;
using Steamworks;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Tooltips;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Selection.UI.Panel.Leaderboard
{
    /// <summary>
    ///     Displays a row from the leaderboard score list
    /// </summary>
    internal sealed class LeaderboardRow : PoolableSprite<Score>
    {
        private LeaderboardList List { get; }
        private LeaderboardRowsView View => List.RowsView;
        private SkinV2SelectionLeaderboardConfig Config { get; }
        private Sprite Background { get; }
        private SharedTextAtlasSprite Rank { get; }
        private Sprite Grade { get; }
        private Sprite Avatar { get; }
        private Sprite Flag { get; }
        private SharedTextAtlasSprite Clan { get; }
        private SharedTextAtlasSprite Username { get; }
        private Sprite Clock { get; }
        private SharedTextAtlasSprite Time { get; }
        private SharedTextAtlasSprite PerformanceRating { get; }
        private SharedTextAtlasSprite ComboAccuracy { get; }
        private Sprite BeatabilityBar { get; }
        private TooltipOptions BeatabilityTooltip { get; }
        private IDisposable BeatabilityTooltipRegistration { get; }
        private List<Sprite> ModifierIcons { get; } = new();

        public override int HEIGHT => (int) Math.Round(Config.RowHeight + Config.Gap);

        /// <summary>
        ///     The pooled wrapper only controls recycling; all visuals draw through shared layers.
        /// </summary>
        public override void DrawToSpriteBatch()
        {
        }

        internal LeaderboardRow(LeaderboardList list, Score item, int index,
            SkinV2SelectionLeaderboardConfig config) : base(list, item, index)
        {
            List = list;
            Config = config;
            Size = new ScalableVector2(Math.Max(1, list.ContentContainer.Width), Config.RowHeight);
            Tint = Color.Transparent;

            Background = new Sprite
            {
                Parent = View.BackgroundLayer,
                Size = Size,
                Image = RoundedRectTextureCache.Get(Width, Height, Config.CornerRadius),
                Tint = SkinV2Color.Parse(Config.ComponentColor),
                UsePreviousSpriteBatchOptions = true
            };

            Rank = CreateText(LeaderboardTextField.Rank, SkinV2Color.Parse(Config.TextColor));

            Grade = new Sprite
            {
                Parent = View.GradeLayer,
                Size = new ScalableVector2(Config.GradeSize, Config.GradeSize),
                UsePreviousSpriteBatchOptions = true
            };

            Avatar = new Sprite
            {
                Parent = View.AvatarLayer,
                Size = new ScalableVector2(Config.AvatarSize, Config.AvatarSize),
                UsePreviousSpriteBatchOptions = true
            };

            Flag = new Sprite
            {
                Parent = View.FlagLayer,
                Size = new ScalableVector2(Config.FlagSize, Config.FlagSize),
                Region = Flags.GetRegion("XX"),
                UsePreviousSpriteBatchOptions = true
            };

            Clan = CreateText(LeaderboardTextField.Clan, SkinV2Color.Parse(Config.TextColor));
            Username = CreateText(LeaderboardTextField.Username, SkinV2Color.Parse(Config.TextColor));

            Clock = new Sprite
            {
                Parent = View.ClockLayer,
                Size = new ScalableVector2(Config.ModifierIconHeight, Config.ModifierIconHeight),
                Region = CapsuleIcons.Get(CapsuleIcon.Time),
                Tint = SkinV2Color.Parse(Config.MetadataColor),
                UsePreviousSpriteBatchOptions = true
            };

            Time = CreateText(LeaderboardTextField.Time, SkinV2Color.Parse(Config.MetadataColor));
            PerformanceRating = CreateText(LeaderboardTextField.Rating, SkinV2Color.Parse(Config.RatingColor));
            ComboAccuracy = CreateText(LeaderboardTextField.ComboAccuracy, SkinV2Color.Parse(Config.TextColor));

            BeatabilityBar = new Sprite
            {
                Parent = View.BeatabilityLayer,
                Size = new ScalableVector2(Config.BeatabilityBarWidth, Config.BeatabilityBarHeight),
                Image = RoundedRectTextureCache.Get(Config.BeatabilityBarWidth, Config.BeatabilityBarHeight, Config.BeatabilityBarRadius),
                UsePreviousSpriteBatchOptions = true
            };

            BeatabilityTooltip = new TooltipOptions
            {
                HoverDelayMilliseconds = 350,
                Style = new TooltipStyle
                {
                    BackgroundColor = ColorHelper.HexToColor("#161616"),
                    BorderThickness = 2,
                    TextSize = 20,
                    TextWeight = FontWeight.SemiBold
                }
            };
            BeatabilityTooltipRegistration = BeatabilityBar.AddTooltip(BeatabilityTooltip);

            SteamManager.SteamUserAvatarLoaded += OnSteamUserAvatarLoaded;
            ModManager.ModsChanged += OnModsChanged;
        }

        /// <summary>
        ///     Rebinds the visuals to another score and atlas entry
        /// </summary>
        public override void UpdateContent(Score item, int index)
        {
            Item = item;
            Index = index;

            UpdateTextKeys();
            Grade.Image = GetGradeTexture(item);
            UpdateAvatar();
            UpdateIdentity();
            UpdateModifiers();
            UpdateScoreSummary();
            LayoutVisuals();
        }

        private SharedTextAtlasSprite CreateText(LeaderboardTextField field, Color tint) =>
            new(View.TextAtlas, LeaderboardRowsView.GetTextKey(Index, field))
            {
                Parent = View.TextLayer,
                Alignment = Alignment.TopLeft,
                Tint = tint,
                UsePreviousSpriteBatchOptions = true
            };

        private void UpdateTextKeys()
        {
            Rank.SetKey(LeaderboardRowsView.GetTextKey(Index, LeaderboardTextField.Rank));
            Clan.SetKey(LeaderboardRowsView.GetTextKey(Index, LeaderboardTextField.Clan));
            Username.SetKey(LeaderboardRowsView.GetTextKey(Index, LeaderboardTextField.Username));
            Time.SetKey(LeaderboardRowsView.GetTextKey(Index, LeaderboardTextField.Time));
            PerformanceRating.SetKey(LeaderboardRowsView.GetTextKey(Index, LeaderboardTextField.Rating));
            ComboAccuracy.SetKey(LeaderboardRowsView.GetTextKey(Index, LeaderboardTextField.ComboAccuracy));
        }

        private void UpdateIdentity()
        {
            try
            {
                Flag.Region = Flags.GetRegion(Item.IsOnline ? Item.Country ?? "XX" : "XX");
            }
            catch
            {
                Flag.Region = Flags.GetRegion("XX");
            }

            var hasClan = !string.IsNullOrEmpty(Item.ClanTag);
            Clan.Visible = hasClan;
            Clan.Tint = hasClan && !string.IsNullOrEmpty(Item.ClanAccentColor)
                ? ParseColor(Item.ClanAccentColor, SkinV2Color.Parse(Config.TextColor)) : SkinV2Color.Parse(Config.TextColor);
        }

        private void UpdateAvatar()
        {
            var steamId = GetSteamId();
            var largeAvatar = TryGetLargeAvatar(steamId);
            var avatar = largeAvatar ?? TryGetFallbackAvatar(steamId) ?? UserInterface.UnknownAvatar;
            List.AvatarCache.Bind(Avatar, avatar);

            if (steamId > 0 && largeAvatar == null)
                SteamManager.SendAvatarRetrievalRequest(steamId);
        }

        private void UpdateModifiers()
        {
            foreach (var icon in ModifierIcons)
                icon.Destroy();
            ModifierIcons.Clear();

            foreach (var region in GetModifierRegions(Item))
                AddModifierIcon(region);
        }

        private void AddModifierIcon(TextureRegion region)
        {
            ModifierIcons.Add(new Sprite
            {
                Parent = View.ModifierLayer,
                Size = new ScalableVector2(Config.ModifierIconWidth, Config.ModifierIconHeight),
                Region = region,
                UsePreviousSpriteBatchOptions = true
            });
        }

        /// <summary>
        ///     Positions shared-layer sprites for every row
        /// </summary>
        private void LayoutVisuals()
        {
            var rowX = X;
            var rowY = Y;

            Background.Position = Position;

            var x = rowX + Config.Gap;
            Rank.SetDisplayed(List.ShowRank);
            if (List.ShowRank)
            {
                PositionText(Rank, x, rowY, Height);
                x += Rank.LayoutWidth + Config.Gap;
            }

            Grade.Position = new ScalableVector2(x, rowY + (Height - Grade.Height) / 2f);
            x += Grade.Width + Config.Gap;

            Avatar.Position = new ScalableVector2(x, rowY + (Height - Avatar.Height) / 2f);
            x += Avatar.Width + Config.Gap;

            var detailsHeight = Config.FlagSize + Config.Gap + Config.ModifierIconHeight;
            var identityY = rowY + (Height - detailsHeight) / 2f;
            var metadataY = identityY + Config.FlagSize + Config.Gap;

            Flag.Position = new ScalableVector2(x, identityY);
            var identityX = x + Flag.Width + 5;

            if (Clan.Visible)
            {
                PositionText(Clan, identityX, identityY, Config.FlagSize);
                identityX += Clan.LayoutWidth;
            }

            identityX += 5;
            PositionText(Username, identityX, identityY, Config.FlagSize);

            var metadataX = x;
            foreach (var icon in ModifierIcons)
            {
                icon.Position = new ScalableVector2(metadataX,
                    metadataY + (Config.ModifierIconHeight - icon.Height) / 2f);
                metadataX += icon.Width + 5;
            }

            if (ModifierIcons.Count > 0)
                metadataX += Config.Gap - 5;

            Clock.Position = new ScalableVector2(metadataX, metadataY);
            PositionText(Time, Clock.X + Clock.Width + 5, metadataY, Config.ModifierIconHeight);

            var barX = rowX + Width - Config.Gap - BeatabilityBar.Width;
            BeatabilityBar.Position = new ScalableVector2(barX, rowY + (Height - BeatabilityBar.Height) / 2f);

            var scoreRight = barX - Config.Gap;
            var scoreHeight = PerformanceRating.LayoutHeight + ComboAccuracy.LayoutHeight;
            var scoreY = rowY + (Height - scoreHeight) / 2f;

            PositionTextLayout(PerformanceRating, scoreRight - PerformanceRating.LayoutWidth, scoreY);
            PositionTextLayout(ComboAccuracy, scoreRight - ComboAccuracy.LayoutWidth, scoreY + PerformanceRating.LayoutHeight);
        }

        private static void PositionText(SharedTextAtlasSprite text, float contentX, float bandY, float bandHeight) => 
            PositionTextLayout(text, contentX, bandY + (bandHeight - text.LayoutHeight) / 2f);

        private static void PositionTextLayout(SharedTextAtlasSprite text, float contentX, float layoutY) =>
            PositionTextContent(text, contentX, layoutY + text.CapTopOffset);

        private static void PositionTextContent(SharedTextAtlasSprite text, float contentX, float contentY) =>
            text.Position = new ScalableVector2(contentX - text.ContentInset.X, contentY - text.ContentInset.Y);

        /// <summary>
        ///     Updates the beatability bar and tooltip for the currently selected modifiers
        /// </summary>
        private void UpdateScoreSummary()
        {
            var map = MapManager.Selected.Value;
            if (map == null)
            {
                BeatabilityBar.Tint = SkinV2Color.Parse(Config.UnbeatableColor);
                BeatabilityTooltip.Text = LocalizationManager.Get("Screen_Selection_CannotBeatScoreWithModifiers");
                BeatabilityTooltip.Style.BorderColor = SkinV2Color.Parse(Config.UnbeatableColor);
                return;
            }

            var processor = new RatingProcessorKeys(map.DifficultyFromMods(ModManager.Mods));
            var beatable = processor.CalculateRating(100) >= Item.PerformanceRating;
            var color = SkinV2Color.Parse(beatable ? Config.BeatableColor : Config.UnbeatableColor);
            BeatabilityBar.Tint = color;
            BeatabilityTooltip.Style.BorderColor = color;

            if (!beatable)
            {
                BeatabilityTooltip.Text = LocalizationManager.Get("Screen_Selection_CannotBeatScoreWithModifiers");
                return;
            }

            var requiredAccuracy = processor.GetAccuracyFromRating(Item.PerformanceRating);
            BeatabilityTooltip.Text = LocalizationManager.Get("Screen_Selection_RequiredAccuracyToBeatScore", StringHelper.AccuracyToString((float) requiredAccuracy));
        }

        private void OnModsChanged(object sender, ModsChangedEventArgs args) => ScheduleUpdate(() =>
        {
            if (!IsDisposed)
                UpdateScoreSummary();
        });

        private ulong GetSteamId()
        {
            if (ConfigManager.LeaderboardSection.Value == LeaderboardType.Local)
                return SteamUser.GetSteamID().m_SteamID;

            return (ulong) Math.Max(0, Item.SteamId);
        }

        private static Texture2D TryGetLargeAvatar(ulong steamId)
        {
            if (SteamManager.UserAvatarsLarge != null &&
                SteamManager.UserAvatarsLarge.TryGetValue(steamId, out var avatar) &&
                avatar != UserInterface.UnknownAvatar)
                return avatar;

            return null;
        }

        private static Texture2D TryGetFallbackAvatar(ulong steamId)
        {
            if (SteamManager.UserAvatars != null &&
                SteamManager.UserAvatars.TryGetValue(steamId, out var avatar) &&
                avatar != UserInterface.UnknownAvatar)
                return avatar;

            return null;
        }

        private void OnSteamUserAvatarLoaded(object sender, SteamAvatarLoadedEventArgs args)
        {
            if (args.SteamId != GetSteamId())
                return;

            ScheduleUpdate(() =>
            {
                if (!IsDisposed && args.SteamId == GetSteamId())
                    List.AvatarCache.Bind(Avatar,
                        TryGetLargeAvatar(args.SteamId) ?? TryGetFallbackAvatar(args.SteamId) ?? args.Texture);
            });
        }

        private static Texture2D GetGradeTexture(Score score)
        {
            var accuracy = LeaderboardRowsView.GetDisplayedAccuracy(score);
            var grade = score.Grade == API.Enums.Grade.F
                ? API.Enums.Grade.F : GradeHelper.GetGradeFromAccuracy(accuracy);
            return DifficultyRow.GetGradeTexture(grade) ?? UserInterface.Logo;
        }

        private static Color ParseColor(string value, Color fallback)
        {
            try
            {
                return ColorHelper.HexToColor(value);
            }
            catch
            {
                return fallback;
            }
        }

        internal static IReadOnlyList<TextureRegion> GetModifierRegions(Score score)
        {
            var regions = new List<TextureRegion>();
            var scoreMods = (ModIdentifier) score.Mods;
            var rate = ModHelper.GetRateFromMods(scoreMods);
            var rateMod = ModHelper.GetModsFromRate(rate);

            if (rateMod != ModIdentifier.None)
                regions.Add(GlobalGameplayAssets.GetRate(rate));

            foreach (var modifier in ModManager.GetModsList(scoreMods))
            {
                if (modifier != rateMod && HasGlobalIcon(modifier))
                    regions.Add(GlobalGameplayAssets.GetMod(modifier));
            }

            return regions;
        }

        private static bool HasGlobalIcon(ModIdentifier modifier) => modifier is
            ModIdentifier.Mirror or ModIdentifier.Autoplay or ModIdentifier.Coop or ModIdentifier.NoFail or
            ModIdentifier.NoSliderVelocity or ModIdentifier.NoMiss or ModIdentifier.NoMines or
            ModIdentifier.NoLongNotes or ModIdentifier.FullLN or ModIdentifier.Inverse or
            ModIdentifier.Randomize or ModIdentifier.HeatlthAdjust or ModIdentifier.NoPause or
            ModIdentifier.Paused;

        public override void Destroy()
        {
            SteamManager.SteamUserAvatarLoaded -= OnSteamUserAvatarLoaded;
            ModManager.ModsChanged -= OnModsChanged;

            BeatabilityTooltipRegistration?.Dispose();
            List.AvatarCache.Unbind(Avatar);

            foreach (var icon in ModifierIcons)
                icon.Destroy();
            ModifierIcons.Clear();

            Background.Destroy();
            Rank.Destroy();
            Grade.Destroy();
            Avatar.Destroy();
            Flag.Destroy();
            Clan.Destroy();
            Username.Destroy();
            Clock.Destroy();
            Time.Destroy();
            PerformanceRating.Destroy();
            ComboAccuracy.Destroy();
            BeatabilityBar.Destroy();

            base.Destroy();
        }
    }
}
