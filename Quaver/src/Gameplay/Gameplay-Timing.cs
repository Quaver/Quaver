using Quaver.GameState;

namespace Quaver.Gameplay
{
    internal partial class StatePlayScreen : GameStateBase
    {
        //Audio Variables
        private float tempAudioLength = 20000; //TEMP
        private float _gameAudioPosition;

        //Gameplay Variables
        private const int _playStartDelayed = 500;

        private float _actualSongTime;
        private float _currentSongTime;
        private int _curSVPart;

        //SV + Timing Point Variables
        //private List<TimingObject> _svQueue, _timingQueue, _barQueue, _activeBars;
        //private GameObject[] _activeBarObjects;
        private ulong _curSVPos;

        private float _songEndOffset;

        //Audio File Variables
        private bool _songIsDone;

        private ulong[] _svCalc; //Stores SV position data for efficiency
        private float _averageBpm = 100;

        /// <summary>
        ///     Initialize Timing Contents.
        /// </summary>
        internal void InitializeTiming()
        {
            //TODO: Timing Initializer

            //Reference Variables
            var i = 0;
            _curSVPos = (ulong) (-_playStartDelayed + 10000f); //10000ms added since curSVPos is a uLong
            _curSVPart = 0;

            //Declare Other Values
            _currentSongTime = -_playStartDelayed;
            _actualSongTime = -_playStartDelayed / 1000f;
            //_activeBarObjects = new GameObject[maxNoteCount];

            //Create Timing Points + SVs on a list
            //TimingObject newTime;
            //_svQueue = new List<TimingObject>();
            /*
            for (i = 0; i < _qFile.SliderVelocities.Count; i++)
            {
                newTime = new TimingObject();
                newTime.StartTime = _qFile.SliderVelocities[i].StartTime;
                newTime.Multiplier = _qFile.SliderVelocities[i].Multiplier;
                _svQueue.Add(newTime);
            }

            _timingQueue = new List<TimingObject>();
            for (i = 0; i < _qFile.TimingPoints.Count; i++)
            {
                newTime = new TimingObject();
                newTime.StartTime = _qFile.TimingPoints[i].StartTime;
                newTime.BPM = _qFile.TimingPoints[i].BPM;
                _timingQueue.Add(newTime);
            }*/

            //Calculate Average BPM
            time_CalculateAverageBpm();

            //Create SVs
            /*if (!mod_noSV && _svQueue.Count > 1)
            {
                time_ConvertTPtoSV();
                time_NormalizeSVs();
            }
            //If there's no SV, create a single SV Point
            else
            {
                TimingObject newTp = new TimingObject();
                newTp.StartTime = 0;
                newTp.Multiplier = 1f;
                _svQueue.Add(newTp);
            }

            //Calculates SV for efficiency
            _svCalc = new ulong[_svQueue.Count];
            ulong svPosTime = 0;
            _svCalc[0] = 0;
            for (i = 0; i < _svQueue.Count; i++)
            {
                if (i + 1 < _svQueue.Count)
                {
                    ulong templ = (ulong)((_svQueue[i + 1].StartTime - _svQueue[i].StartTime) * _svQueue[i].Multiplier);
                    if (templ > 10000000) templ = 10000000;
                    svPosTime += templ;
                    _svCalc[i + 1] = svPosTime;
                }
                else break;
            }*/

            //Create Timing bars
            //_barQueue = new List<TimingObject>();
            //time_CreateBars();
        }

        //Set the position of the current play time
        private void SetCurrentSongTime(double dt)
        {
            //_gameAudioPosition = _gameAudio.GetAudioPosition()
            //Calculate Time after Song Done
            /*
            if (!_songIsDone && _gameAudioPosition < _songAudio.clip.length) _songAudio.UnPause();
            else if (_songIsDone) _SongEndOffset += Time.deltaTime;

            //Calculate Actual Song Time
            if (_songIsDone)
            {
                _ActualSongTime = _songAudio.clip.length + _SongEndOffset;
            }
            else
            {
                if (_ActualSongTime < 0) _ActualSongTime += Time.deltaTime;
                else _ActualSongTime = ((_songAudio.time) + (_ActualSongTime + Time.deltaTime)) / 2f;
            }
            _CurrentSongTime = (_ActualSongTime * 1000f) - _config_offset;
            */
            
        }

