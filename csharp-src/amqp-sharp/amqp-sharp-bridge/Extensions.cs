using System;

namespace amqp_sharp_bridge
{
    public static class Extensions 
    {
        public static byte Add(this byte a, byte b)
        {
            return (byte)(a + b);
        }
    }
}