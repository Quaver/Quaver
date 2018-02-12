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
            int targetPoolSize = (int)(40 * GameBase.WindowUIScale / GameBase.WindowRectangle.Height) + 10;
            SongSelectButton newButton = null;

            for (var i = 0; i < targetPoolSize || i < GameBase.Mapsets.Count; i++)
            {
                newButton = new SongSelectButton(null, GameBase.WindowUIScale)
                {
                    Image = GameBase.UI.BlankBox,
                    //Alignment = Alignment.TopRight,
                    //Position = new UDim2(-5, OrganizerSize),
                    //Parent = Boundary
                };
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
    }
}
