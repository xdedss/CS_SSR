using System;
using System.Collections.Generic;
using ICities;
using UnityEngine;
using System.Reflection;

namespace CS_SSR
{
    public class SSRMod : IUserMod
    {
        public string Name { get { return "Screen Space Reflection"; } }

        public string Description { get { return "emmmmmm"; } }
    }

    public class FlagsModLoading : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            Camera.main.gameObject.AddComponent<SSRDebug>();
        }
    }

    public class FlagsModAssetData : AssetDataExtensionBase
    {
        public override void OnAssetLoaded(string name, object asset, Dictionary<string, byte[]> userData)
        {

        }
    }
}