        private void time_DestroyBars()
        {
            //for (int i = 0; i < _activeBars.Count; i++) Destroy(_activeBars[i].TimingBar);
        }

        //Creates timing bars (used to measure 16 beats)
        private void time_CreateBars()
        {
            /*
            int i = 0;
            if (!mod_split && _config_timingBars)
            {
                float curBarTime = 0;
                for (i = 0; i < _timingQueue.Count; i++)
                {
                    curBarTime = _timingQueue[i].StartTime;

                    if (_barQueue.Count > 0 && _barQueue[0].StartTime + 2 > curBarTime) _barQueue.RemoveAt(0);
                    curBarTime += 1000f * 4f * 60f / (_timingQueue[i].BPM);
                    TimingObject curTiming;

                    if (i + 1 < _timingQueue.Count)
                    {
                        while (curBarTime < _timingQueue[i + 1].StartTime)
                        {
                            curTiming = new TimingObject();
                            curTiming.StartTime = (int)(curBarTime);
                            _barQueue.Add(curTiming);
                            curBarTime += 1000f * 4f * 60f / (_timingQueue[i].BPM);
                        }
                    }
                    else
                    {
                        while (curBarTime < _songAudio.clip.length * 1000f)
                        {
                            curTiming = new TimingObject();
                            curTiming.StartTime = (int)(curBarTime);
                            _barQueue.Add(curTiming);
                            curBarTime += 1000f * 4f * 60f / (_timingQueue[i].BPM);
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
        private GameObject time_InstantiateBar(GameObject curBar)
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

        //Creates SV Points
        private void time_ConvertTPtoSV()
        {
            /*
            //AverageBpm Reference Variables
            int i = 0;
            int j = 0;
            int hij = 0;

            //Create and converts timing points to SV's
            for (j = 0; j < _timingQueue.Count; j++)
            {
                if (_timingQueue[j].StartTime < _svQueue[0].StartTime - 0.01f)
                {
                    TimingObject newTp = new TimingObject();
                    newTp.StartTime = _timingQueue[j].StartTime;
                    if (_timingQueue[j].BPM == AverageBpm) newTp.Multiplier = 1;
                    else newTp.Multiplier = _svQueue[0].Multiplier;
                    _svQueue.Insert(0, newTp);
                }
                else if (_timingQueue[j].StartTime > _svQueue[_svQueue.Count - 1].StartTime + 0.01f)
                {
                    TimingObject newTp = new TimingObject();
                    newTp.StartTime = _timingQueue[j].StartTime;
                    newTp.Multiplier = _svQueue[_svQueue.Count - 1].Multiplier;
                    _svQueue.Add(newTp);
                }
                else
                {
                    for (i = hij; i < _svQueue.Count; i++)
                    {
                        if (i + 1 < _svQueue.Count && _timingQueue[j].StartTime < _svQueue[i + 1].StartTime)
                        {
                            if (Mathf.Abs(_timingQueue[j].StartTime - _svQueue[i].StartTime) > 1f)
                            {
                                TimingObject newTp = new TimingObject();
                                newTp.StartTime = _timingQueue[j].StartTime;
                                newTp.Multiplier = 1f;
                                _svQueue.Add(newTp);
                                hij = i;
                            }
                            break;
                        }
                    }
                }
            }
            */
        }

