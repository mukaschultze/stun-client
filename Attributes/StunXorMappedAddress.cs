using System;
using System.Net;
using UnityEngine;

namespace IceColdMirror.Stun {

    /// <summary>
    /// The XOR-MAPPED-ADDRESS attribute is identical to the MAPPED-ADDRESS
    /// attribute, except that the reflexive transport address is obfuscated
    /// through the XOR function.
    /// 
    /// https://tools.ietf.org/html/rfc8489#section-14.2
    /// </summary>
    public static class StunAttributeXorMappedAddress {

        private static readonly byte[] magicCookieBytes = new byte[] {
            (byte)((StunMessageHeader.MAGIC_COOKIE >> 24) & 0xFF),
            (byte)((StunMessageHeader.MAGIC_COOKIE >> 16) & 0xFF),
            (byte)((StunMessageHeader.MAGIC_COOKIE >> 8) & 0xFF),
            (byte)((StunMessageHeader.MAGIC_COOKIE >> 0) & 0xFF),
        };

        public static IPEndPoint GetXorMappedAddress(this StunAttribute attribute) {
            var variable = attribute.Variable;

            if (variable[0] != 0)
                Debug.LogWarning($"XOR-MAPPED-ADDRESS first byte must be 0x00, got 0x{variable[0].ToString("X2")}");

            var family = (AddressFamily)variable[1];
            var xPort = (ushort)((variable[2] << 8) | variable[3]);
            // xor port with 16 most significant bit of magic cookie
            var port = (ushort)(xPort ^ ((magicCookieBytes[0] << 8) | magicCookieBytes[1]));

            var addressSize = variable.Length - sizeof(ushort) * 2;

            if (family == AddressFamily.IPv4 && addressSize != 4)
                Debug.LogWarning($"XOR-MAPPED-ADDRESS with family {family} needs to have 32 bits, got {addressSize * 8} bits");
            else if (family == AddressFamily.IPv6 && addressSize != 16)
                Debug.LogWarning($"XOR-MAPPED-ADDRESS with family {family} needs to have 128 bits, got {addressSize * 8} bits");

            var addressBytes = new byte[addressSize];
            Array.Copy(variable, 4, addressBytes, 0, addressBytes.Length);

            // xor each address byte with the magic cookie byte
            for (int i = 0; i < 4; i++)
                addressBytes[i] ^= magicCookieBytes[i];

            if (family == AddressFamily.IPv6) {
                var header = attribute.Owner.header;
                // getting the transaction id generates GC, only call when ipv6
                var transactionID = header.TransactionId;

                // xor each byte with the concatenation of magic cookie and transaction id
                for (int i = 0; i < transactionID.Length; i++)
                    addressBytes[i + 4] ^= transactionID[i];
            }

            var ipAddress = new IPAddress(addressBytes);
            return new IPEndPoint(ipAddress, port);
        }

        public static void SetXorMappedAddress(this StunAttribute attribute, IPEndPoint endPoint) {
            throw new NotImplementedException();
        }

    }
}
