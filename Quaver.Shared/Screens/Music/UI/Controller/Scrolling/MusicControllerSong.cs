using System;
using System.Collections.Generic;
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
        /// <summary>
        /// </summary>
        public static int SongHeight => 53;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = SongHeight;

        /// <summary>
        /// </summary>
        private SpriteTextPlus Title { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Artist { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Creator { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Bpm { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Length { get; set; }

        /// <summary>
        /// </summary>
        private ImageButton Button { get; set; }

        /// <summary>
        /// </summary>
        private Sprite BottomLine { get; set; }

        /// <summary>
        /// </summary>
        private Dictionary<MusicControllerTableColumnType, SpriteTextPlus> ColumnData { get; set; }

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

            CreateButton();
            CreateTitle();
            CreateArtist();
            CreateDateAdded();
            CreateBpm();
            CreateLength();
            CreateBottomLine();

            // Allows for automatic positioning of the data within the columns
            ColumnData = new Dictionary<MusicControllerTableColumnType, SpriteTextPlus>
            {
                {MusicControllerTableColumnType.Title, Title},
                {MusicControllerTableColumnType.Artist, Artist},
                {MusicControllerTableColumnType.Creator, Creator},
                {MusicControllerTableColumnType.BPM, Bpm},
                {MusicControllerTableColumnType.Length, Length}
            };

            MapManager.Selected.ValueChanged += OnMapChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleButtonAnimations();
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;

            base.Destroy();
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

            ScheduleUpdate(() =>
            {
                Tint = Index % 2 == 0 ? ColorHelper.HexToColor("#363636") : ColorHelper.HexToColor("#242424");

                Title.Text = item.Title;
                Artist.Text = item.Artist;
                Creator.Text = item.Creator;
                Bpm.Text = $"{(int) Item.Maps.First().Bpm}".ToString();
                Length.Text = $"{new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(Item.Maps.First().SongLength):mm:ss}";

                var container = (MusicControllerScrollContainer) Container;

                // Truncate and position the text
                foreach (var data in ColumnData)
                {
                    var column = container.SongContainer.Columns.Find(x => x.Type == data.Key);

                    data.Value.TruncateWithEllipsis((int) column.Container.Width - 50);
                    data.Value.X = column.Container.X + column.Header.X;
                    // data.Value.Tint = MapManager.Selected.Value?.Mapset == Item ? ColorHelper.HexToColor("#57D6FF") : Color.White;
                }

                Button.Alpha = MapManager.Selected.Value?.Mapset == Item ? 0.35f : 0;
            });
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
            Creator = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateBpm()
        {
            Bpm = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateLength()
        {
            Length = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateButton() => Button = new MusicControllerSongButton(Container, UserInterface.BlankBox,
            (o, e) =>
            {
                if (MapManager.Selected.Value?.Mapset == Item.Maps.First().Mapset)
                    return;

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

        /// <summary>
        /// </summary>
        private void CreateBottomLine() => BottomLine = new Sprite
        {
            Parent = this,
            UsePreviousSpriteBatchOptions = true,
            Alignment = Alignment.BotCenter,
            Size = new ScalableVector2(Width - 50, 1),
            Alpha = 0.45f
        };

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e) => UpdateContent(Item, Index);

        /// <summary>
        /// </summary>
        private void HandleButtonAnimations()
        {
            if (MapManager.Selected.Value?.Mapset == Item)
            {
                Button.Tint = new Color(87, 214, 255);
                Button.Alpha = 0.45f;
            }
            else if (Button.IsHovered)
            {
                Button.Tint = Color.White;
                Button.Alpha = 0.45f;
            }
            else
                Button.Alpha = 0;
        }
    }
}