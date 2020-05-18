using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Modifiers.Mods;
using Quaver.Shared.Online;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Modifiers.Components
{
    public class SelectableModifier : Button
    {
        /// <summary>
        /// </summary>
        public ModifierSelector Selector { get; set; }

        /// <summary>
        /// </summary>
        public IGameplayModifier Mod { get; }

        /// <summary>
        /// </summary>
        protected Sprite Icon { get; }

        /// <summary>
        /// </summary>
        protected SpriteTextPlus Name { get; }

        /// <summary>
        /// </summary>
        public Tooltip Tooltip { get; }

        /// <summary>
        /// </summary>
        public Color OriginalColor { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="mod"></param>
        public SelectableModifier(int width, IGameplayModifier mod)
        {
            Mod = mod;
            Size = new ScalableVector2(width, 48);
            Tint = ColorHelper.HexToColor("#464545");

            var game = GameBase.Game as QuaverGame;
            Depth = game?.CurrentScreen?.Type == QuaverScreenType.Editor ? 0 : 1;

            const int paddingLeft = 10;

            Icon = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(60, 30),
                Image = GetTexture(),
                Alignment = Alignment.MidLeft,
                X = paddingLeft,
                UsePreviousSpriteBatchOptions = true
            };

            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), mod.Name.ToUpper(), 22)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Icon.X + Icon.Width + paddingLeft,
                UsePreviousSpriteBatchOptions = true,
                Tint = Mod.ModColor
            };

            Tooltip = new Tooltip(mod.Description, mod.ModColor);

            Hovered += (sender, args) => Selector?.ActivateTooltip(Tooltip);

            LeftHover += (sender, args) =>
            {
                if (Selector == null)
                    return;

                Tooltip.Parent = null;
            };

            Clicked += (sender, args) =>
            {
                if (Mod is ModSpeed || Mod is ModJudgementWindows)
                    return;

                if (!CanActivateMultiplayerMod())
                    return;

                if (ModManager.IsActivated(Mod.ModIdentifier))
                    ModManager.RemoveMod(Mod.ModIdentifier, true);
                else
                    ModManager.AddMod(Mod.ModIdentifier, true);
            };

            ModManager.ModsChanged += OnModsChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            PerformHoverAnimation(gameTime);

            Name.Alpha = CanActivateMultiplayerMod() ? 1f : 0.60f;
            Icon.Alpha = Name.Alpha;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Tooltip.Destroy();
            ModManager.ModsChanged -= OnModsChanged;

            base.Destroy();
        }

        /// <summary>
        ///     Performs an animation when hovered over the button
        /// </summary>
        /// <param name="gameTime"></param>
        private void PerformHoverAnimation(GameTime gameTime)
        {
            var game = GameBase.Game as QuaverGame;

            if (DialogManager.Dialogs.Count != 0)
            {
                if (game?.CurrentScreen?.Type != QuaverScreenType.Editor)
                    return;
            }

            var color = ScreenRectangle.Contains(MouseManager.CurrentState.Position.ToPoint()) ? ColorHelper.HexToColor("#464545") : OriginalColor;
            FadeToColor(color, gameTime.ElapsedGameTime.TotalMilliseconds, 30);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModsChanged(object sender, ModsChangedEventArgs e) => Icon.Image = GetTexture();

        private Texture2D GetTexture()
        {
            try
            {
                if (Mod.GetType() == typeof(ModSpeed))
                    return TextureManager.Load($@"Quaver.Resources/Textures/UI/Mods/N-1.1x.png");
                if (Mod.GetType() == typeof(ModJudgementWindows) || Mod.GetType() == typeof(ModLongNoteAdjust))
                    return TextureManager.Load($@"Quaver.Resources/Textures/UI/Mods/N-JW.png");

                return ModManager.GetTexture(Mod.ModIdentifier, !ModManager.IsActivated(Mod.ModIdentifier));
            }
            catch (Exception e)
            {
                return FontAwesome.Get(FontAwesomeIcon.fa_question_sign);
            }
        }

        /// <summary>
        ///     Returns if the user is allowed to activate the multiplayer mod
        /// </summary>
        /// <returns></returns>
        protected bool CanActivateMultiplayerMod()
        {
            var game = OnlineManager.CurrentGame;

            if (game == null)
                return true;

            if (Mod is ModJudgementWindows)
                return true;

            if (!Mod.AllowedInMultiplayer)
                return false;

            var isHost = game.HostId == OnlineManager.Self.OnlineUser.Id;

            if (Mod.OnlyMultiplayerHostCanCanChange && !isHost)
                return false;

            if (Mod is ModSpeed && !isHost && !game.FreeModType.HasFlag(MultiplayerFreeModType.Rate))
                return false;

            if (!(Mod is ModSpeed) && !isHost && !game.FreeModType.HasFlag(MultiplayerFreeModType.Regular))
                return false;

            return true;
        }
    }
}