using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.GameState.States;
using Quaver.Logging;

namespace Quaver.Gameplay
{
    /// <summary>
    /// This class handles the interaction between note and input.
    /// </summary>
    class NoteManager
    {
        public PlayScreenState PlayScreen { get; set; }

        public void Initialize(PlayScreenState playScreen)
        {
            PlayScreen = playScreen;
        }

        /// <summary>
        /// This method gets called when a key gets pressed.
        /// </summary>
        /// <param name="keyLane"></param>
        public void Input(int keyLane, bool keyDown)
        {
            // Update Receptor in Playfield
            PlayScreen.Playfield.UpdateReceptor(keyLane, keyDown);

            //Check for Note press/LN press
            if (keyDown)
            {
                //Reference Variables
                int noteIndex = -1;
                int i;

                //Search for closest HitObject that is inside the HitTiming Window
                for (i = 0; i < PlayScreen.NoteRendering.HitObjectPoolSize && i < PlayScreen.NoteRendering.HitObjectPool.Count; i++)
                {
                    if (PlayScreen.NoteRendering.HitObjectPool[i].KeyLane == keyLane + 1 && PlayScreen.NoteRendering.HitObjectPool[i].StartTime - PlayScreen.Timing.CurrentSongTime > -PlayScreen.ScoreManager.HitWindow[4])
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
                        if (Math.Abs(PlayScreen.NoteRendering.HitObjectPool[noteIndex].StartTime - PlayScreen.Timing.CurrentSongTime) <= PlayScreen.ScoreManager.HitWindow[i])
                        {
                            //Score manager stuff
                            PlayScreen.ScoreManager.Count(i, false, PlayScreen.NoteRendering.HitObjectPool[noteIndex].StartTime - PlayScreen.Timing.CurrentSongTime);
                            PlayScreen.GameplayUI.UpdateAccuracyBox(i);
                            PlayScreen.Playfield.UpdateJudge(i, false, PlayScreen.NoteRendering.HitObjectPool[noteIndex].StartTime - PlayScreen.Timing.CurrentSongTime);

                            // If the player is spamming
                            if (i >= 3)
                                PlayScreen.NoteRendering.KillNote(noteIndex);
                            else
                            {
                                //If the object is an LN, hold it at the receptors
                                if (PlayScreen.NoteRendering.HitObjectPool[noteIndex].IsLongNote) PlayScreen.NoteRendering.HoldNote(noteIndex);

                                //If the object is not an LN, recycle it.
                                else PlayScreen.NoteRendering.RecycleNote(noteIndex);
                            }

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
                for (i = 0; i < PlayScreen.NoteRendering.HitObjectHold.Count; i++)
                {
                    if (PlayScreen.NoteRendering.HitObjectHold[i].KeyLane == keyLane + 1)
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
                    //Only checks MARV/PERF/GREAT/GOOD
                    int releaseTiming = -1;
                    for (i = 0; i < 4; i++)
                    {
                        if (Math.Abs(PlayScreen.NoteRendering.HitObjectHold[noteIndex].EndTime - PlayScreen.Timing.CurrentSongTime) <= PlayScreen.ScoreManager.HitWindow[i] * 1.25f)
                        {
                            releaseTiming = i;
                            break;
                        }
                    }

                    //If LN has been released during a HitWindow
                    if (releaseTiming > -1)
                    {
                        PlayScreen.ScoreManager.Count(i, true);
                        PlayScreen.GameplayUI.UpdateAccuracyBox(i);
                        PlayScreen.Playfield.UpdateJudge(i, true);
                        PlayScreen.NoteRendering.KillHold(noteIndex,true);
                    }
                    //If LN has been missed
                    else
                    {
                        PlayScreen.ScoreManager.Count(4, true);
                        PlayScreen.GameplayUI.UpdateAccuracyBox(4);
                        PlayScreen.Playfield.UpdateJudge(4, true);
                        PlayScreen.NoteRendering.KillHold(noteIndex);
                    }
                }
            }
        }
    }
}
