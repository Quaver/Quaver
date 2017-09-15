// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Graphics;

namespace Quaver.Gameplay
{
    public partial class PlayScreen
    {
        /*SKIN.INI VALUES*/
        private const int skin_bgMaskBufferSize = 12; //Adds a buffer space to the bg mask (+8 pixels wide)
        private const int skin_noteBufferSpacing = 0; //Spaces notes 0 pixels a part
        private const int skin_timingBarPixelSize = 2;
        private const float skin_hitLightingScale = 4.0f; //Sets the scale of the hit lighting (relative to units)
        private const int skin_columnSize = 250;
        private const int skin_receptorYOffset = 50; //Sets the receptor's y position offset, relative to the edge of the screen.
                                                     //private float[] skin_receptorRotations = new float[4] { -90f, 0f, 180f, 90f }; //Rotation of arrows if arrow skin is used
        private float[] _skin_receptorRotations = new float[4] { 0, 0, 0, 0 }; //Rotation of arrows if arrow skin is used

        //Other skinning variables
        private float _receptorYPos;
        private float[] _receptorXPos, _receptorXOffset, _receptorSize;

        //Skinning GameObject References (Temp)
        public Sprite[] receptorSprite;
        public GameObject NoteHitParticle;
        public GameObject circleParticleSystem;
        public GameObject timingBar;

        //Initialize Skin component
        private void skin_init()
        {
            //Declare Receptor Variables
            int i = 0;
            _receptorXPos = new float[4];
            _receptorXOffset = new float[4];
            _receptorSize = new float[4];

            //Set Receptor Variables
            if (_config_upScroll) _scrollNegativeFactor = -1f;
            _receptorYPos = _scrollNegativeFactor * (skin_columnSize / 256f + (float)skin_receptorYOffset / 100f - 10f);
            _receptors = new GameObject[4];
            _hitLighting = new GameObject[4];

            //Create Gameobjects for each receptor
            for (i = 0; i < 4; i++)
            {
                //Create receptor + xpos
                _receptors[i] = receptorBar.transform.Find("R" + (i + 1)).gameObject;
                _receptorXPos[i] = (i - 1.5f) * ((skin_columnSize + (float)skin_noteBufferSpacing) / config_PixelUnitSize);

                //Set receptor pos
                if (i >= 2 && mod_split) _receptors[i].transform.localPosition = new Vector3(_receptorXPos[i], -_receptorYPos, 1);
                else _receptors[i].transform.localPosition = new Vector3(_receptorXPos[i], _receptorYPos, 1);

                //Scale + rotate receptors
                _receptors[i].transform.localScale = Vector3.one * (skin_columnSize / (float)_receptors[i].transform.GetComponent<SpriteRenderer>().sprite.rect.size.x);
                _receptors[i].transform.transform.eulerAngles = new Vector3(0, 0, _skin_receptorRotations[i]);

                //Create hitlighting for receptors
                _hitLighting[i] = lightingBar.transform.Find("L" + (i + 1)).gameObject;
                _hitLighting[i].transform.localScale = new Vector3(_receptors[i].transform.localScale.x,_scrollNegativeFactor * skin_hitLightingScale, 1f);
                _hitLighting[i].transform.localPosition = _receptors[i].transform.localPosition + new Vector3(0, _hitLighting[i].transform.localScale.y * 2f, 1f);
            }

            //Set bg Mask Scale
            bgMask.transform.localScale = new Vector3(
                ((float)(skin_columnSize + skin_bgMaskBufferSize + skin_noteBufferSpacing) / config_PixelUnitSize) * 4f * (config_PixelUnitSize / (float)bgMask.transform.GetComponent<SpriteRenderer>().sprite.rect.size.x),
                20f * (config_PixelUnitSize / (float)bgMask.transform.GetComponent<SpriteRenderer>().sprite.rect.size.y) , 1f);
        }

        private void skin_NoteDown(int kkey)
        {
            _keyDown[kkey] = true;
            _receptors[kkey].transform.localScale = Vector3.one * (skin_columnSize / config_PixelUnitSize) * 1.1f;
            _receptors[kkey].transform.GetComponent<SpriteRenderer>().sprite = receptorSprite[1];
            _hitLighting[kkey].SetActive(true); //temp
        }

        private void skin_NoteUp(int kkey)
        {
            _keyDown[kkey] = false;
            _receptors[kkey].transform.GetComponent<SpriteRenderer>().sprite = receptorSprite[0];
            _hitLighting[kkey].SetActive(false); //temp
        }

        private void skin_NoteBurst(int kkey)
        {
            //Create particles
            GameObject cp = Instantiate(circleParticleSystem, particleContainer.transform);
            cp.transform.localPosition = _receptors[kkey - 1].transform.localPosition + new Vector3(0, 0, 4f);

            GameObject hb = Instantiate(NoteHitParticle, particleContainer.transform);
            hb.transform.localPosition = _receptors[kkey - 1].transform.localPosition + new Vector3(0, 0, -2f);
            hb.transform.eulerAngles = _receptors[kkey - 1].transform.eulerAngles;
            hb.transform.localScale = _receptors[kkey - 1].transform.localScale;
            hb.GetComponent<NoteBurst>().startSize = hb.transform.localScale.x - 0.05f;
            hb.GetComponent<NoteBurst>().burstSize = 0.1f;
            hb.GetComponent<NoteBurst>().burstLength = 0.35f;
        }

    }
}
