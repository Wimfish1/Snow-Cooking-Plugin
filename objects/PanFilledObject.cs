﻿namespace Ocelot.SnowCooking.objects
{
    public class PanFilledObject
    {
        public double progress = 0;
        public float angle_x = 0;
        public float angle_y = 0;
        public float angle_z = 0;
        public ulong owner;
        public ulong group;
        public PanFilledObject(double progress, float angle_x, float angle_y, float angle_z, ulong owner, ulong group)
        {
            this.progress = progress;
            this.angle_x = angle_x;
            this.angle_y = angle_y;
            this.angle_z = angle_z;
            this.owner = owner;
            this.group = group;
        }
    }
}
