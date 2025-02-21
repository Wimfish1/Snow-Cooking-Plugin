using SDG.Unturned;
using System.Linq;
using UnityEngine;

namespace Ocelot.SnowCooking.functions
{
    public class DryinglampFunctions
    {
        public static void Update()
        {
            foreach (var panFilled in SnowCookingPlugin.Instance.panFilledList.ToList())
            {
                if (!panFilled.Key)
                    break;
                if (!Physics.Raycast(panFilled.Key.position, Vector3.up, out RaycastHit raycastHit, 14,
                        RayMasks.BARRICADE)) continue;
                foreach (var lamp in SnowCookingPlugin.Instance.dryingLampList.ToList())
                {
                    if (!lamp)
                        break;
                    foreach (var generator in PowerTool.checkGenerators(lamp.position, PowerTool.MAX_POWER_RANGE, ushort.MaxValue))
                    {
                        if (!generator || !lamp)
                            break;
                        if (!generator.isPowered || !(generator.wirerange >=
                                                      (lamp.position - generator.transform.position).magnitude))
                            continue;
                        if (!lamp || !raycastHit.transform)
                            break;
                        if (lamp != raycastHit.transform) continue;
                        double progressAdded = 100.0 / SnowCookingPlugin.Instance.Configuration.Instance.dryingDurationSecs;
                        panFilled.Value.progress += progressAdded;
                        if (!(panFilled.Value.progress >= 100)) continue;
                        BarricadeManager.dropBarricade(
                            new Barricade(SnowCookingPlugin.Instance.Configuration.Instance.panPowderId), null,
                            panFilled.Key.position, panFilled.Value.angle_x, panFilled.Value.angle_y,
                            panFilled.Value.angle_z, panFilled.Value.owner, panFilled.Value.group);
                        if (!panFilled.Key)
                            break;
                        BarricadeManager.tryGetInfo(panFilled.Key, out var x, out var y, out var plant, out var index, out var region);
                        if (index < region.barricades.Count)
                        {
                            BarricadeManager.destroyBarricade(region, x, y, plant, index);
                        }
                        if (!panFilled.Key)
                            break;
                        SnowCookingPlugin.Instance.panFilledList.Remove(panFilled.Key);
                    }
                }
            }
        }
    }
}
