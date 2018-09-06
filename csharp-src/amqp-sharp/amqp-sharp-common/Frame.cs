using System;
using System.IO;

namespace amqp_sharp_common
{
    public abstract class Frame
    {
        private readonly FrameType _type;
        private readonly ushort _channel;

        public Frame(FrameType type, UInt16 channel)
        {
            _type = type;
            _channel = channel;
        }

        protected abstract byte[] ToSlice();

        protected virtual byte[] ToSlice(byte[] body)
        {
            var memoryStream = new MemoryStream(8 + body.Length);
//            memoryStream.WriteByte(_type);
            return body;
        }

        void Encode(MemoryStream io)
        {
            var buffer = this.ToSlice();
            io.Write(buffer, (int) io.Position, buffer.Length);
        }
    }
}