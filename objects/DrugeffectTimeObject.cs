using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocelot.SnowCooking.objects
{
    public class DrugeffectTimeObject
    {
        public long time = SnowCookingPlugin.getCurrentTime();
        public string playerId;
        public DrugeffectTimeObject(string playerId)
        {
            this.playerId = playerId;
        }
    }
}
