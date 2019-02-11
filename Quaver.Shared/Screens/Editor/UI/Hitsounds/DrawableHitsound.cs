using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Editor.Actions.Rulesets.Keys;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.HitObjects;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;

namespace Quaver.Shared.Screens.Editor.UI.Hitsounds
{
    public class DrawableHitsound : ImageButton
    {
        /// <summary>
        /// </summary>
        private EditorHitsoundsPanel Panel { get; }

        /// <summary>
        /// </summary>
        private HitSounds Hitsounds { get; }

        /// <summary>
        /// </summary>
        private Texture2D Icon { get; }

        /// <summary>
        /// </summary>
        private string Name { get; }

        /// <summary>
        /// </summary>
        private Sprite SpriteIcon { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap HitsoundName { get; set; }

        /// <summary>
        /// </summary>
        private Sprite ActivatedSprite { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="hitsounds"></param>
        /// <param name="icon"></param>
        /// <param name="name"></param>
        public DrawableHitsound(EditorHitsoundsPanel panel, HitSounds hitsounds, Texture2D icon, string name) : base(UserInterface.BlankBox)
        {
            Panel = panel;
            Hitsounds = hitsounds;
            Icon = icon;
            Name = name;
            Size = new ScalableVector2(panel.Width, 52);

            CreateSpriteIcon();
            CreateHitsoundName();
            CreateActivatedSprite();

            Panel.SelectedObjectHitsounds.ValueChanged += OnSelectedHitsoundsChanged;
            Clicked += OnClicked;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            AnimateSelection(gameTime);
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            Panel.SelectedObjectHitsounds.ValueChanged -= OnSelectedHitsoundsChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateSpriteIcon() => SpriteIcon = new Sprite
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            X = 12,
            Size = new ScalableVector2(16, 16),
            Image = Icon
        };

        /// <summary>
        /// </summary>
        private void CreateHitsoundName() => HitsoundName = new SpriteTextBitmap(FontsBitmap.AllerRegular, Name)
        {
            Parent = this,
            FontSize = 16,
            Alignment = Alignment.MidLeft,
            X = SpriteIcon.X + SpriteIcon.Width + 10,
        };

        /// <summary>
        /// </summary>
        private void CreateActivatedSprite() => ActivatedSprite = new Sprite
        {
            Parent = this,
            Alignment = Alignment.MidRight,
            X = -12,
            Size = new ScalableVector2(16, 16),
            Image = FontAwesome.Get(FontAwesomeIcon.fa_remove_symbol),
            Tint = Color.Crimson
        };

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void AnimateSelection(GameTime gameTime)
        {
            var targetAlpha = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position) ? 0.25f : 0f;
            Alpha = MathHelper.Lerp(Alpha, targetAlpha, (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 30, 1));
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedHitsoundsChanged(object sender, BindableValueChangedEventArgs<HitSounds> e)
        {
            if (e.Value.HasFlag(Hitsounds))
            {
                ActivatedSprite.Image = FontAwesome.Get(FontAwesomeIcon.fa_check_sign_in_a_rounded_black_square);
                ActivatedSprite.Tint = Color.LimeGreen;
            }
            else
            {
                ActivatedSprite.Image = FontAwesome.Get(FontAwesomeIcon.fa_remove_symbol);
                ActivatedSprite.Tint = Color.Crimson;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClicked(object sender, EventArgs e)
        {
            var ruleset = (EditorRulesetKeys) Panel.Screen.Ruleset;

            if (ruleset.SelectedHitObjects.Count == 0)
            {
                NotificationManager.Show(NotificationLevel.Error, "You need to select objects before changing their hitsounds!");
                return;
            }

            if (Panel.SelectedObjectHitsounds.Value.HasFlag(Hitsounds))
            {
                ruleset.ActionManager.Perform(new EditorActionRemoveHitsoundKeys(ruleset,
                    new List<DrawableEditorHitObject>(ruleset.SelectedHitObjects), Hitsounds));
            }
            else
            {
                ruleset.ActionManager.Perform(new EditorActionAddHitsoundKeys(ruleset,
                    new List<DrawableEditorHitObject>(ruleset.SelectedHitObjects), Hitsounds));
            }
        }
    }
}