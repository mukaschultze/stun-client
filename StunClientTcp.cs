using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace IceColdMirror.Stun {

    public class StunClientTcp {

        public async Task<StunMessage> SendRequest(StunMessage request, string stunServer) {
            return await this.SendRequest(request, new Uri(stunServer));
        }

        public async Task<StunMessage> SendRequest(StunMessage request, Uri stunServer) {
            if (stunServer.Scheme == "stuns")
                throw new NotImplementedException("STUN secure is not supported");

            if (stunServer.Scheme != "stun")
                throw new ArgumentException("URI must have stun scheme", nameof(stunServer));

            using(var tcp = new TcpClient(new IPEndPoint(IPAddress.Any, 0))) {
                var requestBytes = request.Serialize();
                await tcp.ConnectAsync(stunServer.Host, stunServer.Port);
                await tcp.GetStream().WriteAsync(requestBytes, 0, requestBytes.Length);
                var stream = tcp.GetStream();
                return new StunMessage(stream);
            }
        }

    }
}
