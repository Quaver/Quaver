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

        private bool spriteMode;
        private string bufferPath;
        private RawImage bufferTexture;
        private GameObject bufferSprite;

        //Initialization
        public void init(bool spriteBuffer, string bgPath, RawImage bgTexture, GameObject bgSprite = null)
        {
            spriteMode = spriteBuffer;
            bufferPath = bgPath;
            bufferTexture = bgTexture;
            bufferSprite = bgSprite;
            StartCoroutine(BufferImage());

        }

        //Buffer Image async
        private IEnumerator BufferImage()
        {
            
            if (spriteMode && bufferSprite != null)
            {
                //Create buffer image
                Sprite sprite = new Sprite();
                Texture2D texture = new Texture2D(64, 64, TextureFormat.ARGB32, true);
                WWW data = new WWW(bufferPath);

                //Wait before loading image onto unity as texture
                yield return data;
                yield return new WaitForSeconds(0.5f);

                //Apply image to texture
                data.LoadImageIntoTexture(texture);

                //Apply texture to bg + resize bg if bg isnt null
                if (bufferSprite != null)
                {
                    sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    bufferSprite.GetComponent<SpriteRenderer>().sprite = sprite;
                    bufferSprite.GetComponent<BackgroundDimAnimator>().dim = bufferSprite.GetComponent<BackgroundDimAnimator>().Manager.SelectScreenDim;

                    if (sprite.rect.size.y / sprite.rect.size.x >= Screen.width / Screen.height)
                        bufferSprite.transform.localScale = Vector3.one * (20f * (100f / (float)sprite.rect.size.y));
                    else
                        bufferSprite.transform.localScale = Vector3.one * ((float)Screen.width / (float)Screen.height) * 20f * (100f / (float)sprite.rect.size.x);
                }
            }
            else if (!spriteMode && bufferTexture != null)
            {
                //Create buffer image
                Texture2D texture = new Texture2D(64, 64, TextureFormat.ARGB32, true);
                WWW data = new WWW(bufferPath);

                //Wait before loading image onto unity as texture
                yield return data;
                yield return new WaitForSeconds(0.5f);

                //Apply image texture
                data.LoadImageIntoTexture(texture);

                //Apply texture to ui
                if (bufferTexture != null)
                    bufferTexture.texture = texture;
            }

            //Removes game object
            Debug.Log("[Background Buffer] DONE!");
            Destroy(this.gameObject);
        }
    }
}
