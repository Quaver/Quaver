using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.Colors.Add;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Panels
{
    public class EditorPanelColorPicker : EditorPanel
    {
        private BindableList<HitObjectInfo> SelectedHitObjects { get; }
        private EditorActionManager ActionManager { get; }

        private Container LeftList;
        private Container RightList;
        private const float Padding = 15f;

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

            CreateLeftList();
            CreateRightList();
        }

        private void CreateLeftList()
        {
            LeftList = new Container()
            {
                Parent = Content,
                Size = new ScalableVector2(Content.Width / 2 - 2 * Padding, Content.Height - 2 * Padding),
                Position = new ScalableVector2(Padding, Padding)
            };

            CreateSnapItems(LeftList, new[] { SnapColor.Red, SnapColor.Blue, SnapColor.Yellow, SnapColor.Orange, SnapColor.Green });
        }

        private void CreateRightList()
        {
            RightList = new Container()
            {
                Parent = Content,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(Content.Width / 2 - 2 * Padding, Content.Height - 2 * Padding),
                Position = new ScalableVector2(-Padding, Padding)
            };

            CreateSnapItems(RightList, new[] { SnapColor.Purple, SnapColor.Pink, SnapColor.Cyan, SnapColor.White, SnapColor.None });
        }

        private void CreateSnapItems(Container parent, SnapColor[] snaps)
        {
            for (var i = 0; i < snaps.Length; i++)
            {
                new DrawableEditorColorPicker(snaps[i], SelectedHitObjects, ActionManager)
                {
                    Parent = parent,
                    Size = new ScalableVector2(parent.Width, (parent.Height / snaps.Length) - 2.5f),
                    Y = i * parent.Height / snaps.Length,
                };
            }
        }
    }

    public class DrawableEditorColorPicker : ImageButton
    {
        /// <summary>
        /// </summary>
        public SnapColor Color { get; }

        private BindableList<HitObjectInfo> SelectedHitObjects { get; }
        private EditorActionManager ActionManager { get; }
        private Sprite Line { get; set; }
        private SpriteTextPlus Name { get; set; }
        private SpriteTextPlus SnapText { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="colorIndex"></param>
        /// <param name="selectedHitObjects"></param>
        /// <param name="manager"></param>
        public DrawableEditorColorPicker(SnapColor colorIndex, BindableList<HitObjectInfo> selectedHitObjects,
            EditorActionManager manager) : base(UserInterface.OptionsSidebarButtonBackground)
        {
            Color = colorIndex;
            SelectedHitObjects = selectedHitObjects;
            ActionManager = manager;

            CreateLine();
            CreateName();

            Alpha = 0;
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

            ActionManager.Perform(new EditorActionSetColor(ActionManager, new List<HitObjectInfo>(SelectedHitObjects.Value), new List<int>(), (int)Color));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (IsHovered)
            {
                Alpha = 0.75f;
                Tint = ColorHelper.BeatSnapToColor((int)Color);
                Name.Tint = Tint;
            }
            else
            {
                Alpha = 0;
                Tint = Microsoft.Xna.Framework.Color.White;
                Name.Tint = Tint;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateLine()
        {
            Line = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Tint = ColorHelper.BeatSnapToColor((int)Color),
                Size = new ScalableVector2(4, 26f)
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
                X = Line.X + Line.Width + 10f
            };

            if (Color != SnapColor.None)
            {
                SnapText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoRegular), $"(1/{(int)Color})", 18)
                {
                    Parent = this,
                    Alignment = Alignment.MidLeft,
                    X = Name.X + Name.Width + 5f
                };
            }
        }
    }
}