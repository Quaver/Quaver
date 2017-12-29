using Quaver.Graphics.Sprite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.Gameplay.PlayScreen
{
    class MeasureBarManager : IHelper
    {
        internal ulong TrackPosition { get; set; }
        Boundary Boundary { get; set; }

        public void Draw()
        {
            Boundary.Draw();
        }

        public void Initialize(IGameState state)
        {
            Boundary = new Boundary();
            CreateBarQueue();
        }

        public void UnloadContent()
        {
            Boundary.Destroy();
        }

        public void Update(double dt)
        {
            Boundary.Update(dt);
        }

        //Creates timing bars (used to measure 16 beats)
        internal void CreateBarQueue()
        {
            /*
            int i = 0;
            if (!mod_split && _config_timingBars)
            {
                float curBarTime = 0;
                for (i = 0; i < TimingQueue.Count; i++)
                {
                    curBarTime = TimingQueue[i].StartTime;

                    if (_barQueue.Count > 0 && _barQueue[0].StartTime + 2 > curBarTime) _barQueue.RemoveAt(0);
                    curBarTime += 1000f * 4f * 60f / (TimingQueue[i].BPM);
                    TimingObject curTiming;

                    if (i + 1 < TimingQueue.Count)
                    {
                        while (curBarTime < TimingQueue[i + 1].StartTime)
                        {
                            curTiming = new TimingObject();
                            curTiming.StartTime = (int)(curBarTime);
                            _barQueue.Add(curTiming);
                            curBarTime += 1000f * 4f * 60f / (TimingQueue[i].BPM);
                        }
                    }
                    else
                    {
                        while (curBarTime < _songAudio.clip.length * 1000f)
                        {
                            curTiming = new TimingObject();
                            curTiming.StartTime = (int)(curBarTime);
                            _barQueue.Add(curTiming);
                            curBarTime += 1000f * 4f * 60f / (TimingQueue[i].BPM);
                        }
                    }
                }

                //Create all bars in music
                List<TimingObject> tempBars = new List<TimingObject>();
                for (i = 0; i < _barQueue.Count; i++)
                {
                    TimingObject hoo = new TimingObject();

                    hoo.StartTime = _barQueue[i].StartTime;
                    tempBars.Add(hoo);
                }
                _barQueue = new List<TimingObject>(tempBars);

                //Create starting bars
                _activeBars = new List<TimingObject>();
                for (i = 0; i < maxNoteCount; i++)
                {
                    if (_barQueue.Count > 0) _activeBarObjects[i] = time_InstantiateBar(null);
                    else break;
                }

            }
            */
        }

        //Creates a bar object
        /*
        private GameObject CreateBarObjects(GameObject curBar)
        {

            if (curBar == null)
            {
                curBar = Instantiate(timingBar, hitContainer.transform);
                curBar.transform.localScale = new Vector3(((float)(skin_columnSize + skin_bgMaskBufferSize + skin_noteBufferSpacing) / config_PixelUnitSize) * 4f * (config_PixelUnitSize / (float)curBar.transform.GetComponent<SpriteRenderer>().sprite.rect.size.x),
                    (config_PixelUnitSize / (float)curBar.transform.GetComponent<SpriteRenderer>().sprite.rect.size.y) * ((float)skin_timingBarPixelSize / config_PixelUnitSize)
                    , 1f);
            }
            TimingObject newTimeOb = new TimingObject();
            newTimeOb.StartTime = _barQueue[0].StartTime;
            newTimeOb.TimingBar = curBar;

            curBar.transform.localPosition = new Vector3(0f, PosFromSV(newTimeOb.StartTime), 2f);

            _activeBars.Add(newTimeOb);
            _barQueue.RemoveAt(0);
            return curBar;
        }
        */

        //Recycles bar objects from object pool or destroys them
        /*
        private void time_RemoveBar(GameObject curBar)
        {
            if (_barQueue.Count > 0) time_InstantiateBar(curBar);
            else Destroy(curBar);
        }*/
    }
}
