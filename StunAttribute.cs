using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IceColdMirror.Stun {

    public enum StunAttributeType : ushort {
        /* Comprehension-required range (0x0000-0x7FFF): */
        MAPPED_ADDRESS = 0x0001,
        RESPONSE_ADDRESS = 0x0002, // Reserved [RFC5389]
        CHANGE_REQUEST = 0x0003, // Reserved [RFC5389]
        SOURCE_ADDRESS = 0x0004, // Reserved [RFC5389]
        CHANGED_ADDRESS = 0x0005, // Reserved [RFC5389]
        USERNAME = 0x0006,
        PASSWORD = 0x0007, // Reserved [RFC5389]
        MESSAGE_INTEGRITY = 0x0008,
        ERROR_CODE = 0x0009,
        UNKNOWN_ATTRIBUTES = 0x000A,
        REFLECTED_FROM = 0x000B, // Reserved [RFC5389]
        REALM = 0x0014,
        NONCE = 0x0015,
        XOR_MAPPED_ADDRESS = 0x0020,

        /* Comprehension-optional range (0x8000-0xFFFF) */
        SOFTWARE = 0x8022,
        ALTERNATE_SERVER = 0x8023,
        FINGERPRINT = 0x8028,

        // New attributes
        /* Comprehension-required range (0x0000-0x7FFF): */
        MESSAGE_INTEGRITY_SHA256 = 0x001C,
        PASSWORD_ALGORITHM = 0x001D,
        USERHASH = 0x001E,

        /* Comprehension-optional range (0x8000-0xFFFF) */
        PASSWORD_ALGORITHMS = 0x8002,
        ALTERNATE_DOMAIN = 0x8003,
    }

    /**
     *  0                   1                   2                   3
     *  0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
     * +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
     * |         Type                  |            Length             |
     * +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
     * |                         Value (variable)                ....
     * +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
     *
     * https://tools.ietf.org/html/rfc8489#section-14
     */
    public class StunAttribute {

        private static readonly Dictionary<StunAttributeType, Type> handlers = new Dictionary<StunAttributeType, Type>() {
            {
            StunAttributeType.SOFTWARE,
            typeof(StunAttributeSoftware)
            }
        };

        private byte[] data = new byte[4]; // start with space for type and length

        public StunMessage Owner { get; private set; }

        public StunAttributeType Type {
            get => (StunAttributeType)((data[0] << 8) | data[1]);
            protected set {
                this.data[0] = (byte)(((ushort)value >> 8) & 0xFF);
                this.data[1] = (byte)(((ushort)value >> 0) & 0xFF);
            }
        }

        public ushort Length {
            get => (ushort)((data[2] << 8) | data[3]);
            private set {
                this.data[2] = (byte)(((ushort)value >> 8) & 0xFF);
                this.data[3] = (byte)(((ushort)value >> 0) & 0xFF);
            }
        }

        public ushort AttrbiuteLength {
            get => (ushort)this.data.Length;
        }

        public byte[] Variable {
            get {
                var arr = new byte[this.Length];
                Array.Copy(data, 4, arr, 0, arr.Length);
                return arr;
            }
            set {
                this.Length = (ushort)value.Length;
                // data must be aligned to 32-bit boundaries
                var padding = (4 - (this.Length % 4)) % 4;
                // resize to acommodate type, length,
                // variable and the alignment padding
                Array.Resize(ref data, this.Length + padding + 4);
                Array.Copy(value, 0, this.data, 4, value.Length);
            }
        }

        public StunAttribute(Stream stream, StunMessage owner) {
            stream.Read(this.data, 0, 4); // type and length
            var length = this.Length;
            var padding = (4 - (this.Length % 4)) % 4;
            var variableLength = this.Length + padding;
            Array.Resize(ref data, variableLength + 4);
            stream.Read(this.data, 4, variableLength);

            this.Owner = owner;
        }

        public StunAttribute(StunAttributeType type, StunMessage owner) {
            this.Type = type;
            this.Owner = owner;
        }

        public byte[] Serialize() => this.data;

        public override string ToString() => $"{Type} {Length} bytes: {string.Join(",",Variable.Select(v=>v.ToString("X2")))}";

    }

}
