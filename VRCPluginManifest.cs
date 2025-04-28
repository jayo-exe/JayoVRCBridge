using System.IO;
using System.Reflection;
using UnityEngine;
using VNyanInterface;

namespace JayoVRCBridge
{
    public class VRCPluginManifest : IVNyanPluginManifest
    {
        public string PluginName { get; } = "JayoVRCBridge";
        public string Version { get; } = "v0.1.2";
        public string Title { get; } = "VRChat OSC Bridge";
        public string Author { get; } = "Jayo";
        public string Website { get; } = "https://jayo-exe.itch.io/vrchat-osc-bridge-for-vnyan";

        public void InitializePlugin()
        {
            // we're doing very little error checking since we should know the bundle exists and contain the prefab already!
            // .vnobj file needs to be included in the C# project with Build Action of "Embedded Resource"
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(this.GetType(), "JayoVRCBridge.vnobj"))
            {
                byte[] bundleData = new byte[stream.Length];
                stream.Read(bundleData, 0, bundleData.Length);
                AssetBundle bundle = AssetBundle.LoadFromMemory(bundleData);
                GameObject.Instantiate(bundle.LoadAsset<GameObject>(bundle.GetAllAssetNames()[0]));
            }
        }
    }
}
