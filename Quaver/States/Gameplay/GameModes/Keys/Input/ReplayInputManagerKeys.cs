using System;
using System.Collections.Generic;
using System.ComponentModel;
using Quaver.API.Enums;
using Quaver.States.Gameplay.Replays;

namespace Quaver.States.Gameplay.GameModes.Keys.Input
{
    internal class ReplayInputManagerKeys
    {
        private GameplayScreen Screen { get; }
        
        internal Replay Replay { get; }

        internal List<KeysInputBinding> BindingStore { get; }

        internal int CurrentFrame { get; private set; } = 1;

        internal List<bool> UniquePresses { get; }
        internal List<bool> UniqueReleases { get; }

        internal ReplayInputManagerKeys(GameplayScreen screen, Replay replay)
        {
            Screen = screen;
            Replay = replay;
            BindingStore = new List<KeysInputBinding>();
            UniquePresses = new List<bool>();
            UniqueReleases = new List<bool>();

            for (var i = 0; i < screen.Map.FindKeyCountFromMode(); i++)
            {
                BindingStore.Add(new KeysInputBinding());
                UniquePresses.Add(false);
                UniqueReleases.Add(false);
            }
        }
      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        internal void Update(double dt)
        {
            if (CurrentFrame >= Replay.Frames.Count || !(Screen.Timing.CurrentTime >= Replay.Frames[CurrentFrame].Time)) 
                return;
          
            // Get active keys in both the current and previous frames.
            var previousActive = Replay.KeyPressStateToLanes(Replay.Frames[CurrentFrame - 1].Keys);
            var currentActive = Replay.KeyPressStateToLanes(Replay.Frames[CurrentFrame].Keys);

            foreach (var activeLane in currentActive)
            {
                if (!previousActive.Contains(activeLane))
                    UniquePresses[activeLane] = true;
            }
            
            foreach (var activeLane in previousActive)
            {
                if (!currentActive.Contains(activeLane))
                    UniqueReleases[activeLane] = true;
            }

            CurrentFrame++;
        }
    }
}