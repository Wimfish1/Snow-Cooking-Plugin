﻿namespace Ocelot.SnowCooking
{
    public class HeaterObject
    {
        public double progress = 0;
        public bool isActive = false;
        public long time = 0;
        public HeaterObject(double progress, bool isActive)
        {
            this.progress = progress;
            this.isActive = isActive;
            time = SnowCookingPlugin.GetCurrentTime();
        }
    }
}
