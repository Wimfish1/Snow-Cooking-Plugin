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
using System.Linq;
using UnityEngine;
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
            Logger.Log("Edited And Maintained By Wimfish1");

            BarricadeManager.onDeployBarricadeRequested += BarricadeDeployed;
            //UnturnedPlayerEvents.OnPlayerUpdateGesture += OnPlayerUpdateGesture;
            PlayerAnimator.OnGestureChanged_Global += OnGestureChanged;
            EffectManager.onEffectButtonClicked += ButtonClick;
            UseableConsumeable.onConsumePerformed += ConsumeAction;
            BarricadeManager.onSalvageBarricadeRequested += BarricadeSalvaged;
            BarricadeManager.onDamageBarricadeRequested += BarricadeDamaged;
            U.Events.OnPlayerDisconnected += PlayerDisconnected;
            BarricadeManager.onModifySignRequested += OnModifySign;
            if (Level.isLoaded)
            {
                BarricadeFunctions.AddExistingBarricades(1);
            }
            else
            {
                Level.onLevelLoaded += BarricadeFunctions.AddExistingBarricades;
            }
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
            //UnturnedPlayerEvents.OnPlayerUpdateGesture -= OnPlayerUpdateGesture;
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
            // DO STUFF EVERY GAME FRAME E.G 60/s
            if (GetCurrentTime() - timer >= 1)
            {
                timer = GetCurrentTime();

                HeaterFunctions.Update();
                PanFunctions.Update();
                DryinglampFunctions.Update();
                CocaineBagFunctions.Update();
            }
        }

        private Dictionary<Vector3, Transform> GetAllObjects()
        {
            Dictionary<Vector3, Transform> objectsOnMap = new Dictionary<Vector3, Transform>();
            foreach (var region in BarricadeManager.regions)
            {
                foreach (var drop in region.drops.Where(drop => !objectsOnMap.ContainsKey(drop.model.position)))
                {
                    objectsOnMap.Add(drop.model.position, drop.model);
                }
            }
            return objectsOnMap;
        }

        public Transform GetPlacedObjectTransform(Vector3 objectPosition)
        {
            Dictionary<Vector3, Transform> objectsOnMap;
            objectsOnMap = GetAllObjects();

            foreach (var mapObject in objectsOnMap.ToList())
            {
                if (mapObject.Key == objectPosition)
                {
                    return mapObject.Value;
                }
            }
            return null; //Never happens
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
    }
}