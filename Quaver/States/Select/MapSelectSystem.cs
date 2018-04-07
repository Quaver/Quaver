using System;
using Quaver.Database.Maps;
using Quaver.GameState;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.States.Select
{
    /// <summary>
    ///     Insert song sorting + other map organizing tools here
    /// </summary>
    internal class MapSelectSystem : IGameStateComponent
    {
        private ButtonOrganizer ButtonOrganizer = new ButtonOrganizer();

        private QuaverContainer Boundary { get; set; }

        public bool ScrollingDisabled { get; set; }

        public object TogglePitch { get; private set; }

        private float OrganizerSize { get; set; }

        private float TargetPosition { get; set; }

        private int SelectedMapIndex { get; set; } = 0;

        private float SelectedMapTween { get; set; } = 0;

        /// <summary>
        ///     Keeps track if this state has already been loaded. (Used for audio loading.)
        /// </summary>
        private bool FirstLoad { get; set; }

        public void Draw()
        {
            ButtonOrganizer.Draw();
            Boundary.Draw();
        }

        public void Initialize(IGameState state)
        {
            Boundary = new QuaverContainer();

            ButtonOrganizer.Initialize(state);

            if (GameBase.SelectedMap == null)
                MapsetHelper.SelectRandomMap();
        }

        public void UnloadContent()
        {
            ButtonOrganizer.UnloadContent();
            Boundary.Destroy();
        }

        public void Update(double dt)
        {
            ButtonOrganizer.Update(dt);
            Boundary.Update(dt);
        }
    }
}
