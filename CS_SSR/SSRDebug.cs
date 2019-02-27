#define dbg

using System;
using System.Collections;
using System.IO;
using UnityEngine;
using ColossalFramework;

namespace CS_SSR
{
    class SSRDebug : MonoBehaviour
    {
        ScreenSpaceReflection ssr;
        Material ssrmat;
        Shader backfaceshader;
        ComputeShader cs;

        void Start()
        {
            StartCoroutine(LoadResources());
        }

        void Update()
        {
            if (Camera.main.depthTextureMode != DepthTextureMode.DepthNormals)
            {
                Camera.main.depthTextureMode = DepthTextureMode.DepthNormals;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                LogMsg(Environment.CurrentDirectory);
                StartCoroutine(LoadResources());
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                ssr.dbg--;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                ssr.dbg++;
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                Camera.main.depthTextureMode = DepthTextureMode.DepthNormals;//backup
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                var tm = Singleton<TerrainManager>.instance;
                LogMsg(tm.TransparentWater.ToString());
                LogMsg(tm.m_waterMaterial.shader.name);
                LogMsg(tm.m_waterTransparentMaterial.shader.name);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                var tm = Singleton<TerrainManager>.instance;
                tm.m_waterMaterial = ssrmat;
                tm.m_waterTransparentMaterial = ssrmat;
            }
        }

        IEnumerator LoadResources()
        {
            LogMsg("start");
            yield return new WaitForSeconds(2);

            string abpath = Environment.CurrentDirectory + '/' + "ssrassets";
            byte[] abbytes = Properties.Resources.ssrassets;
            try
            {
                FileStream fs = File.Create(abpath);
                fs.Write(abbytes, 0, abbytes.Length);
                fs.Flush();
                fs.Close();
            }
            catch (Exception ex)
            {
                LogErr(ex.ToString() + ex.StackTrace);
            }
            WWW www = new WWW("file:///" + abpath);

            LogMsg("start www");
            yield return www;
            LogMsg("finish www");

            try
            {
                if (www != null && www.assetBundle != null)
                {
                    AssetBundle ab = www.assetBundle;
                    //var a = ab.LoadAsset("assets/testssr/screenspaceraytrace.cginc");
                    ab.LoadAsset("assets/testssr/screenspacereflection.shader");
                    cs = ab.LoadAsset<ComputeShader>("assets/testssr/blurssr.compute");
                    backfaceshader = ab.LoadAsset<Shader>("assets/testssr/backfaceshader.shader");
                    ssrmat = ab.LoadAsset<Material>("assets/testssr/ssrmat.mat");
                    if (cs != null && backfaceshader != null && ssrmat != null)
                    {
                        //succeed
                        //LogMsg(a.GetType().ToString());
                    }
                    else
                    {
                        LogErr("can not load assets:" + cs + " " + backfaceshader + " " + ssrmat);
                    }
                }
                else
                {
                    LogErr("can not load file");
                }
            }
            catch (Exception ex)
            {
                LogErr(ex.ToString() + ex.StackTrace);
            }

            LogMsg("resources loaded");

            yield return new WaitForSeconds(1);
            yield return 0;
            ssr = Camera.main.gameObject.AddComponent<ScreenSpaceReflection>();
            ssr.mat = ssrmat;
            ssr.blur = cs;
            ssr.backfaceShader = backfaceshader;
            LogMsg("ssr component");

            if (File.Exists(abpath))
            {
                File.Delete(abpath);
            }

        }

        void OnGUI()
        {
#if dbg
            GUI.Label(new Rect(0, 0, 200, 30), "SSRDebug is on");
            GUI.Label(new Rect(0, 30, 200, 30), "aspect " + Camera.main.aspect);
            GUI.Label(new Rect(0, 60, 200, 30), "depth " + Camera.main.depthTextureMode);
            GUI.Label(new Rect(0, 90, 200, 30), "n&f " + Camera.main.nearClipPlane + "~" + Camera.main.farClipPlane);

            ssr.pixelThickness = GUI.HorizontalSlider(new Rect(20, 120, 400, 30), ssr.pixelThickness, 0, 1);
            GUI.Label(new Rect(420, 120, 200, 30), ssr.pixelThickness.ToString());
            ssr.reflectionDecay = GUI.HorizontalSlider(new Rect(20, 150, 400, 30), ssr.reflectionDecay, 0, 5);
            GUI.Label(new Rect(420, 150, 200, 30), ssr.reflectionDecay.ToString());
            ssr.rayPar.x = GUI.HorizontalSlider(new Rect(20, 180, 400, 30), ssr.rayPar.x, 0, 200);
            GUI.Label(new Rect(420, 180, 200, 30), ssr.rayPar.x.ToString());
            ssr.rayPar.y = GUI.HorizontalSlider(new Rect(20, 210, 400, 30), ssr.rayPar.y, 16, 80);
            GUI.Label(new Rect(420, 210, 200, 30), ssr.rayPar.y.ToString());
            ssr.rayPar.z = GUI.HorizontalSlider(new Rect(20, 240, 400, 30), ssr.rayPar.z, 4, 32);
            GUI.Label(new Rect(420, 240, 200, 30), ssr.rayPar.z.ToString());
            ssr.rayPar.w = GUI.HorizontalSlider(new Rect(20, 270, 400, 30), ssr.rayPar.w, 1, 8);
            GUI.Label(new Rect(420, 270, 200, 30), ssr.rayPar.w.ToString());

#endif
        }

        void LogErr(string msg)
        {
            Debug.LogError(msg);
#if dbg
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, msg);
#endif
        }

        void LogMsg(string msg)
        {
            Debug.Log(msg);
#if dbg
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, msg);
#endif
        }

    }
}
