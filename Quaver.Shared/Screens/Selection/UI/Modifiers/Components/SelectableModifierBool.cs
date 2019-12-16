using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Assets;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Selection.UI.Modifiers.Components
{
    public class SelectableModifierBool : SelectableModifier
    {
        /// <summary>
        /// </summary>
        private IconButton OnOffButton { get; }

        private Texture2D Texture => ModManager.IsActivated(Mod.ModIdentifier) ? UserInterface.On : UserInterface.Off;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="mod"></param>
        public SelectableModifierBool(int width, IGameplayModifier mod) : base(width, mod)
        {
            OnOffButton = new IconButton(Texture, (sender, args) =>
            {
                if (ModManager.IsActivated(Mod.ModIdentifier))
                    ModManager.RemoveMod(Mod.ModIdentifier, true);
                else
                    ModManager.AddMod(Mod.ModIdentifier, true);
            })
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(78, 23),
                X = -12,
                UsePreviousSpriteBatchOptions = true
            };

            ModManager.ModsChanged += OnModsChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            OnOffButton.IsPerformingFadeAnimations = CanActivateMultiplayerMod();

            if (!OnOffButton.IsPerformingFadeAnimations)
                OnOffButton.Alpha = Name.Alpha;

            base.Update(gameTime);
        }


        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            ModManager.ModsChanged -= OnModsChanged;
            base.Destroy();
        }

        private void OnModsChanged(object sender, ModsChangedEventArgs e) => ScheduleUpdate(() => OnOffButton.Image = Texture);
    }
}