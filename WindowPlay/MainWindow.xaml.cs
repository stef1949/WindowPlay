using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Makaretu.Dns;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WindowPlay
{
    public sealed partial class MainWindow : Window
    {
        ServiceDiscovery serviceDiscovery;
        ServiceProfile airplayService;
        RTSPServer rtspServer;
        AudioPlayer audioPlayer;
        RTPReceiver rtpReceiver;

        public MainWindow()
        {
            this.InitializeComponent();
            // Initialize the audio player for 44100 Hz, stereo PCM output.
            audioPlayer = new AudioPlayer(44100, 2);
        }

        private async void myButton_Click(object sender, RoutedEventArgs e)
        {
            Log("Starting AirPlay service using Makaretu.MDNS...");

            // Create a service profile with the desired service name, type, and port.
            airplayService = new ServiceProfile("WindowPlay Receiver", "_airplay._tcp", 7000);

            // Add required TXT record properties for AirPlay.
            airplayService.AddProperty("version", "1.0");
            airplayService.AddProperty("deviceid", "12:34:56:78:9A:BC");  // Your device's MAC address
            airplayService.AddProperty("model", "AppleTV3,2");             // Mimic an AirPlay receiver model
            airplayService.AddProperty("features", "0x5A7FFFF7,0x1E");     // Feature bitmask (example value)
            airplayService.AddProperty("srcvers", "220.68");               // Source version

            // Initialize the service discovery instance and advertise the service.
            serviceDiscovery = new ServiceDiscovery();
            // Run Advertise on a background thread to avoid blocking the UI.
            await Task.Run(() => serviceDiscovery.Advertise(airplayService));

            Log("Service advertised successfully.");

            // Start the RTSP server on port 7000.
            rtspServer = new RTSPServer(7000);
            // When PLAY is received, start the RTP receiver.
            rtspServer.OnPlayRequested = () =>
            {
                // Start RTP receiver on port 7000 (or the negotiated port)
                rtpReceiver = new RTPReceiver(7000);
                rtpReceiver.OnAudioDataReceived = (pcmData) =>
                {
              
                    // Replace the following line:
                    audioPlayer.AddSamples(pcmData);
                };
                _ = Task.Run(() => rtpReceiver.StartAsync());
                Log("RTP receiver started.");
            };

            // When TEARDOWN is received, stop the RTP receiver.
            rtspServer.OnTeardownRequested = () =>
            {
                rtpReceiver?.Stop();
                Log("RTP receiver stopped.");
            };

            _ = Task.Run(() => rtspServer.StartAsync());
            Log("RTSP server started on port 7000.");
        }

        private void Log(string message)
        {
            // Append the message to the LogTextBlock defined in your XAML.
            LogTextBlock.Text += message + Environment.NewLine;
        }
    }
}
