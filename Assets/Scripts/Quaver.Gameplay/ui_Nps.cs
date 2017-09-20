using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Quaver.Gameplay
{
    public partial class PlayScreen
    {
        private GameObject _npsGraph;

        private List<int> _npsQueue;
        private GameObject[] _npsGraphObjects;
        private RectTransform[] _npsGraphTransforms;
        private RectTransform[] _npsTextMarks;
        private RawImage[] _npsGraphImage;
        private Text _npsText;

        private int[] _npsData = new int[9];
        private float[] _graphData;

        private const int _graphSize = 80;
        private const float _graphInterval = 0.075f;
        private float _curGraphInterval = 0;
        private const float _graphObjectSize = 500f/ _graphSize;
        private const float colorInterval = 5f;
        private float _graphTween = 0;
        private float _textTween = 0;
        private float _graphScaleTween = 10f;
        
        private void nps_init()
        {
            //Set reference objects
            int i = 0;
            _npsGraph = uiCanvas.transform.Find("npsGraph").gameObject;
            _npsText = _npsGraph.transform.Find("npsText").GetComponent<Text>();

            //Create npsQueue
            _npsQueue = new List<int>();
            for (i = 0; i < _noteQueue.Count; i++)
            _npsQueue.Add(_noteQueue[i].StartTime);

            //Create reference lists/arrays
            _graphData = new float[_graphSize];
            _npsGraphObjects = new GameObject[_graphSize];
            _npsGraphTransforms = new RectTransform[_graphSize];
            _npsGraphImage = new RawImage[_graphSize];
            _npsTextMarks = new RectTransform[7];

            //Create graphObjects
            for (i = 0; i < _graphSize; i++)
            {
                //Get components
                GameObject newGraphObject = new GameObject("GraphObject");
                newGraphObject.transform.parent = _npsGraph.transform;
                newGraphObject.AddComponent<CanvasRenderer>();
                _npsGraphImage[i] = newGraphObject.AddComponent<RawImage>();
                _npsGraphTransforms[i] = newGraphObject.GetComponent<RectTransform>();

                //Set position/Size
                newGraphObject.transform.localScale = Vector3.one;
                _npsGraphTransforms[i].sizeDelta = new Vector2(_graphObjectSize, _graphObjectSize);
                _npsGraphTransforms[i].localPosition = new Vector2(500f - (i+0.5f) * _graphObjectSize, -200f);
                _npsGraphObjects[i] = newGraphObject;
            }
            for(i = 0; i < 7; i++)
            {
                _npsTextMarks[i] = _npsGraph.transform.Find("npsMark_" + ((i + 1) * 10)).GetComponent<RectTransform>();
            }
        }

        private void nps_Update()
        {
            if (_config_EnableNpsGraph)
            {
                //Convert graph start to data if it's time
                int i = 0;
                for (i = 0; i < 10 && i < _npsQueue.Count; i++)
                {
                    if (_npsQueue[i] <= _curSongTime)
                    {
                        _npsData[0]++;
                        _npsQueue.RemoveAt(i);
                        i--;
                    }
                }

                //UpdateGraph
                _curGraphInterval += Time.deltaTime;
                if (_curGraphInterval >= _graphInterval)
                {
                    //Update graph interval
                    _curGraphInterval -= _graphInterval;

                    //Set graph tween and text
                    _graphTween += (((float)(_npsData[0] + _npsData[1] + _npsData[2] + _npsData[3] + _npsData[4] + _npsData[5] + _npsData[6] + _npsData[7] + _npsData[8]) / 9f * 1f/_graphInterval) - _graphTween)/2f;
                    _textTween += (_graphTween - _textTween) / 2f;
                    _npsText.text = "Notes/sec: " + string.Format("{0:f2}", _textTween);

                    //Shift npsData
                    for (i = 8; i >= 1; i--) _npsData[i] = _npsData[i - 1];
                    _npsData[0] = 0;

                    //Shift graphData
                    float _highestNps = 0;
                    for (i = _graphSize-1; i >= 1; i--)
                    {
                        if (_graphData[i] > _highestNps) _highestNps = _graphData[i];
                        _graphData[i] = _graphData[i - 1];
                    }
                    _graphData[0] = _graphTween;
                    _graphScaleTween += (_highestNps + 10f - _graphScaleTween)/2f;

                    //Set GraphObject Transform
                    for (i=0; i < _graphSize; i++)
                    {
                        //Set object color
                        _npsGraphImage[i].color = colorGraph(_graphData[i]);

                        //Set object position and scale
                        float orig = (_graphData[i] / _graphScaleTween) * 200f - 200f;
                        if (i >= 1)
                        {
                            float sizeCheck = orig;
                            if (sizeCheck >= _npsGraphTransforms[i - 1].localPosition.y)
                            {
                                sizeCheck = Mathf.Abs(_npsGraphTransforms[i - 1].localPosition.y - orig + _npsGraphTransforms[i - 1].sizeDelta.y / 2f - _graphObjectSize);
                                if (sizeCheck < _graphObjectSize) sizeCheck = _graphObjectSize;
                                _npsGraphTransforms[i].sizeDelta = new Vector2(_graphObjectSize, sizeCheck);
                                _npsGraphTransforms[i].localPosition = new Vector2(500f - (i + 0.5f) * _graphObjectSize, orig +(_graphObjectSize - sizeCheck) / 2f);

                            }
                            else
                            {
                                sizeCheck = Mathf.Abs(_npsGraphTransforms[i - 1].localPosition.y - orig - _npsGraphTransforms[i - 1].sizeDelta.y / 2f + _graphObjectSize);
                                if (sizeCheck < _graphObjectSize) sizeCheck = _graphObjectSize;
                                _npsGraphTransforms[i].sizeDelta = new Vector2(_graphObjectSize, sizeCheck);
                                _npsGraphTransforms[i].localPosition = new Vector2(500f - (i + 0.5f) * _graphObjectSize, orig - (_graphObjectSize - sizeCheck) / 2f);
                            }
                        }
                        else _npsGraphTransforms[i].localPosition = new Vector2(500f - (i+0.5f) * _graphObjectSize, orig);

                        //Set position of text labels
                        if (i < 7) _npsTextMarks[i].localPosition = new Vector2(15,(200f * (10 * (i + 1))/_graphScaleTween) - 200f);
                    }
                }
            }
        }

        //Change graph object color to match nps
        private Color colorGraph(float npsValue)
        {
            Color newColor = new Color();
            float colorTween = npsValue;
            if (npsValue < colorInterval)
            {
                colorTween = colorTween / colorInterval;
                newColor = new Color(0.5f-(colorTween*0.5f), 0.5f - (colorTween * 0.5f), 1f);
            }
            else if (npsValue < colorInterval * 2)
            {
                colorTween = (colorTween-colorInterval) / colorInterval;
                newColor = new Color(0, colorTween, 1f);
            }
            else if (npsValue < colorInterval * 3)
            {
                colorTween = (colorTween - (colorInterval * 2)) / colorInterval;
                newColor = new Color(0, 1f, 1f-colorTween);
            }
            else if (npsValue < colorInterval * 4)
            {
                colorTween = (colorTween - (colorInterval * 3)) / colorInterval;
                newColor = new Color(colorTween, 1f, 0f);
            }
            else if (npsValue < colorInterval * 5)
            {
                colorTween = (colorTween - (colorInterval * 4)) / colorInterval;
                newColor = new Color(1f, 1f-colorTween, 0f);
            }
            else if (npsValue < colorInterval * 6)
            {
                colorTween = (colorTween - (colorInterval * 5)) / colorInterval;
                newColor = new Color(1f, 0f, colorTween);
            }
            else if (npsValue < colorInterval * 7)
            {
                colorTween = (colorTween - (colorInterval * 6)) / colorInterval;
                newColor = new Color(1f, colorTween, 1f);
            }
            else
            {
                newColor = new Color(1f,1f, 1f);
            }
            return newColor;
        }

    }
}
