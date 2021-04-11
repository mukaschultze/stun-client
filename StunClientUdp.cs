using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace IceColdMirror.Stun {

    public class StunClientUdp {

        public async Task<StunMessage> SendRequest(StunMessage request, string stunServer) {
            return await this.SendRequest(request, new Uri(stunServer));
        }

        public async Task<StunMessage> SendRequest(StunMessage request, Uri stunServer) {
            if (stunServer.Scheme == "stuns")
                throw new NotImplementedException("STUN secure is not supported");

            if (stunServer.Scheme != "stun")
                throw new ArgumentException("URI must have stun scheme", nameof(stunServer));

            using(var udp = new UdpClient(stunServer.Host, stunServer.Port)) {
                var requestBytes = request.Serialize();
                var byteCount = await udp.SendAsync(requestBytes, requestBytes.Length);
                var result = await udp.ReceiveAsync();

                using(var stream = new MemoryStream(result.Buffer)) {
                    return new StunMessage(stream);
                }
            }
        }
    }
}
