using Microsoft.Xna.Framework;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Modifiers.Dialogs.Windows
{
    public class DrawableJudgementWindows : PoolableSprite<JudgementWindows>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override int HEIGHT { get; } = 71;

        /// <summary>
        /// </summary>
        private ImageButton Button { get; }

        /// <summary>
        /// </summary>
        public SpriteTextPlus Name { get; }

        /// <summary>
        ///     Returns the background color of the table
        /// </summary>
        private Color BackgroundColor => Index % 2 == 0 ? ColorHelper.HexToColor("#363636") : Colors.DarkGray;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public DrawableJudgementWindows(PoolableScrollContainer<JudgementWindows> container, JudgementWindows item, int index) : base(container, item, index)
        {
            Size = new ScalableVector2(Container.Width, HEIGHT);
            Alpha = 0;

            Button = new ImageButton(UserInterface.BlankBox, (sender, args) => JudgementWindowsDatabaseCache.Selected.Value = item)
            {
                Parent = this,
                Size = Size,
                Tint = BackgroundColor
            };

            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), item.Name, 24)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 18,
            };

            JudgementWindowsDatabaseCache.Selected.ValueChanged += OnSelectedJudgementWindowChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            PerformHHoverAnimation(gameTime);
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            JudgementWindowsDatabaseCache.Selected.ValueChanged -= OnSelectedJudgementWindowChanged;

            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(JudgementWindows item, int index) => ScheduleUpdate(() => Name.Text = item.Name);

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void PerformHHoverAnimation(GameTime gameTime)
        {
            Color color;

            Button.Alpha = 1;

            if (JudgementWindowsDatabaseCache.Selected.Value == Item)
                color = ColorHelper.HexToColor("#636363");
            else if (Button.IsHovered)
                color = ColorHelper.HexToColor("#575757");
            else
            {
                color = Color.Transparent;
                Button.Alpha = 0;
            }

            Button.FadeToColor(color, gameTime.ElapsedGameTime.TotalMilliseconds, 30);
        }

        private void OnSelectedJudgementWindowChanged(object sender, BindableValueChangedEventArgs<JudgementWindows> e)
            => UpdateContent(Item, Index);
    }
}