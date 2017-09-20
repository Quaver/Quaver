
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Qua;
using Quaver.Graphics;
using Quaver.Main;
using Quaver.Audio;

using Quaver.Cache; //_debug
using Quaver.Config; //_debug

namespace Quaver.Gameplay
{
    public partial class PlayScreen : GameStateObject
    {
        //UI + State
        private bool loaded;
        public GameObject Game_UI;


        //DEBUG ONLY (temp)
        public bool DEBUG_MODE = false;
        public string DEBUG_SONGPATH = "E:\\GitHub\\Quaver\\Test\\Qua\\wtc.qua";

        //Config Values
        private int _config_scrollSpeed;
        private bool _config_timingBars;
        private bool _config_upScroll;
        private int _config_offset;
        private const bool _config_EnableNpsGraph = true;
        private const bool _config_EnableMAdisplay = true;
        private KeyCode[] _config_KeyBindings;

        /*Classes/Gameobjects (public variables will be changed later)*/
        public GameObject hitObjectTest;
        public GameObject receptorBar;
        public GameObject lightingBar;
        public GameObject particleContainer;
        public GameObject hitContainer;
        public GameObject bgMask;

        /*GSM REFERENCES (Anything that references the game state manager)*/
        private QuaFile _qFile;
        public AudioSource _songAudio;

        /*GAME MODS*/
        private const bool mod_noSV = false;
        private const bool mod_pull = false;
        private const bool mod_split = false;
        private const bool mod_spin = false;
        private const bool mod_shuffle = false;

        //Score Values
        //0 MARV, 1 PERF, 2 GREAT, 3 GOOD, 4 BAD, 5 MISS, 6 ACE, 7 EARLY, 8 LATE
        private int[] _ScoreSpread;
        private float _acc;
        private int _score;

        public void Start()
        {
            if (DEBUG_MODE) //_debug
            {
                print("[PlayScreen] DEBUGGING MODE");
                BeatmapCacheIndex.CreateDatabase();
                Cfg GameConfig = ConfigLoader.Load();
                List<CachedBeatmap> LoadedBeatmaps = BeatmapCacheIndex.LoadBeatmaps(GameConfig);
                //Load first song (for debug)
                AudioPlayer.LoadSong(LoadedBeatmaps[0], _songAudio, false, (float)config_playStartDelayed / 1000f);
                //print(_songAudio.clip.length);
            }
            StateUI = Instantiate(Game_UI, Manager.ActiveStateUISet.transform);
            StartGame();
        }

        private void RetryGame()
        {
            //Stop Audio
            _songAudio.Stop();
            _songDone = false;
            loaded = false;

            //Destroy Objects
            np_DestroyNotes();
            time_DestroyBars();

            //Reset UI
            ui_Reset();

            //TODO: Transition Animation

            //Start Game
            StartGame();
        }

        private void StartGame()
        {
            //Parse Map
            if (DEBUG_MODE) _qFile = QuaParser.Parse(DEBUG_SONGPATH); //_debug
            else _qFile = QuaParser.Parse(Manager.currentMap.Path);

            //Check if beatmap is valid
            if (!_qFile.IsValidQua)
            {
                print("IS NOT VALID QUA FILE");
            }
            else if (_qFile.IsValidQua)
            {
                //Declare Config Variables
                if (!DEBUG_MODE)
                {
                    _config_scrollSpeed = Manager.GameConfig.ScrollSpeed;
                    _config_timingBars = true; //should be config
                    _config_upScroll = !Manager.GameConfig.DownScroll;
                    _config_KeyBindings = new KeyCode[] { Manager.GameConfig.KeyLaneMania1, Manager.GameConfig.KeyLaneMania2, Manager.GameConfig.KeyLaneMania3, Manager.GameConfig.KeyLaneMania4 };
                    _config_offset = Manager.GameConfig.GlobalOffset;
                }
                else
                {
                    _config_scrollSpeed = 23;
                    _config_timingBars = true; //should be config
                    _config_upScroll = true;
                    _config_KeyBindings = new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.K, KeyCode.L };
                    _config_offset = 0;
                }

                //Initializes skin
                skin_init();

                //Initializes Timing
                time_init();

                //Starts rendering notes + sets gameplay variables
                np_init();

                //Plays the song, but delayed
                if (!DEBUG_MODE) AudioPlayer.LoadSong(Manager.currentMap, Manager.SongAudioSource, false, (float)config_playStartDelayed / 1000f);
                loaded = true;
            }
        }

        public void Update()
        {
            if (_qFile.IsValidQua && loaded && (isActive || DEBUG_MODE)) //_debug
            {
                //Check what to do after unpaused
                if (_songAudio.time >= _songAudio.clip.length && !_songDone) _songDone = true;

                //Song Time Calculation (ms)
                time_SetCurrentSongTime();

                //Calculates curSV Position
                time_GetCurrentSVPos();

                //Receptor Modifiers
                if (mod_spin || mod_shuffle) ModifyReceptors();

                //Move TimingBars
                time_MoveTimingBars();

                //Move notes and check if miss
                np_MovePlayNotes();

                //LN Check
                np_ResizeLongNotes();

                //GhostNote Check (darkened keys)
                np_MoveGhostNotes();

                //Key Press Check
                input_CheckInput();

                //Update UI
                ui_Update();
            }
            else
            {
                if (_songAudio.isPlaying) _songAudio.Pause();
            }
        }

