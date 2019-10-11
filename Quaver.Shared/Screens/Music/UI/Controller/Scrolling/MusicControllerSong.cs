using System;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using TimeAgo;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Music.UI.Controller.Scrolling
{
    public sealed class MusicControllerSong : PoolableSprite<Mapset>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = 53;

        /// <summary>
        /// </summary>
        private SpriteTextPlus Title { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Artist { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus DateAdded { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Bpm { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Length { get; set; }

        /// <summary>
        /// </summary>
        private ImageButton Button { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public MusicControllerSong(PoolableScrollContainer<Mapset> container, Mapset item, int index) : base(container, item, index)
        {
            Size = new ScalableVector2(container.Width, HEIGHT);
            Alpha = 0;

            CreateTitle();
            CreateArtist();
            CreateDateAdded();
            CreateBpm();
            CreateLength();

            Button = new ImageButton(UserInterface.BlankBox, (o, e) =>
                {
                    MapManager.Selected.Value = Item.Maps.First();
                })
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(Width - 50, Height - 2),
                UsePreviousSpriteBatchOptions = true,
                Alpha = 0,
                Tint = Colors.MainBlue
            };

            new Sprite()
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(Width - 50, 1),
                Alpha = 0.45f
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(Mapset item, int index)
        {
            Item = item;
            Index = index;

            Tint = Index % 2 == 0 ? ColorHelper.HexToColor("#363636") : ColorHelper.HexToColor("#242424");

            var container = (MusicControllerScrollContainer) Container;

            var titleColumn = container.SongContainer.Columns.Find(x => x.Type == MusicControllerTableColumnType.Title);
            Title.Text = item.Title;
            Title.TruncateWithEllipsis((int) titleColumn.Container.Width - 50);
            Title.X = titleColumn.Container.X + titleColumn.Header.X;

            Title.Tint = MapManager.Selected.Value?.Mapset == Item ? Colors.MainAccent : Color.White;

            var artistColumn = container.SongContainer.Columns.Find(x => x.Type == MusicControllerTableColumnType.Artist);
            Artist.Text = item.Artist;
            Artist.TruncateWithEllipsis((int) artistColumn.Container.Width - 50);
            Artist.X = artistColumn.Container.X + artistColumn.Header.X;
            Artist.Tint = MapManager.Selected.Value?.Mapset == Item ? Colors.MainAccent : Color.White;

            var dateColumn = container.SongContainer.Columns.Find(x => x.Type == MusicControllerTableColumnType.DateAdded);
            DateAdded.Text = "2 days ago";
            DateAdded.TruncateWithEllipsis((int) dateColumn.Container.Width - 50);
            DateAdded.X = dateColumn.Container.X + dateColumn.Header.X;
            DateAdded.Tint = MapManager.Selected.Value?.Mapset == Item ? Colors.MainAccent : Color.White;

            var bpmColumn = container.SongContainer.Columns.Find(x => x.Type == MusicControllerTableColumnType.BPM);
            Bpm.Text = $"{(int) Item.Maps.First().Bpm}".ToString();
            Bpm.TruncateWithEllipsis((int) bpmColumn.Container.Width - 50);
            Bpm.X = bpmColumn.Container.X + bpmColumn.Header.X;
            Bpm.Tint = MapManager.Selected.Value?.Mapset == Item ? Colors.MainAccent : Color.White;

            var lengthColumn = container.SongContainer.Columns.Find(x => x.Type == MusicControllerTableColumnType.Length);

            var currTime = new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(Item.Maps.First().SongLength);
            Length.Text = currTime.ToString("mm:ss");
            Length.TruncateWithEllipsis((int) lengthColumn.Container.Width - 50);
            Length.X = lengthColumn.Container.X + lengthColumn.Header.X;
            Length.Tint = MapManager.Selected.Value?.Mapset == Item ? Colors.MainAccent : Color.White;

            Button.Alpha = MapManager.Selected.Value?.Mapset == Item ? 0.35f : 0;
        }

        /// <summary>
        /// </summary>
        private void CreateTitle()
        {
            Title = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateArtist()
        {
            Artist = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDateAdded()
        {
            DateAdded = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true
            };
        }

        private void CreateBpm()
        {
            Bpm = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true
            };
        }

        private void CreateLength()
        {
            Length = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true
            };
        }
    }
}