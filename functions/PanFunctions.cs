using JetBrains.Annotations;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Ocelot.SnowCooking.functions
{
    public class PanFunctions
    {
        public static void OnGestureChanged(UnturnedPlayer player, EPlayerGesture gesture)
        {
            foreach (var panPowder in SnowCookingPlugin.Instance.panPowderList.ToList())
            {
                if (Physics.Raycast(player.Player.look.aim.position, player.Player.look.aim.forward, out RaycastHit raycastHit, 2, RayMasks.BARRICADE))
                {
                    if (raycastHit.transform == panPowder.Key)
                    {
                        int amountBags = UnityEngine.Random.Range(SnowCookingPlugin.Instance.Configuration.Instance.snowBagsMin, SnowCookingPlugin.Instance.Configuration.Instance.snowBagsMax);
                        for (int i = 0; i < amountBags; i++)
                        {
                            ItemManager.dropItem(new Item(SnowCookingPlugin.Instance.Configuration.Instance.snowBagId, true), new Vector3(panPowder.Key.position.x, panPowder.Key.position.y + 2, panPowder.Key.position.z), false, true, false);
                        }
                        BarricadeManager.dropBarricade(new Barricade(SnowCookingPlugin.Instance.Configuration.Instance.panId), null, panPowder.Key.position, panPowder.Value.angle_x, panPowder.Value.angle_y, panPowder.Value.angle_z, panPowder.Value.owner, panPowder.Value.group);

                        BarricadeManager.tryGetInfo(panPowder.Key, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region);
                        BarricadeManager.destroyBarricade(region, x, y, plant, index);
                        SnowCookingPlugin.Instance.panPowderList.Remove(panPowder.Key);
                    }
                }
            }
        }
        //public static void OnPlayerUpdateGesture(UnturnedPlayer player, UnturnedPlayerEvents.PlayerGesture gesture)
        //{
        //    foreach (var panPowder in SnowCookingPlugin.Instance.panPowderList.ToList())
        //    {
        //        if (Physics.Raycast(player.Player.look.aim.position, player.Player.look.aim.forward, out RaycastHit raycastHit, 2, RayMasks.BARRICADE))
        //        {
        //            if (raycastHit.transform == panPowder.Key)
        //            {
        //                int amountBags = UnityEngine.Random.Range(SnowCookingPlugin.Instance.Configuration.Instance.snowBagsMin, SnowCookingPlugin.Instance.Configuration.Instance.snowBagsMax);
        //                for (int i = 0; i < amountBags; i++)
        //                {
        //                    ItemManager.dropItem(new Item(SnowCookingPlugin.Instance.Configuration.Instance.snowBagId, true), new Vector3(panPowder.Key.position.x, panPowder.Key.position.y + 2, panPowder.Key.position.z), false, true, false);
        //                }
        //                BarricadeManager.dropBarricade(new Barricade(SnowCookingPlugin.Instance.Configuration.Instance.panId), null, panPowder.Key.position, panPowder.Value.angle_x, panPowder.Value.angle_y, panPowder.Value.angle_z, panPowder.Value.owner, panPowder.Value.group);

        //                BarricadeManager.tryGetInfo(panPowder.Key, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region);
        //                BarricadeManager.destroyBarricade(region, x, y, plant, index);
        //                SnowCookingPlugin.Instance.panPowderList.Remove(panPowder.Key);
        //            }
        //        }
        //    }
        //}
        public static void Update()
        {
            foreach (var pan in SnowCookingPlugin.Instance.panList.ToList())
            {
                if (pan.Key == null)
                    break;
                if (Physics.Raycast(pan.Key.position, Vector3.down, out RaycastHit raycastHit, 14, RayMasks.BARRICADE))
                {
                    if (pan.Key == null)
                        break;
                    foreach (var heater in SnowCookingPlugin.Instance.heaterList.ToList())
                    {
                        if (heater.Key == null)
                            return;
                        if (raycastHit.transform == heater.Key)
                        {
                            double temp = (SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / 100.0) * heater.Value.progress;
                            if (temp >= SnowCookingPlugin.Instance.Configuration.Instance.heaterHotDegree)
                            {
                                if (pan.Key == null)
                                    break;
                                if (pan.Value.progress <= 100)
                                {
                                    foreach (var cocaLeaves in SnowCookingPlugin.Instance.cocaLeavesList.ToList())
                                    {
                                        if (cocaLeaves == null)
                                            break;
                                        if (Physics.Raycast(cocaLeaves.position, Vector3.down, out RaycastHit raycastHitLeaves, 14, RayMasks.BARRICADE))
                                        {
                                            if (cocaLeaves == null || pan.Key == null)
                                                break;
                                            if (raycastHitLeaves.transform == pan.Key)
                                            {
                                                double progressAdded = 100.0 / SnowCookingPlugin.Instance.Configuration.Instance.cookingDurationSecs;
                                                pan.Value.progress += progressAdded;
                                                if (temp >= SnowCookingPlugin.Instance.Configuration.Instance.heaterTooHotDegree)
                                                {
                                                    if (cocaLeaves == null)
                                                        break;
                                                    Transform ashes = BarricadeManager.dropBarricade(new Barricade(SnowCookingPlugin.Instance.Configuration.Instance.ashesId), null, cocaLeaves.position, 0, 0, 0, pan.Value.owner, pan.Value.group);

                                                    BarricadeManager.tryGetInfo(cocaLeaves, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region);
                                                    BarricadeManager.destroyBarricade(region, x, y, plant, index);
                                                    SnowCookingPlugin.Instance.cocaLeavesList.Remove(cocaLeaves);
                                                    if (ashes == null)
                                                        break;
                                                    EffectManager.sendEffect(SnowCookingPlugin.Instance.Configuration.Instance.cocaLeavesBurnedEffectId, 2, ashes.position);
                                                    return;
                                                }
                                                if (pan.Value.progress >= 100)
                                                {
                                                    //coca leaves stuff
                                                    if (cocaLeaves == null)
                                                        break;
                                                    BarricadeManager.tryGetInfo(cocaLeaves, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region);
                                                    BarricadeManager.destroyBarricade(region, x, y, plant, index);
                                                    if (cocaLeaves == null)
                                                        break;
                                                    SnowCookingPlugin.Instance.cocaLeavesList.Remove(cocaLeaves);

                                                    //pan stuff
                                                    if (pan.Key == null)
                                                        break;
                                                    BarricadeManager.tryGetInfo(pan.Key, out byte xpan, out byte ypan, out ushort plantpan, out ushort indexpan, out BarricadeRegion regionpan);
                                                    BarricadeManager.dropBarricade(new Barricade(SnowCookingPlugin.Instance.Configuration.Instance.panFilledId), null, pan.Key.position, pan.Value.angle_x, pan.Value.angle_y, pan.Value.angle_z, pan.Value.owner, pan.Value.group);
                                                    BarricadeManager.destroyBarricade(regionpan, xpan, ypan, plantpan, indexpan);
                                                    if (pan.Key == null)
                                                        break;
                                                    SnowCookingPlugin.Instance.panList.Remove(pan.Key);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
