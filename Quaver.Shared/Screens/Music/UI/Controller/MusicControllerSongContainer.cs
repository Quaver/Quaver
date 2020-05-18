using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Music.UI.Controller.Scrolling;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Music.UI.Controller
{
    public class MusicControllerSongContainer : Sprite
    {
        /// <summary>
        /// </summary>
        private Bindable<List<Mapset>> AvailableSongs { get; }

        /// <summary>
        /// </summary>
        private Bindable<string> CurrentSearchQuery { get; }

        /// <summary>
        /// </summary>
        public List<MusicControllerTableColumn> Columns { get; private set; }

        /// <summary>
        /// </summary>
        private MusicControllerScrollContainer ScrollContainer { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        /// <param name="availableSongs"></param>
        /// <param name="searchQuery"></param>
        public MusicControllerSongContainer(ScalableVector2 size, Bindable<List<Mapset>> availableSongs, Bindable<string> searchQuery)
        {
            AvailableSongs = availableSongs;
            CurrentSearchQuery = searchQuery;

            Tint = ColorHelper.HexToColor("#262626");
            Size = size;

            CreateTableColumns();
            CreateScrollContainer();
        }

        /// <summary>
        /// </summary>
        private void CreateTableColumns()
        {
            Columns = new List<MusicControllerTableColumn>();

            for (var i = 0; i < Enum.GetValues(typeof(MusicControllerTableColumnType)).Length; i++)
            {
                var type = (MusicControllerTableColumnType) i;

                float width;
                string title;

                switch (type)
                {
                    case MusicControllerTableColumnType.Title:
                        width = Width * 0.40f;
                        title = "Title";
                        break;
                    case MusicControllerTableColumnType.Artist:
                        width = Width * 0.25f;
                        title = "Artist";
                        break;
                    case MusicControllerTableColumnType.Creator:
                        width = Width * 0.15f;
                        title = "Creator";
                        break;
                    case MusicControllerTableColumnType.BPM:
                        width = Width * 0.10f;
                        title = "BPM";
                        break;
                    case MusicControllerTableColumnType.Length:
                        width = Width * 0.10f;
                        title = "Length";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var column = new MusicControllerTableColumn(type, title.ToUpper(), width, Height)
                {
                    Container =
                    {
                        Parent = this,
                        // Tint = i % 2 == 0 ? Colors.BlueishDarkGray : Colors.DarkGray
                    }
                };

                Columns.Add(column);

                if (i == 0)
                    continue;

                var previous = Columns[i - 1];
                column.Container.X = previous.Container.X + previous.Container.Width;
            }
        }

        /// <summary>
        /// </summary>
        private void CreateScrollContainer()
        {
            var size = new ScalableVector2(Width, Height - Columns.First().DividerLine.Y - Columns.First().DividerLine.Height);

            ScrollContainer = new MusicControllerScrollContainer(this, AvailableSongs, CurrentSearchQuery, size)
            {
                Parent = this,
                Y = Columns.First().DividerLine.Y + Columns.First().DividerLine.Height
            };
        }
    }
}