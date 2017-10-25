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

                int nIndex = -1;
                int i;
                for (i = 0; i < NoteRendering.HitObjectPoolSize && i < NoteRendering.HitObjectQueue.Count; i++)
                {
                    if (NoteRendering.HitObjectQueue[i].KeyLane == keyLane + 1 && NoteRendering.HitObjectQueue[i].StartTime - Timing.CurrentSongTime > -HitTiming[4])
                    {
                        nIndex = i;
                        break;
                    }
                }

                if (nIndex > -1)
                {
                    for (i = 0; i < 5; i++)
                    {
                        if (Math.Abs(NoteRendering.HitObjectQueue[nIndex].StartTime - Timing.CurrentSongTime) <= HitTiming[i])
                        {
                            LogTracker.QuickLog("NOTE INDEX: "+nIndex+" "+TimingNames[i], TimingColors[i], 0.5f);
                            NoteRendering.RecycleNote(nIndex); //TODO: Add to LN queue instead of recycling early
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
