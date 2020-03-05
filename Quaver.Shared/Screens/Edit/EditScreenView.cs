using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Graphs;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.UI.Footer;
using Quaver.Shared.Screens.Edit.UI.Menu;
using Quaver.Shared.Screens.Edit.UI.Panels;
using Quaver.Shared.Screens.Edit.UI.Panels.Layers;
using Quaver.Shared.Screens.Edit.UI.Playfield;
using Quaver.Shared.Screens.Edit.UI.Playfield.Selection;
using TagLib.Matroska;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
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
        private EditorPlayfield Playfield { get; set; }

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
        private bool IsImGuiHovered { get; set; }

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

            MenuBar = new EditorFileMenuBar(EditScreen);

            EditScreen.UneditableMap.ValueChanged += OnUneditableMapChanged;
            EditScreen.BackgroundBrightness.ValueChanged += OnBackgroundBrightnessChanged;

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

            DrawPlugins(gameTime);
            MenuBar?.Draw(gameTime);

            if (ImGui.IsAnyItemHovered() || ImGui.IsMouseDragging())
                IsImGuiHovered = true;

            Button.IsGloballyClickable = !IsImGuiHovered;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Container?.Destroy();
            MenuBar?.Destroy();

            // ReSharper disable twice DelegateSubtraction
            EditScreen.UneditableMap.ValueChanged -= OnUneditableMapChanged;
            EditScreen.BackgroundBrightness.ValueChanged -= OnBackgroundBrightnessChanged;
        }

        /// <summary>
        /// </summary>
        private void CreateBackground()
        {
            var tex = EditScreen.BackgroundStore.Texture ?? UserInterface.Triangles;
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
            EditScreen.LongNoteOpacity, EditScreen.SelectedHitObjects, EditScreen.SelectedLayer, EditScreen.DefaultLayer) { Parent = Container};

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
            EditScreen.SelectedLayer, EditScreen.DefaultLayer)
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

            UnEditablePlayfield = new EditorPlayfield(EditScreen.UneditableMap.Value, EditScreen.ActionManager, EditScreen.Skin,
                EditScreen.Track,EditScreen.BeatSnap, EditScreen.PlayfieldScrollSpeed,
                EditScreen.AnchorHitObjectsAtMidpoint, EditScreen.ScaleScrollSpeedWithRate,
                EditScreen.BeatSnapColor, EditScreen.ViewLayers, EditScreen.CompositionTool, EditScreen.LongNoteOpacity,
                EditScreen.SelectedHitObjects, EditScreen.SelectedLayer, EditScreen.DefaultLayer, true)
            {
                Parent = Container,
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

            const int spacing = 60;

            Playfield.X = -Playfield.Width / 2 - spacing;
            UnEditablePlayfield.X = Playfield.Width / 2 + spacing;

            Playfield.ResetObjectPositions();
            UnEditablePlayfield.ResetObjectPositions();
        }

        /// <summary>
        ///     Called when the user wants to view an uneditable map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUneditableMapChanged(object sender, BindableValueChangedEventArgs<Qua> e)
        {
            Container.ScheduleUpdate(() =>
            {
                UnEditablePlayfield?.Destroy();

                if (e.Value == null)
                {
                    Playfield.X = 0;
                    return;
                }

                CreateOtherDifficultyPlayfield();
                PositionPlayfields();
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

                if (ImGui.IsAnyItemHovered())
                    IsImGuiHovered = true;
            }
        }
    }
}