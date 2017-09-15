// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("")]
    public class ImageEffectBase : MonoBehaviour
    {
        /// Provides a shader property that is set in the inspector
        /// and a material instantiated from the shader
        public Shader shader;

        private Material _material;


        protected virtual void Start()
        {
            // Disable if we don't support image effects
            if (!SystemInfo.supportsImageEffects)
            {
                enabled = false;
                return;
            }

            // Disable the image effect if the shader can't
            // run on the users graphics card
            if (!shader || !shader.isSupported)
                enabled = false;
        }


        protected Material material
        {
            get
            {
                if (_material == null)
                {
                    _material = new Material(shader);
                    _material.hideFlags = HideFlags.HideAndDontSave;
                }
                return _material;
            }
        }


        protected virtual void OnDisable()
        {
            if (_material)
            {
                DestroyImmediate(_material);
            }
        }
    }
}
