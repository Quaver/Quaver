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
        private Boundary Boundary { get; set; }
        internal List<BarObject> BarObjectQueue { get; set; }
        internal List<BarObject> BarObjectActive { get; set; }
        internal float PlayfieldSize { get; set; }

        public void Draw()
        {
            Boundary.Draw();
        }

        public void Initialize(IGameState state)
        {
            Boundary = new Boundary(0, 0, PlayfieldSize, null)
            {
                Alignment = Graphics.Alignment.MidCenter
            };
            BarObjectQueue = new List<BarObject>();
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
                    var newBar = new BarObject()
                    {
                        OffsetFromReceptor = (ulong)curTime
                    };
                    BarObjectQueue.Add(newBar);
                    curTime += bpmInterval;
                }
            }
            Console.WriteLine("Total Timing Bars: " + BarObjectQueue.Count);

            //todo: remove this. temp
            BarObjectActive = BarObjectQueue;
            for (var i=0; i< BarObjectActive.Count; i++)
            {
                BarObjectActive[i].Initialize(Boundary, 2, 0);
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
