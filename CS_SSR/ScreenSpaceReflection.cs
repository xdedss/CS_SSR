using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ScreenSpaceReflection : MonoBehaviour
{

    public int dbg = 0;

    public Vector4 rayPar = new Vector4(80, 64, 16, 4);
    public float pixelThickness = 0.18f;
    public float reflectionDecay = 0f;

    public Material mat;
    public ComputeShader blur;
    public Shader backfaceShader;
    public RenderTexture mid;
    public RenderTexture mid2;

    private int ScreenWidth
    {
        get
        {
            return Screen.width;
        }
    }

    private int ScreenHeight
    {
        get
        {
            return Mathf.RoundToInt(Screen.width / Camera.main.aspect);
        }
    }

    private void Start()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
        backfaceCamera = null;
        mid = new RenderTexture(ScreenWidth, ScreenHeight, 0);
        mid.enableRandomWrite = true;
        mid.Create();
        mid2 = new RenderTexture(ScreenWidth, ScreenHeight, 0);
        mid2.enableRandomWrite = true;
        mid2.Create();
    }

    private Camera backfaceCamera;
    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderBackface();
        mat.SetTexture("_BackfaceTex", GetBackfaceTexture());
        mat.SetMatrix("_WorldToView", GetComponent<Camera>().worldToCameraMatrix);

        mat.SetInt("_Debug", dbg);
        mat.SetVector("_RayPar", rayPar);
        mat.SetVector("_nfplane", new Vector4(Camera.main.nearClipPlane, Camera.main.farClipPlane, pixelThickness, reflectionDecay));
        
        blur.SetBool("dbg", dbg != 0);

        if (mid == null || mid2 == null || mid.width != ScreenWidth || mid.height != ScreenHeight || mid2.width != ScreenWidth || mid2.height != ScreenHeight)
        {
            mid = new RenderTexture(ScreenWidth, ScreenHeight, 0);
            mid.enableRandomWrite = true;
            mid.Create();
            mid2 = new RenderTexture(ScreenWidth, ScreenHeight, 0);
            mid2.enableRandomWrite = true;
            mid2.Create();
        }
        Graphics.Blit(source, mid, mat, 0);
        //Graphics.Blit(mid, destination);

        int kernel = blur.FindKernel("CSMain");
        blur.SetTexture(kernel, "Source", source);
        blur.SetTexture(kernel, "Input", mid);
        blur.SetTexture(kernel, "Result", mid2);
        blur.Dispatch(kernel, Mathf.CeilToInt(ScreenWidth / 8f), Mathf.CeilToInt(ScreenHeight / 8f), 1);

        Graphics.Blit(mid2, destination);
    }

    private void RenderBackface()
    {
        if (backfaceCamera == null)
        {
            var t = new GameObject();
            var mainCamera = Camera.main;
            t.transform.SetParent(mainCamera.transform);
            t.hideFlags = HideFlags.HideAndDontSave;
            backfaceCamera = t.AddComponent<Camera>();
            backfaceCamera.CopyFrom(mainCamera);
            backfaceCamera.enabled = false;
            backfaceCamera.clearFlags = CameraClearFlags.SolidColor;
            backfaceCamera.backgroundColor = Color.black;
            backfaceCamera.renderingPath = RenderingPath.DeferredShading;
            backfaceCamera.depthTextureMode = DepthTextureMode.DepthNormals;
            backfaceCamera.SetReplacementShader(backfaceShader, "RenderType");
            //backfaceCamera.targetTexture = GetBackfaceTexture();
        }
        else if (backfaceCamera.aspect != Camera.main.aspect
            || backfaceCamera.nearClipPlane != Camera.main.nearClipPlane
            || backfaceCamera.farClipPlane != Camera.main.farClipPlane)
        {
            backfaceCamera.CopyFrom(Camera.main);
            backfaceCamera.enabled = false;
            backfaceCamera.clearFlags = CameraClearFlags.SolidColor;
            backfaceCamera.backgroundColor = Color.black;
            backfaceCamera.renderingPath = RenderingPath.DeferredShading;
            backfaceCamera.depthTextureMode = DepthTextureMode.DepthNormals;
            backfaceCamera.SetReplacementShader(backfaceShader, "RenderType");
            //backfaceCamera.targetTexture = GetBackfaceTexture();
        }
        backfaceCamera.targetTexture = GetBackfaceTexture();
        backfaceCamera.Render();

    }

    [SerializeField]
    private RenderTexture backfaceText;
    private RenderTexture GetBackfaceTexture()
    {
        if (backfaceText == null || backfaceText.width != ScreenWidth || backfaceText.height != ScreenHeight)
        {
            backfaceText = new RenderTexture(ScreenWidth, ScreenHeight, 24, RenderTextureFormat.RFloat);
            backfaceText.filterMode = FilterMode.Point;     //VERY FUCKING IMPORTANT! COST ME TOATL 5 HOURS TO DEBUG
        }
        return backfaceText;
    }
}
