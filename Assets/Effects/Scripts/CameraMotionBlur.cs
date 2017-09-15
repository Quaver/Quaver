// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Camera/Camera Motion Blur")]
    public class CameraMotionBlur : PostEffectsBase
    {
        // make sure to match this to MAX_RADIUS in shader ('k' in paper)
        private static float s_MAX_RADIUS = 10.0f;

        public enum MotionBlurFilter
        {
            CameraMotion = 0,			// global screen blur based on cam motion
            LocalBlur = 1,				// cheap blur, no dilation or scattering
            Reconstruction = 2,			// advanced filter (simulates scattering) as in plausible motion blur paper
            ReconstructionDX11 = 3,		// advanced filter (simulates scattering) as in plausible motion blur paper
            ReconstructionDisc = 4,		// advanced filter using scaled poisson disc sampling
        }

        // settings
        public MotionBlurFilter filterType = MotionBlurFilter.Reconstruction;
        public bool preview = false;				// show how blur would look like in action ...
        public Vector3 previewScale = Vector3.one;	// ... given this movement vector

        // params
        public float movementScale = 0.0f;
        public float rotationScale = 1.0f;
        public float maxVelocity = 8.0f;	// maximum velocity in pixels
        public float minVelocity = 0.1f;	// minimum velocity in pixels
        public float velocityScale = 0.375f;	// global velocity scale
        public float softZDistance = 0.005f;	// for z overlap check softness (reconstruction filter only)
        public int velocityDownsample = 1;	// low resolution velocity buffer? (optimization)
        public LayerMask excludeLayers = 0;
        private GameObject _tmpCam = null;

        // resources
        public Shader shader;
        public Shader dx11MotionBlurShader;
        public Shader replacementClear;

        private Material _motionBlurMaterial = null;
        private Material _dx11MotionBlurMaterial = null;

        public Texture2D noiseTexture = null;
        public float jitter = 0.05f;

        // (internal) debug
        public bool showVelocity = false;
        public float showVelocityScale = 1.0f;

        // camera transforms
        private Matrix4x4 _currentViewProjMat;
        private Matrix4x4 _prevViewProjMat;
        private int _prevFrameCount;
        private bool _wasActive;
        // shortcuts to calculate global blur direction when using 'CameraMotion'
        private Vector3 _prevFrameForward = Vector3.forward;
        private Vector3 _prevFrameUp = Vector3.up;
        private Vector3 _prevFramePos = Vector3.zero;
        private Camera _camera;


        private void CalculateViewProjection()
        {
            Matrix4x4 viewMat = _camera.worldToCameraMatrix;
            Matrix4x4 projMat = GL.GetGPUProjectionMatrix(_camera.projectionMatrix, true);
            _currentViewProjMat = projMat * viewMat;
        }


        private new void Start()
        {
            CheckResources();

            if (_camera == null)
                _camera = GetComponent<Camera>();

            _wasActive = gameObject.activeInHierarchy;
            CalculateViewProjection();
            Remember();
            _wasActive = false; // hack to fake position/rotation update and prevent bad blurs
        }

        private void OnEnable()
        {
            if (_camera == null)
                _camera = GetComponent<Camera>();

            _camera.depthTextureMode |= DepthTextureMode.Depth;
        }

        private void OnDisable()
        {
            if (null != _motionBlurMaterial)
            {
                DestroyImmediate(_motionBlurMaterial);
                _motionBlurMaterial = null;
            }
            if (null != _dx11MotionBlurMaterial)
            {
                DestroyImmediate(_dx11MotionBlurMaterial);
                _dx11MotionBlurMaterial = null;
            }
            if (null != _tmpCam)
            {
                DestroyImmediate(_tmpCam);
                _tmpCam = null;
            }
        }


        public override bool CheckResources()
        {
            CheckSupport(true, true); // depth & hdr needed
            _motionBlurMaterial = CheckShaderAndCreateMaterial(shader, _motionBlurMaterial);

            if (supportDX11 && filterType == MotionBlurFilter.ReconstructionDX11)
            {
                _dx11MotionBlurMaterial = CheckShaderAndCreateMaterial(dx11MotionBlurShader, _dx11MotionBlurMaterial);
            }

            if (!isSupported)
                ReportAutoDisable();

            return isSupported;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (false == CheckResources())
            {
                Graphics.Blit(source, destination);
                return;
            }

            if (filterType == MotionBlurFilter.CameraMotion)
                StartFrame();

            // use if possible new RG format ... fallback to half otherwise
            var rtFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf) ? RenderTextureFormat.RGHalf : RenderTextureFormat.ARGBHalf;

            // get temp textures
            RenderTexture velBuffer = RenderTexture.GetTemporary(divRoundUp(source.width, velocityDownsample), divRoundUp(source.height, velocityDownsample), 0, rtFormat);
            int tileWidth = 1;
            int tileHeight = 1;
            maxVelocity = Mathf.Max(2.0f, maxVelocity);

            float _maxVelocity = maxVelocity; // calculate 'k'
            // note: 's' is hardcoded in shaders except for DX11 path

            // auto DX11 fallback!
            bool fallbackFromDX11 = filterType == MotionBlurFilter.ReconstructionDX11 && _dx11MotionBlurMaterial == null;

            if (filterType == MotionBlurFilter.Reconstruction || fallbackFromDX11 || filterType == MotionBlurFilter.ReconstructionDisc)
            {
                maxVelocity = Mathf.Min(maxVelocity, s_MAX_RADIUS);
                tileWidth = divRoundUp(velBuffer.width, (int)maxVelocity);
                tileHeight = divRoundUp(velBuffer.height, (int)maxVelocity);
                _maxVelocity = velBuffer.width / tileWidth;
            }
            else
            {
                tileWidth = divRoundUp(velBuffer.width, (int)maxVelocity);
                tileHeight = divRoundUp(velBuffer.height, (int)maxVelocity);
                _maxVelocity = velBuffer.width / tileWidth;
            }

            RenderTexture tileMax = RenderTexture.GetTemporary(tileWidth, tileHeight, 0, rtFormat);
            RenderTexture neighbourMax = RenderTexture.GetTemporary(tileWidth, tileHeight, 0, rtFormat);
            velBuffer.filterMode = FilterMode.Point;
            tileMax.filterMode = FilterMode.Point;
            neighbourMax.filterMode = FilterMode.Point;
            if (noiseTexture) noiseTexture.filterMode = FilterMode.Point;
            source.wrapMode = TextureWrapMode.Clamp;
            velBuffer.wrapMode = TextureWrapMode.Clamp;
            neighbourMax.wrapMode = TextureWrapMode.Clamp;
            tileMax.wrapMode = TextureWrapMode.Clamp;

            // calc correct viewprj matrix
            CalculateViewProjection();

            // just started up?
            if (gameObject.activeInHierarchy && !_wasActive)
            {
                Remember();
            }
            _wasActive = gameObject.activeInHierarchy;

            // matrices
            Matrix4x4 invViewPrj = Matrix4x4.Inverse(_currentViewProjMat);
            _motionBlurMaterial.SetMatrix("_InvViewProj", invViewPrj);
            _motionBlurMaterial.SetMatrix("_PrevViewProj", _prevViewProjMat);
            _motionBlurMaterial.SetMatrix("_ToPrevViewProjCombined", _prevViewProjMat * invViewPrj);

            _motionBlurMaterial.SetFloat("_MaxVelocity", _maxVelocity);
            _motionBlurMaterial.SetFloat("_MaxRadiusOrKInPaper", _maxVelocity);
            _motionBlurMaterial.SetFloat("_MinVelocity", minVelocity);
            _motionBlurMaterial.SetFloat("_VelocityScale", velocityScale);
            _motionBlurMaterial.SetFloat("_Jitter", jitter);

            // texture samplers
            _motionBlurMaterial.SetTexture("_NoiseTex", noiseTexture);
            _motionBlurMaterial.SetTexture("_VelTex", velBuffer);
            _motionBlurMaterial.SetTexture("_NeighbourMaxTex", neighbourMax);
            _motionBlurMaterial.SetTexture("_TileTexDebug", tileMax);

            if (preview)
            {
                // generate an artifical 'previous' matrix to simulate blur look
                Matrix4x4 viewMat = _camera.worldToCameraMatrix;
                Matrix4x4 offset = Matrix4x4.identity;
                offset.SetTRS(previewScale * 0.3333f, Quaternion.identity, Vector3.one); // using only translation
                Matrix4x4 projMat = GL.GetGPUProjectionMatrix(_camera.projectionMatrix, true);
                _prevViewProjMat = projMat * offset * viewMat;
                _motionBlurMaterial.SetMatrix("_PrevViewProj", _prevViewProjMat);
                _motionBlurMaterial.SetMatrix("_ToPrevViewProjCombined", _prevViewProjMat * invViewPrj);
            }

            if (filterType == MotionBlurFilter.CameraMotion)
            {
                // build blur vector to be used in shader to create a global blur direction
                Vector4 blurVector = Vector4.zero;

                float lookUpDown = Vector3.Dot(transform.up, Vector3.up);
                Vector3 distanceVector = _prevFramePos - transform.position;

                float distMag = distanceVector.magnitude;

                float farHeur = 1.0f;

                // pitch (vertical)
                farHeur = (Vector3.Angle(transform.up, _prevFrameUp) / _camera.fieldOfView) * (source.width * 0.75f);
                blurVector.x = rotationScale * farHeur;//Mathf.Clamp01((1.0ff-Vector3.Dot(transform.up, prevFrameUp)));

                // yaw #1 (horizontal, faded by pitch)
                farHeur = (Vector3.Angle(transform.forward, _prevFrameForward) / _camera.fieldOfView) * (source.width * 0.75f);
                blurVector.y = rotationScale * lookUpDown * farHeur;//Mathf.Clamp01((1.0ff-Vector3.Dot(transform.forward, prevFrameForward)));

                // yaw #2 (when looking down, faded by 1-pitch)
                farHeur = (Vector3.Angle(transform.forward, _prevFrameForward) / _camera.fieldOfView) * (source.width * 0.75f);
                blurVector.z = rotationScale * (1.0f - lookUpDown) * farHeur;//Mathf.Clamp01((1.0ff-Vector3.Dot(transform.forward, prevFrameForward)));

                if (distMag > Mathf.Epsilon && movementScale > Mathf.Epsilon)
                {
                    // forward (probably most important)
                    blurVector.w = movementScale * (Vector3.Dot(transform.forward, distanceVector)) * (source.width * 0.5f);
                    // jump (maybe scale down further)
                    blurVector.x += movementScale * (Vector3.Dot(transform.up, distanceVector)) * (source.width * 0.5f);
                    // strafe (maybe scale down further)
                    blurVector.y += movementScale * (Vector3.Dot(transform.right, distanceVector)) * (source.width * 0.5f);
                }

                if (preview) // crude approximation
                    _motionBlurMaterial.SetVector("_BlurDirectionPacked", new Vector4(previewScale.y, previewScale.x, 0.0f, previewScale.z) * 0.5f * _camera.fieldOfView);
                else
                    _motionBlurMaterial.SetVector("_BlurDirectionPacked", blurVector);
            }
            else
            {
                // generate velocity buffer
                Graphics.Blit(source, velBuffer, _motionBlurMaterial, 0);

                // patch up velocity buffer:

                // exclude certain layers (e.g. skinned objects as we cant really support that atm)

                Camera cam = null;
                if (excludeLayers.value != 0)// || dynamicLayers.value)
                    cam = GetTmpCam();

                if (cam && excludeLayers.value != 0 && replacementClear && replacementClear.isSupported)
                {
                    cam.targetTexture = velBuffer;
                    cam.cullingMask = excludeLayers;
                    cam.RenderWithShader(replacementClear, "");
                }
            }

            if (!preview && Time.frameCount != _prevFrameCount)
            {
                // remember current transformation data for next frame
                _prevFrameCount = Time.frameCount;
                Remember();
            }

            source.filterMode = FilterMode.Bilinear;

            // debug vel buffer:
            if (showVelocity)
            {
                // generate tile max and neighbour max
                //Graphics.Blit (velBuffer, tileMax, motionBlurMaterial, 2);
                //Graphics.Blit (tileMax, neighbourMax, motionBlurMaterial, 3);
                _motionBlurMaterial.SetFloat("_DisplayVelocityScale", showVelocityScale);
                Graphics.Blit(velBuffer, destination, _motionBlurMaterial, 1);
            }
            else
            {
                if (filterType == MotionBlurFilter.ReconstructionDX11 && !fallbackFromDX11)
                {
                    // need to reset some parameters for dx11 shader
                    _dx11MotionBlurMaterial.SetFloat("_MinVelocity", minVelocity);
                    _dx11MotionBlurMaterial.SetFloat("_VelocityScale", velocityScale);
                    _dx11MotionBlurMaterial.SetFloat("_Jitter", jitter);

                    // texture samplers
                    _dx11MotionBlurMaterial.SetTexture("_NoiseTex", noiseTexture);
                    _dx11MotionBlurMaterial.SetTexture("_VelTex", velBuffer);
                    _dx11MotionBlurMaterial.SetTexture("_NeighbourMaxTex", neighbourMax);

                    _dx11MotionBlurMaterial.SetFloat("_SoftZDistance", Mathf.Max(0.00025f, softZDistance));
                    _dx11MotionBlurMaterial.SetFloat("_MaxRadiusOrKInPaper", _maxVelocity);

                    // generate tile max and neighbour max
                    Graphics.Blit(velBuffer, tileMax, _dx11MotionBlurMaterial, 0);
                    Graphics.Blit(tileMax, neighbourMax, _dx11MotionBlurMaterial, 1);

                    // final blur
                    Graphics.Blit(source, destination, _dx11MotionBlurMaterial, 2);
                }
                else if (filterType == MotionBlurFilter.Reconstruction || fallbackFromDX11)
                {
                    // 'reconstructing' properly integrated color
                    _motionBlurMaterial.SetFloat("_SoftZDistance", Mathf.Max(0.00025f, softZDistance));

                    // generate tile max and neighbour max
                    Graphics.Blit(velBuffer, tileMax, _motionBlurMaterial, 2);
                    Graphics.Blit(tileMax, neighbourMax, _motionBlurMaterial, 3);

                    // final blur
                    Graphics.Blit(source, destination, _motionBlurMaterial, 4);
                }
                else if (filterType == MotionBlurFilter.CameraMotion)
                {
                    // orange box style motion blur
                    Graphics.Blit(source, destination, _motionBlurMaterial, 6);
                }
                else if (filterType == MotionBlurFilter.ReconstructionDisc)
                {
                    // dof style motion blur defocuing and ellipse around the princical blur direction
                    // 'reconstructing' properly integrated color
                    _motionBlurMaterial.SetFloat("_SoftZDistance", Mathf.Max(0.00025f, softZDistance));

                    // generate tile max and neighbour max
                    Graphics.Blit(velBuffer, tileMax, _motionBlurMaterial, 2);
                    Graphics.Blit(tileMax, neighbourMax, _motionBlurMaterial, 3);

                    Graphics.Blit(source, destination, _motionBlurMaterial, 7);
                }
                else
                {
                    // simple & fast blur (low quality): just blurring along velocity
                    Graphics.Blit(source, destination, _motionBlurMaterial, 5);
                }
            }

            // cleanup
            RenderTexture.ReleaseTemporary(velBuffer);
            RenderTexture.ReleaseTemporary(tileMax);
            RenderTexture.ReleaseTemporary(neighbourMax);
        }

        private void Remember()
        {
            _prevViewProjMat = _currentViewProjMat;
            _prevFrameForward = transform.forward;
            _prevFrameUp = transform.up;
            _prevFramePos = transform.position;
        }

        private Camera GetTmpCam()
        {
            if (_tmpCam == null)
            {
                string name = "_" + _camera.name + "_MotionBlurTmpCam";
                GameObject go = GameObject.Find(name);
                if (null == go) // couldn't find, recreate
                    _tmpCam = new GameObject(name, typeof(Camera));
                else
                    _tmpCam = go;
            }

            _tmpCam.hideFlags = HideFlags.DontSave;
            _tmpCam.transform.position = _camera.transform.position;
            _tmpCam.transform.rotation = _camera.transform.rotation;
            _tmpCam.transform.localScale = _camera.transform.localScale;
            _tmpCam.GetComponent<Camera>().CopyFrom(_camera);

            _tmpCam.GetComponent<Camera>().enabled = false;
            _tmpCam.GetComponent<Camera>().depthTextureMode = DepthTextureMode.None;
            _tmpCam.GetComponent<Camera>().clearFlags = CameraClearFlags.Nothing;

            return _tmpCam.GetComponent<Camera>();
        }

        private void StartFrame()
        {
            // take only x% of positional changes into account (camera motion)
            // TODO: possibly do the same for rotational part
            _prevFramePos = Vector3.Slerp(_prevFramePos, transform.position, 0.75f);
        }

        private static int divRoundUp(int x, int d)
        {
            return (x + d - 1) / d;
        }
    }
}
