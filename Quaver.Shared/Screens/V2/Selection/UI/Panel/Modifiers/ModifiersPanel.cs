using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Server.Client.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Modifiers.Mods;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Selection.UI.Panel.Modifiers
{
    /// <summary>
    ///     Displays gameplay modifier controls
    /// </summary>
    internal sealed class ModifiersPanel : Container
    {
        private SkinV2SelectionModifiersConfig Config { get; }
        private Container OverlayHost { get; }
        private ModifierRowsView RowsView { get; }
        private FlexContainer RankedHeader { get; }
        private ModifierSplitRow JudgementRow { get; }
        private ModifierSplitRow SpeedRow { get; }
        private List<ModifierToggleRow> RankedRows { get; }
        private FlexContainer UnrankedHeader { get; }
        private List<ModifierToggleRow> UnrankedRows { get; }
        private RoundedButton ResetButton { get; }

        internal ModifiersPanel(SkinV2SelectionModifiersConfig config, Container overlayHost, Container footerHost)
        {
            Config = config;
            OverlayHost = overlayHost;
            var rankedModifiers = new List<ModifierDefinition>
            {
                new(new ModMirror()),
                new(new ModNoMiss())
            };
            var unrankedModifiers = new List<ModifierDefinition>
            {
                new(new ModAutoplay()),
                new(new ModNoFail()),
                new(new ModRandomize()),
                new(new ModNoSliderVelocities()),
                new(new ModNoMines()),
                new(new ModNoLongNotes()),
                new(new ModFullLN()),
                new(new ModInverse()),
                new(new ModCoop(), "Co-operation")
            };

            RowsView = new ModifierRowsView(this, Config, rankedModifiers.Concat(unrankedModifiers).ToList());

            RankedHeader = CreateSectionHeader("Ranked", "These mods can be used for ranked scores");
            JudgementRow = CreateJudgementRow();
            SpeedRow = CreateSpeedRow();
            RankedRows = rankedModifiers.Select(RowsView.CreateRow).ToList();

            UnrankedHeader = CreateSectionHeader("Unranked", "Scores won’t be submitted while using them");
            UnrankedRows = unrankedModifiers.Select(RowsView.CreateRow).ToList();

            JudgementWindowsDatabaseCache.Selected.ValueChanged += OnJudgementWindowChanged;
            ModManager.ModsChanged += OnPanelModsChanged;

            ResetButton = new RoundedButton((sender, args) => ModManager.RemoveAllMods())
            {
                Parent = footerHost,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(Config.ResetButtonWidth, Config.RowHeight),
                CornerRadius = Config.CornerRadius,
                Tint = SkinV2Color.Parse(Config.RowColor),
                PerformHoverFade = true
            };
            ResetButton.SetLabel(FontManager.GetWobbleFont(Config.Font), "Reset Modifiers", Config.FontSize, Color.White);

            SizeChanged += OnSizeChanged;
        }

        private const string ToggleOnTextKey = "toggle:on";
        private const string ToggleOffTextKey = "toggle:off";

        private static string GetModifierTextKey(ModIdentifier identifier) => $"modifier:{identifier}";

        private ModifierSplitRow CreateJudgementRow()
        {
            var entries = JudgementWindowsDatabaseCache.Presets
                .Select(window => (DropdownEntry<JudgementWindows>) new DropdownOption<JudgementWindows>(window, window.Name))
                .ToList();

            var style = CreateDropdownStyle();
            var dropdown = new V2Dropdown<JudgementWindows>(1, JudgementWindowsDatabaseCache.Selected, entries, FontManager.GetWobbleFont(Config.Font), style, OverlayHost)
            {
                Size = new ScalableVector2(1, Config.RowHeight)
            };

            return new ModifierSplitRow(Config, GetJudgementTexture(JudgementWindowsDatabaseCache.Selected.Value), GlobalGameplayAssets.JudgementDisplaySize, 
                "Judgement Windows", SkinV2Color.Parse("#F2C94CFF"), Config.JudgementLabelWidth, dropdown)
            {
                Parent = this
            };
        }

        private ModifierSplitRow CreateSpeedRow()
        {
            var speed = new ModifierSpeedControl(Config);
            return new ModifierSplitRow(Config, GlobalGameplayAssets.GetRate(ModHelper.GetRateFromMods(ModManager.Mods)), GlobalGameplayAssets.RateDisplaySize, 
                "Speed", SkinV2Color.Parse("#8963EBFF"), Config.SpeedLabelWidth, speed, true)
            {
                Parent = this
            };
        }

        /// <summary>
        ///     Builds the joined title and description shown above each modifier group
        /// </summary>
        private FlexContainer CreateSectionHeader(string title, string description)
        {
            var header = new FlexContainer
            {
                Parent = this,
                Size = new ScalableVector2(1, Config.RowHeight),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch
            };
            var titleButton = CreateSectionHeaderPart(header, title, Config.SectionColor, Config.SectionTextColor);
            titleButton.CornerRadii = new RoundedRectCornerRadii(Config.CornerRadius, 0, 0, Config.CornerRadius);
            header.SetItemOptions(titleButton, new FlexItemOptions { Basis = titleButton.Width, Shrink = 0 });

            var descriptionButton = CreateSectionHeaderPart(header, description, Config.RowColor, Config.DescriptionColor);
            descriptionButton.CornerRadii = new RoundedRectCornerRadii(0, Config.CornerRadius, Config.CornerRadius, 0);
            header.SetItemOptions(descriptionButton, new FlexItemOptions { Basis = descriptionButton.Width, Shrink = 0 });

            header.Width = titleButton.Width + descriptionButton.Width;
            header.RefreshLayout();
            return header;
        }

        private RoundedButton CreateSectionHeaderPart(Container parent, string text, string color, string textColor)
        {
            var button = new RoundedButton
            {
                Parent = parent,
                Size = new ScalableVector2(1, Config.RowHeight),
                WidthMode = ButtonSizeMode.Auto,
                AutoSizePadding = new Vector2(Config.LabelPadding * 2, 0),
                CornerRadius = Config.CornerRadius,
                Tint = SkinV2Color.Parse(color),
                PerformHoverFade = false,
                IsClickable = false,
                IsInteractionEnabled = false
            };
            button.SetLabel(FontManager.GetWobbleFont(Config.Font), text, Config.FontSize, SkinV2Color.Parse(textColor));
            button.Label.Alignment = Alignment.MidLeft;
            button.Label.X = Config.LabelPadding;

            return button;
        }

        private SkinV2DropdownConfig CreateDropdownStyle() => new()
        {
            Height = Config.RowHeight,
            ItemHeight = Config.RowHeight,
            HorizontalPadding = Config.ControlPadding,
            CornerRadius = Config.CornerRadius,
            Font = Config.Font,
            FontSize = Config.FontSize,
            TriggerColor = Config.RowColor,
            ItemColor = Config.RowColor,
            HoverColor = "#354451FF",
            SelectedItemColor = "#6B83B2FF",
            TextColor = "#F2C94CFF",
            SelectedTextColor = "#F2C94CFF",
            IconColor = "#F2C94CFF"
        };

        private void OnJudgementWindowChanged(object sender, BindableValueChangedEventArgs<JudgementWindows> args) => JudgementRow.SetBadge(GetJudgementTexture(args.Value));

        private void OnPanelModsChanged(object sender, ModsChangedEventArgs args) => SpeedRow.SetBadge(GlobalGameplayAssets.GetRate(ModHelper.GetRateFromMods(ModManager.Mods)));

        private static TextureRegion GetJudgementTexture(JudgementWindows windows)
        {
            if (windows == null || !windows.IsDefault)
                return GlobalGameplayAssets.GetJudgement(GlobalJudgementWindow.Custom);

            var preset = windows?.Name?.TrimEnd('*');
            var judgement = preset switch
            {
                "Peaceful" => GlobalJudgementWindow.Peaceful,
                "Lenient" => GlobalJudgementWindow.Lenient,
                "Chill" => GlobalJudgementWindow.Chill,
                "Standard" => GlobalJudgementWindow.Standard,
                "Strict" => GlobalJudgementWindow.Strict,
                "Tough" => GlobalJudgementWindow.Tough,
                "Extreme" => GlobalJudgementWindow.Extreme,
                "Impossible" => GlobalJudgementWindow.Impossible,
                _ => GlobalJudgementWindow.Custom
            };
            return GlobalGameplayAssets.GetJudgement(judgement);
        }

        private static Color GetModifierColor(ModIdentifier identifier) => identifier switch
        {
            ModIdentifier.Mirror => SkinV2Color.Parse("#4FA3B0FF"),
            ModIdentifier.NoMiss => SkinV2Color.Parse("#FF3A6FFF"),
            ModIdentifier.Autoplay => SkinV2Color.Parse("#41AFFFFF"),
            ModIdentifier.NoFail => SkinV2Color.Parse("#41AFFFFF"),
            ModIdentifier.Randomize => SkinV2Color.Parse("#25C88CFF"),
            ModIdentifier.NoSliderVelocity => SkinV2Color.Parse("#FF3A6FFF"),
            ModIdentifier.NoMines => SkinV2Color.Parse("#FF3A6FFF"),
            ModIdentifier.NoLongNotes => SkinV2Color.Parse("#FFAD3AFF"),
            ModIdentifier.FullLN => SkinV2Color.Parse("#FFAD3AFF"),
            ModIdentifier.Inverse => SkinV2Color.Parse("#FFAD3AFF"),
            ModIdentifier.Coop => SkinV2Color.Parse("#33C0D9FF"),
            _ => Color.White
        };

        private void OnSizeChanged(object sender, ScalableVector2 size)
        {
            RowsView.Resize(size);

            var innerWidth = Math.Max(1, Width - Config.HorizontalInset * 2);
            var y = Config.HorizontalInset;

            Position(RankedHeader, Config.HorizontalInset, ref y, RankedHeader.Width);
            Position(JudgementRow, Config.HorizontalInset, ref y, innerWidth);
            Position(SpeedRow, Config.HorizontalInset, ref y, innerWidth);
            foreach (var row in RankedRows)
                Position(row, Config.HorizontalInset, ref y, innerWidth);

            Position(UnrankedHeader, Config.HorizontalInset, ref y, UnrankedHeader.Width);
            foreach (var row in UnrankedRows)
                Position(row, Config.HorizontalInset, ref y, innerWidth);
        }

        private void Position(Drawable drawable, float x, ref float y, float width)
        {
            if (drawable is ModifierToggleRow modifierRow)
                modifierRow.SetLayout(x, y, width, Config.RowHeight);
            else
            {
                drawable.Position = new ScalableVector2(x, y);
                drawable.Size = new ScalableVector2(width, Config.RowHeight);
            }
            y += Config.RowHeight + Config.RowGap;
        }

        public override void Destroy()
        {
            JudgementWindowsDatabaseCache.Selected.ValueChanged -= OnJudgementWindowChanged;
            ModManager.ModsChanged -= OnPanelModsChanged;

            RowsView.Dispose();
            base.Destroy();
        }

        private sealed class ModifierSplitRow : Container
        {
            private SkinV2SelectionModifiersConfig Config { get; }
            private float LabelWidth { get; }
            private Drawable Control { get; }
            private ModifierSplitLabel LabelContent { get; }
            private Container LabelPanel { get; }
            private Container ControlPanel { get; }

            public ModifierSplitRow(SkinV2SelectionModifiersConfig config, TextureRegion badge, Point badgeSize, string name, Color color, float labelWidth, Drawable control, bool wrapControl = false)
            {
                Config = config;
                LabelWidth = labelWidth;
                Control = control;
                LabelPanel = CreateRoundedPanel(this, config, config.RowColor);
                LabelContent = new ModifierSplitLabel(LabelPanel, config, badge, badgeSize, name, color);
                ControlPanel = wrapControl ? CreateRoundedPanel(this, config, config.RowColor) : null;
                Control.Parent = ControlPanel ?? this;

                SizeChanged += OnSizeChanged;
            }

            private void OnSizeChanged(object sender, ScalableVector2 size)
            {
                var controlWidth = Math.Max(1, Width - LabelWidth - Config.RowGap);
                LabelPanel.Size = new ScalableVector2(LabelWidth, Height);
                LabelContent.Layout(new ScalableVector2(0, 0), LabelPanel.Size);

                if (ControlPanel != null)
                {
                    ControlPanel.Position = new ScalableVector2(LabelWidth + Config.RowGap, 0);
                    ControlPanel.Size = new ScalableVector2(controlWidth, Height);
                    Control.Alignment = Alignment.MidCenter;
                    return;
                }

                Control.Alignment = Alignment.TopLeft;
                Control.Position = new ScalableVector2(LabelWidth + Config.RowGap, 0);
                Control.Size = new ScalableVector2(controlWidth, Height);
            }

            public void SetBadge(TextureRegion badge) => LabelContent.SetBadge(badge);
        }

        private sealed class ModifierDefinition
        {
            internal IGameplayModifier Modifier { get; }
            internal string DisplayName { get; }
            internal Color Color { get; }

            internal ModifierDefinition(IGameplayModifier modifier, string displayName = null)
            {
                Modifier = modifier;
                DisplayName = displayName ?? modifier.Name;
                Color = GetModifierColor(modifier.ModIdentifier);
            }
        }

        /// <summary>
        ///     Owns the shared atlas and draw layers used by every modifier row
        /// </summary>
        private sealed class ModifierRowsView : IDisposable
        {
            private Container Parent { get; }
            private SkinV2SelectionModifiersConfig Config { get; }
            internal SharedTextAtlas TextAtlas { get; }
            internal Container BackgroundLayer { get; }
            internal Container BadgeLayer { get; }
            internal Container NameLayer { get; }
            internal ToggleV2LayerSet ToggleLayers { get; }

            internal ModifierRowsView(Container parent, SkinV2SelectionModifiersConfig config, IReadOnlyList<ModifierDefinition> definitions)
            {
                Parent = parent;
                Config = config;
                TextAtlas = CreateTextAtlas(definitions);
                BackgroundLayer = CreateLayer(parent);
                BadgeLayer = CreateLayer(parent);
                NameLayer = CreateLayer(parent);
                ToggleLayers = new ToggleV2LayerSet(parent);
            }

            private SharedTextAtlas CreateTextAtlas(IReadOnlyList<ModifierDefinition> definitions)
            {
                var modifierFont = FontManager.GetWobbleFont(Config.Font);
                var toggleFont = FontManager.GetWobbleFont(Fonts.InterSemiBold);

                var atlasDefinitions = definitions.Select(definition => new SharedTextAtlasDefinition(GetModifierTextKey(definition.Modifier.ModIdentifier), definition.DisplayName, modifierFont, Config.FontSize)).ToList();
                atlasDefinitions.Add(new SharedTextAtlasDefinition(ToggleOnTextKey, "ON", toggleFont, 11));
                atlasDefinitions.Add(new SharedTextAtlasDefinition(ToggleOffTextKey, "OFF", toggleFont, 11));

                return new SharedTextAtlas(atlasDefinitions);
            }

            private static Container CreateLayer(Container parent) => new()
            {
                Parent = parent,
                UsePreviousSpriteBatchOptions = true
            };

            internal ModifierToggleRow CreateRow(ModifierDefinition definition) => new(Config, this, definition) { Parent = Parent };

            internal void Resize(ScalableVector2 size)
            {
                BackgroundLayer.Size = size;
                BadgeLayer.Size = size;
                NameLayer.Size = size;
                ToggleLayers.Resize(size);
            }

            public void Dispose() => TextAtlas.Dispose();
        }

        /// <summary>
        ///     Connects a toggleable modifier to its row
        /// </summary>
        private sealed class ModifierToggleRow : Container
        {
            private SkinV2SelectionModifiersConfig Config { get; }
            private IGameplayModifier Modifier { get; }
            private Sprite Background { get; }
            private Sprite Badge { get; }
            private SharedTextAtlasSprite Name { get; }
            private Bindable<bool> Enabled { get; }
            private ToggleV2 Toggle { get; }
            private bool Synchronizing { get; set; }

            internal ModifierToggleRow(SkinV2SelectionModifiersConfig config, ModifierRowsView view, ModifierDefinition definition)
            {
                Config = config;
                Modifier = definition.Modifier;
                Background = new Sprite
                {
                    Parent = view.BackgroundLayer,
                    Tint = SkinV2Color.Parse(config.RowColor),
                    UsePreviousSpriteBatchOptions = true
                };

                Enabled = new Bindable<bool>(ModManager.IsActivated(Modifier.ModIdentifier));
                Badge = new Sprite
                {
                    Parent = view.BadgeLayer,
                    Alignment = Alignment.TopLeft,
                    Size = new ScalableVector2(GlobalGameplayAssets.ModDisplaySize.X, GlobalGameplayAssets.ModDisplaySize.Y),
                    Region = GlobalGameplayAssets.GetMod(Modifier.ModIdentifier, !Enabled.Value),
                    Tint = Color.White,
                    UsePreviousSpriteBatchOptions = true
                };

                Name = new SharedTextAtlasSprite(view.TextAtlas,
                    GetModifierTextKey(Modifier.ModIdentifier))
                {
                    Parent = view.NameLayer,
                    Alignment = Alignment.TopLeft,
                    Tint = definition.Color,
                    UsePreviousSpriteBatchOptions = true
                };

                Toggle = new ToggleV2(Enabled, view.ToggleLayers, view.TextAtlas, ToggleOnTextKey, ToggleOffTextKey)
                {
                    Parent = this
                };

                Toggle.ValueEdited += OnToggleEdited;
                ModManager.ModsChanged += OnModsChanged;
            }

            internal void SetLayout(float x, float y, float width, float height)
            {
                Position = new ScalableVector2(x, y);
                Size = new ScalableVector2(width, height);

                Background.Position = Position;
                Background.Size = Size;
                Background.Image = RoundedRectTextureCache.Get(width, height, Config.CornerRadius);

                Badge.Position = new ScalableVector2(x + Config.LabelPadding, y + (height - Badge.Height) / 2f);

                var nameX = Badge.X + Badge.Width + Config.LabelPadding;
                Name.Position = new ScalableVector2(
                    nameX - Name.ContentInset.X,
                    y + (height - Name.ContentHeight) / 2f - Name.ContentInset.Y);

                Toggle.SetVisualPosition(new ScalableVector2(
                    x + width - Config.ControlPadding - Toggle.Width,
                    y + (height - Toggle.Height) / 2f));
            }

            private void OnToggleEdited(object sender, EventArgs args)
            {
                if (Synchronizing)
                    return;

                if (!CanChangeModifier(Modifier))
                {
                    Synchronize();
                    return;
                }

                if (Enabled.Value)
                    ModManager.AddMod(Modifier.ModIdentifier, true);
                else
                    ModManager.RemoveMod(Modifier.ModIdentifier, true);
            }

            private void OnModsChanged(object sender, ModsChangedEventArgs args) => Synchronize();

            private void Synchronize()
            {
                Synchronizing = true;
                Enabled.Value = ModManager.IsActivated(Modifier.ModIdentifier);
                Badge.Region = GlobalGameplayAssets.GetMod(Modifier.ModIdentifier, !Enabled.Value);
                Synchronizing = false;
            }

            public override void Destroy()
            {
                Toggle.ValueEdited -= OnToggleEdited;
                ModManager.ModsChanged -= OnModsChanged;

                Toggle.Destroy();
                Background.Destroy();
                Badge.Destroy();
                Name.Destroy();
                Enabled.Dispose();

                base.Destroy();
            }
        }

        private sealed class ModifierSplitLabel
        {
            private SkinV2SelectionModifiersConfig Config { get; }
            private Sprite Badge { get; }
            private SpriteTextPlus TextName { get; }

            internal ModifierSplitLabel(Container parent, SkinV2SelectionModifiersConfig config, TextureRegion badge, Point badgeSize, string name, Color color)
            {
                Config = config;
                Badge = new Sprite
                {
                    Parent = parent,
                    Alignment = Alignment.TopLeft,
                    Size = new ScalableVector2(badgeSize.X, badgeSize.Y),
                    Region = badge,
                    Tint = Color.White,
                    UsePreviousSpriteBatchOptions = true
                };

                TextName = new SpriteTextPlus(FontManager.GetWobbleFont(Config.Font), name, Config.FontSize, true)
                {
                    Parent = parent,
                    Alignment = Alignment.TopLeft,
                    Tint = color,
                    UsePreviousSpriteBatchOptions = true
                };
            }

            internal void SetBadge(TextureRegion badge) => Badge.Region = badge;

            internal void Layout(ScalableVector2 origin, ScalableVector2 size)
            {
                Badge.Position = new ScalableVector2(
                    origin.X.Value + Config.LabelPadding,
                    origin.Y.Value + (size.Y.Value - Badge.Height) / 2f);

                TextName.Position = new ScalableVector2(
                    Badge.X + Badge.Width + Config.LabelPadding,
                    origin.Y.Value + (size.Y.Value - TextName.Height) / 2f);

                var localNameX = TextName.X - origin.X.Value;
                var available = Math.Max(1, size.X.Value - localNameX - Config.LabelPadding);

                TextName.TruncateWithEllipsis((int) available);
            }

            internal void Destroy()
            {
                Badge.Destroy();
                TextName.Destroy();
            }
        }

        private sealed class ModifierSpeedControl : Container
        {
            private SkinV2SelectionModifiersConfig Config { get; }
            private BindableFloat Rate { get; }
            private SliderV2 Slider { get; }
            private bool Synchronizing { get; set; }

            public ModifierSpeedControl(SkinV2SelectionModifiersConfig config)
            {
                Config = config;
                Rate = new BindableFloat(ModHelper.GetRateFromMods(ModManager.Mods), 0.5f, 2f);
                Slider = new SliderV2(Rate, new ScalableVector2(Config.SpeedTrackWidth, Config.SpeedTrackHeight), 2, true, "x", SkinV2Color.Parse("#8963EBFF"))
                {
                    Parent = this,
                    Alignment = Alignment.MidCenter
                };
                Size = Slider.Size;

                Rate.ValueChanged += OnRateChanged;
                ModManager.ModsChanged += OnModsChanged;
            }

            private void OnRateChanged(object sender, BindableValueChangedEventArgs<float> args)
            {
                if (Synchronizing)
                    return;

                var rate = Microsoft.Xna.Framework.MathHelper.Clamp((float) Math.Round(args.Value * 20) / 20f, 0.5f, 2f);
                if (!CanChangeModifier(new ModSpeed(ModIdentifier.Speed11X)))
                {
                    Synchronize();
                    return;
                }

                if (Math.Abs(Rate.Value - rate) > 0.001f)
                    Rate.Value = rate;

                if (Math.Abs(ModHelper.GetRateFromMods(ModManager.Mods) - rate) > 0.001f)
                    ModManager.AddSpeedMods(rate);
            }

            private void OnModsChanged(object sender, ModsChangedEventArgs args) => Synchronize();

            private void Synchronize()
            {
                Synchronizing = true;
                Rate.Value = ModHelper.GetRateFromMods(ModManager.Mods);
                Synchronizing = false;
            }

            public override void Destroy()
            {
                Rate.ValueChanged -= OnRateChanged;
                ModManager.ModsChanged -= OnModsChanged;

                Rate.Dispose();
                base.Destroy();
            }
        }

        private static Container CreateRoundedPanel(Container parent, SkinV2SelectionModifiersConfig config, string color)
        {
            var panel = new Container { Parent = parent };
            var background = CreateRoundedBackground(panel, config, color);
            panel.SizeChanged += (sender, size) => background.Size = size;

            return panel;
        }

        private static NineSliceSprite CreateRoundedBackground(Container parent, SkinV2SelectionModifiersConfig config, string color)
        {
            var margin = (int) Math.Ceiling(config.CornerRadius);
            var textureSize = Math.Max(2, margin * 2 + 2);
            var background = new NineSliceSprite
            {
                Parent = parent,
                Image = RoundedRectTextureCache.Get(textureSize, textureSize, config.CornerRadius),
                Margins = new SliceMargins(margin),
                Tint = SkinV2Color.Parse(color)
            };
            return background;
        }

        private static bool CanChangeModifier(IGameplayModifier modifier)
        {
            var game = OnlineManager.CurrentGame;

            if (game == null)
                return true;

            if (!modifier.AllowedInMultiplayer)
                return false;

            var isHost = game.HostId == OnlineManager.Self?.OnlineUser?.Id;
            if (modifier.OnlyMultiplayerHostCanCanChange && !isHost)
                return false;

            if (modifier is ModSpeed && !isHost && !game.FreeModType.HasFlag(MultiplayerFreeModType.Rate))
                return false;

            return modifier is ModSpeed || isHost || game.FreeModType.HasFlag(MultiplayerFreeModType.Regular);
        }
    }
}
