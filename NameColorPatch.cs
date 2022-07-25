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
	public static class NameColorPatch {
		static string colorcode = "";
		[HarmonyPatch(typeof(Menu), "Start")]
		static class StartPatch {
			private static void Postfix(Menu __instance) {
				var player = __instance.menu.transform.Find("carInfo/player");
				if (player != null) {
					GameObject player_color = UnityEngine.Object.Instantiate<GameObject>(player.gameObject);
					player_color.name = "player_color";
					player_color.transform.SetParent(player);
					player_color.transform.localScale = player.localScale;
					player_color.transform.localPosition = new Vector3(520, 0, 0);
					
					var icon = player_color.transform.Find("icon (1)/text (1)").gameObject;
					var text = icon.GetComponent<Text>();
					text.text = "色 - COLOR";

					var placeholder = player_color.transform.Find("InputField/Placeholder").gameObject;
					text = placeholder.GetComponent<Text>();
					text.text = "Color Code";
					
					InputField inputfield = player_color.transform.Find("InputField").gameObject.GetComponent<InputField>();
					inputfield.text = colorcode; 
					inputfield.characterLimit  = 6; 
					inputfield.characterValidation  = InputField.CharacterValidation.Alphanumeric; 

					inputfield.onEndEdit.SetPersistentListenerState(0, UnityEventCallState.Off);
					inputfield.onEndEdit.AddListener((t) => {
						colorcode = t;
					});
				}
			}
		}
		
		
		[HarmonyPatch(typeof(PhotonNetwork), "NickName", MethodType.Setter)]
		static class SetNickPatch {
			private static void Prefix(ref string value) {
				if (colorcode.Length > 0) {
					value = $"<color=#{colorcode}>{value}</color>";
				}
			}
		}
		
		private static void _SendMessage(Chat _this, string player) {
			if (_this.messageText.text.Length > 0) {
				string text = player + ": ";
				text += _this.messageText.text;
				_this.photonView.RPC("NewMessage", RpcTarget.All, new object[] { text });
				_this.messageText.text = "";
				if (_this.gameUI.inMission && _this.gameUI.preStart < 2.96f) {
					// pass
				} else {
					_this.gameUI.activeCar.Driveble = true;
				}
			}
		}
		
		[HarmonyPatch(typeof(Chat), "SendMessage")]
		static class SendMsgPatch {
			private static bool Prefix(Chat __instance, GlobalManager ___globalManager) {
				if (colorcode.Length > 0) {
					string name = $"<color=#{colorcode}>{___globalManager.playerData.playerName}</color>";
					_SendMessage(__instance, name);
					return false;
				}
				return true;
			}
		}
	}
}
