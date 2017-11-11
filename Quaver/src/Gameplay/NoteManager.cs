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
        internal static string[] TimingNames { get; } = new string[5]{"MARV","PERF","GREAT","GOOD","BAD"};

        //Temp
        private static Color[] TimingColors { get;  } = new Color[5]{Color.White,Color.LightBlue,Color.LightGreen,Color.Yellow,Color.Magenta};


        /// <summary>
        /// This method gets called when a key gets pressed.
        /// </summary>
        /// <param name="keyLane"></param>
        internal static void Input(int keyLane, bool keyDown)
        {
            //Check for Note press/LN press
            if (keyDown)
            {
                //Reference Variables
                int noteIndex = -1;
                int i;

                //Search for closest HitObject that is inside the HitTiming Window
                for (i = 0; i < NoteRendering.HitObjectPoolSize && i < NoteRendering.HitObjectPool.Count; i++)
                {
                    if (NoteRendering.HitObjectPool[i].KeyLane == keyLane + 1 && NoteRendering.HitObjectPool[i].StartTime - Timing.CurrentSongTime > -ScoreManager.HitWindow[4])
                    {
                        noteIndex = i;
                        break;
                    }
                }

                //If such HitObject exists, it will do key-press stuff to it
                if (noteIndex > -1)
                {
                    //Check which HitWindow this object's timing is in
                    for (i = 0; i < 5; i++)
                    {
                        if (Math.Abs(NoteRendering.HitObjectPool[noteIndex].StartTime - Timing.CurrentSongTime) <= ScoreManager.HitWindow[i])
                        {
                            //Score manager stuff
                            //TODO: proper judge display
                            Playfield.judgeSprite.Text = "PRESS "+TimingNames[i];
                            Playfield.judgeSprite.TextColor = TimingColors[i];
                            //LogManager.QuickLog("NOTE INDEX: PRESS "+ noteIndex + ", "+TimingNames[i], TimingColors[i], 0.5f);
                            ScoreManager.Count(i, NoteRendering.HitObjectPool[noteIndex].StartTime - Timing.CurrentSongTime);

                            //If the object is an LN, hold it at the receptors
                            if (NoteRendering.HitObjectPool[noteIndex].IsLongNote) NoteRendering.HoldNote(noteIndex);

                            //If the object is not an LN, recycle it.
                            else  NoteRendering.RecycleNote(noteIndex);
                            break;
                        }
                    }
                }
            }
            //Check for LN release
            else
            {
                //Reference Variables
                int noteIndex = -1;
                int i;

                //Search for closest HitObject that is inside the HitTiming Window
                for (i = 0; i < NoteRendering.HitObjectHold.Count; i++)
                {
                    if (NoteRendering.HitObjectHold[i].KeyLane == keyLane + 1)
                    {
                        noteIndex = i;
                        break;
                    }
                }

                //If such HitObject exists, it will do key-press stuff to it
                if (noteIndex > -1)
                {
                    //Check which HitWindow this object's timing is in.
                    //Since it's an LN, the hit window is increased by 1.25x.
                    //Only checks MARV/PERF/GREAT
                    int releaseTiming = -1;
                    for (i = 0; i < 3; i++)
                    {
                        if (Math.Abs(NoteRendering.HitObjectHold[noteIndex].EndTime - Timing.CurrentSongTime) <= ScoreManager.HitWindow[i] * 1.25f)
                        {
                            releaseTiming = i;
                            break;
                        }
                    }

                    //If LN has been missed
                    if (releaseTiming > -1)
                    {
                        //TODO: proper judge display
                        //LogManager.QuickLog("NOTE INDEX: RELEASE " + noteIndex + ", " + TimingNames[releaseTiming], TimingColors[releaseTiming], 0.5f);
                        Playfield.judgeSprite.Text = "RELEASE "+TimingNames[releaseTiming];
                        Playfield.judgeSprite.TextColor = TimingColors[releaseTiming];
                        ScoreManager.Count(i);
                        NoteRendering.KillHold(noteIndex,true);
                    }
                    //If LN has been released during a HitWindow
                    else
                    {
                        //TODO: proper judge display
                        //LogManager.QuickLog("NOTE INDEX: RELEASE " + noteIndex + ", EARLY", TimingColors[4], 0.5f);
                        Playfield.judgeSprite.Text = "RELEASE "+TimingNames[4];
                        Playfield.judgeSprite.TextColor = TimingColors[4];
                        ScoreManager.Count(3);
                        NoteRendering.KillHold(noteIndex);
                    }
                }
            }
        }
    }
}
