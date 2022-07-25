using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace KanjoRemastered {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Kanjozoku Game.exe")]
    public class RemasteredBase : BaseUnityPlugin {
        
        public Harmony Harmony { get; } = new Harmony(PluginInfo.PLUGIN_GUID);
        
        public static RemasteredBase Instance = null;
        
        private void Awake() {
            /* Keep Instance */
            Instance = this;
            
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
