using Ocelot.SnowCooking.functions;
using Ocelot.SnowCooking.objects;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Environment = System.Environment;
using Logger = Rocket.Core.Logging.Logger;

namespace Ocelot.SnowCooking
{
    public class SnowCookingPlugin : RocketPlugin<SnowCookingConfiguration>
    {
        public static SnowCookingPlugin Instance;
        public const string VERSION = "1.2.14";

        private int Frame = 0;
        private double timer = 0;
        public Dictionary<Transform, HeaterObject> heaterList = new Dictionary<Transform, HeaterObject>();
        public Dictionary<Transform, PanObject> panList = new Dictionary<Transform, PanObject>();
        public Dictionary<string, Vector3> heaterUiOpened = new Dictionary<string, Vector3>();
        public Dictionary<Transform, PanFilledObject> panFilledList = new Dictionary<Transform, PanFilledObject>();
        public Dictionary<Transform, PanPowderObject> panPowderList = new Dictionary<Transform, PanPowderObject>();

        public List<Transform> cocaLeavesList = new List<Transform>();
        public List<Transform> dryingLampList = new List<Transform>();
        public List<DrugeffectTimeObject> drugeffectPlayersList = new List<DrugeffectTimeObject>();
        protected override void Load()
        {
            Instance = this;
            Logger.Log("SnowCookingPlugin v" + VERSION + " loaded!", ConsoleColor.Yellow);
            Logger.Log("Original Plugin By: Ocelot");
            Logger.Log("Edited And Maintained By Wimfish1 :-)");

            BarricadeManager.onDeployBarricadeRequested += BarricadeDeployed;
            PlayerAnimator.OnGestureChanged_Global += OnGestureChanged;
            EffectManager.onEffectButtonClicked += ButtonClick;
            UseableConsumeable.onConsumePerformed += ConsumeAction;
            BarricadeManager.onSalvageBarricadeRequested += BarricadeSalvaged;
            BarricadeManager.onDamageBarricadeRequested += BarricadeDamaged;
            U.Events.OnPlayerDisconnected += PlayerDisconnected;
            BarricadeManager.onModifySignRequested += OnModifySign;

            if (Level.isLoaded)
            {
                AddExistingBarricades(1);
            }
            else
            {
                Level.onLevelLoaded += AddExistingBarricades;
            }
        }

        
        private void AddExistingBarricades(int level)
        {
            Logger.Log("Adding map Barricades to list...", ConsoleColor.Green);

            foreach (var region in BarricadeManager.regions)
            {
                foreach (var drop in region.drops)
                {
                    if (drop.asset.id != Configuration.Instance.dryingLampId) continue; // Only process drying lamps
                    var barricade = region.findBarricadeByInstanceID(drop.instanceID);

                    if (barricade == null) continue;

                    Transform barricadeTransform = null;

                    try
                    {
                        barricadeTransform = GetPlacedObjectTransform(barricade.point);

                        if (barricadeTransform != null)
                        {
                            if (!dryingLampList.Contains(barricadeTransform))
                            {
                                dryingLampList.Add(barricadeTransform);
                            }
                            else
                            {
                                Logger.Log("Duplicated entry detected, skipping object. (No need to worry)", ConsoleColor.Yellow);
                            }
                        }
                        else
                        {
                            Logger.Log($"Could not find transform for barricade at {barricade.point}.", ConsoleColor.Red);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error getting transform for barricade at {barricade.point}: {ex.Message}", ConsoleColor.Red);
                    }
                }
            }
            Logger.Log("All barricades added.", ConsoleColor.Green);
        }
        
        public void OnGestureChanged(PlayerAnimator arg1, EPlayerGesture gesture)
        {
            if (gesture != EPlayerGesture.PUNCH_LEFT && gesture != EPlayerGesture.PUNCH_RIGHT) return;
            HeaterFunctions.OnGestureChanged(UnturnedPlayer.FromPlayer(arg1.player), gesture);
            PanFunctions.OnGestureChanged(UnturnedPlayer.FromPlayer(arg1.player), gesture);
        }

        public void BarricadeDamaged(CSteamID instigatorSteamID, Transform barricadeTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            BarricadeFunctions.BarricadeDamaged(barricadeTransform, pendingTotalDamage);
        }

        private void OnModifySign(CSteamID instigator, InteractableSign sign, ref string text, ref bool shouldAllow)
        {
            foreach (var heater in heaterList)
            {
                if (heater.Key != sign.transform) continue;
                shouldAllow = false;
                ChatManager.serverSendMessage(Translate("cocaine_sign_edit"), Color.white, null, UnturnedPlayer.FromCSteamID(instigator).SteamPlayer(), EChatMode.SAY, Configuration.Instance.iconImageUrl, true);
            }
        }

        public void BarricadeSalvaged(CSteamID steamID, byte x, byte y, ushort plant, ushort index, ref bool shouldAllow)
        {
            BarricadeFunctions.BarricadeSalvaged(steamID, x, y, plant, index);
        }

        protected override void Unload()
        {
            BarricadeManager.onDeployBarricadeRequested -= BarricadeDeployed; 
            PlayerAnimator.OnGestureChanged_Global -= OnGestureChanged;
            EffectManager.onEffectButtonClicked -= ButtonClick;
            UseableConsumeable.onConsumePerformed -= ConsumeAction;
            BarricadeManager.onSalvageBarricadeRequested -= BarricadeSalvaged;
            U.Events.OnPlayerDisconnected -= PlayerDisconnected;
        }

        private void PlayerDisconnected(UnturnedPlayer player)
        {
            if (heaterUiOpened.ContainsKey(player.Id))
            {
                heaterUiOpened.Remove(player.Id);
            }
        }

        public void ConsumeAction(Player instigatingPlayer, ItemConsumeableAsset consumeableAsset)
        {
            CocaineBagFunctions.ConsumeAction(instigatingPlayer, consumeableAsset);
        }

        public void ButtonClick(Player player, string buttonName)
        {
            UiFunctions.ButtonClick(player, buttonName);
        }

        public void BarricadeDeployed(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            var pos = point;
            var ownerPan = owner;
            var groupPan = group;
            var angleXPan = angle_x;
            var angleYPan = angle_y;
            var angleZPan = angle_z;
            BarricadeFunctions.BarricadeDeployed(barricade, asset, hit, pos, angleXPan, angleYPan, angleZPan, ownerPan, groupPan);
        }

        

        public override TranslationList DefaultTranslations => new TranslationList
        {
            {"temperature_symbol", " °C" },
            {"temperature_title", "Temperature" },
            {"cocaine_sign_edit", "<color=#00B92C>[SnowCooking]</color> Sorry, but you are <color=#ff3c19>not allowed</color> to edit this sign." }
        };

        private void Update()
        {
            Frame++;
            if (Frame % 5 != 0) return; // BRICHT METHODE AB WENN DER FRAME NICHT DURCH 5 TEILBAR IST
            if (GetCurrentTime() - timer >= 1)
            {
                timer = GetCurrentTime();

                HeaterFunctions.Update();
                PanFunctions.Update();
                DryinglampFunctions.Update();
                CocaineBagFunctions.Update();
            }
        }

        private static Dictionary<Vector3, Transform> GetAllObjects()
        {
            var objectsOnMap = new Dictionary<Vector3, Transform>();
            foreach (var region in BarricadeManager.regions)
            {
                foreach (var drop in region.drops)
                {
                    if (!objectsOnMap.ContainsKey(drop.model.position))
                    {
                        objectsOnMap.Add(drop.model.position, drop.model);
                    }
                }
            }
            return objectsOnMap;
        }
        
        public Transform GetPlacedObjectTransform(Vector3 objectPosition)
        {
            Dictionary<Vector3, Transform> objectsOnMap = GetAllObjects();
            float tolerance = 0.2f;

            foreach (var mapObject in objectsOnMap.ToList())
            {
                if (Vector3.Distance(mapObject.Key, objectPosition) < tolerance)
                {
                    return mapObject.Value;
                }
            }
            return null;
        }


        //FOR SIGN
        public Dictionary<Vector3, InteractableSign> GetAllSigns()
        {
            Dictionary<Vector3, InteractableSign> objectsOnMap = new Dictionary<Vector3, InteractableSign>();
            foreach (var region in BarricadeManager.regions)
            {
                foreach (var drop in region.drops)
                {
                    if (objectsOnMap.ContainsKey(drop.model.position)) continue;
                    var sign = (InteractableSign)drop.interactable;
                    objectsOnMap.Add(drop.model.position, sign);
                }
            }
            return objectsOnMap;
        }

        public InteractableSign GetPlacedObjectSign(Vector3 objectPosition)
        {
            Dictionary<Vector3, InteractableSign> objectsOnMap;
            objectsOnMap = GetAllSigns();

            for (var index = 0; index < objectsOnMap.ToList().Count; index++)
            {
                var mapObject = objectsOnMap.ToList()[index];
                if (mapObject.Key == objectPosition)
                {
                    return mapObject.Value;
                }
            }

            return null; //Never happens
        }

        public BarricadeData GetBarricadeDataAtPosition(Vector3 position)
        {
            foreach (var region in BarricadeManager.regions)
            {
                for (var index = 0; index < region.barricades.Count; index++)
                {
                    var b = region.barricades[index];
                    if (b.point == position) return b;
                }
            }
            return null;
        }

        public static Int32 GetCurrentTime()
        {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public void Wait(float seconds, System.Action action)
        {
            StartCoroutine(_wait(seconds, action));
        }

        static IEnumerator _wait(float time, System.Action callback)
        {
            yield return new WaitForSeconds(time);
            callback();
        }

        private bool runOnce = false; 
        
        private void RunOnce(int level){
            if(runOnce == false){
                BarricadeFunctions.AddExistingBarricades(level);
                runOnce = true;
            } else {
                Level.onLevelLoaded -= RunOnce;
            }
        }
    }
}