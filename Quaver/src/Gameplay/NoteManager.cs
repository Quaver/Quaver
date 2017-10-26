using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Logging;

namespace Quaver.Gameplay
{
    /// <summary>
    /// This class handles the interaction between note and input.
    /// </summary>
    class NoteManager
    {
        //Hit Timing Variables
        private static int[] HitTiming { get; } = new int[5] {32, 56, 80, 100, 120};
        private static string[] TimingNames { get; } = new string[5]{"MARV","PERF","GREAT","GOOD","BAD"};

        //Temp
        private static Color[] TimingColors { get;  } = new Color[5]{Color.White,Color.LightBlue,Color.LightGreen,Color.Yellow,Color.Red};


        /// <summary>
        /// This method gets called when a key gets pressed.
        /// </summary>
        /// <param name="keyLane"></param>
        internal static void Input(int keyLane, bool keyDown)
        {
            if (keyDown)
            {
                //Do key press stuff
                int noteIndex = -1;
                int i;
                for (i = 0; i < NoteRendering.HitObjectPoolSize && i < NoteRendering.HitObjectPool.Count; i++)
                {
                    if (NoteRendering.HitObjectPool[i].KeyLane == keyLane + 1 && NoteRendering.HitObjectPool[i].StartTime - Timing.CurrentSongTime > -HitTiming[4])
                    {
                        noteIndex = i;
                        break;
                    }
                }

                if (noteIndex > -1)
                {
                    for (i = 0; i < 5; i++)
                    {
                        if (Math.Abs(NoteRendering.HitObjectPool[noteIndex].StartTime - Timing.CurrentSongTime) <= HitTiming[i])
                        {
                            LogTracker.QuickLog("NOTE INDEX: "+ noteIndex + " "+TimingNames[i], TimingColors[i], 0.5f);
                            NoteRendering.RecycleNote(noteIndex); //TODO: Add to LN queue instead of recycling early
                            break;
                        }
                    }
                }
            }
            else
            {
                //Do key release stuff
            }
        }
    }
}
