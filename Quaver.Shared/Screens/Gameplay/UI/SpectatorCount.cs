using Microsoft.Xna.Framework;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Online;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Gameplay.UI
{
    public class SpectatorCount : Container
    {
        private Sprite Eye { get; }

        private SpriteTextBitmap SpectatorsText { get; }

        public SpectatorCount()
        {
            SetChildrenVisibility = true;

            Eye = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(20, 20),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_eye_open),
                SetChildrenVisibility = true
            };

            SpectatorsText = new SpriteTextBitmap(FontsBitmap.GothamRegular, $" ", false)
            {
                Parent = this,
                FontSize = 16,
                X = Eye.X + Eye.Width + 10
            };
            
            UpdateSpectatorText();
            
            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnSpectatorJoined += OnSpectatorJoined;
                OnlineManager.Client.OnSpectatorLeft += OnSpectatorLeft;
            }
        }

        public override void Update(GameTime gameTime)
        {
            Visible = OnlineManager.IsBeingSpectated && ConfigManager.ShowSpectators.Value;

            base.Update(gameTime);
        }

        public override void Destroy()
        {
            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnSpectatorJoined -= OnSpectatorJoined;
                OnlineManager.Client.OnSpectatorLeft -= OnSpectatorLeft;
            }

            base.Destroy();
        }

        private void OnSpectatorJoined(object sender, SpectatorJoinedEventArgs e)
        {
            UpdateSpectatorText();
        }

        private void OnSpectatorLeft(object sender, SpectatorLeftEventArgs e)
        {
            UpdateSpectatorText();
        }

        private void UpdateSpectatorText()
        {
            var text = $"Spectators ({OnlineManager.Spectators.Count})\n\n";

            foreach (var spectator in OnlineManager.Spectators)
                text += $"- {spectator.Value.OnlineUser?.Username ?? $"User {spectator.Key}"}\n";

            SpectatorsText.Text = text;
            Size = new ScalableVector2(Eye.X + Eye.Width + 10 + SpectatorsText.Width, SpectatorsText.Height);
        }
    }
}