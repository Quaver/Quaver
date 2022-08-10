using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.Hitsounds.Add;
using Quaver.Shared.Screens.Edit.Actions.Hitsounds.Remove;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;
using DrawingColor = System.Drawing.Color;

namespace Quaver.Shared.Screens.Edit.UI.Panels
{
    public class EditorPanelColorPicker : EditorPanel
    {
        /// <summary>
        /// </summary>
        private BindableList<HitObjectInfo> SelectedHitObjects { get; }

        /// <summary>
        /// </summary>
        private EditorActionManager ActionManager { get; }

        /// <summary>
        /// </summary>
        private List<DrawableEditorColorPicker> HitsoundList { get; }

        /// <summary>
        /// </summary>
        private Bindable<HitSounds> SelectedHitsounds { get; } = new Bindable<HitSounds>(0);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="selectedHitObjects"></param>
        /// <param name="manager"></param>
        public EditorPanelColorPicker(BindableList<HitObjectInfo> selectedHitObjects, EditorActionManager manager)
            : base("Snap Colors")
        {
            ActionManager = manager;
            SelectedHitObjects = selectedHitObjects;

            Depth = 1;

            HitsoundList = new List<DrawableEditorColorPicker>
            {
                new DrawableEditorColorPicker(SnapColor.Red, SelectedHitObjects, ActionManager),
                new DrawableEditorColorPicker(SnapColor.Blue, SelectedHitObjects, ActionManager),
                new DrawableEditorColorPicker(SnapColor.Purple, SelectedHitObjects, ActionManager),
                new DrawableEditorColorPicker(SnapColor.Yellow, SelectedHitObjects, ActionManager),
                new DrawableEditorColorPicker(SnapColor.Pink, SelectedHitObjects, ActionManager),
                new DrawableEditorColorPicker(SnapColor.Orange, SelectedHitObjects, ActionManager),
                new DrawableEditorColorPicker(SnapColor.Cyan, SelectedHitObjects, ActionManager),
                new DrawableEditorColorPicker(SnapColor.Green, SelectedHitObjects, ActionManager),
                new DrawableEditorColorPicker(SnapColor.White, SelectedHitObjects, ActionManager)
            };

            AlignSounds();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            SelectedHitsounds.Dispose();
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void AlignSounds()
        {
            for (var i = 0; i < HitsoundList.Count; i++)
            {
                var sound = HitsoundList[i];

                sound.Parent = Content;
                sound.Size = new ScalableVector2(Content.Width, Content.Height / HitsoundList.Count - 2.5f);
                sound.X = 4;
                sound.Y = 7f + sound.Height * i;
            }
        }
    }

    public class DrawableEditorColorPicker : ImageButton
    {
        /// <summary>
        /// </summary>
        public SnapColor Color { get; }

        /// <summary>
        /// </summary>
        private BindableList<HitObjectInfo> SelectedHitObjects { get; }

        /// <summary>
        /// </summary>
        private EditorActionManager ActionManager { get; }

        /// <summary>
        /// </summary>
        private Sprite Icon { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; set; }

        /// <summary>
        /// </summary>
        private Sprite BorderLine { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="sound"></param>
        /// <param name="selectedHitsounds"></param>
        /// <param name="selectedHitObjects"></param>
        /// <param name="manager"></param>
        public DrawableEditorColorPicker(SnapColor colorIndex, BindableList<HitObjectInfo> selectedHitObjects,
            EditorActionManager manager) : base(UserInterface.OptionsSidebarButtonBackground)
        {
            Color = colorIndex;
            SelectedHitObjects = selectedHitObjects;
            ActionManager = manager;

            CreateIcon();
            CreateName();
            CreateLine();

            Alpha = 0;
            BorderLine.Alpha = 0;

            Clicked += OnClicked;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClicked(object sender, EventArgs e)
        {
            if (SelectedHitObjects.Value.Count == 0)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You need to select objects before changing the color!");
                return;
            }

            ActionManager.Perform(new EditorActionSetColor(ActionManager, new List<HitObjectInfo>(SelectedHitObjects.Value), (int)Color));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (IsHovered)
            {
                var col = DrawingColor.FromName(Color.ToString());
                Alpha = 0.45f;
                Tint = new Microsoft.Xna.Framework.Color(col.R, col.G, col.B);
                BorderLine.Alpha = Alpha;
            }
            else
            {
                Tint = Microsoft.Xna.Framework.Color.White;
                Alpha = 0;
                BorderLine.Alpha = Alpha;
            }

            Name.Tint = Tint;
            Icon.Tint = Tint;

            BorderLine.Height = Height;
            BorderLine.Tint = Tint;

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateIcon()
        {
            var icon = GetTexture();

            Icon = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 17,
                Size = new ScalableVector2(20, 20f * icon.Height / icon.Width),
                Image = icon
            };
        }

        /// <summary>
        /// </summary>
        private void CreateName()
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), Color.ToString(), 21)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Icon.X + Icon.Width + 14
            };
        }

        /// <summary>
        /// </summary>
        private void CreateLine() => BorderLine = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(4, 0)
        };

        private Texture2D GetTexture() => FontAwesome.Get(FontAwesomeIcon.fa_angle_arrow_pointing_to_right);
    }
}