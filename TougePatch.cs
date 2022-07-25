using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using Steamworks;
using System;
using System.Text;

using Photon.Pun;
using Photon.Realtime;

namespace KanjoRemastered {
    [HarmonyPatch]
    public static class LobbyTougePatch {
        private static string GetSteamAuthTicket(out HAuthTicket ticket) {
            byte[] array = new byte[1024];
            uint num;
            ticket = SteamUser.GetAuthSessionTicket(array, array.Length, out num);
            Array.Resize<byte>(ref array, (int)num);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < num; i++) {
                stringBuilder.AppendFormat("{0:x2}", array[i]);
            }
            return stringBuilder.ToString();
        }
        
        public static void GoToTouge(Menu _this) {
            _this.clickSound.Play();
            GlobalManager globalManager = Traverse.Create(_this).Field("globalManager").GetValue() as GlobalManager;
            
            if (globalManager.playerData.offlineMode == 0)
            {
                if (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.OSXEditor && SteamManager.Initialized)
                {
                    HAuthTicket hAuthTicket = (HAuthTicket)Traverse.Create(_this).Field("hAuthTicket").GetValue();
                    
                    string steamAuthTicket = GetSteamAuthTicket(out hAuthTicket);
                    PhotonNetwork.AuthValues = new AuthenticationValues();
                    PhotonNetwork.AuthValues.UserId = SteamUser.GetSteamID().ToString();
                    PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Steam;
                    PhotonNetwork.AuthValues.AddAuthParameter("ticket", steamAuthTicket);
                }
                PhotonNetwork.NickName = globalManager.playerData.playerName;
                PhotonNetwork.SendRate = 20;
                PhotonNetwork.SerializationRate = 20;
                PhotonNetwork.ConnectUsingSettings();
                globalManager.trackMode = true;
                globalManager.trackID = 9; 
                _this.loading.SetActive(true);
                _this.roomCount = 0;
                return;
            }
            globalManager.trackMode = true;
            globalManager.trackID = 9;
            _this.loading.SetActive(true);
            _this.roomCount = 0;
            Application.LoadLevel("game");
        }
        
        [HarmonyPatch(typeof(Menu), "Start")]
        static class StartPatch {
            private static void Postfix(Menu __instance) { // This is super unclean - if someone can do this better - please
                var map = __instance.menu.transform.Find("map");
                var play = __instance.menu.transform.Find("play");
                var tracks = __instance.menu.transform.Find("tracks");
                
                map.localPosition += new Vector3(0, 100, 0);
                play.localPosition += new Vector3(0, 100, 0);
                
                GameObject touge = UnityEngine.Object.Instantiate<GameObject>(tracks.gameObject);
                touge.name = "touge";
                touge.transform.SetParent(__instance.menu.transform);
                touge.transform.localPosition = tracks.localPosition;
                touge.transform.localScale = tracks.localScale;
                
                tracks.localPosition += new Vector3(0, 100, 0);
                
                var ttext = touge.transform.Find("text").gameObject.GetComponent<Text>();
                ttext.text = "GO TO TOUGE";
                var tbutton = touge.GetComponent<Button>();
                
                tbutton.onClick.SetPersistentListenerState(0, UnityEventCallState.Off);
                tbutton.onClick.AddListener(() => GoToTouge(__instance));
            }
        }
    }
    
    [HarmonyPatch]
    public static class GameTougePatch {
		static Vector3 tougeSpawn = new Vector3(3380, 160, -965);
		static Quaternion tougeRot = new Quaternion(0, 180, 0, 1);
		
		static void _SpawnCar(GameUI _this, GlobalManager globalManager, CarData carData) {
			if (_this.activeCar) {
				UnityEngine.Object.Destroy(_this.activeCar.gameObject);
			}
			if (PhotonNetwork.InRoom) {
				_this.activeCar = PhotonNetwork.Instantiate("Cars/" + carData.name, tougeSpawn, tougeRot, 0, null).GetComponent<Vehicle>();
			} else {
				_this.activeCar = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Cars/" + carData.name), tougeSpawn, tougeRot).GetComponent<Vehicle>();
			}
			_this.activeCar.carData = carData;
			_this.activeCar.forcedDriftMode = false;
			_this.activeCar.driftMode = false;
		}
		
        [HarmonyPatch(typeof(GameUI), "Start")]
        static class StartPatch {
            private static void Postfix(GameUI __instance) {
				GlobalManager globalManager = UnityEngine.Object.FindObjectOfType<GlobalManager>();
				if (globalManager.trackMode && globalManager.trackID == 9) {
					UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
					GameObject[] rootObjects = activeScene.GetRootGameObjects();
					foreach (var obj in rootObjects) {
						if (obj.name == "touge_3") {
							obj.SetActive(true);
							return;
						}
					}
					__instance.map.SetActive(true); // Fallback
				}
				

				
            }
        }
		
        [HarmonyPatch(typeof(GameUI), "SpawnCar")]
        static class SpawnCar {
            private static bool Prefix(GameUI __instance, CarData carData) {
                GlobalManager globalManager = Traverse.Create(__instance).Field("globalManager").GetValue() as GlobalManager;
				if (globalManager.trackMode && globalManager.trackID == 9) {
					_SpawnCar(__instance, globalManager, carData);
					return false;
				}
				return true;
            }
        }
    }
}
