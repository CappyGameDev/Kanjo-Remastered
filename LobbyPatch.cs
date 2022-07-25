using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using Photon.Pun;
using Photon.Realtime;

namespace KanjoRemastered {
	[HarmonyPatch]
	public static class LobbyPatch {
		static InputField inputfield = null;
		[HarmonyPatch(typeof(Menu), "Start")]
		static class StartPatch {
			private static void Postfix(Menu __instance) { // This is super unclean - if someone can do this better - please
				var player = __instance.menu.transform.Find("carInfo/player");
				if (player != null) {
					GameObject roomname = UnityEngine.Object.Instantiate<GameObject>(player.gameObject);
					roomname.name = "roomname";
					roomname.transform.SetParent(__instance.menu.transform);
					roomname.transform.localPosition = new Vector3(742,0,0);
					
					var icon = roomname.transform.Find("icon (1)/text (1)").gameObject;
					var text = icon.GetComponent<Text>();
					text.text = "ロビー - LOBBY";

					var placeholder = roomname.transform.Find("InputField/Placeholder").gameObject;
					text = placeholder.GetComponent<Text>();
					text.text = "Enter room code...";
					
					inputfield = roomname.transform.Find("InputField").gameObject.GetComponent<InputField>();
					inputfield.text = ""; 
					inputfield.characterLimit  = 16; 
					inputfield.characterValidation  = InputField.CharacterValidation.Alphanumeric; 

					inputfield.onEndEdit.SetPersistentListenerState(0, UnityEventCallState.Off);
				}
			}
		}
		
		
		[HarmonyPatch(typeof(Menu), "OnJoinedLobby")]
		static class JoinedPatch {
			private static bool Prefix(Menu __instance, GlobalManager ___globalManager) {
				if (inputfield != null && inputfield.text.Length > 0) {
					string roomName = "room";
					if (___globalManager.trackID == 9) {
						roomName = "touge";
					} else if (___globalManager.trackMode) {
						roomName = "track" + ___globalManager.trackID.ToString();
					}
					
					roomName += inputfield.text;
					
					PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions
					{
						MaxPlayers = 50,
						PlayerTtl = 1000,
						CleanupCacheOnLeave = true,
						IsOpen = true,
						IsVisible = false
					}, TypedLobby.Default, null);

					return false;
				}
				return true;
			}
		}
	}
}
