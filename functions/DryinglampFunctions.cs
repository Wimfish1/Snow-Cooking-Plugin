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
    public class DryinglampFunctions
    {
        public static void Update()
        {
            foreach (var panFilled in SnowCookingPlugin.Instance.panFilledList.ToList())
            {
                if (panFilled.Key == null)
                    break;
                if (Physics.Raycast(panFilled.Key.position, Vector3.up, out RaycastHit raycastHit, 14, RayMasks.BARRICADE))
                {
                    foreach (var lamp in SnowCookingPlugin.Instance.dryingLampList.ToList())
                    {
                        if (lamp == null)
                            break;
                        foreach (var Generator in PowerTool.checkGenerators(lamp.position, PowerTool.MAX_POWER_RANGE, ushort.MaxValue))
                        {
                            if (Generator == null || lamp == null)
                                break;
                            if (Generator.isPowered && Generator.wirerange >= (lamp.position - Generator.transform.position).magnitude)
                            {
                                if (lamp == null || raycastHit.transform == null)
                                    break;
                                if (lamp == raycastHit.transform)
                                {
                                    double progressAdded = 100.0 / SnowCookingPlugin.Instance.Configuration.Instance.dryingDurationSecs;
                                    panFilled.Value.progress += progressAdded;
                                    if (panFilled.Value.progress >= 100)
                                    {
                                        BarricadeManager.dropBarricade(new Barricade(SnowCookingPlugin.Instance.Configuration.Instance.panPowderId), null, panFilled.Key.position, panFilled.Value.angle_x, panFilled.Value.angle_y, panFilled.Value.angle_z, panFilled.Value.owner, panFilled.Value.group);
                                        if (panFilled.Key == null)
                                            break;
                                        BarricadeManager.tryGetInfo(panFilled.Key, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region);
                                        if (index >= 0 && index < region.barricades.Count)
                                        {
                                            BarricadeManager.destroyBarricade(region, x, y, plant, index);
                                        }
                                        if (panFilled.Key == null)
                                            break;
                                        SnowCookingPlugin.Instance.panFilledList.Remove(panFilled.Key);
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
