using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Structures;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Modifiers;
using Quaver.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Screens.Gameplay.Rulesets.Input;
using Quaver.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Screens.Gameplay.Rulesets.Keys.Playfield.Lines;
using Quaver.Skinning;

namespace Quaver.Screens.Gameplay.Rulesets.Keys
{
    public class GameplayRulesetKeys : GameplayRuleset
    {
        /// <summary>
        ///     Reference to the timing line manager.
        ///
        ///     It gets initialized in GameplayRulesetKeys because it relies on both
        ///     the playfield and the HitObjectManager.
        ///
        ///     We can't intiialize it in Playfield as that gets created first.
        /// </summary>
        public TimingLineManager TimingLineManager { get; }

        /// <summary>
        ///     Dictates if we are currently using downscroll or not.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static bool IsDownscroll
        {
            get
            {
                switch (MapManager.Selected.Value.Qua.Mode)
                {
                    case GameMode.Keys4:
                        return ConfigManager.DownScroll4K.Value;
                    case GameMode.Keys7:
                        return ConfigManager.DownScroll7K.Value;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="map"></param>
        public GameplayRulesetKeys(GameplayScreen screen, Qua map) : base(screen, map) => TimingLineManager = CreateTimingLineManager();

        /// <inheritdoc />
        /// <summary>
        ///
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!Screen.Failed && !Screen.IsPaused)
                TimingLineManager.UpdateObjectPool();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        protected override ScoreProcessor CreateScoreProcessor(Qua map) => new ScoreProcessorKeys(map, ModManager.Mods);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void CreatePlayfield() => Playfield = new GameplayPlayfieldKeys(Screen, this);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override HitObjectManager CreateHitObjectManager() => new HitObjectManagerKeys(this, Map);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override IGameplayInputManager CreateInputManager() => new KeysInputManager(this, Map.Mode);

        /// <summary>
        ///     Create the Timing Line Objects Manager
        /// </summary>
        /// <returns></returns>
        private TimingLineManager CreateTimingLineManager() => new TimingLineManager(this);
    }
}
