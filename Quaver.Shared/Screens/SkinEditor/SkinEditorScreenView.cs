using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using ModeHelper = Quaver.API.Helpers.ModeHelper;
using Quaver.API.Maps;
using Quaver.API.Replays;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Results.UI;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Form;
using Wobble.Graphics.UI;
using Wobble.Managers;
using Wobble.Platform;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.SkinEditor
{
    public sealed class SkinEditorScreenView : ScreenView
    {
        private const float Padding = 14;
        private const float HeaderHeight = 196;
        private const float HeaderTitleHeight = 45;
        private const float ControlHeight = 30;
        private const float ControlSpacing = 10;
        private const float FieldWidth = 214;
        private const float FieldHeight = 54;
        private const float FieldSpacing = 10;
        private const float ToggleSize = 22;

        private SkinEditorScreen SkinScreen => (SkinEditorScreen)Screen;

        private string LoadedSkin { get; set; }

        private string NewFolderNameValue { get; set; }

        private Dictionary<SkinEditorProperty, string> Values { get; set; }

        private SkinEditorPreviewScreen SelectedPreview { get; set; } = SkinEditorPreviewScreen.MainMenu;

        private Sprite HeaderPanel { get; set; }

        private ScrollContainer Inspector { get; set; }

        private Sprite PreviewPanel { get; set; }

        private Sprite PreviewSurface { get; set; }

        private SpriteTextPlus LoadedSkinText { get; set; }

        private LabelledTextbox NewFolderName { get; set; }

        private TextboxTabControl TabControl { get; set; }

        private SkinEditorMenuBar MenuBar { get; set; }

        private ImageButton HeaderToggle { get; set; }

        private Bindable<bool> HeaderVisible { get; } = new(true);

        private readonly Dictionary<SkinEditorProperty, LabelledTextbox> Fields = new();

        private Map CachedGameplayMap { get; set; }

        private Qua CachedGameplayQua { get; set; }

        private GameplayScreen GameplayPreviewScreen { get; set; }

        private Map GameplayPreviewMap { get; set; }

        private Container GameplayPreviewViewport { get; set; }

        private List<Map> PreviewTrackQueue { get; } = new();

        private int PreviewTrackQueuePosition { get; set; } = -1;

        private Random PreviewTrackRandom { get; } = new();

        private bool LoadingNextPreviewTrack { get; set; }

        private bool GameplayPreviewCompletionPending { get; set; }

        public SkinEditorScreenView(SkinEditorScreen screen) : base(screen)
        {
            LoadedSkin = SkinEditorManager.GetCurrentLocalSkin();
            NewFolderNameValue = SkinEditorManager.GetSuggestedFolderName();
            Values = SkinEditorManager.LoadPropertyValues(LoadedSkin);
            EnsureSelectedMapTrackPlaying();
            MenuBar = new SkinEditorMenuBar(this);

            CreateBackground();
            RefreshLayout();
        }

        public override void Update(GameTime gameTime)
        {
            GameplayPreviewScreen?.Update(gameTime);
            HandlePreviewTrackCompletion();
            UpdateHeaderToggle();
            Container?.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#202020"));
            Container?.Draw(gameTime);

            if (MenuBar != null && HeaderVisible.Value && !SkinScreen.Exiting)
            {
                MenuBar.Draw(gameTime);
                GameBase.Game.TryEndBatch();
            }
            else
            {
                Button.IsGloballyClickable = true;
            }

            if (HeaderVisible.Value)
                HeaderToggle?.Draw(gameTime);
        }

        public override void Destroy()
        {
            DestroyGameplayPreview();
            MenuBar?.Destroy();
            Container?.Destroy();
        }

        private void CreateBackground()
        {
            new Sprite
            {
                Parent = Container,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
                Tint = ColorHelper.HexToColor("#202020")
            };
        }

        private void RefreshLayout()
        {
            CommitFieldValues();

            TabControl?.Destroy();
            TabControl = null;

            HeaderPanel?.Destroy();
            HeaderPanel = null;
            Inspector = null;
            LoadedSkinText = null;
            NewFolderName = null;
            Fields.Clear();

            PreviewPanel?.Destroy();
            PreviewPanel = null;
            PreviewSurface = null;

            CreatePreviewPanel();
            RebuildPreview();
            CreateHeaderToggle();
        }

        private void CreateHeaderToggle()
        {
            if (HeaderToggle == null || HeaderToggle.IsDisposed)
            {
                HeaderToggle = new ImageButton(FontAwesome.Get(FontAwesomeIcon.fa_eye_with_a_diagonal_line_interface_symbol_for_invisibility),
                    (sender, args) => SetHeaderVisible(!HeaderVisible.Value))
                {
                    Parent = Container,
                    Alignment = Alignment.TopRight,
                    Tint = Color.White,
                    Depth = -1000
                };
            }

            HeaderToggle.Alignment = Alignment.TopRight;
            HeaderToggle.Size = new ScalableVector2(ToggleSize, ToggleSize);
            HeaderToggle.X = -Padding;
            HeaderToggle.Y = HeaderVisible.Value ? 10 : Padding;
            HeaderToggle.Tint = Color.White;
            HeaderToggle.Depth = -1000;
            UpdateHeaderToggle();
            BringHeaderToggleToFront();
        }

        private void UpdateHeaderToggle()
        {
            if (HeaderToggle == null || HeaderToggle.IsDisposed)
                return;

            HeaderToggle.Visible = true;
            HeaderToggle.IsClickable = true;
            HeaderToggle.Y = HeaderVisible.Value ? 10 : Padding;
            HeaderToggle.Image = HeaderVisible.Value
                ? FontAwesome.Get(FontAwesomeIcon.fa_eye_with_a_diagonal_line_interface_symbol_for_invisibility)
                : FontAwesome.Get(FontAwesomeIcon.fa_eye_open);
        }

        private void BringHeaderToggleToFront()
        {
            if (HeaderToggle == null || HeaderToggle.Parent == null)
                return;

            HeaderToggle.DestroyIfParentIsNull = false;
            HeaderToggle.Parent = null;
            HeaderToggle.Parent = Container;
            HeaderToggle.DestroyIfParentIsNull = true;
        }

        internal string LoadedSkinName => LoadedSkin;

        internal string NewSkinFolderName
        {
            get => NewFolderNameValue;
            set => NewFolderNameValue = value;
        }

        internal bool IsHeaderVisible => HeaderVisible.Value;

        internal SkinEditorPreviewScreen SelectedPreviewScreen => SelectedPreview;

        internal void SetHeaderVisible(bool visible)
        {
            HeaderVisible.Value = visible;
            Button.IsGloballyClickable = true;
            UpdateHeaderToggle();
        }

        internal void SelectPreview(SkinEditorPreviewScreen preview)
        {
            CommitFieldValues();
            SelectedPreview = preview;

            if (SelectedPreview == SkinEditorPreviewScreen.Gameplay)
                EnsureSelectedMapTrackPlaying();

            RefreshLayout();
        }

        internal IReadOnlyList<SkinEditorProperty> GetCurrentProperties()
        {
            var properties = GetVisibleProperties().ToList();
            EnsurePropertyValues(properties);
            return properties;
        }

        internal string GetPropertyValue(SkinEditorProperty property) =>
            Values.TryGetValue(property, out var value) ? value : property.DefaultValue;

        internal void SetPropertyValue(SkinEditorProperty property, string value)
        {
            Values[property] = value;

            if (SelectedPreview == SkinEditorPreviewScreen.Gameplay)
                ApplyGameplayPreviewValues();

            RebuildPreview();
        }

        internal string GetPreviewDisplayName(SkinEditorPreviewScreen preview) => GetPreviewName(preview);

        internal void CreateSkin() => OnCreateSkin(this, EventArgs.Empty);

        internal void SaveSkin() => OnSaveSkin(this, EventArgs.Empty);

        internal void UseAndReloadSkin() => OnUseAndReloadSkin(this, EventArgs.Empty);

        internal void OpenSkinFolder() => OnOpenFolder(this, EventArgs.Empty);

        internal void ExitToMenu() => SkinScreen.ExitToMenu();

        internal void SwitchToSkin(string skin)
        {
            TryRunEditorAction(() =>
            {
                CommitFieldValues();
                LoadedSkin = skin;
                Values = SkinEditorManager.LoadPropertyValues(LoadedSkin);
                NewFolderNameValue = SkinEditorManager.MakeUniqueFolderNameForDisplay($"{LoadedSkin} Copy");
                SkinEditorManager.SelectAndReload(LoadedSkin);
                RefreshLayout();
                NotificationManager.Show(NotificationLevel.Success, $"Switched to skin: {LoadedSkin}");
            });
        }

        private void CreateHeader()
        {
            HeaderPanel = new Sprite
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                Image = UserInterface.EditorPanelBackground,
                Size = new ScalableVector2(WindowManager.Width, HeaderHeight),
                Tint = Color.White
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Skin Editor".ToUpper(), 24)
            {
                Parent = HeaderPanel,
                Alignment = Alignment.MidLeft,
                X = 18,
                Y = -HeaderHeight / 2f + HeaderTitleHeight / 2f,
                Tint = Color.White
            };

            LoadedSkinText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 17)
            {
                Parent = HeaderPanel,
                Alignment = Alignment.TopRight,
                X = -Padding - ToggleSize - ControlSpacing,
                Y = 16,
                Tint = Colors.MainAccent
            };

            UpdateLoadedSkinText();

            new Sprite
            {
                Parent = HeaderPanel,
                Alignment = Alignment.TopLeft,
                Y = HeaderTitleHeight,
                Size = new ScalableVector2(WindowManager.Width, 1),
                Tint = ColorHelper.HexToColor("#101010")
            };

            var left = Padding;
            AddModeButton("Skin Info", SkinEditorPreviewScreen.SkinInfo, ref left);
            AddModeButton("Main Menu", SkinEditorPreviewScreen.MainMenu, ref left);
            AddModeButton("Song Select", SkinEditorPreviewScreen.SongSelect, ref left);
            AddModeButton("Gameplay", SkinEditorPreviewScreen.Gameplay, ref left);
            AddModeButton("Results", SkinEditorPreviewScreen.Results, ref left);

            var right = WindowManager.Width - Padding - ToggleSize - ControlSpacing;
            AddRightAlignedAction("Open Folder", FontAwesome.Get(FontAwesomeIcon.fa_open_folder), OnOpenFolder, 128,
                ref right);
            AddRightAlignedAction("Use & Reload", FontAwesome.Get(FontAwesomeIcon.fa_refresh_arrow),
                OnUseAndReloadSkin, 142, ref right);
            AddRightAlignedAction("Save", FontAwesome.Get(FontAwesomeIcon.fa_save_file_option), OnSaveSkin, 86,
                ref right);
            AddRightAlignedAction("Home", FontAwesome.Get(FontAwesomeIcon.fa_home),
                (sender, args) => SkinScreen.ExitToMenu(), 82, ref right);

            var createWidth = 124;
            var folderWidth = 210;
            right -= ControlSpacing;

            var create = CreateIconTextActionButton("Create New", FontAwesome.Get(FontAwesomeIcon.fa_plus_black_symbol),
                OnCreateSkin, createWidth);
            create.Parent = HeaderPanel;
            create.Alignment = Alignment.TopRight;
            create.X = -(WindowManager.Width - right);
            create.Y = 52;

            right -= createWidth + ControlSpacing;

            NewFolderName = new LabelledTextbox(folderWidth, "New Skin Folder", 12, 30, 12, 3,
                "Folder name", NewFolderNameValue)
            {
                Parent = HeaderPanel,
                Alignment = Alignment.TopRight,
                X = -(WindowManager.Width - right),
                Y = 46,
                Tint = Color.Transparent,
                Textbox =
                {
                    AllowSubmission = false,
                    MaxCharacters = 128
                }
            };
        }

        private void CreateInspector()
        {
            var y = HeaderTitleHeight + ControlHeight + 16;
            var height = HeaderHeight - y - Padding;
            var width = WindowManager.Width - Padding * 2 - ToggleSize - ControlSpacing;

            Inspector = new ScrollContainer(new ScalableVector2(width, height), new ScalableVector2(width, 0))
            {
                Parent = HeaderPanel,
                Alignment = Alignment.TopLeft,
                X = Padding,
                Y = y,
                Tint = ColorHelper.HexToColor("#202020"),
                Scrollbar =
                {
                    Width = 4,
                    X = -4,
                    Tint = ColorHelper.HexToColor("#636363")
                },
                EasingType = Easing.OutQuint,
                TimeToCompleteScroll = 700,
                ScrollSpeed = 260
            };

            Inspector.AddBorder(ColorHelper.HexToColor("#303030"), 2);
        }

        private void CreatePreviewPanel()
        {
            PreviewPanel = new Sprite
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(0, 0),
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
                Tint = ColorHelper.HexToColor("#151515")
            };
        }

        private void AddModeButton(string text, SkinEditorPreviewScreen mode, ref float x)
        {
            var button = CreateActionButton(text, (sender, args) =>
            {
                CommitFieldValues();
                SelectedPreview = mode;
                if (SelectedPreview == SkinEditorPreviewScreen.Gameplay)
                    EnsureSelectedMapTrackPlaying();
                RefreshLayout();
            }, 112);

            button.Alpha = mode == SelectedPreview ? 0.75f : 0;
            button.Tint = ColorHelper.HexToColor("#2F2F2F");
            button.Text.Tint = mode == SelectedPreview ? Colors.MainAccent : ColorHelper.HexToColor("#D7D7D7");

            button.Parent = HeaderPanel;
            button.Alignment = Alignment.TopLeft;
            button.X = x;
            button.Y = 52;
            x += button.Width + ControlSpacing;
        }

        private void AddRightAlignedAction(string text, Texture2D icon, EventHandler onClick, float width, ref float right)
        {
            var button = CreateIconTextActionButton(text, icon, onClick, width);
            button.Parent = HeaderPanel;
            button.Alignment = Alignment.TopRight;
            button.X = -(WindowManager.Width - right);
            button.Y = 52;
            right -= width + ControlSpacing;
        }

        private void RebuildInspector()
        {
            CommitFieldValues();
            Fields.Clear();

            if (Inspector == null)
                return;

            for (var i = Inspector.ContentContainer.Children.Count - 1; i >= 0; i--)
                Inspector.ContentContainer.Children[i].Destroy();

            TabControl?.Destroy();
            TabControl = new TextboxTabControl(new List<Textbox>()) { Parent = HeaderPanel };

            var properties = GetVisibleProperties().ToList();
            EnsurePropertyValues(properties);

            var columns = Math.Max(1, (int)((Inspector.Width - Padding * 2 + FieldSpacing) / (FieldWidth + FieldSpacing)));
            var rows = Math.Max(1, (int)Math.Ceiling(properties.Count / (float)columns));

            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                var column = i % columns;
                var row = i / columns;
                var value = Values.TryGetValue(property, out var existing) ? existing : property.DefaultValue;
                var field = new LabelledTextbox(FieldWidth, property.Label, 12, 30, 12, 3,
                    property.Placeholder, value)
                {
                    X = Padding + column * (FieldWidth + FieldSpacing),
                    Y = FieldSpacing + row * (FieldHeight + FieldSpacing),
                    Tint = Color.Transparent,
                    Textbox =
                    {
                        AllowSubmission = false,
                        MaxCharacters = 256
                    }
                };

                field.Textbox.OnStoppedTyping += _ =>
                {
                    Values[property] = field.Textbox.RawText;
                    if (SelectedPreview == SkinEditorPreviewScreen.Gameplay)
                        ApplyGameplayPreviewValues();
                    RebuildPreview();
                };

                Fields[property] = field;
                TabControl.AddTextbox(field.Textbox);
                Inspector.AddContainedDrawable(field);
            }

            Inspector.ContentContainer.Height = Math.Max(Inspector.Height, rows * (FieldHeight + FieldSpacing) + FieldSpacing);
            Inspector.ScrollTo(0, 0);
        }

        private void AddInspectorHeader(string text, ref float totalHeight)
        {
            var header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), text, 24)
            {
                Position = new ScalableVector2(Padding, totalHeight),
                Tint = Colors.MainAccent
            };

            Inspector.AddContainedDrawable(header);
            totalHeight += header.Height + 18;
        }

        private void RebuildPreview()
        {
            CommitFieldValues();

            DestroyGameplayPreview();
            PreviewSurface?.Destroy();
            PreviewSurface = new Sprite
            {
                Parent = PreviewPanel,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(PreviewPanel.Width, PreviewPanel.Height),
                Tint = ColorHelper.HexToColor("#101010")
            };

            switch (SelectedPreview)
            {
                case SkinEditorPreviewScreen.SkinInfo:
                    DrawSkinInfoPreview();
                    break;
                case SkinEditorPreviewScreen.MainMenu:
                    DrawMainMenuPreview();
                    break;
                case SkinEditorPreviewScreen.SongSelect:
                    DrawSongSelectPreview();
                    break;
                case SkinEditorPreviewScreen.Gameplay:
                    DrawGameplayPreview();
                    break;
                case SkinEditorPreviewScreen.Results:
                    DrawResultsPreview();
                    break;
            }
        }

        private void DrawSkinInfoPreview()
        {
            var name = Get("General", "Name", "Untitled Skin");
            var author = Get("General", "Author", "Unknown");
            var version = Get("General", "Version", "v1.0");

            AddText(name, 44, 42, 64, Color.White);
            AddText($"by {author}", 24, 48, 122, Colors.MainAccent);
            AddText($"Version {version}", 18, 48, 164, ColorHelper.HexToColor("#CFCFCF"));

            AddPreviewCard(48, 224, PreviewSurface.Width - 96, 170, ColorHelper.HexToColor("#242424"));
            AddText("Create a new skin folder, switch preview surfaces, edit values, then save or use & reload.",
                20, 78, 270, Color.White);
        }

        private void DrawMainMenuPreview()
        {
            var skin = SkinManager.Skin?.MainMenu;
            var buttonColor = ReadColor("MainMenu", "NavigationButtonTextColor", Color.White);
            var quitColor = ReadColor("MainMenu", "NavigationQuitButtonTextColor", ColorHelper.HexToColor("#F9645D"));
            var borderColor = ReadColor("MenuBorder", "ButtonTextColor", Color.White);
            var borderHover = ReadColor("MenuBorder", "ButtonTextHoveredColor", Colors.MainAccent);
            var lineColor = ReadColor("MenuBorder", "BackgroundLineColor", Colors.MainBlue);
            var accent = ReadColor("MenuBorder", "ForegroundLineColor", Color.White);
            var visualizerColor = ReadColor("MainMenu", "AudioVisualizerColor", Colors.MainBlue);
            var visualizerOpacity = ReadFloat("MainMenu", "AudioVisualizerOpacity", 0.85f);
            var hoverAlpha = ReadFloat("MainMenu", "NavigationButtonHoveredAlpha", 0.35f);

            AddPreviewImageOrCard(skin?.Background, 0, 0, PreviewSurface.Width, PreviewSurface.Height,
                ColorHelper.HexToColor("#202020"));
            AddTopBottomBars(lineColor, accent, borderColor, borderHover);

            AddPreviewImageOrCard(skin?.LogoBackground, 38, 70, 300, 66, ColorHelper.HexToColor("#232323"));
            AddText("QUAVER", 38, 54, 84, Color.White);

            var labels = new[] { "Single Player", "Multiplayer", "Editor", "Skin Editor", "Options", "Quit Game" };
            for (var i = 0; i < labels.Length; i++)
            {
                var y = 154 + i * 54;
                var buttonTexture = i == 0
                    ? skin?.NavigationButtonSelected ?? skin?.NavigationButton
                    : skin?.NavigationButton;

                AddPreviewImageOrCard(buttonTexture, 54, y, 310, 40, ColorHelper.HexToColor("#2A2A2A"));
                if (i == 3)
                    AddPreviewImageOrCard(skin?.NavigationButtonHovered, 56, y + 2, 306, 36,
                        new Color(Colors.MainAccent.R, Colors.MainAccent.G, Colors.MainAccent.B) * hoverAlpha,
                        Color.White, hoverAlpha);
                AddText(labels[i], 18, 76, y + 10, i == labels.Length - 1 ? quitColor : buttonColor);
            }

            AddPreviewImageOrCard(skin?.TipPanel, PreviewSurface.Width - 390, 86, 332, 96,
                ColorHelper.HexToColor("#242424"));
            AddText("TIP", 18, PreviewSurface.Width - 360, 106, skin?.TipTitleColor ?? Colors.MainAccent);
            AddText("Try a lower scroll speed for dense streams.", 14, PreviewSurface.Width - 360, 134,
                skin?.TipTextColor ?? Color.White);

            AddPreviewImageOrCard(skin?.NewsPanel, PreviewSurface.Width - 390, 206, 332, 142,
                ColorHelper.HexToColor("#242424"));
            AddText("LATEST", 18, PreviewSurface.Width - 360, 228, skin?.NewsTitleColor ?? Color.White);
            AddText(DateTime.Now.ToString("MMM dd", CultureInfo.InvariantCulture), 14, PreviewSurface.Width - 360, 256,
                skin?.NewsDateColor ?? Colors.MainAccent);
            AddText("Weekend ranking queue is now open.", 13,
                PreviewSurface.Width - 360, 286, skin?.NewsTextColor ?? ColorHelper.HexToColor("#CFCFCF"));

            AddPreviewImageOrCard(skin?.JukeboxOverlay, PreviewSurface.Width - 390, PreviewSurface.Height - 152, 332, 84,
                ColorHelper.HexToColor("#242424"));
            AddText("Now Playing", 14, PreviewSurface.Width - 360, PreviewSurface.Height - 132,
                skin?.JukeboxProgressBarColor ?? Colors.MainAccent);
            AddText(GetSelectedMapTitle(), 13, PreviewSurface.Width - 360, PreviewSurface.Height - 108, Color.White);

            if (skin?.NoteVisualizer != null)
            {
                AddPreviewImage(skin.NoteVisualizer, PreviewSurface.Width - 450, PreviewSurface.Height - 62, 390, 28,
                    visualizerColor, visualizerOpacity);
            }
            else
            {
                for (var i = 0; i < 34; i++)
                {
                    var h = 20 + (i % 7) * 12;
                    AddPreviewCard(PreviewSurface.Width - 40 - i * 14, PreviewSurface.Height - 60 - h, 8, h,
                        new Color(visualizerColor.R, visualizerColor.G, visualizerColor.B) * visualizerOpacity);
                }
            }
        }

        private void DrawSongSelectPreview()
        {
            var skin = SkinManager.Skin?.SongSelect;
            var map = MapManager.Selected.Value;
            var mapset = map?.Mapset;
            var brightness = MathHelper.Clamp(ReadInt("SongSelect", "MapBackgroundBrightness", 15), 0, 100) / 100f;
            var displayBackground = ReadBool("SongSelect", "DisplayMapBackground", true);
            var titleColor = ReadColor("SongSelect", "MapsetPanelSongTitleColor", Color.White);
            var artistColor = ReadColor("SongSelect", "MapsetPanelSongArtistColor", ColorHelper.HexToColor("#0587E5"));
            var creatorColor = ReadColor("SongSelect", "MapsetPanelCreatorColor", Color.White);
            var byColor = ReadColor("SongSelect", "MapsetPanelByColor", ColorHelper.HexToColor("#757575"));
            var hoverAlpha = ReadFloat("SongSelect", "MapsetPanelHoveringAlpha", 0.35f);
            var leaderboardTitle = ReadColor("SongSelect", "LeaderboardTitleColor", Color.White);
            var rankColor = ReadColor("SongSelect", "LeaderboardScoreRankColor", Color.White);
            var ratingColor = ReadColor("SongSelect", "LeaderboardScoreRatingColor", ColorHelper.HexToColor("#E9B736"));

            if (displayBackground)
            {
                if (map != null && BackgroundHelper.Map != map)
                    BackgroundHelper.Load(map);

                AddPreviewImageOrCard(BackgroundHelper.RawTexture, 0, 0, PreviewSurface.Width, PreviewSurface.Height,
                    ColorHelper.HexToColor("#3B4A55"));
                AddPreviewCard(0, 0, PreviewSurface.Width, PreviewSurface.Height, Color.Black * (1 - brightness));
            }
            else
            {
                AddPreviewImageOrCard(UserInterface.TrianglesWallpaper, 0, 0, PreviewSurface.Width, PreviewSurface.Height,
                    ColorHelper.HexToColor("#3B4A55"));
            }

            AddTopBottomBars(Colors.MainBlue, Color.White, Color.White, Colors.MainAccent);

            AddPreviewImageOrCard(skin?.SelectFilterPanelLeft, 34, 86, 260, 82, ColorHelper.HexToColor("#202020"));
            AddPreviewImageOrCard(skin?.SelectFilterPanelRight, PreviewSurface.Width - 410, 58, 360, 62,
                ColorHelper.HexToColor("#202020"));

            AddPreviewImageOrCard(skin?.LeaderboardPanel, 34, 178, 260, PreviewSurface.Height - 286,
                ColorHelper.HexToColor("#202020"));
            AddText("LEADERBOARD", 20, 56, 112, leaderboardTitle);
            for (var i = 0; i < 6; i++)
            {
                var y = 208 + i * 42;
                AddPreviewCard(56, y, 216, 38, i % 2 == 0 ? ColorHelper.HexToColor("#303030") : ColorHelper.HexToColor("#262626"));
                AddText($"#{i + 1}", 16, 70, y + 9, rankColor);
                AddText($"{24.50 - i:0.00}", 16, 210, y + 9, ratingColor);
            }

            AddPreviewImageOrCard(skin?.PersonalBestPanel, 34, PreviewSurface.Height - 92, 260, 50,
                ColorHelper.HexToColor("#202020"));
            AddText("PERSONAL BEST", 16, 56, PreviewSurface.Height - 78, skin?.PersonalBestTitleColor ?? Color.White);
            AddText("No score yet", 14, 56, PreviewSurface.Height - 56, skin?.NoPersonalBestColor ?? ColorHelper.HexToColor("#CFCFCF"));

            if (mapset != null && !BackgroundHelper.MapsetBanners.ContainsKey(mapset.Directory))
                BackgroundHelper.LoadMapsetBanner(mapset);

            var rightX = PreviewSurface.Width - 410;
            var maps = mapset?.Maps ?? (map != null ? new List<Map> { map } : new List<Map>());
            var selectedIndex = Math.Max(0, maps.IndexOf(map));

            for (var i = 0; i < 5; i++)
            {
                var item = maps.Count == 0 ? null : maps[(selectedIndex + i) % maps.Count];
                var y = 140 + i * 76;
                var selected = item == map || i == 0;
                var texture = selected ? skin?.MapsetSelected : skin?.MapsetDeselected;

                AddPreviewImageOrCard(texture, rightX, y, 360, 60, ColorHelper.HexToColor("#2A2A2A"));
                if (i == 1)
                    AddPreviewImageOrCard(skin?.MapsetHovered, rightX + 2, y + 2, 356, 56,
                        new Color(Colors.MainAccent.R, Colors.MainAccent.G, Colors.MainAccent.B) * hoverAlpha,
                        Color.White, hoverAlpha);

                var banner = item?.Mapset != null && BackgroundHelper.MapsetBanners.TryGetValue(item.Mapset.Directory, out var tex)
                    ? tex
                    : UserInterface.DefaultBanner;
                AddPreviewImageOrCard(banner, rightX + 8, y + 8, 72, 44, ColorHelper.HexToColor("#1A1A1A"));
                AddPreviewImage(skin?.MapsetBannerMask ?? UserInterface.MapsetBannerMask, rightX + 8, y + 8, 72, 44);

                AddPreviewImage(GetModeTexture(item), rightX + 282, y + 8, 28, 20);
                AddPreviewImage(GetRankedStatusTexture(item), rightX + 318, y + 8, 24, 24);

                AddText(item?.Title ?? "Sample Song Title", 15, rightX + 92, y + 8, titleColor);
                AddText(item?.Artist ?? "Artist", 13, rightX + 92, y + 30, artistColor);
                AddText("by", 12, rightX + 240, y + 32, byColor);
                AddText(item?.Creator ?? "Creator", 12, rightX + 262, y + 32, creatorColor);
            }
        }

        private void DrawGameplayPreview()
        {
            ApplyGameplayPreviewValues();

            var map = MapManager.Selected.Value;

            if (map == null)
            {
                AddText("Select a song to preview gameplay.", 22, 44, 78, Color.White);
                return;
            }

            if (GameplayPreviewScreen == null || GameplayPreviewScreen.IsDisposed || GameplayPreviewMap != map)
                CreateGameplayPreviewScreen(map);

            AttachGameplayPreviewScreen();
        }

        private void CreateGameplayPreviewScreen(Map map)
        {
            DestroyGameplayPreview();

            try
            {
                if (BackgroundHelper.Map != map)
                    BackgroundHelper.Load(map);

                if (AudioEngine.Track == null || AudioEngine.Track.IsDisposed)
                    AudioEngine.LoadCurrentTrack();

                var qua = map.LoadQua();
                map.Qua = qua;
                map.Qua.ApplyMods(ModManager.Mods);

                var scores = map.Scores.Value ?? ScoreDatabaseCache.FetchMapScores(map.Md5Checksum);
                map.Scores.Value = scores;

                var md5 = map.GetAlternativeMd5();
                var replay = Replay.GeneratePerfectReplayKeys(
                    new Replay(qua.Mode, ConfigManager.Username.Value, ModManager.Mods, md5), qua);

                GameplayPreviewScreen = new GameplayScreen(qua, md5, scores, replay,
                    isPlayTesting: false,
                    playTestTime: 0,
                    isCalibratingOffset: false,
                    spectatorClient: null,
                    options: null,
                    isSongSelectPreview: false,
                    isTestPlayingInNewEditor: false,
                    useExistingAudioTime: false,
                    shouldShowEpilepsyWarning: false,
                    isSkinEditorPreview: true);

                GameplayPreviewScreen.SkinEditorPreviewCompleted += OnGameplayPreviewCompleted;
                GameplayPreviewMap = map;
            }
            catch (Exception e)
            {
                DestroyGameplayPreview();
                NotificationManager.Show(NotificationLevel.Error, $"Could not load gameplay preview: {e.Message}");
            }
        }

        private void AttachGameplayPreviewScreen()
        {
            if (GameplayPreviewScreen == null || PreviewSurface == null)
                return;

            GameplayPreviewViewport = new Container
            {
                Parent = PreviewSurface,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height)
            };

            var scale = Math.Min(PreviewSurface.Width / WindowManager.Width, PreviewSurface.Height / WindowManager.Height);
            GameplayPreviewViewport.Scale = new Vector2(scale, scale);

            var gameplayView = (GameplayScreenView)GameplayPreviewScreen.View;
            var background = gameplayView.Background;
            background.Parent = GameplayPreviewViewport;
            background.Alignment = Alignment.TopLeft;

            var playfield = GameplayPreviewScreen.Ruleset.Playfield.Container;
            playfield.Parent = GameplayPreviewViewport;

            gameplayView.Container.Parent = GameplayPreviewViewport;
            gameplayView.Container.Alignment = Alignment.TopLeft;
            gameplayView.Container.Position = new ScalableVector2(0, 0);
            gameplayView.Container.Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
        }

        private void DestroyGameplayPreview()
        {
            if (GameplayPreviewScreen != null)
            {
                GameplayPreviewScreen.SkinEditorPreviewCompleted -= OnGameplayPreviewCompleted;
                GameplayPreviewScreen.Destroy();
                GameplayPreviewScreen = null;
            }

            GameplayPreviewMap = null;
            GameplayPreviewViewport = null;
            GameplayPreviewCompletionPending = false;
        }

        private void OnGameplayPreviewCompleted(object sender, EventArgs args)
        {
            GameplayPreviewCompletionPending = true;
        }

        private void HandlePreviewTrackCompletion()
        {
            if (LoadingNextPreviewTrack)
                return;

            if (GameplayPreviewCompletionPending)
            {
                GameplayPreviewCompletionPending = false;
                SelectNextPreviewTrack();
                return;
            }

            if (SelectedPreview == SkinEditorPreviewScreen.Gameplay || MapManager.Mapsets.Count == 0)
                return;

            var track = AudioEngine.Track;
            if (track == null || track.IsDisposed || track.HasPlayed && track.IsStopped)
                SelectNextPreviewTrack();
        }

        private void SelectNextPreviewTrack()
        {
            if (LoadingNextPreviewTrack || MapManager.Mapsets.Count == 0)
                return;

            try
            {
                LoadingNextPreviewTrack = true;
                EnsurePreviewTrackQueueSeeded();

                if (PreviewTrackQueuePosition == PreviewTrackQueue.Count - 1)
                {
                    var mapsets = MapManager.Mapsets.Where(x => x.Maps.Count != 0).ToList();

                    if (mapsets.Count == 0)
                        return;

                    var randomSet = PreviewTrackRandom.Next(0, mapsets.Count);
                    var randomMap = PreviewTrackRandom.Next(0, mapsets[randomSet].Maps.Count);
                    MapManager.Selected.Value = mapsets[randomSet].Maps[randomMap];
                    PreviewTrackQueue.Add(MapManager.Selected.Value);
                }
                else
                {
                    MapManager.Selected.Value = PreviewTrackQueue[PreviewTrackQueuePosition + 1];
                }

                PreviewTrackQueuePosition++;

                if (SelectedPreview == SkinEditorPreviewScreen.Gameplay)
                    RefreshLayout();
                else
                {
                    EnsureSelectedMapTrackPlaying();
                    RebuildPreview();
                }
            }
            finally
            {
                LoadingNextPreviewTrack = false;
            }
        }

        private void EnsurePreviewTrackQueueSeeded()
        {
            var current = MapManager.Selected.Value;

            if (current == null)
                return;

            if (PreviewTrackQueue.Count == 0)
            {
                PreviewTrackQueue.Add(current);
                PreviewTrackQueuePosition = 0;
                return;
            }

            if (PreviewTrackQueuePosition >= 0 && PreviewTrackQueuePosition < PreviewTrackQueue.Count &&
                PreviewTrackQueue[PreviewTrackQueuePosition] == current)
                return;

            var existing = PreviewTrackQueue.IndexOf(current);

            if (existing != -1)
            {
                PreviewTrackQueuePosition = existing;
                return;
            }

            PreviewTrackQueue.Add(current);
            PreviewTrackQueuePosition = PreviewTrackQueue.Count - 1;
        }

        private void DrawResultsPreview()
        {
            var skin = SkinManager.Skin?.Results;
            var filterAlpha = ReadFloat("Results", "ResultsBackgroundFilterAlpha", 0.35f);
            var type = Get("Results", "ResultsBackgroundType", "Header");

            AddPreviewImageOrCard(skin?.ResultsBackground, 0, 0, PreviewSurface.Width, PreviewSurface.Height,
                ColorHelper.HexToColor("#26323C"));
            AddTopBottomBars(Colors.MainBlue, Color.White, Color.White, Colors.MainAccent);

            if (type.Equals("Background", StringComparison.OrdinalIgnoreCase) ||
                type.Equals("Full", StringComparison.OrdinalIgnoreCase))
                AddPreviewImageOrCard(skin?.ResultsBackgroundFilter, 0, 0, PreviewSurface.Width, PreviewSurface.Height,
                    Color.Black * filterAlpha, Color.White, filterAlpha);
            else
                AddPreviewImageOrCard(skin?.ResultsBackgroundFilter, 0, 58, PreviewSurface.Width, 130,
                    Color.Black * filterAlpha, Color.White, filterAlpha);

            AddText("RESULTS", 34, 54, 94, Color.White);
            AddText("S", 76, PreviewSurface.Width - 160, 82, Colors.MainAccent);

            AddPreviewImageOrCard(skin?.ResultsAvatarBorder, 54, 204, 86, 86, ColorHelper.HexToColor("#242424"));
            AddPreviewImageOrCard(skin?.ResultsAvatarMask, 64, 214, 66, 66, ColorHelper.HexToColor("#3A3A3A"));

            AddPreviewImageOrCard(skin?.ResultsTabSelectorBackground, 168, 214, 280, 46,
                ColorHelper.HexToColor("#242424"));
            AddText("Overview", 18, 194, 226, Colors.MainAccent);
            AddText("Performance", 18, 314, 226, ColorHelper.HexToColor("#CFCFCF"));

            AddPreviewImageOrCard(skin?.ResultsScoreContainerPanel, 54, 318, 260, 94, ColorHelper.HexToColor("#242424"));
            if (skin?.ResultsLabelAccuracy != null)
                AddPreviewImage(skin.ResultsLabelAccuracy, 78, 340, 106, 18);
            else
                AddText("Accuracy", 18, 78, 340, ColorHelper.HexToColor("#CFCFCF"));
            AddText("98.74%", 30, 78, 366, Color.White);

            AddPreviewImageOrCard(skin?.ResultsScoreContainerPanel, 340, 318, 260, 94, ColorHelper.HexToColor("#242424"));
            if (skin?.ResultsLabelMaxCombo != null)
                AddPreviewImage(skin.ResultsLabelMaxCombo, 364, 340, 116, 18);
            else
                AddText("Max Combo", 18, 364, 340, ColorHelper.HexToColor("#CFCFCF"));
            AddText("1247x", 30, 364, 366, Color.White);

            AddPreviewImageOrCard(skin?.ResultsGraphContainerPanel, 54, 444, PreviewSurface.Width - 108, 112,
                ColorHelper.HexToColor("#242424"));
            AddPreviewCard(78, 510, PreviewSurface.Width - 156, 4, Colors.MainAccent);
        }

        private void AddTopBottomBars(Color lineColor, Color accentColor, Color textColor, Color hoverColor)
        {
            var border = SkinManager.Skin?.MenuBorder;

            AddPreviewImageOrCard(border?.Background, 0, 0, PreviewSurface.Width, 42, ColorHelper.HexToColor("#181818"));
            AddPreviewImageOrCard(border?.BackgroundFooter, 0, PreviewSurface.Height - 42, PreviewSurface.Width, 42,
                ColorHelper.HexToColor("#181818"));
            AddPreviewCard(0, 40, PreviewSurface.Width, 2, lineColor);
            AddPreviewCard(0, PreviewSurface.Height - 42, PreviewSurface.Width, 2, lineColor);
            AddPreviewCard(PreviewSurface.Width - 180, 40, 120, 2, accentColor);
            AddText("Home", 14, 28, 12, textColor);
            AddText("Skins", 14, 94, 12, hoverColor);
        }

        private Sprite AddPreviewCard(float x, float y, float width, float height, Color color)
        {
            return new Sprite
            {
                Parent = PreviewSurface,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(x, y),
                Size = new ScalableVector2(width, height),
                Tint = color
            };
        }

        private Sprite AddPreviewImage(Texture2D image, float x, float y, float width, float height) =>
            AddPreviewImage(image, x, y, width, height, Color.White);

        private Sprite AddPreviewImage(Texture2D image, float x, float y, float width, float height, Color tint,
            float alpha = 1f)
        {
            if (image == null || image.IsDisposed)
                return null;

            return new Sprite
            {
                Parent = PreviewSurface,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(x, y),
                Size = new ScalableVector2(width, height),
                Image = image,
                Tint = tint,
                Alpha = alpha
            };
        }

        private Sprite AddPreviewImageOrCard(Texture2D image, float x, float y, float width, float height,
            Color fallbackColor) =>
            AddPreviewImageOrCard(image, x, y, width, height, fallbackColor, Color.White);

        private Sprite AddPreviewImageOrCard(Texture2D image, float x, float y, float width, float height,
            Color fallbackColor, Color tint, float alpha = 1f)
        {
            var sprite = AddPreviewImage(image, x, y, width, height, tint, alpha);
            return sprite ?? AddPreviewCard(x, y, width, height, fallbackColor);
        }

        private void AddText(string text, int size, float x, float y, Color color)
        {
            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), text, size)
            {
                Parent = PreviewSurface,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(x, y),
                Tint = color
            };
        }

        private IEnumerable<SkinEditorProperty> GetGameplayProperties()
        {
            var map = MapManager.Selected.Value;
            var qua = GetSelectedQua();
            var mode = map?.Mode ?? qua?.Mode ?? ConfigManager.SelectedGameMode.Value;
            var hasScratch = map?.HasScratchKey ?? qua?.HasScratchKey ?? false;
            var keyCount = ModeHelper.ToKeyCount(mode, hasScratch);
            var section = SkinKeys.ModeShorthand(mode).ToUpper();
            var shorthand = ModeHelper.ToShortHand(mode, hasScratch);
            var keys = GetSkinKeys(mode);

            var defaultColumnSize = keys?.ColumnSize > 0 ? ((int)keys.ColumnSize).ToString() : keyCount <= 4 ? "85" : "58";
            var defaultNotePadding = keys?.NotePadding > 0 ? ((int)keys.NotePadding).ToString() : "0";
            var defaultHitOffset = ((int)(keys?.HitPosOffsetY ?? 0)).ToString();
            var defaultReceptorOffset = ((int)(keys?.ReceptorPosOffsetY ?? 0)).ToString();

            yield return new SkinEditorProperty("Gameplay", section, "DefaultSkin", $"{shorthand} Default Style",
                "Bar, Arrow, or Circle", "Bar");
            yield return new SkinEditorProperty("Gameplay", section, "ColumnSize", $"{shorthand} Column Size",
                "Pixels", defaultColumnSize);
            yield return new SkinEditorProperty("Gameplay", section, "HitPosOffsetY", $"{shorthand} Hit Position Y",
                "Pixels", defaultHitOffset);
            yield return new SkinEditorProperty("Gameplay", section, "ReceptorPosOffsetY", $"{shorthand} Receptor Y",
                "Pixels", defaultReceptorOffset);
            yield return new SkinEditorProperty("Gameplay", section, "NotePadding", $"{shorthand} Note Padding",
                "Pixels", defaultNotePadding);
            yield return new SkinEditorProperty("Gameplay", section, "ColorObjectsBySnapDistance",
                $"{shorthand} Snap Colors", "True or False", (keys?.ColorObjectsBySnapDistance ?? false).ToString());
            yield return new SkinEditorProperty("Gameplay", section, "UseHitObjectSheet",
                $"{shorthand} Object Sheet", "True or False", (keys?.UseHitObjectSheet ?? false).ToString());
            yield return new SkinEditorProperty("Gameplay", section, "RotateHitObjectsByColumn",
                $"{shorthand} Rotate Objects", "True or False", (keys?.RotateHitObjectsByColumn ?? false).ToString());

            for (var lane = 0; lane < keyCount; lane++)
            {
                yield return new SkinEditorProperty("Gameplay", section, $"ColumnColor{lane + 1}",
                    $"{shorthand} Column {lane + 1} Color", "R,G,B", GetDefaultColumnColor(keys, lane));
            }
        }

        private IEnumerable<SkinEditorProperty> GetVisibleProperties()
        {
            if (SelectedPreview == SkinEditorPreviewScreen.Gameplay)
                return GetGameplayProperties();

            var sections = SelectedPreview switch
            {
                SkinEditorPreviewScreen.SkinInfo => new[] { "General" },
                SkinEditorPreviewScreen.MainMenu => new[] { "General", "MenuBorder", "MainMenu" },
                SkinEditorPreviewScreen.SongSelect => new[] { "MenuBorder", "SongSelect" },
                SkinEditorPreviewScreen.Results => new[] { "MenuBorder", "Results" },
                _ => Array.Empty<string>()
            };

            return SkinEditorManager.EditableProperties.Where(x => sections.Contains(x.Section));
        }

        private void EnsurePropertyValues(IReadOnlyCollection<SkinEditorProperty> properties)
        {
            var missing = properties.Where(x => !Values.ContainsKey(x)).ToList();

            if (missing.Count == 0)
                return;

            foreach (var value in SkinEditorManager.LoadPropertyValues(LoadedSkin, missing))
                Values[value.Key] = value.Value;
        }

        private void ApplyGameplayPreviewValues()
        {
            var map = MapManager.Selected.Value;
            var qua = GetSelectedQua();
            var mode = map?.Mode ?? qua?.Mode ?? ConfigManager.SelectedGameMode.Value;
            var keys = GetSkinKeys(mode);

            if (keys == null)
                return;

            var section = SkinKeys.ModeShorthand(mode).ToUpper();
            var values = Values
                .Where(x => x.Key.Section == section)
                .ToDictionary(x => x.Key.Key, x => x.Value);

            keys.ApplySkinEditorPreviewValues(values);
        }

        private Qua GetSelectedQua()
        {
            var map = MapManager.Selected.Value;

            if (map == null)
                return null;

            if (CachedGameplayMap == map && CachedGameplayQua != null)
                return CachedGameplayQua;

            try
            {
                CachedGameplayMap = map;
                CachedGameplayQua = map.Qua ?? map.LoadQua();
            }
            catch (Exception)
            {
                CachedGameplayQua = null;
            }

            return CachedGameplayQua;
        }

        private static void EnsureSelectedMapTrackPlaying()
        {
            var map = MapManager.Selected.Value;

            if (map == null)
                return;

            try
            {
                var trackMissing = AudioEngine.Track == null || AudioEngine.Track.IsDisposed;
                var currentMap = AudioEngine.Map;
                var sameMap = currentMap != null && currentMap.Md5Checksum == map.Md5Checksum;

                if (trackMissing || !sameMap)
                {
                    var oldTrack = AudioEngine.Track;
                    AudioEngine.Track = AudioEngine.LoadMapAudioTrack(map);
                    AudioEngine.Map = map;

                    if (oldTrack != null && !oldTrack.IsDisposed)
                        oldTrack.Dispose();

                    if (map.AudioPreviewTime > 0 && AudioEngine.Track.Length > map.AudioPreviewTime)
                        AudioEngine.Track.Seek(map.AudioPreviewTime);
                }

                if (!AudioEngine.Track.IsPlaying)
                    AudioEngine.Track.Play();
            }
            catch (Exception)
            {
                // Preview audio is best-effort; the visual editor should stay usable without it.
            }
        }

        private static SkinKeys GetSkinKeys(GameMode mode)
        {
            if (SkinManager.Skin?.Keys == null)
                return null;

            return SkinManager.Skin.Keys.TryGetValue(mode, out var keys) ? keys : null;
        }

        private static string GetDefaultColumnColor(SkinKeys keys, int lane)
        {
            if (keys?.ColumnColors != null && keys.ColumnColors.Count > lane && keys.ColumnColors[lane] != Color.Transparent)
                return ToColorValue(keys.ColumnColors[lane]);

            return ToColorValue(lane % 2 == 0 ? Color.White : Colors.MainAccent);
        }

        private static string ToColorValue(Color color) => $"{color.R},{color.G},{color.B}";

        private static string GetSelectedMapTitle()
        {
            var map = MapManager.Selected.Value;
            return map != null ? $"{map.Artist} - {map.Title}" : "No selected map";
        }

        private static Texture2D GetModeTexture(Map map)
        {
            var skin = SkinManager.Skin?.SongSelect;

            if (map == null)
                return skin?.GameModeOther ?? UserInterface.KeysNonePanel;

            var modes = (map.Mapset?.Maps ?? new List<Map> { map })
                .Select(x => x.Mode)
                .Distinct()
                .OrderBy(x => ModeHelper.ToKeyCount(x))
                .ToList();

            var k4 = modes.Count == 1 && modes[0] == GameMode.Keys4;
            var k7 = modes.Count == 1 && modes[0] == GameMode.Keys7;
            var k47 = modes.Count == 2 && modes.Contains(GameMode.Keys4) && modes.Contains(GameMode.Keys7);

            if (k4)
                return skin?.GameMode4K ?? UserInterface.Keys4Panel;

            if (k7)
                return skin?.GameMode7K ?? UserInterface.Keys7Panel;

            if (k47)
                return skin?.GameMode4K7K ?? UserInterface.Keys47Panel;

            return skin?.GameModeOther ?? UserInterface.KeysNonePanel;
        }

        private static Texture2D GetRankedStatusTexture(Map map)
        {
            var skin = SkinManager.Skin?.SongSelect;

            if (map == null)
                return skin?.StatusNotSubmitted ?? UserInterface.StatusNotSubmitted;

            if (map.Game != MapGame.Quaver)
            {
                return map.Game switch
                {
                    MapGame.Osu => skin?.StatusOsu ?? UserInterface.StatusOtherGameOsu,
                    MapGame.Etterna => skin?.StatusStepmania ?? UserInterface.StatusOtherGameEtterna,
                    _ => skin?.StatusVarious ?? UserInterface.StatusVarious
                };
            }

            var status = map.Mapset?.Maps.Max(x => x.RankedStatus) ?? map.RankedStatus;
            return status switch
            {
                RankedStatus.NotSubmitted => skin?.StatusNotSubmitted ?? UserInterface.StatusNotSubmitted,
                RankedStatus.Unranked => skin?.StatusUnranked ?? UserInterface.StatusUnranked,
                RankedStatus.Ranked => skin?.StatusRanked ?? UserInterface.StatusRanked,
                RankedStatus.DanCourse => skin?.StatusNotSubmitted ?? UserInterface.StatusNotSubmitted,
                _ => skin?.StatusVarious ?? UserInterface.StatusVarious
            };
        }

        private void CommitFieldValues()
        {
            if (NewFolderName != null)
                NewFolderNameValue = NewFolderName.Textbox.RawText;

            foreach (var field in Fields)
                Values[field.Key] = field.Value.Textbox.RawText;
        }

        private void OnCreateSkin(object sender, EventArgs args)
        {
            TryRunEditorAction(() =>
            {
                CommitFieldValues();
                LoadedSkin = SkinEditorManager.CreateEditableSkin(NewFolderNameValue, Values);
                Values = SkinEditorManager.LoadPropertyValues(LoadedSkin);
                NewFolderNameValue = SkinEditorManager.MakeUniqueFolderNameForDisplay($"{LoadedSkin} Copy");
                UpdateLoadedSkinText();
                RefreshLayout();
                NotificationManager.Show(NotificationLevel.Success, $"Created skin: {LoadedSkin}");
            });
        }

        private void OnSaveSkin(object sender, EventArgs args)
        {
            TryRunEditorAction(() =>
            {
                CommitFieldValues();
                SkinEditorManager.SaveProperties(LoadedSkin, Values);
                NotificationManager.Show(NotificationLevel.Success, "Skin settings saved!");
            });
        }

        private void OnUseAndReloadSkin(object sender, EventArgs args)
        {
            TryRunEditorAction(() =>
            {
                CommitFieldValues();
                SkinEditorManager.SaveProperties(LoadedSkin, Values);
                SkinEditorManager.SelectAndReload(LoadedSkin);
                NotificationManager.Show(NotificationLevel.Success, "Skin saved and queued for reload!");
            });
        }

        private void OnOpenFolder(object sender, EventArgs args)
        {
            TryRunEditorAction(() =>
            {
                if (string.IsNullOrWhiteSpace(LoadedSkin))
                    throw new InvalidOperationException("Create or select a local skin first.");

                Utils.NativeUtils.OpenNatively(SkinEditorManager.GetSkinDirectory(LoadedSkin));
            });
        }

        private void UpdateLoadedSkinText()
        {
            if (LoadedSkinText == null)
                return;

            LoadedSkinText.Text = string.IsNullOrWhiteSpace(LoadedSkin)
                ? "Previewing default values - create a skin to save"
                : $"Editing: {LoadedSkin}";
        }

        private static TextButton CreateActionButton(string text, EventHandler onClick, float width)
        {
            var button = new TextButton(UserInterface.BlankBox, Fonts.Exo2Medium, text, 14, onClick)
            {
                Size = new ScalableVector2(width, ControlHeight),
                Tint = ColorHelper.HexToColor("#222222"),
                Text = { Tint = Color.White }
            };

            return button;
        }

        private static IconTextButton CreateIconTextActionButton(string text, Texture2D icon, EventHandler onClick,
            float width)
        {
            var button = new IconTextButton(icon, FontManager.GetWobbleFont(Fonts.LatoBlack), text, onClick)
            {
                Size = new ScalableVector2(width, ControlHeight)
            };

            button.Icon.Size = new ScalableVector2(16, 16);
            button.Icon.X = 6;
            button.Text.FontSize = 16;
            button.Text.X = button.Icon.Width + 8;
            return button;
        }

        private static void TryRunEditorAction(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                NotificationManager.Show(NotificationLevel.Error, e.Message);
            }
        }

        private string Get(string section, string key, string fallback)
        {
            var property = Values.Keys.FirstOrDefault(x => x.Section == section && x.Key == key)
                           ?? SkinEditorManager.EditableProperties.FirstOrDefault(x => x.Section == section && x.Key == key);
            return property != null && Values.TryGetValue(property, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : fallback;
        }

        private bool ReadBool(string section, string key, bool fallback) =>
            bool.TryParse(Get(section, key, fallback.ToString()), out var value) ? value : fallback;

        private int ReadInt(string section, string key, int fallback) =>
            int.TryParse(Get(section, key, fallback.ToString()), out var value) ? value : fallback;

        private float ReadFloat(string section, string key, float fallback) =>
            float.TryParse(Get(section, key, fallback.ToString(CultureInfo.InvariantCulture)), NumberStyles.Float,
                CultureInfo.InvariantCulture, out var value) ? value : fallback;

        private Color ReadColor(string section, string key, Color fallback)
        {
            var value = Get(section, key, "");
            var split = value.Split(',');

            if (split.Length < 3)
                return fallback;

            return byte.TryParse(split[0].Trim(), out var r)
                   && byte.TryParse(split[1].Trim(), out var g)
                   && byte.TryParse(split[2].Trim(), out var b)
                ? new Color(r, g, b)
                : fallback;
        }

        private static string GetPreviewName(SkinEditorPreviewScreen mode) => mode switch
        {
            SkinEditorPreviewScreen.SkinInfo => "Skin Info",
            SkinEditorPreviewScreen.MainMenu => "Main Menu",
            SkinEditorPreviewScreen.SongSelect => "Song Select",
            SkinEditorPreviewScreen.Gameplay => $"Gameplay ({GetGameplayPreviewName()})",
            SkinEditorPreviewScreen.Results => "Results",
            _ => mode.ToString()
        };

        private static string GetGameplayPreviewName()
        {
            var map = MapManager.Selected.Value;

            if (map == null)
                return ModeHelper.ToShortHand(ConfigManager.SelectedGameMode.Value);

            return ModeHelper.ToShortHand(map.Mode, map.HasScratchKey);
        }
    }
}
