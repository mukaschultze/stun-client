using System;
using System.IO;
using System.Security.Cryptography;

namespace IceColdMirror.Stun {

    public enum StunMessageClass : ushort {
        Request = 0b00,
        Indication = 0b01,
        Success = 0b10,
        Error = 0b11,
    }

    public enum StunMessageMethod : ushort {
        Binding = 0x001,
    }

    public class StunMessageHeader {

        public const uint MAGIC_COOKIE = 0x2112A442;

        private readonly byte[] data = new byte[20];

        public ushort Type {
            get => (ushort)((data[0] << 8) | data[1]);
            set {
                this.data[0] = (byte)((value >> 8) & 0xFF);
                this.data[1] = (byte)((value >> 0) & 0xFF);
            }
        }

        public ushort Length {
            get => (ushort)((data[2] << 8) | data[3]);
            set {
                this.data[2] = (byte)((value >> 8) & 0xFF);
                this.data[3] = (byte)((value >> 0) & 0xFF);
            }
        }

        public StunMessageClass Class {
            get => (StunMessageClass)(
                (this.Type & 0x0100) >> 7 |
                (this.Type & 0x0010) >> 4
            );
            set => this.Type = (ushort)((ushort)value | (ushort)this.Method);
        }

        public StunMessageMethod Method {
            get => (StunMessageMethod)(
                (this.Type & 0x3E00) >> 2 |
                (this.Type & 0x00E0) >> 1 |
                (this.Type & 0x000F)
            );
            set => this.Type = (ushort)((ushort)value | (ushort)this.Class);
        }

        public string TransactionIdBase64 {
            get => Convert.ToBase64String(this.data, 8, 96 / 8);
        }

        public byte[] TransactionId {
            get {
                var arr = new byte[96 / 8];
                Array.Copy(this.data, 8, arr, 0, arr.Length);
                return arr;
            }
        }

        public StunMessageHeader() {
            this.data[4] = (byte)((MAGIC_COOKIE >> 24) & 0xFF);
            this.data[5] = (byte)((MAGIC_COOKIE >> 16) & 0xFF);
            this.data[6] = (byte)((MAGIC_COOKIE >> 8) & 0xFF);
            this.data[7] = (byte)((MAGIC_COOKIE >> 0) & 0xFF);

            this.Class = StunMessageClass.Request;
            this.Method = StunMessageMethod.Binding;

            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(this.data, 8, 12); // random transaction id
        }

        public StunMessageHeader(Stream stream) {
            stream.Read(this.data, 0, 20);

            if ((Type & 0xC000) != 0)
                throw new ArgumentException("Header must start with two padding zeroes", nameof(stream));

            var magicCookie = (this.data[4] << 24) | (this.data[5] << 16) | (this.data[6] << 8) | this.data[7];

            if (magicCookie != MAGIC_COOKIE)
                throw new ArgumentException($"Magic cookie doesn't match, expected 0x{MAGIC_COOKIE.ToString("X4")} got 0x{magicCookie.ToString("X4")}", nameof(stream));
        }

        public byte[] Serialize() => this.data;

        public override string ToString() {
            return $"StunHeader of type {Type.ToString("X4")}, class {Class}, method {Method}, and {Length} bytes of message";
        }

    }

}
