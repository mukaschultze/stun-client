using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace IceColdMirror.Stun {
    public class StunMessage {

        public StunMessageHeader header = new StunMessageHeader();
        public List<StunAttribute> attributes = new List<StunAttribute>();

        public StunMessage() {
            this.header = new StunMessageHeader();
        }

        public StunMessage(Stream stream) {
            this.header = new StunMessageHeader(stream);

            var pos = 0;
            while (pos < header.Length) {
                var attr = new StunAttribute(stream, this);
                pos += attr.AttrbiuteLength;
                this.attributes.Add(attr);
            }
        }

        public byte[] Serialize() {
            var serializedAttributes = this.attributes.Select(a => a.Serialize());

            this.header.Length = (ushort)serializedAttributes.Sum(a => a.Length);

            var message = new byte[this.header.Length + 20];
            var curIndex = 20;

            foreach (var attr in serializedAttributes) {
                Array.Copy(attr, 0, message, curIndex, attr.Length);
                curIndex += attr.Length;
            }

            var header = this.header.Serialize();
            Array.Copy(header, 0, message, 0, 20);
            return message;
        }

        public override string ToString() => $"{this.header}\n{string.Join("\n", this.attributes.Select(a=>a.ToString()))}";

    }
}
