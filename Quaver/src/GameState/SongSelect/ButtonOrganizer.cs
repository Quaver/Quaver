using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.SongSelect
{
    class ButtonOrganizer : IHelper
    {
        /// <summary>
        ///     Reference to the list of song select buttons
        /// </summary>
        private List<MapsetSelectButton> SongSelectButtons { get; set; } = new List<MapsetSelectButton>();

        private List<MapDifficultySelectButton> DiffSelectButtons { get; set; } = new List<MapDifficultySelectButton>();

        private List<EventHandler> SongSelectEvents { get; set; } = new List<EventHandler>();

        private Boundary Boundary { get; set; }

        private int SelectedSongIndex { get; set; }
        private int SelectedDiffIndex { get; set; }

        /// <summary>
        ///     Size of the button sorter. It is determined by how much buttons will be displayed on screen.
        /// </summary>
        private float OrganizerSize { get; set; }

        // Indexing
        private int SongButtonPoolSize { get; set; }
        private int BeatmapStartIndex { get; set; }
        private int SelectedBeatmapIndex { get; set; }

        private double DiffButtonsStart { get; set; }
        private double DiffButtonsEnd { get; set; }


        public void Initialize(IGameState state)
        {
            Boundary = new Boundary();
            GenerateButtonPool();
        }

        public void UnloadContent()
        {
            Boundary.Destroy();
        }

        public void Update(double dt)
        {
            Boundary.Update(dt);
        }

        public void Draw()
        {
            Boundary.Draw();
        }

        /// <summary>
        ///     Generates a button pool depending on your screen size
        /// </summary>
        public void GenerateButtonPool()
        {
            int targetPoolSize = (int)(40 * GameBase.WindowUIScale / GameBase.WindowRectangle.Height) + 10;
            MapsetSelectButton newButton = null;

            for (var i = 0; i < targetPoolSize && i < GameBase.Mapsets.Count; i++)
            {
                newButton = new MapsetSelectButton(GameBase.WindowUIScale, i, GameBase.Mapsets[i])
                {
                    Image = GameBase.UI.BlankBox,
                    Alignment = Alignment.TopRight,
                    Position = new UDim2(-5, OrganizerSize + 50 ), // todo: +50 is temp, add buffer spacing later for boundary/songselectUI overlap
                    Parent = Boundary
                };

                OrganizerSize += newButton.SizeY;
                //todo: use index for song select button
            }
        }

        /// <summary>
        ///     Will shift buttons down by 1 seemlessly
        /// </summary>
        /// <param name="amount"></param>
        public void ShiftButtonPool(int amount)
        {

        }

        private void SongSelectButtonClicked(int index)
        {
            // if index == SelectedSongIndex, remove diff select buttons + set selected song index to null
            SelectedSongIndex = index;
        }

        private void DiffSelectButtonClicked(int index)
        {
            // if index == SelectedDiffIndex, play map
            SelectedDiffIndex = index;
        }
    }
}
