using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.V2.UI.Filters;
using Quaver.Shared.Skinning.V2;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Tooltips;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Chatting;

internal sealed class ChatEmotePicker : Sprite
{
    private SkinV2ChatEmotesConfig Config { get; }

    private ChatEmoteAtlas Atlas { get; }

    private Bindable<string> SearchQuery { get; } = new(string.Empty);

    private V2FilterSearchTextbox SearchBox { get; }

    private FlexContainer Grid { get; }

    private List<IDisposable> TooltipRegistrations { get; } = new();

    public event Action<string> EmoteSelected;

    public ChatEmotePicker(ChatEmoteAtlas atlas, SkinV2ChatEmotesConfig config)
    {
        Atlas = atlas;
        Config = config;
        Size = new ScalableVector2(config.PickerWidth, config.PickerHeight);
        Tint = SkinV2Color.Parse(config.BackgroundColor);

        var searchStyle = new V2FilterFieldStyle
        {
            Height = config.SearchHeight,
            SearchIconSize = config.SearchIconSize,
            SearchIconInset = config.SearchIconInset,
            CornerRadius = config.CornerRadius,
            BackgroundColor = SkinV2Color.Parse(config.SearchBackgroundColor),
            TextColor = SkinV2Color.Parse(config.SearchTextColor),
            PlaceholderColor = SkinV2Color.Parse(config.SearchPlaceholderColor),
            CursorColor = SkinV2Color.Parse(config.SearchTextColor)
        };
        SearchBox = new V2FilterSearchTextbox(SearchQuery, LocalizationManager.Get("Chat_SearchEmotes"), FontManager.GetWobbleFont(config.SearchFont), config.SearchFontSize, searchStyle, Math.Max(1, Width - config.SearchBorderSpacing * 2))
        {
            Parent = this,
            Position = new ScalableVector2(config.SearchBorderSpacing, config.SearchBorderSpacing)
        };
        SearchBox.SetResultsText(string.Empty);

        Grid = new FlexContainer
        {
            Parent = this,
            Direction = FlexDirection.Row,
            Wrap = FlexWrap.Wrap,
            AlignItems = FlexAlignItems.FlexStart,
            AlignContent = FlexAlignContent.FlexStart,
            ColumnGap = config.ColumnGap,
            RowGap = config.RowGap
        };

        SearchQuery.ValueChanged += OnSearchQueryChanged;
        RebuildGrid();
        ApplySize();
    }

    public void Resize(float width, float height) => Size = new ScalableVector2(Math.Max(1, width), Math.Max(1, height));

    protected override void OnRectangleRecalculated()
    {
        base.OnRectangleRecalculated();
        if (Config != null)
            ApplySize();
    }

    private void ApplySize()
    {
        if (Width <= 0 || Height <= 0)
            return;

        var texture = RoundedRectTextureCache.Get(Width, Height, Config.CornerRadius);
        if (Image != texture)
            Image = texture;

        if (SearchBox == null || Grid == null)
            return;

        var contentWidth = Math.Max(1, Width - Config.Padding * 2);
        var searchWidth = Math.Max(1, Width - Config.SearchBorderSpacing * 2);
        var gridY = Config.SearchBorderSpacing + Config.SearchHeight + Config.Padding;

        SearchBox.Position = new ScalableVector2(Config.SearchBorderSpacing, Config.SearchBorderSpacing);
        SearchBox.Width = searchWidth;

        Grid.Position = new ScalableVector2(Config.Padding, gridY);
        Grid.Size = new ScalableVector2(contentWidth, Math.Max(1, Height - gridY - Config.Padding));
        Grid.RefreshLayout();
    }

    private void RebuildGrid()
    {
        foreach (var registration in TooltipRegistrations)
            registration.Dispose();

        TooltipRegistrations.Clear();

        while (Grid.Children.Count > 0)
            Grid.Children[^1].Destroy();

        var query = (SearchQuery.Value ?? string.Empty).Trim().Trim(':');
        foreach (var name in ChatEmoteAtlas.Names.Where(name => string.IsNullOrEmpty(query) || name.Contains(query, StringComparison.OrdinalIgnoreCase)))
        {
            var button = new RoundedButton
            {
                Parent = Grid,
                Size = new ScalableVector2(Config.ButtonSize, Config.ButtonSize),
                CornerRadius = Config.CornerRadius,
                Tint = Color.Transparent
            };
            button.SetIcon(Atlas.Get(name), new Vector2(Config.ButtonSize, Config.ButtonSize));
            button.Clicked += (_, _) => EmoteSelected?.Invoke(name);

            TooltipRegistrations.Add(button.AddTooltip(ChatEmoteAtlas.GetAlias(name)));
            Grid.SetItemOptions(button, new FlexItemOptions { Basis = Config.ButtonSize, Shrink = 0 });
        }

        Grid.RefreshLayout();
    }

    private void OnSearchQueryChanged(object sender, BindableValueChangedEventArgs<string> args) => RebuildGrid();

    public override void Destroy()
    {
        SearchQuery.ValueChanged -= OnSearchQueryChanged;

        foreach (var registration in TooltipRegistrations)
            registration.Dispose();

        TooltipRegistrations.Clear();
        EmoteSelected = null;

        base.Destroy();
        SearchQuery.Dispose();
    }
}