        // Modifies the receptors if visual mods are on.
        private void ModifyReceptors()
        {
            for (int k = 0; k < 4; k++)
            {
                //Spin Receptors
                if (mod_spin) _skin_receptorRotations[k] += 0.5f;
                if (_skin_receptorRotations[k] >= 360) _skin_receptorRotations[k] -= 360;

                //XOff Receptors
                if (mod_shuffle)
                {
                    if (k == 0 || k == 2) _receptorXOffset[k] = (Mathf.Pow(Mathf.Sin(_curSongTime * (Mathf.PI / 180f) / 10f), 3) + 1f) * skin_columnSize / config_PixelUnitSize / 2f;
                    else _receptorXOffset[k] = -(Mathf.Pow(Mathf.Sin(_curSongTime * (Mathf.PI / 180f) / 10f), 3) + 1f) * skin_columnSize / config_PixelUnitSize / 2f;
                }

                //Set Receptor Transform
                if (mod_spin) _receptors[k].transform.localEulerAngles = new Vector3(0, 0, _skin_receptorRotations[k]);
                if (mod_shuffle) _receptors[k].transform.localPosition = new Vector3(_receptorXPos[k] + _receptorXOffset[k], _receptorYPos, 1);
                _receptors[k].transform.localScale = Vector3.one * (_receptors[k].transform.localScale.x + (((skin_columnSize / config_PixelUnitSize) - _receptors[k].transform.localScale.x) / 3f));
            }
        }

        //Sets the GameObject position of the note
        private void SetNotePos(NoteObject curNote, int lnMode)
        {
            float StartTime = curNote.StartTime;
            float splitFactor = 1f;
            if (lnMode == 2 && StartTime <= _curSongTime) StartTime = _curSongTime;
            if (mod_split && curNote.KeyLane >= 3) splitFactor = -1f;
            if (mod_spin) curNote.HitSprite.transform.eulerAngles = new Vector3(0, 0, _skin_receptorRotations[curNote.KeyLane - 1]);
            if (lnMode != 2 || mod_shuffle) curNote.HitSet.transform.localPosition = new Vector3(_receptorXPos[curNote.KeyLane - 1] + _receptorXOffset[curNote.KeyLane - 1], splitFactor * PosFromSV(StartTime), 0);
            else if (StartTime != _curSongTime) curNote.HitSet.transform.localPosition = new Vector3(_receptorXPos[curNote.KeyLane - 1] + _receptorXOffset[curNote.KeyLane - 1], splitFactor * PosFromSV(StartTime), 0);
            else curNote.HitSet.transform.localPosition = new Vector3(_receptorXPos[curNote.KeyLane - 1] + _receptorXOffset[curNote.KeyLane - 1], _receptorYPos * splitFactor, 0);
            if ((((lnMode != 1 || mod_pull) && lnMode != 3) || _scrollSpeedChanged ) && curNote.EndTime > 0)
            {
                float lnSize = splitFactor * Mathf.Min(Mathf.Abs(PosFromSV(curNote.EndTime) - PosFromSV(StartTime)), 50f);

                curNote.SliderMiddleSprite.size = new Vector2(1f, -_scrollNegativeFactor * lnSize * (config_PixelUnitSize / skin_columnSize));
                curNote.SliderEndObject.transform.localPosition = new Vector3(0f, _scrollNegativeFactor * lnSize, 0.1f);

                if (lnMode == 0)
                {
                    curNote.SliderMiddleObject.transform.localScale = Vector3.one * (skin_columnSize / config_PixelUnitSize);
                    curNote.SliderMiddleSprite.color = new Color(1f, 1f, 1f, 1f);
                    curNote.SliderMiddleObject.SetActive(true);

                    curNote.SliderEndSprite.color = new Color(1f, 1f, 1f, 1f);
                    curNote.SliderEndObject.transform.localScale = Vector3.one * (skin_columnSize / config_PixelUnitSize);
                    curNote.SliderEndObject.SetActive(true);
                    if (splitFactor >= 1) curNote.SliderEndSprite.flipY = !_config_upScroll;
                    else curNote.SliderEndSprite.flipY = _config_upScroll;
                }
            }
        }

        //Calculates the position from SV
        private float PosFromSV(float timePos)
        {
            float returnVal;
            if (Mathf.Abs(timePos - _curSongTime) >= 0.01)
            {
                if (!mod_noSV)
                {
                    ulong svPosTime = 0;
                    int curPos = 0;
                    if (timePos >= _svQueue[_svQueue.Count - 1].StartTime)
                    {
                        curPos = _svQueue.Count - 1;
                    }
                    else
                    {
                        for (int i = 0; i < _svQueue.Count - 1; i++)
                        {
                            if (timePos < _svQueue[i + 1].StartTime)
                            {
                                curPos = i;
                                break;
                            }
                        }
                    }
                    svPosTime = _svCalc[curPos] + (ulong)(15000 + ((timePos - _svQueue[curPos].StartTime) * _svQueue[curPos].Multiplier));
                    //10000ms added for negative, since svPos is a ulong

                    returnVal = (float)(svPosTime - _curSVPos - 5000f) / 1000f * (float)_config_scrollSpeed * (1 / _songAudio.pitch);
                }
                else returnVal = (timePos - _curSongTime) / 1000f * (float)_config_scrollSpeed * (1 / _songAudio.pitch);
            }
            else returnVal = 0;

            if (mod_pull) returnVal = (2f * Mathf.Max(Mathf.Pow(returnVal, 0.6f), 0)) + (Mathf.Min(timePos - _curSongTime, 0f) / 1000f * (float)_config_scrollSpeed * (1 / _songAudio.pitch));

            return (returnVal * _scrollNegativeFactor) + _receptorYPos;
        }
    }

    //Dont rmeove below this
}