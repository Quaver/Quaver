using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Panels
{
    public class EditorPanelCompositionTools : EditorPanel
    {
        /// <summary>
        /// </summary>
        private Bindable<EditorCompositionTool> Tool { get; }

        /// <summary>
        /// </summary>
        private List<DrawableEditorCompositionTool> ToolList { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="tool"></param>
        public EditorPanelCompositionTools(Bindable<EditorCompositionTool> tool) : base("Composition Tools")
        {
            Tool = tool;
            Depth = 1;

            ToolList = new List<DrawableEditorCompositionTool>
            {
                new DrawableEditorCompositionTool(Tool, EditorCompositionTool.Select, UserInterface.EditorIconSelect, "Select"),
                new DrawableEditorCompositionTool(Tool, EditorCompositionTool.Note, UserInterface.EditorIconNote, "Note"),
                new DrawableEditorCompositionTool(Tool, EditorCompositionTool.LongNote, UserInterface.EditorIconLongNote, "Long Note"),
            };

            AlignTools();
        }

        /// <summary>
        /// </summary>
        private void AlignTools()
        {
            for (var i = 0; i < ToolList.Count; i++)
            {
                var tool = ToolList[i];

                tool.Parent = Content;
                tool.Size = new ScalableVector2(Content.Width, Content.Height / ToolList.Count - 3);
                tool.X = 4;
                tool.Y = tool.Height * i;
            }
        }
    }

    public class DrawableEditorCompositionTool : ImageButton
    {
        /// <summary>
        /// </summary>
        private Bindable<EditorCompositionTool> SelectedTool { get; }

        /// <summary>
        /// </summary>
        private EditorCompositionTool Tool { get; }

        /// <summary>
        /// </summary>
        private Sprite Icon { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; }

        /// <summary>
        /// </summary>
        private Sprite BorderLine { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="selectedTool"></param>
        /// <param name="tool"></param>
        /// <param name="icon"></param>
        /// <param name="name"></param>
        public DrawableEditorCompositionTool(Bindable<EditorCompositionTool> selectedTool, EditorCompositionTool tool,
            Texture2D icon, string name) : base(UserInterface.OptionsSidebarButtonBackground)
        {
            SelectedTool = selectedTool;
            Tool = tool;

            Icon = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 17,
                Size = new ScalableVector2(20, 20f * icon.Height / icon.Width),
                Image = icon
            };

            if (icon == UserInterface.EditorIconSelect)
            {
                Icon.X += 6;
                Icon.Size = new ScalableVector2(12, 20);
            }

            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), name, 21)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Icon.X + Icon.Width + 14
            };

            if (icon == UserInterface.EditorIconSelect)
                Name.X += 4;

            // ReSharper disable once ObjectCreationAsStatement
            BorderLine = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(4, 0)
            };

            Clicked += (sender, args) => SelectedTool.Value = Tool;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (SelectedTool.Value == Tool)
            {
                Alpha = 1;
                Tint = ColorHelper.HexToColor("#45D6F5");
                BorderLine.Alpha = 1;
            }
            else if (IsHovered)
            {
                Alpha = 0.45f;
                Tint = Color.White;
                BorderLine.Alpha = Alpha;
            }
            else
            {
                Tint = Color.White;
                Alpha = 0;
                BorderLine.Alpha = Alpha;
            }

            Name.Tint = Tint;
            Icon.Tint = Tint;

            BorderLine.Height = Height;
            BorderLine.Tint = Tint;

            base.Update(gameTime);
        }
    }
}