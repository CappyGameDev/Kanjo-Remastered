using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace KanjoRemastered {
	
	[HarmonyPatch]
	static class OptionsPatch {
		static Vector3 spawnPosition;
		static Quaternion spawnRotation;
		
		[HarmonyPatch(typeof(GameUI), "SpawnCar")]
		static class SpawnPatch {
			private static void Postfix(GameUI __instance) {
				spawnPosition = __instance.activeCar.transform.position;
				spawnRotation = __instance.activeCar.transform.rotation;
			}
		}
		
		[HarmonyPatch(typeof(GameUI), "Start")]
		static class StartPatch {
			static void Unflip(GameUI _this) {
				_this.activeCar.transform.position += new Vector3(0, 0.5f, 0);
				_this.activeCar.transform.rotation = new Quaternion(0, _this.activeCar.transform.rotation.y, 0, _this.activeCar.transform.rotation.w);
			}
			
			static void Respawn(GameUI _this) {
				_this.activeCar.transform.position = spawnPosition;
				_this.activeCar.transform.rotation = spawnRotation;
				_this.activeCar.localVel = Vector3.zero;
				_this.activeCar.Speed = 0f;
				_this.activeCar.rigidbody.velocity = Vector3.zero;;
				_this.activeCar.rigidbody.angularVelocity = Vector3.zero;;
			}
			
			static void ToggleDrift(GameUI _this, Text txt) {
				if (_this.activeCar.forcedDriftMode) {
					_this.activeCar.forcedDriftMode = false;
					txt.text = "Standard";
				} else {
					_this.activeCar.forcedDriftMode = true;
					_this.activeCar.driftMode = true;
					txt.text = "Drift";
				}
			}
			
			private static void Postfix(GameUI __instance) {
				Transform gearbox = __instance.pauseMenu.transform.Find("gearbox");
				GameObject unflip = UnityEngine.Object.Instantiate<GameObject>(gearbox.gameObject);
				
				unflip.name = "unflip";
				unflip.transform.SetParent(__instance.pauseMenu.transform);
				unflip.transform.localPosition = gearbox.localPosition;
				unflip.transform.localPosition -= new Vector3(0, 340, 0);
				unflip.transform.localScale = gearbox.localScale;
				
				var ttext = unflip.transform.Find("text").gameObject;
				ttext.SetActive(false);
				
				var tval = unflip.transform.Find("value").gameObject.GetComponent<Text>();
				tval.text = "Unflip";
				var ubutton = unflip.GetComponent<Button>();
				
				ubutton.onClick.SetPersistentListenerState(0, UnityEventCallState.Off);
				
				
				GameObject respawn = UnityEngine.Object.Instantiate<GameObject>(unflip.gameObject);
				respawn.name = "respawn";            
				respawn.transform.SetParent(__instance.pauseMenu.transform);
				respawn.transform.localPosition = gearbox.localPosition;
				respawn.transform.localPosition -= new Vector3(0, 270, 0);
				respawn.transform.localScale = gearbox.localScale;			
				tval = respawn.transform.Find("value").gameObject.GetComponent<Text>();
				tval.text = "Respawn";
				var rbutton = respawn.GetComponent<Button>();


				GameObject driftmode = UnityEngine.Object.Instantiate<GameObject>(unflip.gameObject);
				driftmode.name = "driftmode";            
				driftmode.transform.SetParent(__instance.pauseMenu.transform);
				driftmode.transform.localPosition = gearbox.localPosition;
				driftmode.transform.localPosition -= new Vector3(0, 200, 0);
				driftmode.transform.localScale = gearbox.localScale;			
				tval = driftmode.transform.Find("value").gameObject.GetComponent<Text>();
				tval.text = "Standard";
				
				var xtext = driftmode.transform.Find("text").gameObject;
				xtext.SetActive(true);
				var _ttext = xtext.GetComponent<Text>();
				_ttext.text = "Grip Mode";
				
				GlobalManager globalManager = UnityEngine.Object.FindObjectOfType<GlobalManager>();
				if (globalManager.trackMode && globalManager.trackID < 3) {
					tval.text = "Drift";
				} else {
					tval.text = "Standard";
				}
				
				var dbutton = driftmode.GetComponent<Button>();
				
				ubutton.onClick.AddListener(() => Unflip(__instance));
				rbutton.onClick.AddListener(() => Respawn(__instance));
				dbutton.onClick.AddListener(() => ToggleDrift(__instance, tval));
				
			}
		}
	}
}
