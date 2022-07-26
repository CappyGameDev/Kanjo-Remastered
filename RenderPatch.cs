using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KanjoRemastered {
	[HarmonyPatch]
	public static class RenderPatch {
		public static void CustomRender() {
			UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
			GameObject[] rootObjects = activeScene.GetRootGameObjects();
			foreach (var obj in rootObjects) {
				if (obj.name == "Global Volume") {
					UnityEngine.Rendering.Volume vol = obj.GetComponent<Volume>();
					vol.profile.Remove<FilmGrain>();
					vol.sharedProfile.Remove<FilmGrain>();
				} else if (obj.name.StartsWith("DriftTrack")) {
					GameObject gobj = obj.transform.Find("Global Volume (1)").gameObject;
					UnityEngine.Rendering.Volume vol = gobj.GetComponent<Volume>();
					vol.profile.Remove<FilmGrain>();
					vol.sharedProfile.Remove<FilmGrain>();
				}
			}
		}
		
		[HarmonyPatch(typeof(GameUI), "Start")]
		public static class _GStartPatch {
			private static void Postfix(GameUI __instance) {
				CustomRender();
			}
		}
		
		[HarmonyPatch(typeof(Menu), "Start")]
		public static class _MStartPatch {
			private static void Postfix(Menu __instance) {
				CustomRender();
			}
		}
	}
}
