using System;
using System.Collections.Generic;
using System.Linq;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Screens.Selection.Components;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Quaver.Shared.Screens.Tests.FilterPanel;
using Wobble.Bindables;
using Wobble.Input;
using Alignment = Wobble.Graphics.Alignment;

namespace Quaver.Shared.Screens.Tests.Mapsets
{
    public class TestMapsetScreenView : FilterPanelTestScreenView
    {
        private Mapset TestMapset { get; } = new Mapset()
        {
            Maps = new List<Map>()
        };

        private DrawableMapset Drawable { get; }

        public TestMapsetScreenView(TestMapsetScreen screen) : base(screen)
        {
            for (var i = 0; i < 5; i++)
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

                TestMapset.Maps.Add(new Map
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

            // ReSharper disable once ObjectCreationAsStatement
            new SelectJukebox() {Parent = Container};

            Drawable = new DrawableMapset(null, TestMapset, 0)
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
    }
}