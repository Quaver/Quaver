using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Quaver.Gameplay
{
    public partial class PlayScreen
    {
        private GameObject npsGraph;

        private List<int> _npsQueue;
        private List<GameObject> _npsGraphObjects;
        private int[] _npsData = new int[9];
        private int[] _graphData;

        private const int _graphSize = 60;
        private const float _graphInterval = 0.1f;
        private float _curGraphInterval = 0;
        private const float _graphObjectSize = 500f/ _graphSize;
        private float _graphTween = 0;
        
        private void nps_init()
        {
            int i = 0;
            npsGraph = uiCanvas.transform.Find("npsGraph").gameObject;

            //Create npsQueue
            _npsQueue = new List<int>();
            for (i = 0; i < _noteQueue.Count; i++)
            {
                _npsQueue.Add(_noteQueue[i].StartTime);
            }

            //Create graphObjects
            _graphData = new int[_graphSize];
            _npsGraphObjects = new List<GameObject>();
            for (i = 0; i < _graphSize; i++)
            {
                GameObject newGraphObject = new GameObject("GraphObject");
                newGraphObject.transform.parent = npsGraph.transform;
                newGraphObject.AddComponent<CanvasRenderer>();
                newGraphObject.AddComponent<RawImage>();
                //newGraphObject.AddComponent<RectTransform>();

                newGraphObject.transform.localScale = Vector3.one;
                newGraphObject.GetComponent<RectTransform>().sizeDelta = new Vector2(_graphObjectSize, _graphObjectSize);
                newGraphObject.GetComponent<RectTransform>().localPosition = new Vector2(5 + i * _graphObjectSize, -5 - i*2f);
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
                    if (_npsQueue[0] > _curSongTime)
                    {
                        _npsData[0]++;
                        _npsQueue.RemoveAt(0);
                    }
                }

                //UpdateGraph
                _curGraphInterval += Time.deltaTime;
                if (_curGraphInterval >= _graphInterval)
                {
                    _curGraphInterval -= _graphInterval;

                    //Set graphData
                    _npsGraphObjects[59].GetComponent<RectTransform>().localPosition = new Vector2(5 + 59 * _graphObjectSize, _graphTween);
                    for (i= _graphSize-1; i>=1; i--)
                    {

                    }

                    //Shift npsData
                    for (i = 8; i >= 1; i--) _npsData[i] = _npsData[i-1];
                    _npsData[0] = 0;

                }
            }
            /* nps graph
            local highestNps = 0

            for i = 1, 59 do
                    npsGraph[60 - (i - 1)] = npsGraph[60 - i]

                if npsGraph[60 - (i - 1)] > highestNps then highestNps = npsGraph[60 - (i - 1)] end
                    end

            npsGraph[1] = npsGraph[1] + ((npsData[1] + npsData[2] + npsData[3] + npsData[4] + npsData[5] + npsData[6] + npsData[7] + npsData[8] + npsData[9]) / 9 * (1 / 0.04) - npsGraph[1]) / 8

            if npsGraph[1] > highestNps then highestNps = npsGraph[1] end
            hNpsTween = hNpsTween + (highestNps + 10 - hNpsTween) / 3

            for i = 1, 60 do
                    --npsFrames[tostring(i)].Size = UDim2.new(0, 10, npsGraph[i] / hNpsTween, 0)
                   --npsFrames[tostring(i)].Size = UDim2.new(0, 10, 0, 10)

                local sizCheck = (1 - (npsGraph[i] / hNpsTween)) * 160


                if 61 - i ~= 60 then

                    if sizCheck > npsFrames[tostring(61 - i + 1)].Position.Y.Offset then
                            local orig = sizCheck

                        sizCheck = math.abs((sizCheck + 6) - (npsFrames[tostring(61 - i + 1)].Position.Y.Offset + npsFrames[tostring(61 - i + 1)].Size.Y.Offset))

                        if sizCheck < 4 then sizCheck = 4 end
                          npsFrames[tostring(61 - i)].Size = UDim2.new(0, 5, 0, sizCheck)

                        npsFrames[tostring(61 - i)].Position = UDim2.new(0, (60 - i) * 5, 0, orig - sizCheck + 4)
					else
						npsFrames[tostring(61 - i)].Position = UDim2.new(0, (60 - i) * 5, 0, sizCheck)

                        sizCheck = math.abs((npsFrames[tostring(61 - i + 1)].Position.Y.Offset + 2) - (sizCheck))

                        if sizCheck < 4 then sizCheck = 4 end
                          npsFrames[tostring(61 - i)].Size = UDim2.new(0, 5, 0, sizCheck)

                    end
				else
					local prev = npsFrames["60"].Position.Y.Offset
                    --npsFrames["60"].Position = UDim2.new(0, 295, 0, (prev + ((1 - npsGraph[1] / hNpsTween) * 160 - prev) / 6))

                    npsFrames["60"].Position = UDim2.new(0, 295, 0, (1 - (npsGraph[1] / hNpsTween)) * 160)
                    --print(npsFrames["60"].Position.Y.Offset)

                    npsFrames["60"].Size = UDim2.new(0, 5, 0, 4)

                end

                if i <= 12 then
                    npsRef[tostring(i - 1)].Position = UDim2.new(0, 0, 0, (1 - ((i * 5) / hNpsTween)) * 160)

                end
                --npsFrames[tostring(31 - i)].Position = UDim2.new(0, (60 - i) * 10, 0, orig - sizCheck / 2 - 10)

                npsFrames[tostring(i)].BackgroundColor3 = colorizeNps(npsGraph[61 - i])

            end

            for i = 1, 9 do
                    npsData[9 - (i - 1)] = npsData[9 - i]
   
               end

            npsData[1] = 0

            repeat
            waittime = waittime - 0.05

            until waittime < 0.05

        end
        npsText = npsText + (npsGraph[1] - npsText) / 6

        script.Parent.gamePlay.NPS.npsText.Text = "Notes/Sec: "..(math.floor(npsText * 10) / 10)*/
        }
    }
}
