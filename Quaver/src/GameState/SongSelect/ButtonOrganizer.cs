using Quaver.Graphics.Button;
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
        private List<SongSelectButton> SongSelectButtons { get; set; } = new List<SongSelectButton>();

        private List<EventHandler> SongSelectEvents { get; set; } = new List<EventHandler>();

        // Indexing
        private int SongButtonPoolSize { get; set; }
        private int BeatmapStartIndex { get; set; }
        private int SelectedBeatmapIndex { get; set; }

        private double DiffButtonsStart { get; set; }
        private double DiffButtonsEnd { get; set; }


        public void Initialize(IGameState state)
        {
            throw new NotImplementedException();
        }

        public void UnloadContent()
        {
            throw new NotImplementedException();
        }

        public void Update(double dt)
        {
            throw new NotImplementedException();
        }

        public void Draw()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Generates a button pool depending on your screen size
        /// </summary>
        public void GenerateButtonPool()
        {

        }

        /// <summary>
        ///     Will shift buttons down by 1 seemlessly
        /// </summary>
        /// <param name="amount"></param>
        public void ShiftButtonPool(int amount)
        {

        }
    }
}
