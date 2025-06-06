﻿using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Linq;
using UnityEngine;

namespace Ocelot.SnowCooking.functions
{
    public class PanFunctions
    {
        public static void OnGestureChanged(UnturnedPlayer player, EPlayerGesture gesture)
        {
            foreach (var panPowder in SnowCookingPlugin.Instance.panPowderList.ToList())
            {
                if (!Physics.Raycast(player.Player.look.aim.position, player.Player.look.aim.forward,
                        out var raycastHit, 2, RayMasks.BARRICADE)) continue;
                if (raycastHit.transform != panPowder.Key) continue;
                var amountBags = Random.Range(SnowCookingPlugin.Instance.Configuration.Instance.snowBagsMin, SnowCookingPlugin.Instance.Configuration.Instance.snowBagsMax);
                for (var i = 0; i < amountBags; i++)
                {
                    if (SnowCookingPlugin.Instance.Configuration.Instance.AddItemsDirectlyToInventory)
                    {
                        var item = new Item(SnowCookingPlugin.Instance.Configuration.Instance.snowBagId, EItemOrigin.ADMIN);
                        if (!player.Inventory.tryAddItemAuto(item, true, true, true, false))
                        {
                            ItemManager.dropItem(
                                new Item(SnowCookingPlugin.Instance.Configuration.Instance.snowBagId, true),
                                new Vector3(panPowder.Key.position.x, panPowder.Key.position.y + 2,
                                    panPowder.Key.position.z), false, true, false);
                        }
                    }
                    else
                    {
                        ItemManager.dropItem(new Item(SnowCookingPlugin.Instance.Configuration.Instance.snowBagId,
                                true),
                            new Vector3(panPowder.Key.position.x,
                                panPowder.Key.position.y + 2,
                                panPowder.Key.position.z),
                            false,
                            true,
                            false);
                    }
                }
                BarricadeManager.dropBarricade(new Barricade(SnowCookingPlugin.Instance.Configuration.Instance.panId),
                    null, panPowder.Key.position, panPowder.Value.angle_x, panPowder.Value.angle_y,
                    panPowder.Value.angle_z, panPowder.Value.owner, panPowder.Value.group);

                BarricadeManager.tryGetInfo(panPowder.Key, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region);
                BarricadeManager.destroyBarricade(region, x, y, plant, index);
                SnowCookingPlugin.Instance.panPowderList.Remove(panPowder.Key);
            }
        }
        public static void Update()
        {
            foreach (var pan in SnowCookingPlugin.Instance.panList.ToList())
            {
                if (!pan.Key)
                    break;
                if (!Physics.Raycast(pan.Key.position, Vector3.down, out RaycastHit raycastHit, 14,
                        RayMasks.BARRICADE)) continue;
                if (!pan.Key)
                    break;
                foreach (var heater in SnowCookingPlugin.Instance.heaterList.ToList())
                {
                    if (!heater.Key)
                        return;
                    if (raycastHit.transform != heater.Key) continue;
                    var temp = (SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / 100.0) * heater.Value.progress;
                    if (!(temp >= SnowCookingPlugin.Instance.Configuration.Instance.heaterHotDegree)) continue;
                    if (!pan.Key)
                        break;
                    if (!(pan.Value.progress <= 100)) continue;
                    foreach (var cocaLeaves in SnowCookingPlugin.Instance.cocaLeavesList.ToList())
                    {
                        if (!cocaLeaves)
                            break;
                        if (!Physics.Raycast(cocaLeaves.position, Vector3.down, out RaycastHit raycastHitLeaves, 14,
                                RayMasks.BARRICADE)) continue;
                        if (!cocaLeaves || !pan.Key)
                            break;
                        if (raycastHitLeaves.transform != pan.Key) continue;
                        var progressAdded = 100.0 / SnowCookingPlugin.Instance.Configuration.Instance.cookingDurationSecs;
                        pan.Value.progress += progressAdded;
                        if (temp >= SnowCookingPlugin.Instance.Configuration.Instance.heaterTooHotDegree)
                        {
                            if (!cocaLeaves)
                                break;
                            var ashes = BarricadeManager.dropBarricade(
                                new Barricade(SnowCookingPlugin.Instance.Configuration.Instance.ashesId), null,
                                cocaLeaves.position, 0, 0, 0, pan.Value.owner, pan.Value.group);

                            BarricadeManager.tryGetInfo(cocaLeaves, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region);
                            BarricadeManager.destroyBarricade(region, x, y, plant, index);
                            SnowCookingPlugin.Instance.cocaLeavesList.Remove(cocaLeaves);
                            if (!ashes)
                                break;
                            EffectManager.sendEffect(SnowCookingPlugin.Instance.Configuration.Instance.cocaLeavesBurnedEffectId, 2, ashes.position);
                            return;
                        }

                        if (!(pan.Value.progress >= 100)) continue;
                        {
                            //coca leaves stuff
                            if (!cocaLeaves)
                                break;
                            BarricadeManager.tryGetInfo(cocaLeaves, out var x, out var y, out var plant, out var index, out var region);
                            BarricadeManager.destroyBarricade(region, x, y, plant, index);
                            if (!cocaLeaves)
                                break;
                            SnowCookingPlugin.Instance.cocaLeavesList.Remove(cocaLeaves);

                            //pan stuff
                            if (!pan.Key)
                                break;
                            BarricadeManager.tryGetInfo(pan.Key, out var xpan, out var ypan, out var plantpan,
                                out var indexpan, out var regionpan);
                            BarricadeManager.dropBarricade(
                                new Barricade(SnowCookingPlugin.Instance.Configuration.Instance.panFilledId), null,
                                pan.Key.position, pan.Value.angle_x, pan.Value.angle_y, pan.Value.angle_z,
                                pan.Value.owner, pan.Value.group);
                            BarricadeManager.destroyBarricade(regionpan, xpan, ypan, plantpan, indexpan);
                            if (!pan.Key)
                                break;
                            SnowCookingPlugin.Instance.panList.Remove(pan.Key);
                        }
                    }
                }
            }
        }
    }
}