        //Calculate Average BPM of map
        private void time_CalculateAverageBpm()
        {
            /*
            //AverageBpm Reference Variables
            float longestBpmTime = 0;
            int avgBpmPos = 0;
            int i = 0;

            if (_timingQueue.Count > 1)
            {
                for (i = 0; i < _timingQueue.Count; i++)
                {
                    if (i + 1 < _timingQueue.Count)
                    {
                        if (_timingQueue[i + 1].StartTime - _timingQueue[i].StartTime > longestBpmTime)
                        {
                            avgBpmPos = i;
                            longestBpmTime = _timingQueue[i + 1].StartTime - _timingQueue[i].StartTime;
                        }
                    }
                    else if (i + 1 == _timingQueue.Count)
                    {
                        if ((_songAudio.clip.length * 1000f) - _timingQueue[i].StartTime > longestBpmTime)
                        {
                            avgBpmPos = i;
                            longestBpmTime = (_songAudio.clip.length * 1000f) - _timingQueue[i].StartTime;
                        }
                    }
                }
                AverageBpm = _timingQueue[avgBpmPos].BPM;
            }
            else AverageBpm = _timingQueue[0].BPM;
            */
        }

        //Normalizes SV's in between each BPM change interval
        private void time_NormalizeSVs()
        {
            /*
            //Reference Variables + Sort
            int i = 0;
            int hij = 0;
            _svQueue.Sort(delegate (TimingObject p1, TimingObject p2) { return p1.StartTime.CompareTo(p2.StartTime); });

            //Normalize
            if (_timingQueue.Count >= 1)
            {
                hij = 0;

                for (i = 0; i < _svQueue.Count; i++)
                {
                    if (hij + 1 < _timingQueue.Count)
                    {
                        if (_svQueue[i].StartTime < _timingQueue[hij + 1].StartTime - 0.01f)
                        {
                            TimingObject newSV = new TimingObject();
                            newSV.StartTime = _svQueue[i].StartTime;
                            newSV.Multiplier = Mathf.Min(_svQueue[i].Multiplier * _timingQueue[hij].BPM / AverageBpm, 64f);
                            _svQueue.RemoveAt(i);
                            _svQueue.Insert(i, newSV);
                        }
                        else
                        {
                            hij++;
                            TimingObject newSV = new TimingObject();
                            newSV.StartTime = _svQueue[i].StartTime;
                            newSV.Multiplier = Mathf.Min(_svQueue[i].Multiplier * _timingQueue[hij].BPM / AverageBpm, 64f);
                            _svQueue.RemoveAt(i);
                            _svQueue.Insert(i, newSV);
                        }
                    }
                    else
                    {
                        TimingObject newSV = new TimingObject();
                        newSV.StartTime = _svQueue[i].StartTime;
                        newSV.Multiplier = Mathf.Min(_svQueue[i].Multiplier * _timingQueue[hij].BPM / AverageBpm, 512f);
                        _svQueue.RemoveAt(i);
                        _svQueue.Insert(i, newSV);
                    }
                }
            }
            */
        }

        //Move Timing Bars
        private void time_MoveTimingBars()
        {
            /*
            if (_config_timingBars && !mod_split)
            {
                if (_activeBars.Count > 0 && _CurrentSongTime > _activeBars[0].StartTime + missTime)
                {
                    time_RemoveBar(_activeBars[0].TimingBar);
                    _activeBars.RemoveAt(0);
                }
                else
                {
                    for (int k = 0; k < _activeBars.Count; k++)
                    {
                        _activeBars[k].TimingBar.transform.localPosition = new Vector3(0f, PosFromSV(_activeBars[k].StartTime), 2f);
                    }
                }
            }
            */
        }

        //Calculate CurrentTime's Sv Position
        private void time_GetCurrentSVPos()
        {
            /*
            if (_CurrentSongTime >= _svQueue[_svQueue.Count - 1].StartTime)
            {
                _curSVPart = _svQueue.Count - 1;
            }
            else if (_curSVPart < _svQueue.Count - 2)
            {
                for (int j = _curSVPart; j < _svQueue.Count - 1; j++)
                {
                    if (_CurrentSongTime > _svQueue[_curSVPart + 1].StartTime) _curSVPart++;
                    else break;
                }
            }
            _curSVPos = _svCalc[_curSVPart] + (ulong)(((float)((_CurrentSongTime) - (_svQueue[_curSVPart].StartTime)) * _svQueue[_curSVPart].Multiplier) + 10000);
            */
        }
    }
}