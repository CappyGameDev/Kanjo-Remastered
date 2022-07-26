using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Reflection;

namespace KanjoRemastered {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Kanjozoku Game.exe")]
    public class RemasteredBase : BaseUnityPlugin {
        
        public Harmony Harmony { get; } = new Harmony(PluginInfo.PLUGIN_GUID);
        
        public static RemasteredBase Instance = null;
        
		public static AssetBundle assets;
		
        private void Awake() {
            /* Keep Instance */

			/* Assets */
			/*
			Assembly assembly = Assembly.GetExecutingAssembly();
			var stream = assembly.GetManifestResourceStream("KanjoRemastered.UI.SliderUI.unitypackage");
			if (stream == null) {
				Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} could not load assets!");
				return;
			}
			var array = new byte[stream.Length];
			stream.Read(array, 0, (int)stream.Length);
			assets = UnityEngine.AssetBundle.LoadFromMemory(array);
			*/
			
            /* Unity Patching */
            Harmony.PatchAll();
            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} is loaded!");
        }
        
        private void _Log(string msg, LogLevel lvl) {
            Logger.Log(lvl, msg);
        }

        public static void Log(string msg, LogLevel lvl = LogLevel.Info) {
            if (RemasteredBase.Instance == null)
                return;
            Instance._Log(msg, lvl);
        }
    }

    [HarmonyPatch(typeof(Vehicle), "GamepadControl")] /* Gamepad Camera */
    public static class CamPatch {
        private static void Postfix(Vehicle __instance) {
			var rot = __instance.globalManager.joystickManager.RotateValue;
			if (Mathf.Abs(rot.x) > 0.01f) {
				var angle = rot.x * 90f;
				if (rot.y < -0.01f) {
					angle = 180f + rot.x * -90f;
				}
				__instance.CAM.localEulerAngles = new Vector3(0f, angle, 0f);
			}
        }
    }
}
