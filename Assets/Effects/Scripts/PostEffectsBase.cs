using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class PostEffectsBase : MonoBehaviour
    {
        protected bool supportHDRTextures = true;
        protected bool supportDX11 = false;
        protected bool isSupported = true;

        protected Material CheckShaderAndCreateMaterial(Shader s, Material m2Create)
        {
            if (!s)
            {
                Debug.Log("Missing shader in " + ToString());
                enabled = false;
                return null;
            }

            if (s.isSupported && m2Create && m2Create.shader == s)
                return m2Create;

            if (!s.isSupported)
            {
                NotSupported();
                Debug.Log("The shader " + s.ToString() + " on effect " + ToString() + " is not supported on this platform!");
                return null;
            }
            else
            {
                m2Create = new Material(s);
                m2Create.hideFlags = HideFlags.DontSave;
                if (m2Create)
                    return m2Create;
                else return null;
            }
        }


        protected Material CreateMaterial(Shader s, Material m2Create)
        {
            if (!s)
            {
                Debug.Log("Missing shader in " + ToString());
                return null;
            }

            if (m2Create && (m2Create.shader == s) && (s.isSupported))
                return m2Create;

            if (!s.isSupported)
            {
                return null;
            }
            else
            {
                m2Create = new Material(s);
                m2Create.hideFlags = HideFlags.DontSave;
                if (m2Create)
                    return m2Create;
                else return null;
            }
        }

        private void OnEnable()
        {
            isSupported = true;
        }

        protected bool CheckSupport()
        {
            return CheckSupport(false);
        }

        public virtual bool CheckResources()
        {
            Debug.LogWarning("CheckResources () for " + ToString() + " should be overwritten.");
            return isSupported;
        }

        protected void Start()
        {
            CheckResources();
        }

        protected bool CheckSupport(bool needDepth)
        {
            isSupported = true;
            supportHDRTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
            supportDX11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;

            if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
            {
                NotSupported();
                return false;
            }

            if (needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
            {
                NotSupported();
                return false;
            }

            if (needDepth)
                GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;

            return true;
        }

        protected bool CheckSupport(bool needDepth, bool needHdr)
        {
            if (!CheckSupport(needDepth))
                return false;

            if (needHdr && !supportHDRTextures)
            {
                NotSupported();
                return false;
            }

            return true;
        }

        public bool Dx11Support()
        {
            return supportDX11;
        }


        protected void ReportAutoDisable()
        {
            Debug.LogWarning("The image effect " + ToString() + " has been disabled as it's not supported on the current platform.");
        }

        // deprecated but needed for old effects to survive upgrading
        private bool CheckShader(Shader s)
        {
            Debug.Log("The shader " + s.ToString() + " on effect " + ToString() + " is not part of the Unity 3.2+ effects suite anymore. For best performance and quality, please ensure you are using the latest Standard Assets Image Effects (Pro only) package.");
            if (!s.isSupported)
            {
                NotSupported();
                return false;
            }
            else return false;
        }

        protected void NotSupported()
        {
            enabled = false;
            isSupported = false;
            return;
        }

        protected void DrawBorder(RenderTexture dest, Material material)
        {
            RenderTexture.active = dest;
            bool invertY = true; // source.texelSize.y < 0.0ff;
            // Set up the simple Matrix
            GL.PushMatrix();
            GL.LoadOrtho();

            for (int i = 0; i < material.passCount; i++)
            {
                material.SetPass(i);

                float y1_;
                if (invertY) y1_ = 1f;
                else y1_ = 0f;

                // left

                float[] x1 = new float[4] { 0, 1f - (1f / (dest.width * 1f)), 0, 0 };
                float[] x2 = new float[4] { 1.0f / (float)dest.width, 1f, 1f, 1f };
                float[] y1 = new float[4] { 0, 0, 0, 1f - 1.0f / (float)dest.height };
                float[] y2 = new float[4] { 1f, 1f, 1f / (float)dest.height, 1f };

                GL.Begin(GL.QUADS);

                for (int j = 0; j < 4; i++)
                {
                    GL.TexCoord2(0.0f, y1_);
                    GL.Vertex3(x1[j], y1[j], 0.1f);
                    GL.TexCoord2(1.0f, y1_);
                    GL.Vertex3(x2[j], y1[j], 0.1f);
                    GL.TexCoord2(1.0f, 1f - y1_);
                    GL.Vertex3(x2[j], y2[j], 0.1f);
                    GL.TexCoord2(0.0f, 1f - y1_);
                    GL.Vertex3(x1[j], y2[j], 0.1f);
                }

                GL.End();
            }

            GL.PopMatrix();
        }
    }
}
