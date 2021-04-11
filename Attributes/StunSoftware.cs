using System;
using System.Globalization;
using System.Text;

namespace IceColdMirror.Stun {
    /// <summary>
    /// The SOFTWARE attribute contains a textual description of the software
    /// being used by the agent sending the message.  It is used by clients
    /// and servers.  Its value SHOULD include manufacturer and version
    /// number.  The attribute has no impact on operation of the protocol and
    /// serves only as a tool for diagnostic and debugging purposes.  The
    /// value of SOFTWARE is variable length.  It MUST be a UTF-8-encoded
    /// [RFC3629] sequence of fewer than 128 characters (which can be as long
    /// as 509 when encoding them and as long as 763 bytes when decoding
    /// them).
    /// 
    /// https://tools.ietf.org/html/rfc8489#section-14.14
    /// </summary>
    public static class StunAttributeSoftware {

        public static string GetSoftware(this StunAttribute attribute) {
            return Encoding.UTF8.GetString(attribute.Variable);
        }

        public static void SetSoftware(this StunAttribute attribute, string info) {
            if (info.Length >= 128)
                throw new ArgumentException("String must be less than 128 characteres", nameof(info));

            var bytes = Encoding.UTF8.GetBytes(info);

            if (bytes.Length >= 763)
                throw new ArgumentException("String must be less than 763 bytes", nameof(info));

            attribute.Variable = bytes;
        }

    }
}
