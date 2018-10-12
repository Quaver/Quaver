using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Quaver.Skinning;

namespace Quaver.Screens.Gameplay.Rulesets.Keys
{
    public class GameplayRulesetKeys : GameplayRuleset
    {
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
        public GameplayRulesetKeys(GameplayScreen screen, Qua map) : base(screen, map)
        {
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
        /// <param name="info"></param>
        /// <returns></returns>
        protected override GameplayHitObject CreateHitObject(HitObjectInfo info)
        {
            var playfield = (GameplayPlayfieldKeys)Playfield;
            var objectManager = (HitObjectManagerKeys)HitObjectManager;

            // Create the new HitObject.
            var hitObject = new GameplayHitObjectKeys(this, info)
            {
                Width = playfield.LaneSize,
                OffsetYFromReceptor = Screen.Positioning.GetPositionFromTime(info.StartTime)
            };

            // Calculate position & offset from the receptor.
            // TODO: Handle SV's.
            hitObject.PositionY = hitObject.OffsetYFromReceptor + objectManager.HitPositionOffset;

            // Get Note Snapping
            if (SkinManager.Skin.Keys[Map.Mode].ColorObjectsBySnapDistance)
                hitObject.SnapIndex = GameplayHitObject.GetBeatSnap(info, hitObject.GetTimingPoint(Map.TimingPoints));

            // Disregard non-long note objects after this point, so we can initailize them separately.

            // todo: remove. debugging
            //Console.Out.WriteLine("------------------------------------");
            //Console.Out.WriteLine("START: " + hitObject.OffsetYFromReceptor);

            if (!hitObject.IsLongNote)
                return hitObject;

            // TODO: Handle SV's.
            // (OLD) hitObject.LongNoteOffsetYFromReceptor = info.EndTime;
            hitObject.LongNoteOffsetYFromReceptor = Screen.Positioning.GetPositionFromTime(info.EndTime);

            hitObject.InitialLongNoteSize = (long)((hitObject.LongNoteOffsetYFromReceptor - hitObject.OffsetYFromReceptor) * HitObjectManagerKeys.ScrollSpeed);
            hitObject.CurrentLongNoteSize = hitObject.InitialLongNoteSize;

            // todo: remove. debugging
            //Console.Out.WriteLine("END: " + hitObject.LongNoteOffsetYFromReceptor);

            return hitObject;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override HitObjectManager CreateHitObjectManager() => new HitObjectManagerKeys(this, 255);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override IGameplayInputManager CreateInputManager() => new KeysInputManager(this, Map.Mode);
    }
}
