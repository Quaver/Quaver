using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.UI;

internal sealed class HeaderScreenNavigation : FlexContainer
{
    private SkinV2NavigationConfig Config { get; }
    private RoundedButton SessionTimeButton { get; set; }
    private RoundedButton FriendsOnlineButton { get; set; }
    private SpriteTextPlus FriendsOnlineCount { get; set; }
    private double StatsRefreshTimeRemaining { get; set; }
    public ProfileSummary ProfileButton { get; private set; }

    private FlexContainer StatsLayout { get; set; }

    public HeaderScreenNavigation(SkinV2NavigationConfig config)
    {
        Config = config;
        Size = new ScalableVector2(config.Stats.FriendsWidth + config.ItemSpacing + config.Profile.Width,
            Math.Max(config.Button.Size, config.Stats.ButtonHeight * 2 + config.Stats.RowGap));
        Direction = FlexDirection.Row;
        AlignItems = FlexAlignItems.Center;
        ColumnGap = config.ItemSpacing;

        CreateStatsLayout();
        CreateSessionTimeButton();
        CreateFriendsOnlineButton();
        CreateProfileButton();

        StatsLayout.RefreshLayout();
        RefreshLayout();
        UpdateStats();
    }

    private void CreateStatsLayout()
    {
        var stats = Config.Stats;
        StatsLayout = new FlexContainer
        {
            Parent = this,
            Size = new ScalableVector2(stats.FriendsWidth, stats.ButtonHeight * 2 + stats.RowGap),
            Direction = FlexDirection.Column,
            JustifyContent = FlexJustifyContent.Center,
            AlignItems = FlexAlignItems.FlexEnd,
            RowGap = stats.RowGap
        };
        SetItemOptions(StatsLayout, new FlexItemOptions { Basis = stats.FriendsWidth, Shrink = 0 });
    }

    private void CreateSessionTimeButton()
    {
        var stats = Config.Stats;
        SessionTimeButton = new RoundedButton
        {
            Parent = StatsLayout,
            Size = new ScalableVector2(stats.TimeWidth, stats.ButtonHeight),
            CornerRadius = stats.CornerRadius,
            Tint = SkinV2Color.Parse(stats.BackgroundColor),
            PerformHoverFade = false,
            IsClickable = false,
            IsInteractionEnabled = false
        };
        SessionTimeButton.SetLabel(FontManager.GetWobbleFont(stats.Font), "00:00:00", stats.FontSize, SkinV2Color.Parse(stats.TextColor));
        StatsLayout.SetItemOptions(SessionTimeButton, new FlexItemOptions { Basis = stats.ButtonHeight, Shrink = 0 });
    }

    private void CreateFriendsOnlineButton()
    {
        var stats = Config.Stats;
        var font = FontManager.GetWobbleFont(stats.Font);
        FriendsOnlineButton = new RoundedButton
        {
            Parent = StatsLayout,
            Size = new ScalableVector2(stats.FriendsWidth, stats.ButtonHeight),
            CornerRadius = stats.CornerRadius,
            Tint = SkinV2Color.Parse(stats.BackgroundColor),
            PerformHoverFade = false,
            IsClickable = false,
            IsInteractionEnabled = false
        };
        StatsLayout.SetItemOptions(FriendsOnlineButton, new FlexItemOptions { Basis = stats.ButtonHeight, Shrink = 0 });

        var friendsLayout = new FlexContainer
        {
            Parent = FriendsOnlineButton,
            Size = FriendsOnlineButton.Size,
            Direction = FlexDirection.Row,
            JustifyContent = FlexJustifyContent.Center,
            AlignItems = FlexAlignItems.Center,
            ColumnGap = stats.TextGap
        };
        FriendsOnlineCount = new SpriteTextPlus(font, "0", stats.FontSize)
        {
            Parent = friendsLayout,
            Tint = SkinV2Color.Parse(stats.CountColor),
            UsePreviousSpriteBatchOptions = true
        };
        var label = new SpriteTextPlus(font, LocalizationManager.Get("Screen_Navigation_FriendsOnline"), stats.FontSize)
        {
            Parent = friendsLayout,
            Tint = SkinV2Color.Parse(stats.TextColor),
            UsePreviousSpriteBatchOptions = true
        };
        friendsLayout.SetItemOptions(FriendsOnlineCount, new FlexItemOptions { Shrink = 0 });
        friendsLayout.SetItemOptions(label, new FlexItemOptions { Shrink = 0 });
        friendsLayout.RefreshLayout();
    }

    private void CreateProfileButton()
    {
        ProfileButton = new ProfileSummary(Config.Profile, SkinV2Color.Parse(Config.Button.BackgroundColor), UserInterface.OfflineAvatar, Config.Button.Size)
        {
            Parent = this
        };
        SetItemOptions(ProfileButton, new FlexItemOptions { Basis = Config.Profile.Width, Grow = 1, Shrink = 0 });
    }

    private void UpdateStats()
    {
        var elapsed = TimeSpan.FromMilliseconds(GameBase.Game?.TimeRunning ?? 0);
        var hours = (int)elapsed.TotalHours;
        var time = $"{hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
        if (SessionTimeButton.Label.Text != time)
            SessionTimeButton.Label.Text = time;

        var count = OnlineManager.Connected && OnlineManager.FriendsList != null && OnlineManager.OnlineUsers != null
            ? OnlineManager.FriendsList.Count(id => OnlineManager.OnlineUsers.ContainsKey(id)) : 0;
        
        if (FriendsOnlineCount.Text != count.ToString())
            FriendsOnlineCount.Text = count.ToString();
    }

    public override void Update(GameTime gameTime)
    {
        StatsRefreshTimeRemaining -= gameTime.ElapsedGameTime.TotalMilliseconds;
        if (StatsRefreshTimeRemaining <= 0)
        {
            UpdateStats();
            StatsRefreshTimeRemaining = 1000;
        }

        base.Update(gameTime);
    }
}
