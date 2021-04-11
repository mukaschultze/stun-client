using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace IceColdMirror.Stun {
    public class StunTest : MonoBehaviour {
        private async void Start() {
            var hosts = new string[] {
                "stun://localhost:3478",
                "stun://127.0.0.1:3478",

                // Google
                "stun://stun.l.google.com:19302",
                "stun://stun1.l.google.com:19302",
                "stun://stun2.l.google.com:19302",
                "stun://stun3.l.google.com:19302",
                "stun://stun4.l.google.com:19302",

                // Other
                "stun://stun.voip.blackberry.com:3478",
                "stun://stun.voipgate.com:3478",
                "stun://stun.voys.nl:3478",
                "stun://stun1.faktortel.com.au:3478",

                // TCP
                "stun://stun.sipnet.net:3478",
                "stun://stun.sipnet.ru:3478",
                "stun://stun.stunprotocol.org:3478",
            };

            var req = new StunMessage();

            var software = new StunAttribute(StunAttributeType.SOFTWARE, req);
            software.SetSoftware("Ice Cold Mirror");

            req.attributes.Add(software);

            var clientUdp = new StunClientUdp();
            var clientTcp = new StunClientTcp();

            await Task.WhenAll(hosts.Select(async(host) => {
                try {
                    var res = await clientUdp.SendRequest(req, host).AwaitWithTimeout(1500);
                    Debug.Log("UDP: " + res);
                    var indication = res.attributes.First(a => a.Type == StunAttributeType.XOR_MAPPED_ADDRESS).GetXorMappedAddress();
                    Debug.LogWarning($"UDP: {host} STUN indication is {indication}");
                } catch (Exception e) {
                    Debug.LogException(new Exception($"UDP: STUN host \"{host}\" failed", e));
                }
            }).ToArray());

            await Task.WhenAll(hosts.Select(async(host) => {
                try {
                    var res = await clientTcp.SendRequest(req, host).AwaitWithTimeout(1500);
                    Debug.Log("TCP: " + res);
                    var indication = res.attributes.First(a => a.Type == StunAttributeType.XOR_MAPPED_ADDRESS).GetXorMappedAddress();
                    Debug.LogWarning($"TCP: {host} STUN indication is {indication}");
                } catch (Exception e) {
                    Debug.LogException(new Exception($"TCP: STUN host \"{host}\" failed", e));
                }
            }).ToArray());
        }
    }
}
