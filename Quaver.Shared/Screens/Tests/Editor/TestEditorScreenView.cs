using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Maps;
using Quaver.Shared.Screens.Edit;
using Wobble;
using Wobble.Assets;
using Wobble.Audio.Tracks;
using Wobble.Input;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Editor
{
    public class TestEditorScreenView : ScreenView
    {
        private EditScreen Edit { get; }

        /// <summary>
        /// </summary>
        private IAudioTrack Track { get; }

        private string Dir = $"Quaver.Resources/Maps/PrincessOfWinter";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestEditorScreenView(Screen screen) : base(screen)
        {
            var qua = Qua.Parse(GameBase.Game.Resources.Get($"{Dir}/2043.qua"), false);
            Track = new AudioTrack(GameBase.Game.Resources.Get($"{Dir}/audio.mp3"), false, false);
            var background = new EditorVisualTestBackground(AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get($"{Dir}/Princess of Winter.png")));

            Edit = new EditScreen(qua, Track, background);
        }

        public override void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.D1))
                Edit.UneditableMap.Value = Qua.Parse(GameBase.Game.Resources.Get($"{Dir}/2044.qua"), false);

            Edit.Update(gameTime);
        }

        public override void Draw(GameTime gameTime) => Edit.Draw(gameTime);

        public override void Destroy()
        {
            Edit.Destroy();
            Track.Dispose();
        }
    }
}