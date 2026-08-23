using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Form;
using Wobble.Graphics.UI.Navigation;
using Wobble.Graphics.UI.Tooltips;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Platform;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2.SkinEditor
{
    internal sealed class SkinEditorOverlay : Container
    {
        private static readonly Color PanelColor = new Color(18, 26, 37);
        private static readonly Color FieldColor = new Color(32, 44, 58);
        private static readonly Color AccentColor = new Color(31, 187, 255);
        private static readonly Color MutedColor = new Color(160, 175, 193);
        private const double PreviewDebounceMilliseconds = 100;
        private static readonly HashSet<string> LocalizedPropertyNames = new HashSet<string>(
            new[]
            {
                "Type", "SolidColor", "Path", "Fit", "Stops", "AngleDegrees", "X", "Y",
                "RadialRadius", "Effect", "PrimaryColor", "SecondaryColor", "Primary", "Secondary",
                "Tertiary", "Image", "Color", "HoverColor", "AccentColor", "TextColor",
                "BottomOffset", "FallbackImage", "BackgroundColor", "ForegroundColor",
                "OfflineStatusColor", "Scale"
            }, StringComparer.Ordinal);

        private readonly ISkinV2EditorHost host;
        private readonly SkinEditorSession session;
        private readonly Func<bool> save;
        private readonly Action close;
        private readonly Action editMetadata;
        private readonly Action copyWorkshop;
        private readonly bool readOnly;
        private readonly SkinStoreV2Lease skin;
        private readonly HashSet<string> invalidPaths = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> invalidText =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<RoundedButton, Drawable> selectionBindings =
            new Dictionary<RoundedButton, Drawable>();
        private readonly Queue<SkinEditorAssetButton> thumbnailLoadQueue =
            new Queue<SkinEditorAssetButton>();
        private readonly ConcurrentQueue<SkinEditorAssetButton> thumbnailReadyQueue =
            new ConcurrentQueue<SkinEditorAssetButton>();
        private readonly Dictionary<int, SkinEditorAssetButton> assetButtons =
            new Dictionary<int, SkinEditorAssetButton>();
        private readonly SkinEditorColorPicker colorPicker = new SkinEditorColorPicker();

        private Sprite leftPanel;
        private Sprite rightPanel;
        private Sprite assetPanel;
        private FlexContainer targetRoot;
        private Container inspectorRoot;
        private Container inspectorViewport;
        private Container inspectorContent;
        private Sprite inspectorScrollbar;
        private readonly List<Drawable> inspectorDrawables = new List<Drawable>();
        private readonly List<SpriteTextPlus> inspectorErrorTexts = new List<SpriteTextPlus>();
        private IReadOnlyList<SkinEditorProperty> inspectorProperties = Array.Empty<SkinEditorProperty>();
        private Container selectionRoot;
        private ScrollContainer assetScroll;
        private Dropdown folderDropdown;
        private Textbox searchTextbox;
        private SpriteTextPlus assetBreadcrumb;
        private SpriteTextPlus assetStatus;
        private RoundedButton saveButton;
        private SkinEditorTarget selectedTarget;
        private IReadOnlyList<SkinEditorAsset> assets;
        private SkinEditorAsset[] pendingAssetButtons = Array.Empty<SkinEditorAsset>();
        private int assetButtonColumns = 1;
        private int visibleAssetFirstIndex = -1;
        private int visibleAssetLastIndex = -1;
        private int thumbnailGeneration;
        private int thumbnailReadInFlight;
        private Task<IReadOnlyList<SkinEditorAsset>> assetScanTask;
        private CancellationTokenSource assetScanCancellation;
        private List<string> folderValues = new List<string>();
        private string selectedFolder = string.Empty;
        private string search = string.Empty;
        private float lastWidth = -1;
        private float lastHeight = -1;
        private float inspectorOffset;
        private float inspectorViewportHeight;
        private float inspectorBaseContentHeight;
        private float inspectorErrorTop;
        private Container lastInspectorVisibilityContent;
        private float lastInspectorVisibilityOffset = float.NaN;
        private float lastInspectorVisibilityViewportHeight = float.NaN;
        private float lastInspectorVisibilityContentHeight = float.NaN;
        private bool? lastSaveEnabled;
        private bool previewPending;
        private double previewDelay;
        private bool? clickabilityBeforeColorPicker;

        public SkinEditorOverlay(ISkinV2EditorHost host, SkinEditorSession session,
            Func<bool> save, Action close, Action editMetadata, Action copyWorkshop, bool readOnly)
        {
            this.host = host;
            this.session = session;
            this.save = save;
            this.close = close;
            this.editMetadata = editMetadata;
            this.copyWorkshop = copyWorkshop;
            this.readOnly = readOnly;
            skin = SkinManager.AcquireV2();
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            assets = Array.Empty<SkinEditorAsset>();
            BuildChrome();
            BeginAssetRefresh();
            SelectTarget(GetEditableTargets().FirstOrDefault());
            RefreshSaveState();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateColorPickerInputCapture();
            CompleteAssetRefresh();
            Resize();
            BuildNextAssetButtons();
            UpdateInspectorScrolling();
            var folderMenuOpen = folderDropdown?.Opened == true;
            if (folderDropdown?.ItemContainer != null)
            {
                folderDropdown.ItemContainer.InputEnabled =
                    folderMenuOpen && folderDropdown.ItemContainer.IsHovered();
            }
            if (assetScroll != null)
                assetScroll.InputEnabled = !folderMenuOpen && assetScroll.IsHovered();
            UpdateSelectionHitAreas();
            base.Update(gameTime);

            if (previewPending)
            {
                previewDelay -= gameTime.ElapsedGameTime.TotalMilliseconds;
                if (previewDelay <= 0)
                {
                    previewPending = false;
                    ApplyPreview();
                }
            }

            LoadNextAssetThumbnail();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (colorPicker.IsOpen)
                colorPicker.Draw(gameTime);
        }

        public override void Destroy()
        {
            assetScanCancellation?.Cancel();
            assetScanCancellation?.Dispose();
            assetScanCancellation = null;
            assetScanTask = null;
            thumbnailGeneration++;
            Interlocked.Exchange(ref thumbnailReadInFlight, 0);
            thumbnailLoadQueue.Clear();
            while (thumbnailReadyQueue.TryDequeue(out _))
            {
            }
            RestoreButtonClickability();
            colorPicker.Destroy();
            skin.Dispose();
            base.Destroy();
        }

        private void UpdateColorPickerInputCapture()
        {
            if (colorPicker.IsOpen)
            {
                CaptureButtonClickability();
                return;
            }

            RestoreButtonClickability();
        }

        private void CaptureButtonClickability()
        {
            if (!clickabilityBeforeColorPicker.HasValue)
                clickabilityBeforeColorPicker =
                    Wobble.Graphics.UI.Buttons.Button.IsGloballyClickable;

            Wobble.Graphics.UI.Buttons.Button.IsGloballyClickable = false;
        }

        private void RestoreButtonClickability()
        {
            if (!clickabilityBeforeColorPicker.HasValue)
                return;

            Wobble.Graphics.UI.Buttons.Button.IsGloballyClickable =
                clickabilityBeforeColorPicker.Value;
            clickabilityBeforeColorPicker = null;
        }

        public void RefreshSaveState()
        {
            if (saveButton == null)
                return;

            var enabled = !readOnly && session.IsDirty && !session.HasInvalidInput;
            if (lastSaveEnabled == enabled)
                return;

            lastSaveEnabled = enabled;
            saveButton.IsClickable = enabled;
            saveButton.PerformHoverFade = saveButton.IsClickable;
            saveButton.Alpha = saveButton.IsClickable ? 1 : 0.45f;
        }

        private void BuildChrome()
        {
            leftPanel = CreatePanel(this);
            rightPanel = CreatePanel(this);
            assetPanel = CreatePanel(this);

            targetRoot = new FlexContainer { Parent = leftPanel };
            inspectorRoot = new Container { Parent = rightPanel };
            selectionRoot = new Container { Parent = this };

            CreateHeader(leftPanel, LocalizationManager.Get("SkinEditor_Components"), 18, 18);
            CreateHeader(rightPanel, LocalizationManager.Get("SkinEditor_Title"), 18, 18);
            assetBreadcrumb = CreateHeader(assetPanel, LocalizationManager.Get("SkinEditor_Assets"), 18, 14);

            saveButton = CreateButton(rightPanel, LocalizationManager.Get("SkinEditor_Save"),
                SkinEditorController.RightPanelWidth - 198, 12, 82, 34,
                () => save(), new Color(39, 176, 110));
            CreateButton(rightPanel, LocalizationManager.Get("SkinEditor_Close"),
                SkinEditorController.RightPanelWidth - 104, 12, 86, 34,
                close, new Color(249, 100, 93));

            if (readOnly)
            {
                CreateHeader(rightPanel, LocalizationManager.Get("SkinEditor_WorkshopReadOnly"), 18, 62,
                    new Color(255, 190, 80), 14);
                CreateButton(rightPanel, LocalizationManager.Get("SkinEditor_CopyToLocal"),
                    18, 88, SkinEditorController.RightPanelWidth - 36, 36,
                    copyWorkshop, AccentColor);
            }

            var metadataButton = CreateButton(rightPanel,
                LocalizationManager.Get("SkinEditor_Metadata"), 18, readOnly ? 136 : 60,
                SkinEditorController.RightPanelWidth - 36, 36, editMetadata, AccentColor);
            metadataButton.IsClickable = !readOnly;
            metadataButton.PerformHoverFade = !readOnly;
            metadataButton.Alpha = readOnly ? 0.45f : 1;

            BuildTargetList();
            BuildAssetBrowser();
            BuildSelectionButtons();
            Resize(true);
        }

        private void BuildTargetList()
        {
            targetRoot.Destroy();
            targetRoot = new FlexContainer
            {
                Parent = leftPanel,
                Position = new ScalableVector2(12, 58),
                Size = new ScalableVector2(SkinEditorController.LeftPanelWidth - 24,
                    Math.Max(1, WindowManager.Height - SkinEditorController.AssetPanelHeight - 70)),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch,
                RowGap = 6
            };

            var editableTargets = GetEditableTargets();
            AddTargetGroup(host.EditorGroupLabel,
                editableTargets.Where(x =>
                    !x.ConfigPath.StartsWith("Shared.", StringComparison.Ordinal)));
            AddTargetGroup(LocalizationManager.Get("SkinEditor_Group_SharedComponents"),
                editableTargets.Where(x =>
                    x.ConfigPath.StartsWith("Shared.", StringComparison.Ordinal)));
            targetRoot.RefreshLayout();
        }

        private IReadOnlyList<SkinEditorTarget> GetEditableTargets() => host.EditorTargets
            .Where(x => session.GetProperties(x.ConfigPath).Count > 0)
            .ToArray();

        private void AddTargetGroup(string label, IEnumerable<SkinEditorTarget> targets)
        {
            var entries = targets.ToArray();
            if (entries.Length == 0)
                return;

            var header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), label, 13)
            {
                Parent = targetRoot,
                Size = new ScalableVector2(targetRoot.Width, 22),
                Tint = MutedColor
            };
            targetRoot.SetItemOptions(header, new FlexItemOptions { Basis = 22, Grow = 0, Shrink = 0 });

            foreach (var target in entries)
            {
                var captured = target;
                var button = CreateButton(targetRoot, target.Label, 0, 0,
                    targetRoot.Width - 8, 36, () => SelectTarget(captured),
                    selectedTarget?.Id == target.Id ? AccentColor : FieldColor);
                button.SetLabel(FontManager.GetWobbleFont(Fonts.InterSemiBold), target.Label, 14, Color.White);
                targetRoot.SetItemOptions(button, new FlexItemOptions
                {
                    Basis = 36,
                    Grow = 0,
                    Shrink = 0,
                    AlignSelf = FlexAlignSelf.FlexEnd
                });
            }
        }

        private void SelectTarget(SkinEditorTarget target)
        {
            selectedTarget = target;
            session.FocusedAssetProperty = null;
            BuildTargetList();
            BuildInspector();
            BuildSelectionButtons();
        }

        private void BuildInspector()
        {
            colorPicker.Close();
            inspectorRoot.Destroy();
            inspectorRoot = new Container { Parent = rightPanel };
            var top = readOnly ? 184 : 108;
            var viewportHeight = Math.Max(1,
                WindowManager.Height - SkinEditorController.AssetPanelHeight - top - 12);
            inspectorDrawables.Clear();
            inspectorOffset = 0;
            inspectorViewport = new Container
            {
                Parent = inspectorRoot,
                Size = new ScalableVector2(SkinEditorController.RightPanelWidth - 24, viewportHeight),
                X = 12,
                Y = top
            };
            inspectorContent = new Container
            {
                Parent = inspectorViewport,
                Size = inspectorViewport.Size
            };
            inspectorScrollbar = new Sprite
            {
                Parent = inspectorRoot,
                X = SkinEditorController.RightPanelWidth - 6,
                Y = top,
                Width = 3,
                Tint = AccentColor,
                Visible = false
            };

            var y = 4f;
            inspectorErrorTexts.Clear();
            inspectorProperties = Array.Empty<SkinEditorProperty>();
            if (selectedTarget == null)
            {
                AddInspectorText(LocalizationManager.Get("SkinEditor_SelectComponent"), y, MutedColor);
                FinalizeInspector(viewportHeight, y + 24);
                return;
            }

            AddInspectorText(selectedTarget.Label, y, Color.White, 16);
            y += 28;
            var properties = GetVisibleProperties(session.GetProperties(selectedTarget.ConfigPath));
            inspectorProperties = properties;
            if (properties.Count == 0)
            {
                AddInspectorText(LocalizationManager.Get("SkinEditor_NoEditableProperties"), y, MutedColor);
                FinalizeInspector(viewportHeight, y + 24);
                return;
            }

            foreach (var property in properties)
            {
                y = property.IsGradientStops
                    ? AddGradientStops(property, y)
                    : AddProperty(property, y);
            }

            inspectorErrorTop = y + 4;
            FinalizeInspector(viewportHeight, y + 12);
        }

        private void FinalizeInspector(float viewportHeight, float contentHeight)
        {
            inspectorViewportHeight = viewportHeight;
            inspectorBaseContentHeight = contentHeight;
            RefreshInspectorErrors();
        }

        private void RefreshInspectorErrors()
        {
            if (inspectorContent == null)
                return;

            foreach (var errorText in inspectorErrorTexts.ToArray())
            {
                inspectorDrawables.Remove(errorText);
                errorText.Destroy();
            }
            inspectorErrorTexts.Clear();

            var y = inspectorErrorTop;
            foreach (var error in GetInspectorErrors())
            {
                var errorText = AddInspectorText(error, y, new Color(249, 100, 93), 11);
                errorText.MaxWidth = SkinEditorController.RightPanelWidth - 24;
                inspectorErrorTexts.Add(errorText);
                y += errorText.Height + 4;
            }

            inspectorContent.Height = Math.Max(inspectorViewportHeight,
                inspectorErrorTexts.Count == 0 ? inspectorBaseContentHeight : y + 8);
            lastInspectorVisibilityContent = null;
            UpdateInspectorVisibility();
        }

        private IEnumerable<string> GetInspectorErrors()
        {
            foreach (var property in inspectorProperties)
            {
                var hasError = property.IsGradientStops
                    ? invalidPaths.Any(x => x.StartsWith(property.Path + "[", StringComparison.Ordinal))
                    : invalidPaths.Contains(property.Path);
                if (hasError)
                    yield return $"{LocalizeProperty(property)}: {LocalizeValidationError(property)}";
            }
        }

        private void UpdateInspectorScrolling()
        {
            if (inspectorViewport == null || inspectorContent == null)
                return;

            if (inspectorViewport.IsHovered())
            {
                if (MouseManager.IsScrollingUp(false))
                    inspectorOffset -= 48;
                else if (MouseManager.IsScrollingDown(false))
                    inspectorOffset += 48;
            }

            var maximum = Math.Max(0, inspectorContent.Height - inspectorViewport.Height);
            inspectorOffset = MathHelper.Clamp(inspectorOffset, 0, maximum);
            inspectorContent.Y = -inspectorOffset;

            inspectorScrollbar.Visible = maximum > 0;
            if (inspectorScrollbar.Visible)
            {
                inspectorScrollbar.Height = Math.Max(30,
                    inspectorViewport.Height * inspectorViewport.Height / inspectorContent.Height);
                inspectorScrollbar.Y = inspectorViewport.Y +
                                       inspectorOffset / maximum *
                                       (inspectorViewport.Height - inspectorScrollbar.Height);
            }

            UpdateInspectorVisibility();
        }

        private void UpdateInspectorVisibility()
        {
            if (inspectorViewport == null || inspectorContent == null)
                return;

            if (ReferenceEquals(lastInspectorVisibilityContent, inspectorContent) &&
                lastInspectorVisibilityOffset == inspectorContent.Y &&
                lastInspectorVisibilityViewportHeight == inspectorViewport.Height &&
                lastInspectorVisibilityContentHeight == inspectorContent.Height)
                return;

            lastInspectorVisibilityContent = inspectorContent;
            lastInspectorVisibilityOffset = inspectorContent.Y;
            lastInspectorVisibilityViewportHeight = inspectorViewport.Height;
            lastInspectorVisibilityContentHeight = inspectorContent.Height;

            foreach (var drawable in inspectorDrawables)
            {
                var top = drawable.Y + inspectorContent.Y;
                var visible = top >= 0 && top + drawable.Height <= inspectorViewport.Height;
                drawable.Visible = visible;

                if (drawable is SkinEditorTextbox textbox)
                    textbox.Button.IsInteractionEnabled = visible && !readOnly;
                else if (drawable is Wobble.Graphics.UI.Buttons.Button button)
                    button.IsInteractionEnabled = visible;
            }
        }

        private void AddInspectorDrawable(Drawable drawable)
        {
            drawable.UpdateWhenInvisible = false;
            inspectorDrawables.Add(drawable);
        }

        private IReadOnlyList<SkinEditorProperty> GetVisibleProperties(
            IReadOnlyList<SkinEditorProperty> properties)
        {
            var backgroundType = properties.FirstOrDefault(x =>
                x.ValueType == typeof(NavigationBarBackgroundType) &&
                (x.Path == selectedTarget.ConfigPath + ".Type" ||
                 x.Path == selectedTarget.ConfigPath + ".Background.Type"));
            if (backgroundType == null)
                return properties;

            var root = backgroundType.Path.Substring(0,
                backgroundType.Path.Length - ".Type".Length);
            var selectedType = (NavigationBarBackgroundType) backgroundType.GetValue(session.Working);
            return properties.Where(property =>
            {
                if (!property.Path.StartsWith(root + ".", StringComparison.Ordinal) ||
                    property.Path == backgroundType.Path)
                    return true;

                var relative = property.Path.Substring(root.Length + 1);
                if (relative == "SolidColor")
                    return selectedType == NavigationBarBackgroundType.SolidColor;
                if (relative.StartsWith("Image.", StringComparison.Ordinal))
                    return selectedType == NavigationBarBackgroundType.Image;
                if (relative.StartsWith("Gradient.", StringComparison.Ordinal))
                    return selectedType == NavigationBarBackgroundType.Gradient;
                return true;
            }).ToArray();
        }

        private float AddProperty(SkinEditorProperty property, float y)
        {
            AddInspectorText(LocalizeProperty(property), y, MutedColor, 13);
            var controlY = y + 18;
            var value = property.GetValue(session.Working);

            if (property.ValueType == typeof(bool))
            {
                var bindable = new Bindable<bool>((bool) value);
                var checkbox = new QuaverCheckbox(bindable)
                {
                    Parent = inspectorContent,
                    Y = controlY + 5,
                    IsClickable = !readOnly,
                    DisposeBindableOnDestroy = true
                };
                bindable.ValueChanged += (sender, args) =>
                {
                    if (!readOnly)
                        CommitValue(property, args.Value);
                };
                AddInspectorDrawable(checkbox);
                CreateInspectorResetButton(controlY, () => ResetProperty(property));
                return controlY + 42;
            }

            if (property.ValueType.IsEnum || property.IsFont)
            {
                var options = property.ValueType.IsEnum
                    ? Enum.GetNames(property.ValueType).ToList()
                    : new List<string>
                    {
                        Fonts.InterRegular, Fonts.InterMedium, Fonts.InterSemiBold, Fonts.InterBold,
                        Fonts.InterLight, Fonts.InterHeavy, Fonts.InterBlack
                    };
                var selected = Math.Max(0, options.FindIndex(x =>
                    string.Equals(x, Convert.ToString(value, CultureInfo.InvariantCulture),
                        StringComparison.OrdinalIgnoreCase)));
                var selector = CreateInspectorButton(options[selected], controlY, () =>
                {
                    if (readOnly)
                        return;

                    var next = options[(selected + 1) % options.Count];
                    var nextValue = property.ValueType.IsEnum
                        ? Enum.Parse(property.ValueType, next)
                        : (object) next;
                    if (CommitValue(property, nextValue))
                        BuildInspector();
                }, SkinEditorController.RightPanelWidth - 92);
                selector.IsClickable = !readOnly;
                selector.AddTooltip(new TooltipOptions(
                    LocalizationManager.Get("SkinEditor_SelectorTooltip"))
                {
                    Anchor = TooltipAnchor.TopCenter,
                    MaximumWidth = 240
                });
                CreateInspectorResetButton(controlY, () => ResetProperty(property));
                return controlY + 42;
            }

            var displayedValue = invalidText.TryGetValue(property.Path, out var invalidValue)
                ? invalidValue
                : Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            RoundedButton swatch = null;
            var fieldWidth = property.IsAssetPath
                ? SkinEditorController.RightPanelWidth - 190
                : SkinEditorController.RightPanelWidth - 92;
            var textbox = new SkinEditorTextbox(
                new ScalableVector2(fieldWidth, 36),
                FontManager.GetWobbleFont(Fonts.InterMedium), 14,
                displayedValue,
                onApply: text =>
                {
                    CommitText(property, text);
                    if (swatch != null && !invalidPaths.Contains(property.Path))
                        SetColorSwatchValue(swatch, SkinV2Color.Parse(text));
                })
            {
                Parent = inspectorContent,
                Y = controlY,
                Tint = property.IsAssetPath && session.FocusedAssetProperty == property
                    ? AccentColor
                    : FieldColor
            };
            textbox.Button.IsInteractionEnabled = !readOnly;
            if (property.IsAssetPath)
                textbox.Button.Clicked += (sender, args) =>
                {
                    session.FocusedAssetProperty = property;
                    textbox.Tint = AccentColor;
                    BuildAssetButtons();
                };
            AddInspectorDrawable(textbox);

            if (property.IsColor)
            {
                swatch = CreateColorSwatch(textbox,
                    SkinV2Color.Parse(Convert.ToString(value, CultureInfo.InvariantCulture)),
                    picked =>
                    {
                        textbox.SetAppliedText(picked);
                        CommitText(property, picked);
                        if (!invalidPaths.Contains(property.Path))
                            SetColorSwatchValue(swatch, SkinV2Color.Parse(picked));
                    }, property.Path != "Shared.Brand.AccentColor");
            }

            if (property.IsAssetPath)
            {
                var clear = CreateInspectorButton(LocalizationManager.Get("SkinEditor_Clear"), controlY, () =>
                {
                    session.FocusedAssetProperty = property;
                    CommitValue(property, string.Empty);
                    BuildInspector();
                    BuildAssetButtons();
                }, 90, fieldWidth + 8);
                clear.IsClickable = !readOnly;
            }

            CreateInspectorResetButton(controlY, () => ResetProperty(property));
            return controlY + 56;
        }

        private float AddGradientStops(SkinEditorProperty property, float y)
        {
            AddInspectorText(LocalizeProperty(property), y, MutedColor, 13);
            CreateInspectorResetButton(y, () => ResetProperty(property), 28);
            y += 34;
            var stops = ((IEnumerable<SkinV2GradientStopConfig>) property.GetValue(session.Working)).ToList();
            for (var i = 0; i < stops.Count; i++)
            {
                var index = i;
                var stop = stops[i];
                var positionPath = $"{property.Path}[{index}].Position";
                var colorPath = $"{property.Path}[{index}].Color";
                var position = new SkinEditorTextbox(new ScalableVector2(82, 34),
                    FontManager.GetWobbleFont(Fonts.InterMedium), 13,
                    invalidText.TryGetValue(positionPath, out var invalidPosition)
                        ? invalidPosition
                        : stop.Position.ToString(CultureInfo.InvariantCulture),
                    onApply: text =>
                    {
                        if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                        {
                            MarkInvalid(positionPath, text);
                            return;
                        }
                        var edited = CurrentStops(property);
                        edited[index].Position = parsed;
                        CommitGradientValue(property, edited, positionPath, text);
                    })
                {
                    Parent = inspectorContent,
                    X = 0,
                    Y = y,
                    Tint = FieldColor
                };
                position.Button.IsInteractionEnabled = !readOnly;
                AddInspectorDrawable(position);

                var color = new SkinEditorTextbox(new ScalableVector2(150, 34),
                    FontManager.GetWobbleFont(Fonts.InterMedium), 13,
                    invalidText.TryGetValue(colorPath, out var invalidColor) ? invalidColor : stop.Color,
                    onApply: text =>
                    {
                        var edited = CurrentStops(property);
                        edited[index].Color = text;
                        CommitGradientValue(property, edited, colorPath, text);
                    })
                {
                    Parent = inspectorContent,
                    X = 92,
                    Y = y,
                    Tint = FieldColor
                };
                color.Button.IsInteractionEnabled = !readOnly;
                AddInspectorDrawable(color);

                var colorSwatch = CreateColorSwatch(color, SkinV2Color.Parse(stop.Color), picked =>
                {
                    color.SetAppliedText(picked);
                    var edited = CurrentStops(property);
                    edited[index].Color = picked;
                    CommitGradientValue(property, edited, colorPath, picked);
                });
                colorSwatch.IsClickable = !readOnly;

                var remove = CreateInspectorButton("−", y, () =>
                {
                    var edited = CurrentStops(property);
                    if (edited.Count <= 2)
                        return;
                    edited.RemoveAt(index);
                    if (CommitValue(property, edited))
                    {
                        ClearInvalidPrefix(property.Path + "[");
                        BuildInspector();
                    }
                }, 42, 252);
                remove.IsClickable = !readOnly && stops.Count > 2;
                y += 38;
            }

            var add = CreateInspectorButton(LocalizationManager.Get("SkinEditor_AddStop"), y, () =>
            {
                var edited = CurrentStops(property);
                var largest = edited.Zip(edited.Skip(1), (left, right) =>
                        new { Left = left, Right = right, Gap = right.Position - left.Position })
                    .OrderByDescending(x => x.Gap).First();
                edited.Add(new SkinV2GradientStopConfig(
                    (largest.Left.Position + largest.Right.Position) / 2f, largest.Left.Color));
                edited = edited.OrderBy(x => x.Position).ToList();
                if (CommitValue(property, edited))
                {
                    ClearInvalidPrefix(property.Path + "[");
                    BuildInspector();
                }
            });
            add.IsClickable = !readOnly;
            return y + 42;
        }

        private static List<SkinV2GradientStopConfig> CloneStops(
            IEnumerable<SkinV2GradientStopConfig> stops) =>
            stops.Select(x => new SkinV2GradientStopConfig(x.Position, x.Color)).ToList();

        private RoundedButton CreateColorSwatch(Drawable parent, Color value, Action<string> changed,
            bool allowAlpha = true)
        {
            RoundedButton swatch = null;
            swatch = new RoundedButton((sender, args) =>
            {
                CaptureButtonClickability();
                colorPicker.Open(swatch.Tint, picked =>
                {
                    SetColorSwatchValue(swatch, SkinV2Color.Parse(picked));
                    changed(picked);
                }, allowAlpha);
            })
            {
                Parent = parent,
                Alignment = Alignment.MidRight,
                X = -8,
                Size = new ScalableVector2(22, 22),
                Tint = value,
                CornerRadius = 4,
                IsClickable = !readOnly
            };
            swatch.SetIcon(GlobalIcons.Get(GlobalIcon.Edit), new Vector2(14, 14));
            SetColorSwatchValue(swatch, value);
            return swatch;
        }

        private static void SetColorSwatchValue(RoundedButton swatch, Color value)
        {
            swatch.Tint = value;

            var alpha = value.A / 255f;
            var red = value.R * alpha + FieldColor.R * (1 - alpha);
            var green = value.G * alpha + FieldColor.G * (1 - alpha);
            var blue = value.B * alpha + FieldColor.B * (1 - alpha);
            var luminance = red * 0.2126f + green * 0.7152f + blue * 0.0722f;
            swatch.Icon.Tint = luminance >= 150 ? Color.Black : Color.White;
        }

        private List<SkinV2GradientStopConfig> CurrentStops(SkinEditorProperty property) =>
            CloneStops((IEnumerable<SkinV2GradientStopConfig>) property.GetValue(session.Working));

        private void ResetProperty(SkinEditorProperty property)
        {
            if (readOnly)
                return;

            var defaultValue = property.GetValue(session.Defaults);
            if (property.IsGradientStops)
            {
                defaultValue = CloneStops(
                    (IEnumerable<SkinV2GradientStopConfig>) defaultValue);
            }

            ClearInvalidPrefix(property.Path);
            if (!CommitValue(property, defaultValue))
                return;

            BuildInspector();
            if (property.IsAssetPath)
                BuildAssetButtons();
        }

        private void CommitText(SkinEditorProperty property, string text)
        {
            if (readOnly)
                return;

            if (property.TrySetText(session.Working, text, out _))
            {
                invalidPaths.Remove(property.Path);
                invalidText.Remove(property.Path);
                session.RefreshDirtyState();
                QueuePreview();
            }
            else
            {
                invalidPaths.Add(property.Path);
                invalidText[property.Path] = text ?? string.Empty;
            }

            session.HasInvalidInput = invalidPaths.Count > 0;
            RefreshSaveState();
            RefreshInspectorErrors();
        }

        private bool CommitValue(SkinEditorProperty property, object value)
        {
            if (readOnly || !property.TrySetValue(session.Working, value, out _))
                return false;

            invalidPaths.Remove(property.Path);
            invalidText.Remove(property.Path);
            session.RefreshDirtyState();
            session.HasInvalidInput = invalidPaths.Count > 0;
            QueuePreview();
            RefreshSaveState();
            RefreshInspectorErrors();
            return true;
        }

        private void CommitGradientValue(SkinEditorProperty property, object value, string inputPath,
            string rawText)
        {
            if (readOnly)
                return;

            if (!property.TrySetValue(session.Working, value, out _))
            {
                MarkInvalid(inputPath, rawText);
                return;
            }

            invalidPaths.Remove(inputPath);
            invalidText.Remove(inputPath);
            session.RefreshDirtyState();
            session.HasInvalidInput = invalidPaths.Count > 0;
            QueuePreview();
            RefreshSaveState();
            RefreshInspectorErrors();
        }

        private void MarkInvalid(string inputPath, string rawText)
        {
            invalidPaths.Add(inputPath);
            invalidText[inputPath] = rawText ?? string.Empty;
            session.HasInvalidInput = true;
            RefreshSaveState();
            RefreshInspectorErrors();
        }

        private void ClearInvalidPrefix(string prefix)
        {
            foreach (var path in invalidPaths.Where(x => x.StartsWith(prefix, StringComparison.Ordinal)).ToArray())
            {
                invalidPaths.Remove(path);
                invalidText.Remove(path);
            }

            session.HasInvalidInput = invalidPaths.Count > 0;
            RefreshSaveState();
            RefreshInspectorErrors();
        }

        private void QueuePreview()
        {
            previewPending = true;
            previewDelay = PreviewDebounceMilliseconds;
        }

        private void ApplyPreview()
        {
            var selectedId = selectedTarget?.Id;
            host.ApplySkinEditorPreview(session.Working);
            var editableTargets = GetEditableTargets();
            selectedTarget = editableTargets.FirstOrDefault(x => x.Id == selectedId) ??
                             editableTargets.FirstOrDefault();
            BuildTargetList();
            BuildSelectionButtons();
            RefreshSaveState();
        }

        private void BuildSelectionButtons()
        {
            selectionRoot.Destroy();
            selectionRoot = new Container { Parent = this };
            selectionBindings.Clear();
            foreach (var target in GetEditableTargets())
            {
                foreach (var drawable in target.Drawables)
                {
                    var captured = target;
                    var button = new RoundedButton((sender, args) => SelectTarget(captured))
                    {
                        Parent = selectionRoot,
                        // Keep selection hit areas invisible so the editor never tints the preview.
                        Tint = Color.Transparent,
                        Alpha = 0,
                        PerformHoverFade = false,
                        CornerRadius = 3
                    };
                    button.Hovered += (sender, args) =>
                    {
                        if (captured.Id != selectedTarget?.Id)
                            button.Alpha = 0.09f;
                    };
                    button.LeftHover += (sender, args) =>
                    {
                        if (captured.Id != selectedTarget?.Id)
                            button.Alpha = 0;
                    };
                    selectionBindings[button] = drawable;
                }
            }
        }

        private void UpdateSelectionHitAreas()
        {
            foreach (var pair in selectionBindings)
            {
                var button = pair.Key;
                var target = pair.Value;
                if (target.IsDisposed || !target.Visible)
                {
                    if (button.Visible)
                        button.Visible = false;
                    continue;
                }

                if (!button.Visible)
                    button.Visible = true;

                var rectangle = target.ScreenRectangle;
                if (button.ScreenRectangle.X == rectangle.X &&
                    button.ScreenRectangle.Y == rectangle.Y &&
                    button.ScreenRectangle.Width == rectangle.Width &&
                    button.ScreenRectangle.Height == rectangle.Height)
                    continue;

                button.Position = new ScalableVector2(rectangle.X, rectangle.Y);
                button.Size = new ScalableVector2(rectangle.Width, rectangle.Height);
            }
        }

        private void BuildAssetBrowser()
        {
            searchTextbox = new SkinEditorTextbox(new ScalableVector2(250, 34),
                FontManager.GetWobbleFont(Fonts.InterMedium), 14, string.Empty,
                LocalizationManager.Get("SkinEditor_SearchAssets"),
                onApply: text =>
                {
                    search = text ?? string.Empty;
                    BuildAssetButtons();
                })
            {
                Parent = assetPanel,
                X = 100,
                Y = 10,
                Tint = FieldColor
            };

            folderValues = new[] { string.Empty }.Concat(assets.Select(x => x.Folder)
                    .Where(x => !string.IsNullOrEmpty(x)).Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                .ToList();
            var folderLabels = folderValues.Select(x => string.IsNullOrEmpty(x)
                ? LocalizationManager.Get("SkinEditor_AllFolders")
                : x).ToList();
            folderDropdown = new Dropdown(folderLabels, new ScalableVector2(260, 34), 13,
                AccentColor, 0, 230, 180)
            {
                Parent = assetPanel,
                X = 365,
                Y = 10
            };
            folderDropdown.ItemContainer.Scrollbar.Tint = Color.White;
            folderDropdown.ItemSelected += (sender, args) =>
            {
                selectedFolder = folderValues[args.Index];
                RefreshAssetBreadcrumb();
                BuildAssetButtons();
            };

            CreateButton(assetPanel, LocalizationManager.Get("SkinEditor_Refresh"), 640, 10, 88, 34, () =>
            {
                BeginAssetRefresh();
            }, FieldColor);
            CreateButton(assetPanel, LocalizationManager.Get("SkinEditor_OpenFolder"), 738, 10, 120, 34,
                () => Utils.NativeUtils.OpenNatively(SkinManager.SkinV2.RootDirectory), FieldColor);
            assetStatus = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterMedium), string.Empty, 13)
            {
                Parent = assetPanel,
                X = 874,
                Y = 18,
                Tint = MutedColor,
                Visible = false
            };
            RefreshAssetBreadcrumb();
            BuildAssetButtons();
        }

        private void BeginAssetRefresh()
        {
            assetScanCancellation?.Cancel();
            assetScanCancellation?.Dispose();
            assetScanCancellation = new CancellationTokenSource();
            var token = assetScanCancellation.Token;
            var rootDirectory = SkinManager.SkinV2.RootDirectory;

            assetStatus.Text = LocalizationManager.Get("SkinEditor_LoadingAssets");
            assetStatus.Tint = MutedColor;
            assetStatus.Visible = true;
            assetScanTask = Task.Run(
                () => SkinEditorAssetCatalog.Scan(rootDirectory, token), token);
        }

        private void CompleteAssetRefresh()
        {
            if (assetScanTask == null || !assetScanTask.IsCompleted)
                return;

            var completedTask = assetScanTask;
            assetScanTask = null;
            try
            {
                assets = completedTask.GetAwaiter().GetResult();
                assetStatus.Visible = false;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch
            {
                assets = Array.Empty<SkinEditorAsset>();
                assetStatus.Text = LocalizationManager.Get("SkinEditor_AssetScanFailed");
                assetStatus.Tint = new Color(249, 100, 93);
                assetStatus.Visible = true;
            }

            folderValues = new[] { string.Empty }.Concat(assets.Select(x => x.Folder)
                    .Where(x => !string.IsNullOrEmpty(x)).Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                .ToList();
            folderDropdown.SetOptions(folderValues.Select(x => string.IsNullOrEmpty(x)
                ? LocalizationManager.Get("SkinEditor_AllFolders")
                : x).ToList());
            selectedFolder = string.Empty;
            RefreshAssetBreadcrumb();
            BuildAssetButtons();
        }

        private void RefreshAssetBreadcrumb()
        {
            if (assetBreadcrumb == null)
                return;

            assetBreadcrumb.Text = LocalizationManager.Get("SkinEditor_Assets");
        }

        private void BuildAssetButtons()
        {
            assetScroll?.Destroy();
            thumbnailGeneration++;
            Interlocked.Exchange(ref thumbnailReadInFlight, 0);
            thumbnailLoadQueue.Clear();
            while (thumbnailReadyQueue.TryDequeue(out _))
            {
            }
            assetButtons.Clear();
            visibleAssetFirstIndex = -1;
            visibleAssetLastIndex = -1;
            var width = Math.Max(1, WindowManager.Width - 24);
            assetScroll = new ScrollContainer(new ScalableVector2(width, 166),
                new ScalableVector2(width, 166))
            {
                Parent = assetPanel,
                X = 12,
                Y = 54,
                InputEnabled = true,
                Tint = Color.Transparent,
                Scrollbar = { Tint = AccentColor, Width = 3 }
            };

            pendingAssetButtons = assets.Where(x =>
                    (string.IsNullOrEmpty(selectedFolder) ||
                     string.Equals(x.Folder, selectedFolder, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrWhiteSpace(search) ||
                     x.RelativePath.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0))
                .ToArray();
            const float cellWidth = 122;
            const float cellHeight = 80;
            assetButtonColumns = Math.Max(1, (int) ((width - 8) / cellWidth));
            var rows = (int) Math.Ceiling(pendingAssetButtons.Length / (double) assetButtonColumns);
            assetScroll.ContentContainer.Height = Math.Max(166, rows * cellHeight);

            // Re-adding the same parent moves the dropdown to the end of Wobble's child list.
            // Its expanded item container must render and receive input above the asset viewport.
            if (folderDropdown != null)
                folderDropdown.Parent = assetPanel;
        }

        private void BuildNextAssetButtons()
        {
            const float cellWidth = 122;
            const float cellHeight = 80;

            if (assetScroll == null)
                return;

            var scrollY = Math.Max(0, -assetScroll.ContentContainer.Y);
            var firstRow = Math.Max(0, (int) Math.Floor(scrollY / cellHeight) - 1);
            var rows = (int) Math.Ceiling(assetScroll.Height / cellHeight) + 2;
            var firstIndex = Math.Min(pendingAssetButtons.Length, firstRow * assetButtonColumns);
            var lastIndex = Math.Min(pendingAssetButtons.Length,
                (firstRow + rows) * assetButtonColumns);

            if (firstIndex == visibleAssetFirstIndex && lastIndex == visibleAssetLastIndex)
                return;

            foreach (var index in assetButtons.Keys
                         .Where(x => x < firstIndex || x >= lastIndex).ToArray())
            {
                assetButtons[index].Destroy();
                assetButtons.Remove(index);
            }

            for (var index = firstIndex; index < lastIndex; index++)
            {
                if (assetButtons.ContainsKey(index))
                    continue;

                var asset = pendingAssetButtons[index];
                var selected = session.FocusedAssetProperty != null &&
                               string.Equals(Convert.ToString(
                                       session.FocusedAssetProperty.GetValue(session.Working),
                                       CultureInfo.InvariantCulture),
                                   asset.RelativePath, StringComparison.OrdinalIgnoreCase);
                var button = new SkinEditorAssetButton(skin, assetScroll, asset, selected,
                    QueueAssetThumbnailLoad, () =>
                    {
                        if (readOnly || session.FocusedAssetProperty == null)
                            return;
                        if (CommitValue(session.FocusedAssetProperty, asset.RelativePath))
                        {
                            BuildInspector();
                            BuildAssetButtons();
                        }
                    })
                {
                    X = index % assetButtonColumns * cellWidth,
                    Y = index / assetButtonColumns * cellHeight
                };
                assetScroll.AddContainedDrawable(button);
                assetButtons[index] = button;
            }

            visibleAssetFirstIndex = firstIndex;
            visibleAssetLastIndex = lastIndex;
        }

        private void QueueAssetThumbnailLoad(SkinEditorAssetButton button) =>
            thumbnailLoadQueue.Enqueue(button);

        private void LoadNextAssetThumbnail()
        {
            if (thumbnailReadyQueue.TryDequeue(out var readyButton))
            {
                if (!readyButton.IsDisposed)
                    readyButton.CompleteThumbnailLoad();
                return;
            }

            if (Volatile.Read(ref thumbnailReadInFlight) != 0)
                return;

            while (thumbnailLoadQueue.Count > 0)
            {
                var button = thumbnailLoadQueue.Dequeue();
                if (button.IsDisposed)
                    continue;

                if (Interlocked.CompareExchange(ref thumbnailReadInFlight, 1, 0) != 0)
                    return;

                var generation = thumbnailGeneration;
                if (button.BeginThumbnailRead(completedButton =>
                        QueueThumbnailReadCompleted(completedButton, generation)))
                    return;

                Interlocked.Exchange(ref thumbnailReadInFlight, 0);
            }
        }

        private void QueueThumbnailReadCompleted(SkinEditorAssetButton button, int generation)
        {
            if (generation != thumbnailGeneration)
                return;

            Interlocked.Exchange(ref thumbnailReadInFlight, 0);
            thumbnailReadyQueue.Enqueue(button);
        }

        private RoundedButton CreateInspectorButton(string label, float y, Action action,
            float width = -1, float x = 0)
        {
            var button = new RoundedButton((sender, args) => action())
            {
                X = x,
                Y = y,
                Size = new ScalableVector2(width < 0 ? SkinEditorController.RightPanelWidth - 48 : width, 36),
                Tint = FieldColor,
                CornerRadius = 5
            };
            button.SetLabel(FontManager.GetWobbleFont(Fonts.InterSemiBold), label, 14, Color.White);
            button.Parent = inspectorContent;
            AddInspectorDrawable(button);
            return button;
        }

        private RoundedButton CreateInspectorResetButton(float y, Action action, float height = 36)
        {
            var button = new RoundedButton((sender, args) => action())
            {
                Parent = inspectorContent,
                X = SkinEditorController.RightPanelWidth - 84,
                Y = y,
                Size = new ScalableVector2(36, height),
                Tint = FieldColor,
                CornerRadius = 5,
                IsClickable = !readOnly
            };
            button.SetIcon(GlobalIcons.Get(GlobalIcon.Reset), new Vector2(17, 17));
            button.AddTooltip(new TooltipOptions(LocalizationManager.Get("SkinEditor_ResetDefault"))
            {
                Anchor = TooltipAnchor.TopCenter,
                MaximumWidth = 220
            });
            AddInspectorDrawable(button);
            return button;
        }

        private SpriteTextPlus AddInspectorText(string text, float y, Color color, int size = 14)
        {
            var label = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterMedium), text, size)
            {
                Parent = inspectorContent,
                X = 0,
                Y = y,
                Tint = color
            };
            AddInspectorDrawable(label);
            return label;
        }

        private static Sprite CreatePanel(Drawable parent) => new Sprite
        {
            Parent = parent,
            Tint = PanelColor
        };

        private static SpriteTextPlus CreateHeader(Drawable parent, string text, float x, float y,
            Color? color = null, int size = 18) => new SpriteTextPlus(
            FontManager.GetWobbleFont(Fonts.InterBold), text, size)
        {
            Parent = parent,
            X = x,
            Y = y,
            Tint = color ?? Color.White
        };

        private static RoundedButton CreateButton(Drawable parent, string label, float x, float y,
            float width, float height, Action action, Color color)
        {
            var button = new RoundedButton((sender, args) => action())
            {
                Parent = parent,
                Position = new ScalableVector2(x, y),
                Size = new ScalableVector2(width, height),
                Tint = color,
                CornerRadius = 5
            };
            button.SetLabel(FontManager.GetWobbleFont(Fonts.InterSemiBold), label, 13, Color.White);
            return button;
        }

        private void Resize(bool force = false)
        {
            if (!force && Math.Abs(lastWidth - WindowManager.Width) < 0.01f &&
                Math.Abs(lastHeight - WindowManager.Height) < 0.01f)
                return;

            lastWidth = WindowManager.Width;
            lastHeight = WindowManager.Height;
            Size = new ScalableVector2(lastWidth, lastHeight);
            leftPanel.Size = new ScalableVector2(SkinEditorController.LeftPanelWidth,
                lastHeight - SkinEditorController.AssetPanelHeight);
            targetRoot.Size = new ScalableVector2(SkinEditorController.LeftPanelWidth - 24,
                Math.Max(1, lastHeight - SkinEditorController.AssetPanelHeight - 70));
            targetRoot.RefreshLayout();
            rightPanel.Alignment = Alignment.TopRight;
            rightPanel.Size = new ScalableVector2(SkinEditorController.RightPanelWidth,
                lastHeight - SkinEditorController.AssetPanelHeight);
            assetPanel.Alignment = Alignment.BotLeft;
            assetPanel.Size = new ScalableVector2(lastWidth, SkinEditorController.AssetPanelHeight);
            BuildInspector();
            BuildAssetButtons();
        }

        private static string LocalizeProperty(SkinEditorProperty property)
        {
            var name = property.Name.Replace(" ", string.Empty);
            return LocalizedPropertyNames.Contains(name)
                ? LocalizationManager.Get("SkinEditor_Property_" + name)
                : property.Name;
        }

        private static string LocalizeValidationError(SkinEditorProperty property)
        {
            if (property.IsGradientStops)
                return LocalizationManager.Get("SkinEditor_InvalidGradientStop");
            if (property.Range != null)
                return LocalizationManager.Get("SkinEditor_InvalidRange",
                    property.Range.Minimum, property.Range.Maximum);
            if (property.IsColor)
                return LocalizationManager.Get("SkinEditor_InvalidColor");
            if (property.IsAssetPath)
                return LocalizationManager.Get("SkinEditor_InvalidAssetPath");
            return LocalizationManager.Get("SkinEditor_InvalidValue");
        }

        private sealed class SkinEditorTextbox : Textbox
        {
            private readonly Action<string> apply;
            private string lastObservedText;
            private string lastAppliedText;
            private double timeSinceChange;

            public SkinEditorTextbox(ScalableVector2 size, WobbleFontStore font, int fontSize,
                string initialText = "", string placeholderText = "", Action<string> onApply = null)
                : base(size, font, fontSize, initialText, placeholderText)
            {
                apply = onApply;
                lastObservedText = RawText;
                lastAppliedText = RawText;
                AllowSubmission = false;
            }

            public void SetAppliedText(string text)
            {
                RawText = text;
                lastObservedText = text;
                lastAppliedText = text;
                timeSinceChange = 0;
            }

            public override void Update(GameTime gameTime)
            {
                if (!Visible && !UpdateWhenInvisible)
                    return;

                var submit = Focused && KeyboardManager.IsUniqueKeyPress(Keys.Enter);
                base.Update(gameTime);

                if (!string.Equals(lastObservedText, RawText, StringComparison.Ordinal))
                {
                    lastObservedText = RawText;
                    timeSinceChange = 0;
                }
                else
                    timeSinceChange += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (submit)
                {
                    apply?.Invoke(RawText);
                    lastAppliedText = RawText;
                    Focused = false;
                    Selected = false;
                    return;
                }

                if (timeSinceChange < StoppedTypingActionCalltime ||
                    string.Equals(lastAppliedText, RawText, StringComparison.Ordinal))
                    return;

                apply?.Invoke(RawText);
                lastAppliedText = RawText;
            }
        }

        private sealed class SkinEditorAssetButton : RoundedButton
        {
            private readonly SkinStoreV2Lease skin;
            private readonly ScrollContainer viewport;
            private readonly SkinEditorAsset asset;
            private readonly Action<SkinEditorAssetButton> requestThumbnailLoad;
            private readonly Sprite thumbnail;
            private Task<byte[]> thumbnailReadTask;
            private bool loadRequested;
            private bool loaded;

            public SkinEditorAssetButton(SkinStoreV2Lease skin, ScrollContainer viewport,
                SkinEditorAsset asset, bool selected,
                Action<SkinEditorAssetButton> requestThumbnailLoad, Action action)
                : base((sender, args) => action())
            {
                this.skin = skin;
                this.viewport = viewport;
                this.asset = asset;
                this.requestThumbnailLoad = requestThumbnailLoad;
                Size = new ScalableVector2(116, 76);
                Tint = selected ? AccentColor : FieldColor;
                CornerRadius = 5;
                SetChildrenVisibility = true;
                UpdateWhenInvisible = false;

                thumbnail = new Sprite
                {
                    Parent = this,
                    Alignment = Alignment.TopCenter,
                    Y = 4,
                    Size = new ScalableVector2(50, 42),
                    Image = UserInterface.NoPreviewImage
                };

                var label = new SpriteTextPlus(
                    FontManager.GetWobbleFont(Fonts.InterMedium), asset.Name, 11)
                {
                    Parent = this,
                    Alignment = Alignment.BotCenter,
                    Y = -4,
                    Tint = Color.White
                };
                label.TruncateWithEllipsis(108);
            }

            public override void Update(GameTime gameTime)
            {
                var insideViewport = IsInsideViewport();
                if (Visible != insideViewport)
                    Visible = insideViewport;

                IsInteractionEnabled = insideViewport;

                if (insideViewport && !loadRequested && !loaded)
                {
                    loadRequested = true;
                    requestThumbnailLoad(this);
                }

                base.Update(gameTime);
            }

            public override void Draw(GameTime gameTime)
            {
                if (Visible && IsInsideViewport())
                    base.Draw(gameTime);
            }

            private bool IsInsideViewport() =>
                !viewport.IsDisposed && viewport.Visible &&
                viewport.ScreenRectangle.Intersects(ScreenRectangle);

            public bool BeginThumbnailRead(Action<SkinEditorAssetButton> completed)
            {
                if (loaded || IsDisposed || thumbnailReadTask != null)
                    return false;

                thumbnailReadTask = Task.Run(() =>
                {
                    try
                    {
                        return File.ReadAllBytes(asset.FullPath);
                    }
                    catch
                    {
                        return null;
                    }
                });
                thumbnailReadTask.ContinueWith(_ => completed(this),
                    CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
                return true;
            }

            public void CompleteThumbnailLoad()
            {
                if (loaded || IsDisposed || thumbnailReadTask == null)
                    return;

                var task = thumbnailReadTask;
                thumbnailReadTask = null;
                loaded = true;

                var bytes = task.GetAwaiter().GetResult();
                if (bytes == null || bytes.Length == 0)
                    return;

                try
                {
                    var texture = skin.LoadTextureFromBytes(asset.RelativePath, bytes,
                        UserInterface.NoPreviewImage);
                    var scale = Math.Min(50f / Math.Max(1, texture.Width),
                        42f / Math.Max(1, texture.Height));
                    thumbnail.Size = new ScalableVector2(
                        Math.Max(1, texture.Width * scale),
                        Math.Max(1, texture.Height * scale));
                    thumbnail.Image = texture;
                }
                catch
                {
                    thumbnail.Image = UserInterface.NoPreviewImage;
                }
            }
        }
    }
}
