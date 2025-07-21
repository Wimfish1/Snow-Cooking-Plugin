using JetBrains.Annotations;
using Ocelot.SnowCooking.objects;
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
    public class BarricadeFunctions
    {
        public static void AddExistingBarricades(int level)
        {
            Logger.Log("Adding map objects to list...", ConsoleColor.Green);
            foreach (var region in BarricadeManager.regions)
            {
                foreach (var drop in region.drops)
                {
                    BarricadeData barricade = region.findBarricadeByInstanceID(drop.instanceID);
                    Transform barricadeTransform = SnowCookingPlugin.Instance.GetPlacedObjectTransform(barricade.point);
                    if (SnowCookingPlugin.Instance.Configuration.Instance.heaterIds.Contains(drop.asset.id))
                    {
                        if (SnowCookingPlugin.Instance.heaterList.ContainsKey(barricadeTransform))
                        {
                            Logger.Log("Duplicated entry detected, skipping object. (No need to worry)", ConsoleColor.Yellow);
                        } else
                        {
                            SnowCookingPlugin.Instance.heaterList.Add(barricadeTransform, new HeaterObject(0, false));
                        }
                    }
                    if (drop.asset.id == SnowCookingPlugin.Instance.Configuration.Instance.dryingLampId)
                    {
                        if (SnowCookingPlugin.Instance.dryingLampList.Contains(barricadeTransform))
                        {
                            Logger.Log("Duplicated entry detected, skipping object. (No need to worry)", ConsoleColor.Yellow);
                        }
                        else
                        { 
                            SnowCookingPlugin.Instance.dryingLampList.Add(barricadeTransform);
                        }
                    }
                    if (drop.asset.id == SnowCookingPlugin.Instance.Configuration.Instance.cocaLeavesId)
                    {
                        if (SnowCookingPlugin.Instance.cocaLeavesList.Contains(barricadeTransform))
                        {
                            Logger.Log("Duplicated entry detected, skipping object. (No need to worry)", ConsoleColor.Yellow);
                        }
                        else
                        {
                            SnowCookingPlugin.Instance.cocaLeavesList.Add(barricadeTransform);
                        }
                    }
                }
            }
            Logger.Log("All objects added.", ConsoleColor.Green);
        }

        public static void BarricadeDamaged(Transform barricadeTransform, ushort pendingTotalDamage)
        {
            if (barricadeTransform)
            {
                BarricadeData bData = SnowCookingPlugin.Instance.getBarricadeDataAtPosition(barricadeTransform.position);
                if (bData != null)
                {
                    if (bData.barricade.health <= pendingTotalDamage)
                    {
                        if (SnowCookingPlugin.Instance.Configuration.Instance.heaterIds.Contains(bData.barricade.id))
                        {
                            if (SnowCookingPlugin.Instance.heaterList.ContainsKey(barricadeTransform))
                            {
                                SnowCookingPlugin.Instance.heaterList.Remove(barricadeTransform);
                            }
                        } 
                        else if (SnowCookingPlugin.Instance.Configuration.Instance.dryingLampId == bData.barricade.id)
                        {
                            if (SnowCookingPlugin.Instance.dryingLampList.Contains(barricadeTransform))
                            {
                                SnowCookingPlugin.Instance.dryingLampList.Remove(barricadeTransform);
                            }
                        }
                        else if (SnowCookingPlugin.Instance.Configuration.Instance.panId == bData.barricade.id)
                        {
                            if (SnowCookingPlugin.Instance.panList.ContainsKey(barricadeTransform))
                            {
                                SnowCookingPlugin.Instance.panList.Remove(barricadeTransform);
                            }
                        }
                        else if (SnowCookingPlugin.Instance.Configuration.Instance.panFilledId == bData.barricade.id)
                        {
                            if (SnowCookingPlugin.Instance.panFilledList.ContainsKey(barricadeTransform))
                            {
                                SnowCookingPlugin.Instance.panFilledList.Remove(barricadeTransform);
                            }
                        }
                        else if (SnowCookingPlugin.Instance.Configuration.Instance.panPowderId == bData.barricade.id)
                        {
                            if (SnowCookingPlugin.Instance.panPowderList.ContainsKey(barricadeTransform))
                            {
                                SnowCookingPlugin.Instance.panPowderList.Remove(barricadeTransform);
                            }
                        }
                        else if (SnowCookingPlugin.Instance.Configuration.Instance.cocaLeavesId == bData.barricade.id)
                        {
                            if (SnowCookingPlugin.Instance.cocaLeavesList.Contains(barricadeTransform))
                            {
                                SnowCookingPlugin.Instance.cocaLeavesList.Remove(barricadeTransform);
                            }
                        }
                    }
                }
            }
        }
        public static void BarricadeSalvaged(CSteamID steamID, byte x, byte y, ushort plant, ushort index)
        {

            if (!BarricadeManager.tryGetRegion(x, y, plant, out BarricadeRegion region))
                return;

            if (index < 0 || BarricadeManager.BarricadeRegions[x, y].drops.Count() <= index) return;
            BarricadeDrop bDrop = BarricadeManager.BarricadeRegions[x, y].drops[index];
            if (bDrop == null)
                return;

            if (SnowCookingPlugin.Instance.Configuration.Instance.heaterIds.Contains(bDrop.asset.id))
                {
                    foreach (var item in SnowCookingPlugin.Instance.heaterList.ToList())
                    {
                        if (item.Key == null)
                            break;

                        if (item.Key.position == bDrop.model.position)
                        {
                            BarricadeManager.tryGetInfo(bDrop.model, out byte xBarricade, out byte yBarricade, out ushort plantBarricade, out ushort indexBarricade, out BarricadeRegion regionBarricade);
                            if (x == xBarricade && y == yBarricade && plant == plantBarricade && index == indexBarricade)
                            {
                                if (bDrop.model != null)
                                    SnowCookingPlugin.Instance.heaterList.Remove(bDrop.model);
                            }
                        }
                    }
                } else
                if (bDrop.asset.id == SnowCookingPlugin.Instance.Configuration.Instance.panId)
                {
                    foreach (var item in SnowCookingPlugin.Instance.panList.ToList())
                    {
                        if (item.Key == null)
                            break;

                        if (item.Key.position == bDrop.model.position)
                        {
                            BarricadeManager.tryGetInfo(bDrop.model, out byte xBarricade, out byte yBarricade, out ushort plantBarricade, out ushort indexBarricade, out BarricadeRegion regionBarricade);
                            if (x == xBarricade && y == yBarricade && plant == plantBarricade && index == indexBarricade)
                            {
                                if (bDrop.model != null)
                                    SnowCookingPlugin.Instance.panList.Remove(bDrop.model);
                            }
                        }
                    }
                } else
                if (bDrop.asset.id == SnowCookingPlugin.Instance.Configuration.Instance.panFilledId)
                {
                    foreach (var item in SnowCookingPlugin.Instance.panFilledList.ToList())
                    {
                        if (item.Key == null)
                            break;

                        if (item.Key.position == bDrop.model.position)
                        {
                            BarricadeManager.tryGetInfo(bDrop.model, out byte xBarricade, out byte yBarricade, out ushort plantBarricade, out ushort indexBarricade, out BarricadeRegion regionBarricade);
                            if (x == xBarricade && y == yBarricade && plant == plantBarricade && index == indexBarricade)
                            {
                                if (bDrop.model != null)
                                    SnowCookingPlugin.Instance.panFilledList.Remove(bDrop.model);
                            }
                        }
                    }
                } else
                if (bDrop.asset.id == SnowCookingPlugin.Instance.Configuration.Instance.panPowderId)
                {
                    foreach (var item in SnowCookingPlugin.Instance.panPowderList.ToList())
                    {
                        if (item.Key == null)
                            break;

                        if (item.Key.position == bDrop.model.position)
                        {
                            BarricadeManager.tryGetInfo(bDrop.model, out byte xBarricade, out byte yBarricade, out ushort plantBarricade, out ushort indexBarricade, out BarricadeRegion regionBarricade);
                            if (x == xBarricade && y == yBarricade && plant == plantBarricade && index == indexBarricade)
                            {
                                if (bDrop.model != null)
                                    SnowCookingPlugin.Instance.panPowderList.Remove(bDrop.model);
                            }
                        }
                    }
                } else
                if (bDrop.asset.id == SnowCookingPlugin.Instance.Configuration.Instance.cocaLeavesId)
                {
                    foreach (var item in SnowCookingPlugin.Instance.cocaLeavesList.ToList())
                    {
                        if (item == null)
                            break;

                        if (item.position == bDrop.model.position)
                        {
                            BarricadeManager.tryGetInfo(bDrop.model, out byte xBarricade, out byte yBarricade, out ushort plantBarricade, out ushort indexBarricade, out BarricadeRegion regionBarricade);
                            if (x == xBarricade && y == yBarricade && plant == plantBarricade && index == indexBarricade)
                            {
                                if (bDrop.model != null)
                                    SnowCookingPlugin.Instance.cocaLeavesList.Remove(bDrop.model);
                            }
                        }
                    }
                } else
                if (bDrop.asset.id == SnowCookingPlugin.Instance.Configuration.Instance.dryingLampId)
                {
                    foreach (var item in SnowCookingPlugin.Instance.dryingLampList.ToList())
                    {
                        if (item == null)
                            break;

                        if (item.position == bDrop.model.position)
                        {
                            BarricadeManager.tryGetInfo(bDrop.model, out byte xBarricade, out byte yBarricade, out ushort plantBarricade, out ushort indexBarricade, out BarricadeRegion regionBarricade);
                            if (x == xBarricade && y == yBarricade && plant == plantBarricade && index == indexBarricade)
                            {
                                if (bDrop.model != null)
                                    SnowCookingPlugin.Instance.dryingLampList.Remove(bDrop.model);
                            }
                        }
                    }
                }
        }
        public static void BarricadeDeployed(Barricade barricade, ItemBarricadeAsset asset, Transform hit, Vector3 pos, float angle_x, float angle_y, float angle_z, ulong owner, ulong group)
        {
            if (barricade == null)
                return;
            if (SnowCookingPlugin.Instance.Configuration.Instance.heaterIds.Contains(barricade.id))
            {
                SnowCookingPlugin.Instance.Wait(0.4f, () =>
                {
                    Transform barricadeTransform = SnowCookingPlugin.Instance.GetPlacedObjectTransform(pos);
                    if (barricadeTransform == null) return;
                    try
                    {
                        if (!SnowCookingPlugin.Instance.heaterList.ContainsKey(barricadeTransform))
                        {
                            SnowCookingPlugin.Instance.heaterList.Add(barricadeTransform, new HeaterObject(0, false));
                        }
                    } catch (Exception ex)
                    {
                        return;
                    }
                });
            }

            if (barricade.id == SnowCookingPlugin.Instance.Configuration.Instance.panId)
            {
                SnowCookingPlugin.Instance.Wait(0.4f, () =>
                {
                    Transform barricadeTransform = SnowCookingPlugin.Instance.GetPlacedObjectTransform(pos);
                    if (barricadeTransform == null) return;
                    try
                    {
                        if (!SnowCookingPlugin.Instance.panList.ContainsKey(barricadeTransform))
                        {
                            SnowCookingPlugin.Instance.panList.Add(barricadeTransform, new PanObject(0, angle_x, angle_y, angle_z, owner, group));
                        }
                    } catch (Exception ex)
                    {
                        return;
                    }
                });
            }

            if (barricade.id == SnowCookingPlugin.Instance.Configuration.Instance.cocaLeavesId)
            {
                SnowCookingPlugin.Instance.Wait(0.4f, () =>
                {
                    Transform barricadeTransform = SnowCookingPlugin.Instance.GetPlacedObjectTransform(pos);
                    if (barricadeTransform == null) return;
                    try
                    {
                        if (!SnowCookingPlugin.Instance.cocaLeavesList.Contains(barricadeTransform))
                        {
                            SnowCookingPlugin.Instance.cocaLeavesList.Add(barricadeTransform);
                        }
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                    
                    
                });
            }

            if (barricade.id == SnowCookingPlugin.Instance.Configuration.Instance.panFilledId)
            {
                SnowCookingPlugin.Instance.Wait(0.4f, () =>
                {
                    Transform barricadeTransform = SnowCookingPlugin.Instance.GetPlacedObjectTransform(pos);
                    if (barricadeTransform == null) return;
                    try
                    {
                        if (!SnowCookingPlugin.Instance.panFilledList.ContainsKey(barricadeTransform))
                        {
                            SnowCookingPlugin.Instance.panFilledList.Add(barricadeTransform, new PanFilledObject(0, angle_x, angle_y, angle_z, owner, group));
                        }
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                    
                });
            }

            if (barricade.id == SnowCookingPlugin.Instance.Configuration.Instance.dryingLampId)
            {
                SnowCookingPlugin.Instance.Wait(0.4f, () =>
                {
                    Transform barricadeTransform = SnowCookingPlugin.Instance.GetPlacedObjectTransform(pos);
                    if (barricadeTransform == null) return;
                    try
                    {
                        if (!SnowCookingPlugin.Instance.dryingLampList.Contains(barricadeTransform))
                        {
                            SnowCookingPlugin.Instance.dryingLampList.Add(barricadeTransform);
                        }
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                    
                });
            }

            if (barricade.id == SnowCookingPlugin.Instance.Configuration.Instance.panPowderId)
            {
                SnowCookingPlugin.Instance.Wait(0.4f, () =>
                {
                    Transform barricadeTransform = SnowCookingPlugin.Instance.GetPlacedObjectTransform(pos);
                    if (barricadeTransform == null) return;
                    try
                    {
                        if (!SnowCookingPlugin.Instance.panPowderList.ContainsKey(barricadeTransform))
                        {
                            SnowCookingPlugin.Instance.panPowderList.Add(barricadeTransform, new PanPowderObject(angle_x, angle_y, angle_z, owner, group));
                        }
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                    
                });
            }
        }
    }
}