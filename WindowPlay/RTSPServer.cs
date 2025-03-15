using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace WindowPlay
{
    public class RTSPServer
    {
            private TcpListener listener;
            private int port;
            private CancellationTokenSource cts = new CancellationTokenSource();

            // Callback invoked when PLAY is received
            public Action OnPlayRequested;

            // Callback invoked when TEARDOWN is received (to stop streaming)
            public Action OnTeardownRequested;

            public RTSPServer(int port)
            {
                this.port = port;
                listener = new TcpListener(IPAddress.Any, port);
            }

            public async Task StartAsync()
            {
                listener.Start();
                Console.WriteLine($"RTSP server started on port {port}");
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var client = await listener.AcceptTcpClientAsync();
                        _ = Task.Run(() => HandleClientAsync(client), cts.Token);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error accepting client: " + ex.Message);
                    }
                }
            }

            public void Stop()
            {
                cts.Cancel();
                listener.Stop();
            }

            private async Task HandleClientAsync(TcpClient client)
            {
                using (client)
                {
                    var stream = client.GetStream();
                    byte[] buffer = new byte[4096];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        return;
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Received RTSP request:\n" + request);

                    string cseq = ExtractHeader(request, "CSeq");
                    string method = ExtractMethod(request);
                    string response = "";

                    if (method == "OPTIONS")
                    {
                        response = $"RTSP/1.0 200 OK\r\nCSeq: {cseq}\r\nPublic: OPTIONS, DESCRIBE, SETUP, PLAY, TEARDARWN\r\n\r\n";
                    }
                    else if (method == "DESCRIBE")
                    {
                        // Generate an SDP response.
                        string sdp = "v=0\r\n" +
                                     "o=- 0 0 IN IP4 " + GetLocalIPAddress() + "\r\n" +
                                     "s=AirPlay Stream\r\n" +
                                     "c=IN IP4 0.0.0.0\r\n" +
                                     "t=0 0\r\n" +
                                     "m=audio " + port + " RTP/AVP 96\r\n" +
                                     "a=rtpmap:96 L16/44100/2\r\n";
                        response = $"RTSP/1.0 200 OK\r\nCSeq: {cseq}\r\nContent-Base: rtsp://{GetLocalIPAddress()}:{port}/\r\nContent-Type: application/sdp\r\nContent-Length: {sdp.Length}\r\n\r\n{sdp}";
                    }
                    else if (method == "SETUP")
                    {
                        // In a complete implementation, parse Transport header for client ports.
                        // Here we simply return a fixed session.
                        response = $"RTSP/1.0 200 OK\r\nCSeq: {cseq}\r\nTransport: RTP/AVP;unicast;client_port=7000-7001;server_port={port}-{port + 1}\r\nSession: 12345678\r\n\r\n";
                    }
                    else if (method == "PLAY")
                    {
                        response = $"RTSP/1.0 200 OK\r\nCSeq: {cseq}\r\nSession: 12345678\r\n\r\n";
                        OnPlayRequested?.Invoke();
                    }
                    else if (method == "TEARDOWN")
                    {
                        response = $"RTSP/1.0 200 OK\r\nCSeq: {cseq}\r\nSession: 12345678\r\n\r\n";
                        OnTeardownRequested?.Invoke();
                    }
                    else
                    {
                        response = $"RTSP/1.0 400 Bad Request\r\nCSeq: {cseq}\r\n\r\n";
                    }

                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                }
            }

            private string ExtractMethod(string request)
            {
                var lines = request.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    var parts = lines[0].Split(' ');
                    if (parts.Length > 0)
                        return parts[0];
                }
                return "";
            }

            private string ExtractHeader(string request, string header)
            {
                var lines = request.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.StartsWith(header, StringComparison.OrdinalIgnoreCase))
                        return line.Substring(header.Length + 1).Trim();
                }
                return "";
            }

            private string GetLocalIPAddress()
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip.ToString();
                }
                return "127.0.0.1";
            }
        }
    }

