namespace amqp_sharp_common
{
    public enum FrameType : byte
    {
        Method = 1,
        Header = 2,
        Body = 3,
        Heartbeat = 4
    }
    
    
}