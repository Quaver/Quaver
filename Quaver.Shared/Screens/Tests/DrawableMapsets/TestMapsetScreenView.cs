using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Screens.Selection.Components;
using Quaver.Shared.Screens.Tests.FilterPanel;
using Wobble.Input;
using Alignment = Wobble.Graphics.Alignment;

namespace Quaver.Shared.Screens.Tests.DrawableMapset
{
    public class TestMapsetScreenView : FilterPanelTestScreenView
    {
        private Mapset TestMapset { get; } = new Mapset()
        {
            Maps = new List<Map>()
        };

        private Selection.UI.Mapsets.DrawableMapset Drawable { get; }

        public TestMapsetScreenView(TestMapsetScreen screen) : base(screen)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new SelectJukebox() {Parent = Container};

            SeedMapset(TestMapset, 6);

            Drawable = new Selection.UI.Mapsets.DrawableMapset(null, TestMapset, 0)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };
        }

        public override void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Home))
            {
                if (Drawable.IsSelected)
                    Drawable.Deselect();
                else
                    Drawable.Select();
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Delete))
                MapManager.Selected.Value = TestMapset.Maps.First();

            base.Update(gameTime);
        }

        private void SeedMapset(Mapset mapset, int num)
        {
            for (var i = 0; i < num; i++)
            {
                var difficulty = 0f;

                switch (i)
                {
                    case 0:
                        difficulty = 1.5f;
                        break;
                    case 1:
                        difficulty = 4f;
                        break;
                    case 2:
                        difficulty = 8f;
                        break;
                    case 3:
                        difficulty = 15f;
                        break;
                    case 4:
                        difficulty = 25f;
                        break;
                    case 5:
                        difficulty = 30;
                        break;
                }

                mapset.Maps.Add(new Map
                {
                    Id = 1,
                    Md5Checksum = "test",
                    Artist = "Swan",
                    Title = "Left Right",
                    Creator = "AiAe",
                    DifficultyName = "Offset Calibrator " + (i + 1),
                    AudioPreviewTime = 2000,
                    Bpm = 120,
                    DateAdded = DateTime.Now,
                    MapId = -1,
                    MapSetId = -1,
                    Difficulty10X = difficulty,
                    RankedStatus = RankedStatus.Ranked,
                    Mode = GameMode.Keys4,
                    RegularNoteCount = 5000,
                    LongNoteCount = 200
                });
            }
        }
    }
}