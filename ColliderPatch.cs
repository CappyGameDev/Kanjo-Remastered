using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace KanjoRemastered {
	
	[HarmonyPatch]
	static class ColliderPatch {
		[HarmonyPatch(typeof(GameUI), "Start")]
		static class ColliderStartPatch {
			private static void Postfix(GameUI __instance) {
				var generated = __instance.trackMaps[2].transform.Find("barriers/generated");
				for (int i = 500; i < generated.childCount; i++) {
					var child = generated.transform.GetChild(i).gameObject;
					if (child.name.StartsWith("Collider")) {
						child.SetActive(false);
					}
				}
			}
		}
	}
}
