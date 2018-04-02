using System.Collections.Generic;
using Quaver.GameState;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;

namespace Quaver.States.Gameplay.Mania.UI.Measures
{
    internal class ManiaMeasureBarManager : IGameStateComponent
    {
        internal ulong TrackPosition { get; set; }
        private QuaverContainer QuaverContainer { get; set; }
        internal List<ManiaBarObject> BarObjectQueue { get; set; }
        internal List<ManiaBarObject> BarObjectActive { get; set; }
        internal float PlayfieldSize { get; set; }

        public void Draw()
        {
            QuaverContainer.Draw();
        }

        public void Initialize(IGameState state)
        {
            QuaverContainer = new QuaverContainer(0, 0, PlayfieldSize, null)
            {
                Alignment = Alignment.MidCenter
            };
            BarObjectQueue = new List<ManiaBarObject>();
            CreateBarQueue();
        }

        public void UnloadContent()
        {
            QuaverContainer.Destroy();
        }

        public void Update(double dt)
        {
            QuaverContainer.Update(dt);
        }

        //Creates timing bars (used to measure 16 beats)
        internal void CreateBarQueue()
        {
            /*
            for (var i = 0; i < GameBase.SelectedBeatmap.Qua.TimingPoints.Count; i++)
            {
                var startTime = GameBase.SelectedBeatmap.Qua.TimingPoints[i].StartTime;
                var endTime = 0f;
                var curTime = startTime;
                var bpmInterval = GameBase.SelectedBeatmap.Qua.TimingPoints[i].Bpm * 1000 / 60;

                if (i + 1 < GameBase.SelectedBeatmap.Qua.TimingPoints.Count)
                    endTime = GameBase.SelectedBeatmap.Qua.TimingPoints[i + 1].StartTime;
                else
                    endTime = GameBase.SelectedBeatmap.SongLength;

                while (curTime < endTime - 1)
                {
                    var newBar = new ManiaBarObject()
                    {
                        OffsetFromReceptor = (ulong)curTime
                    };
                    BarObjectQueue.Add(newBar);
                    curTime += bpmInterval;
                }
            }
            Console.WriteLine("Total ManiaTiming Bars: " + BarObjectQueue.Count);

            //todo: remove this. temp
            BarObjectActive = BarObjectQueue;
            for (var i=0; i< BarObjectActive.Count; i++)
            {
                BarObjectActive[i].Initialize(QuaverContainer, 2, 0);
            }*/
        }

        internal void RecycleBar()
        {
            //BarObjectQueue
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
            ManiaTimingObject newTimeOb = new ManiaTimingObject();
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
