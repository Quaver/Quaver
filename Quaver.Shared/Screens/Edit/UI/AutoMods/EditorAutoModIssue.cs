using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.AutoMod.Issues;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.AutoMods
{
    public class EditorAutoModIssue : PoolableSprite<AutoModIssue>
    {
        public sealed override int HEIGHT { get; } = 50;

        private ImageButton Button { get; set; }

        private SpriteTextPlus IssueText { get; set; }

        private Sprite BottomLine { get; set; }

        private ImageButton Icon { get; set; }

        private Color DefaultColor { get; } = ColorHelper.HexToColor("#242424");

        public EditorAutoModIssue(PoolableScrollContainer<AutoModIssue> container, AutoModIssue item, int index)
            : base(container, item, index)
        {
            Size = new ScalableVector2(container.Width, HEIGHT);
            Tint = DefaultColor;

            CreateButton();
            CreateIssueText();
            CreateBottomLine();
            CreateIcon();
        }

        public override void Update(GameTime gameTime)
        {
            if (Button.IsHovered)
            {
                Tint = Color.White;
                Alpha = 0.45f;
            }
            else
            {
                Tint = DefaultColor;
                Alpha = 1;
            }


            base.Update(gameTime);
        }

        public override void UpdateContent(AutoModIssue item, int index)
        {
            Item = item;
            Index = index;

            IssueText.Text = Item.Text;

            Icon.Image = GetIconImage();
            Icon.Tint = Item.Level == AutoModIssueLevel.Ranking ? Colors.MainBlue : Color.White;
            Icon.Size = new ScalableVector2(Icon.Image.Width, Icon.Image.Height);
        }

        private void CreateButton()
        {
            Button = new ContainedButton(Container, UserInterface.BlankBox)
            {
                Parent = this,
                Alpha = 0,
                Size = Size,
                Depth = -1
            };

            Button.Clicked += (o, e) =>
            {
                var container = Container as EditorAutoModScrollPanel;
                container?.Panel.TriggerEvent(Item);
            };
        }

        private void CreateIssueText() => IssueText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
            "", 22)
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            X = 16
        };

        private void CreateBottomLine() => BottomLine = new Sprite
        {
            Parent = this,
            Alignment = Alignment.BotLeft,
            Size = new ScalableVector2(Width, 2),
            Tint = ColorHelper.HexToColor("#BEBEBE"),
            Alpha = 0.50f
        };

        private void CreateIcon()
        {
            Icon = new IconButton(UserInterface.BlankBox)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -IssueText.X,
                Size = new ScalableVector2(20, 20),
                Depth = -1
            };

            var game = GameBase.Game as QuaverGame;

            Icon.Hovered += (o, e) =>
            {
                string text;
                Color color;

                switch (Item.Level)
                {
                    case AutoModIssueLevel.Warning:
                        text = "This is a warning of a potential error. You can ignore this if it isn't an issue.";
                        color = Color.Yellow;
                        break;
                    case AutoModIssueLevel.Critical:
                        text = "This is a critical error which affects the playability of your map.";
                        color = Color.Crimson;
                        break;
                    case AutoModIssueLevel.Ranking:
                        text = "This is a ranking criteria error. Without fixing this, your map cannot be ranked.";
                        color = Colors.MainBlue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                game?.CurrentScreen?.ActivateTooltip(new Tooltip(text, color));
            };

            Icon.LeftHover += (sender, args) => game?.CurrentScreen?.DeactivateTooltip();
        }

        private Texture2D GetIconImage()
        {
            switch (Item.Level)
            {
                case AutoModIssueLevel.Warning:
                    return UserInterface.AutoModIconWarning;
                case AutoModIssueLevel.Critical:
                    return UserInterface.AutoModIconCritical;
                case AutoModIssueLevel.Ranking:
                    return UserInterface.RequiredAccAlert;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}