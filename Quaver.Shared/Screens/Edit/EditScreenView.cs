using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Graphs;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.UI.AutoMods;
using Quaver.Shared.Screens.Edit.UI.Footer;
using Quaver.Shared.Screens.Edit.UI.Menu;
using Quaver.Shared.Screens.Edit.UI.Panels;
using Quaver.Shared.Screens.Edit.UI.Panels.Layers;
using Quaver.Shared.Screens.Edit.UI.Playfield;
using Quaver.Shared.Screens.Edit.UI.Playfield.Selection;
using Quaver.Shared.Screens.Edit.UI.Preview;
using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Screens.Selection.UI.Modifiers;
using Quaver.Shared.Screens.Selection.UI.Preview;
using TagLib.Matroska;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Edit
{
    public class EditScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private EditScreen EditScreen => (EditScreen) Screen;

        /// <summary>
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        /// </summary>
        public EditorPlayfield Playfield { get; set; }

        /// <summary>
        /// </summary>
        private EditorPlayfield UnEditablePlayfield { get; set; }

        /// <summary>
        /// </summary>
        private EditorFooter Footer { get; set; }

        /// <summary>
        /// </summary>
        private EditorPanelDetails Details { get; set; }

        /// <summary>
        /// </summary>
        private EditorPanelCompositionTools CompositionTools { get; set; }

        /// <summary>
        /// </summary>
        private EditorPanelHitsounds Hitsounds { get; set; }

        /// <summary>
        /// </summary>
        public EditorPanelLayers Layers { get; private set; }

        /// <summary>
        /// </summary>
        private EditorFileMenuBar MenuBar { get; }

        /// <summary>
        /// </summary>
        private EditorRectangleSelector Selector { get; set; }

        /// <summary>
        /// </summary>
        private EditorMapPreview MapPreview { get; set; }

        /// <summary>
        /// </summary>
        public EditorAutoModPanelContainer AutoMod { get; private set; }

        /// <summary>
        /// </summary>
        public bool IsImGuiHovered { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreatePlayfield();
            CreateFooter();
            CreateSelector();
            CreateDetailsPanel();
            CreateCompositionTools();
            CreateHitsoundsPanel();
            CreateLayersPanel();
            CreateAutoMod();

            if (EditScreen.DisplayGameplayPreview.Value)
                CreateGameplayPreview();

            MenuBar = new EditorFileMenuBar(EditScreen);

            EditScreen.DisplayGameplayPreview.ValueChanged += OnDisplayGameplayPreviewChanged;
            EditScreen.UneditableMap.ValueChanged += OnUneditableMapChanged;
            EditScreen.BackgroundBrightness.ValueChanged += OnBackgroundBrightnessChanged;
            BackgroundHelper.Loaded += OnBackgroundLoaded;
            Footer.Parent = Container;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Container?.Update(gameTime);
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            Container?.Draw(gameTime);

            IsImGuiHovered = false;

            if (MenuBar != null && !EditScreen.Exiting)
            {
                if (DialogManager.Dialogs.Count == 0)
                    DrawPlugins(gameTime);

                MenuBar?.Draw(gameTime);
                GameBase.Game.SpriteBatch.End();

                if (ImGui.IsAnyItemHovered())
                    IsImGuiHovered = true;
            }

            Button.IsGloballyClickable = !IsImGuiHovered;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Container?.Destroy();

            // ReSharper disable twice DelegateSubtraction
            EditScreen.UneditableMap.ValueChanged -= OnUneditableMapChanged;
            EditScreen.BackgroundBrightness.ValueChanged -= OnBackgroundBrightnessChanged;
            EditScreen.DisplayGameplayPreview.ValueChanged -= OnDisplayGameplayPreviewChanged;

            BackgroundHelper.Loaded -= OnBackgroundLoaded;
        }

        /// <summary>
        /// </summary>
        private void CreateBackground()
        {
            var tex = EditScreen.BackgroundStore?.Texture ?? BackgroundHelper.RawTexture ?? UserInterface.Triangles;
            var dim = tex == UserInterface.Triangles ? 0 : 100 - EditScreen.BackgroundBrightness.Value;

            Background = new BackgroundImage(tex, dim, false)
            {
                Parent = Container
            };
        }

        /// <summary>
        /// </summary>
        private void CreatePlayfield() => Playfield = new EditorPlayfield(EditScreen.WorkingMap, EditScreen.ActionManager, EditScreen.Skin,
            EditScreen.Track, EditScreen.BeatSnap, EditScreen.PlayfieldScrollSpeed, EditScreen.AnchorHitObjectsAtMidpoint,
            EditScreen.ScaleScrollSpeedWithRate, EditScreen.BeatSnapColor, EditScreen.ViewLayers, EditScreen.CompositionTool,
            EditScreen.LongNoteOpacity, EditScreen.SelectedHitObjects, EditScreen.SelectedLayer, EditScreen.DefaultLayer,
            EditScreen.PlaceObjectsOnNearestTick, EditScreen.ShowWaveform, EditScreen.AudioDirection, EditScreen.WaveformFilter) { Parent = Container};

        /// <summary>
        /// </summary>
        private void CreateDetailsPanel() => Details = new EditorPanelDetails(EditScreen.WorkingMap, EditScreen.BeatSnap,
            EditScreen.Track, EditScreen.ActionManager)
        {
            Parent = Container,
            Alignment = Alignment.MidLeft,
            Y = -200
        };

        /// <summary>
        /// </summary>
        private void CreateCompositionTools() => CompositionTools = new EditorPanelCompositionTools(EditScreen.CompositionTool)
        {
            Parent = Container,
            Alignment = Alignment.MidLeft,
            Y = 200
        };

        /// <summary>
        /// </summary>
        private void CreateHitsoundsPanel() => Hitsounds = new EditorPanelHitsounds(EditScreen.SelectedHitObjects, EditScreen.ActionManager)
        {
            Parent = Container,
            Alignment = Alignment.MidRight,
            Y = 200
        };

        private void CreateLayersPanel() => Layers = new EditorPanelLayers(EditScreen.ActionManager, EditScreen.WorkingMap,
            EditScreen.SelectedLayer, EditScreen.DefaultLayer, EditScreen.SelectedHitObjects, EditScreen.ViewLayers)
        {
            Parent = Container,
            Alignment = Alignment.MidRight,
            Y = -200
        };

        /// <summary>
        /// </summary>
        private void CreateFooter() => Footer = new EditorFooter(EditScreen, EditScreen.Track)
        {
            Parent = Container,
            Alignment = Alignment.BotLeft,
        };

        /// <summary>
        /// </summary>
        private void CreateSelector()
        {
            Selector = new EditorRectangleSelector(EditScreen.WorkingMap, EditScreen.CompositionTool, Playfield, Footer,
                EditScreen.Track, EditScreen.SelectedHitObjects)
            {
                Parent = Container
            };
        }

        /// <summary>
        /// </summary>
        private void CreateOtherDifficultyPlayfield()
        {
            if (EditScreen.UneditableMap.Value == null)
                return;

            UnEditablePlayfield = new EditorPlayfield(EditScreen.UneditableMap.Value, EditScreen.ActionManager,
                EditScreen.Skin,
                EditScreen.Track, EditScreen.BeatSnap, EditScreen.PlayfieldScrollSpeed,
                EditScreen.AnchorHitObjectsAtMidpoint, EditScreen.ScaleScrollSpeedWithRate,
                EditScreen.BeatSnapColor, EditScreen.ViewLayers, EditScreen.CompositionTool, EditScreen.LongNoteOpacity,
                EditScreen.SelectedHitObjects, EditScreen.SelectedLayer, EditScreen.DefaultLayer, EditScreen.PlaceObjectsOnNearestTick,
                EditScreen.ShowWaveform, EditScreen.AudioDirection, EditScreen.WaveformFilter, true)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter
            };

            // Reset the parent of the footer, so it draws over this playfield.
            Footer.Parent = Container;
        }

        /// <summary>
        /// </summary>
        private void PositionPlayfields()
        {
            if (UnEditablePlayfield == null)
                return;

            var spacing = EditScreen.WorkingMap.Mode == GameMode.Keys4 ? 120 : 60;

            Playfield.X = -Playfield.Width / 2 - spacing;
            UnEditablePlayfield.X = Playfield.Width / 2 + spacing;

            Playfield.ResetObjectPositions();
            UnEditablePlayfield.ResetObjectPositions();
            
            // Makes it so that the playfield bookmark tooltips appear above reference difficulty
            Playfield.Parent = Container;
        }

        /// <summary>
        ///     Called when the user wants to view an uneditable map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUneditableMapChanged(object sender, BindableValueChangedEventArgs<Qua> e)
        {
            Container.AddScheduledUpdate(() =>
            {
                if (e.Value != null)
                {
                    if (MapPreview != null)
                    {
                        MapPreview.Destroy();
                        MapPreview = null;
                    }

                    EditScreen.DisplayGameplayPreview.Value = false;
                }

                UnEditablePlayfield?.Destroy();

                if (e.Value == null)
                {
                    Playfield.X = 0;
                    return;
                }

                CreateOtherDifficultyPlayfield();
                PositionPlayfields();
                ResetPanelParents();
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundBrightnessChanged(object sender, BindableValueChangedEventArgs<int> e)
        {
            if (Background.Image == UserInterface.Triangles)
            {
                Background.Dim = 0;
                return;
            }

            Background.Dim = 100 - e.Value;
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void DrawPlugins(GameTime gameTime)
        {
            for (var i = 0; i < EditScreen.Plugins.Count; i++)
            {
                var plugin = EditScreen.Plugins[i];

                if (!plugin.IsActive)
                    continue;

                plugin.Draw(gameTime);

                if (ImGui.IsAnyItemHovered() || plugin.IsWindowHovered)
                    IsImGuiHovered = true;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundLoaded(object sender, BackgroundLoadedEventArgs e)
        {
            Background.BrightnessSprite.ClearAnimations();

            Container.AddScheduledUpdate(() =>
            {
                Background.BrightnessSprite.Alpha = 1;
                Background.Image = e.Texture;
                Background.BrightnessSprite.FadeTo(Background.Dim / 100f, Easing.Linear, 250);
            });
        }

        /// <summary>
        /// </summary>
        private void CreateGameplayPreview()
        {
            if (MapPreview != null)
                return;

            MapPreview = new EditorMapPreview(EditScreen.ActionManager, new Bindable<bool>(false), EditScreen.ActiveLeftPanel,
                (int) WindowManager.Height - MenuBorder.HEIGHT - 34, EditScreen.Track, EditScreen.WorkingMap)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 34,
            };

            var spacing = EditScreen.WorkingMap.Mode == GameMode.Keys4 ? 120 : 60;

            Playfield.X = -Playfield.Width / 2 - spacing;
            MapPreview.X = Playfield.Width / 2 + spacing;
            
            // Makes it so that the playfield bookmark tooltips appear above preview.
            Playfield.Parent = Container;
            
            // Makes it so the selector goes above editor after enabling preview.
            Selector.Parent = Container;

            ResetPanelParents();
        }

        /// <summary>
        /// </summary>
        private void CreateAutoMod() => AutoMod = new EditorAutoModPanelContainer(EditScreen)
        {
            Parent = Container
        };

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisplayGameplayPreviewChanged(object sender, BindableValueChangedEventArgs<bool> e)
        {
            if (e.Value)
            {
                UnEditablePlayfield?.Destroy();

                if (MapPreview == null)
                    CreateGameplayPreview();

                return;
            }

            MapPreview?.Destroy();
            MapPreview = null;
            Playfield.X = 0;
        }

        /// <summary>
        ///     To make sure that the panels are always displayed on top
        /// </summary>
        private void ResetPanelParents()
        {
            Layers.Parent = Container;
            Details.Parent = Container;
            CompositionTools.Parent = Container;
            Hitsounds.Parent = Container;
            AutoMod.Parent = Container;
            Footer.Parent = Container;
        }
    }
}
