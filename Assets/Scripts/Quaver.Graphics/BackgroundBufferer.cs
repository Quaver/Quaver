// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Quaver.Cache;
using UnityEngine.UI;

namespace Quaver.Graphics
{
    public class BackgroundBufferer : MonoBehaviour
    {
        private bool _spriteMode;
        private string _bufferPath;
        private RawImage _bufferTexture;
        private GameObject _bufferSprite;

        //Initialization
        public void init(bool spriteBuffer, string bgPath, RawImage bgTexture, GameObject bgSprite = null)
        {
            _spriteMode = spriteBuffer;
            _bufferPath = bgPath;
            _bufferTexture = bgTexture;
            _bufferSprite = bgSprite;
            StartCoroutine(BufferImage());
        }

        //Buffer Image async
        private IEnumerator BufferImage()
        {
            if (_spriteMode && _bufferSprite != null)
            {
                //Create buffer image
                Sprite sprite = new Sprite();
                Texture2D texture = new Texture2D(64, 64, TextureFormat.ARGB32, true);

                //Create file stream
                FileStream fs = new FileStream(_bufferPath, FileMode.Open, FileAccess.Read);
                byte[] imageData = new byte[fs.Length];
                fs.Read(imageData, 0, (int)fs.Length);

                //wait until file stream is done
                while (fs.Position != fs.Length)
                    yield return null;

                //give the image a period to load onto unity
                yield return new WaitForSeconds(0.2f);
                texture.LoadImage(imageData);
                yield return new WaitForSeconds(0.2f);

                //Apply texture to bg + resize bg if bg isnt null
                if (_bufferSprite != null)
                {
                    sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    _bufferSprite.GetComponent<SpriteRenderer>().sprite = sprite;
                    _bufferSprite.GetComponent<BackgroundDimAnimator>().dim = _bufferSprite.GetComponent<BackgroundDimAnimator>().Manager.SelectScreenDim;

                    if (sprite.rect.size.y / sprite.rect.size.x <= Screen.height / Screen.width)
                        _bufferSprite.transform.localScale = Vector3.one * 1.05f * (20f * (100f / (float)sprite.rect.size.y));
                    else
                        _bufferSprite.transform.localScale = Vector3.one * 1.05f * ((float)Screen.width / (float)Screen.height) * 20f * (100f / (float)sprite.rect.size.x);
                }
            }
            else if (!_spriteMode && _bufferTexture != null)
            {
                //Create buffer image
                Texture2D texture = new Texture2D(64, 64, TextureFormat.ARGB32, true);

                //Create file stream
                FileStream fs = new FileStream(_bufferPath, FileMode.Open, FileAccess.Read);
                byte[] imageData = new byte[fs.Length];
                fs.Read(imageData, 0, (int)fs.Length);

                //Wait until filestream is done
                while (fs.Position != fs.Length)
                    yield return null;

                //give the image a period to load onto unity
                yield return new WaitForSeconds(0.4f);
                texture.LoadImage(imageData);
                yield return new WaitForSeconds(0.2f);

                //Apply texture to ui
                if (_bufferTexture != null)
                    _bufferTexture.texture = texture;
            }

            //Removes game object
            Destroy(this.gameObject);
        }
    }
}
