using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Maps;
using Quaver.Shared.Database.Maps;
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
            var mapset = new Mapset { Maps = new List<Map>() };

            // diff name, map file path name (id), diff rating
            var files = new List<Tuple<string, int, double>>()
            {
                new Tuple<string, int, double>("Easy", 2071, 0.95f),
                new Tuple<string, int, double>("Beginner", 2072, 0.96f),
                new Tuple<string, int, double>("Novice", 5507, 1.93),
                new Tuple<string, int, double>("Normal", 2070, 4.59f),
                new Tuple<string, int, double>("Advanced", 2044, 10.75f),
                new Tuple<string, int, double>("Hard", 3940, 13.78f),
                new Tuple<string, int, double>("Insane", 2045, 18.86),
                new Tuple<string, int, double>("Expert", 2043, 26.43),
            };

            foreach (var file in files)
            {
                mapset.Maps.Add(new VisualTestMap()
                {
                    Directory = Dir,
                    Path = $"{file.Item2}.qua",
                    DifficultyName = file.Item1,
                    Mapset = mapset,
                    Difficulty10X = file.Item3
                });
            }

            Track = new AudioTrack(GameBase.Game.Resources.Get($"{Dir}/audio.mp3"), false, false);
            var background = new EditorVisualTestBackground(AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get($"{Dir}/Princess of Winter.png")));

            Edit = new EditScreen(mapset.Maps.Last(), Track, background);
        }

        public override void Update(GameTime gameTime) => Edit.Update(gameTime);

        public override void Draw(GameTime gameTime) => Edit.Draw(gameTime);

        public override void Destroy()
        {
            Edit.Destroy();
            Track.Dispose();
        }
    }
}