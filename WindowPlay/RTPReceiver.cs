using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowPlay
{
    class RTPReceiver
    {
        private UdpClient udpClient;
        private int port;
        private CancellationTokenSource cts = new CancellationTokenSource();

        // Callback to deliver decoded PCM data
        public Action<byte[]> OnAudioDataReceived;

        public RTPReceiver(int port)
        {
            this.port = port;
            udpClient = new UdpClient(port);
        }

        public async Task StartAsync()
        {
            Console.WriteLine($"RTP Receiver started on port {port}");
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, port);
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync();
                    byte[] packet = result.Buffer;
                    // Process RTP packet (this is a placeholder)
                    byte[] pcmData = ProcessRTPPacket(packet);
                    OnAudioDataReceived?.Invoke(pcmData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("RTP Receiver error: " + ex.Message);
                }
            }
        }

        public void Stop()
        {
            cts.Cancel();
            udpClient.Close();
        }

        // Dummy processing: strip a 12-byte RTP header.
        // In a real implementation, perform decryption, reordering, jitter buffering, and decoding.
        private byte[] ProcessRTPPacket(byte[] packet)
        {
            if (packet.Length <= 12)
                return new byte[0];
            byte[] payload = new byte[packet.Length - 12];
            Array.Copy(packet, 12, payload, 0, payload.Length);
            // For example, if payload is raw PCM data:
            return payload;
        }
    }
}
