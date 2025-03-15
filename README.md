# WindowPlay

WindowPlay is a project aimed at creating a versatile and user-friendly window management tool.

## Features

- **Window Resizing:** Easily resize windows to fit your needs.
- **Window Positioning:** Arrange windows in various layouts.
- **RTSP Server:**  
  A simple RTSP server that listens on a specified port (default 7000) and handles basic RTSP commands.  
  - Responds to `OPTIONS`, `DESCRIBE`, `SETUP`, `PLAY`, and `TEARDOWN` methods.
  - Generates a minimal SDP (Session Description Protocol) response.
- **RTP Receiver:**  
  Listens for RTP packets on the negotiated port.  
  - Strips a dummy 12-byte RTP header.
  - Invokes a callback to deliver audio data (assumed PCM for simplicity).
- **Multiple Desktops:** Manage multiple desktops seamlessly.
- **Hotkeys:** Quickly access window management functions using customizable hotkeys.
- **User-friendly Interface:** Simple and intuitive interface for easy navigation and usage.

## Requirements

- [.NET 5+ SDK](https://dotnet.microsoft.com/download)
- [Makaretu.MDNS NuGet Package](https://www.nuget.org/packages/Makaretu.Dns.Multicast.New/0.38.0?_src=template)
- [NAudio NuGet Package](https://www.nuget.org/packages/NAudio/)

## Project Structure

- **MainWindow.xaml / MainWindow.xaml.cs:**  
  Contains the WinUI 3 UI, a button to start the service, and a log display.

- **RTSPServer.cs:**  
  Implements a basic RTSP server to handle RTSP commands and session management.

- **RTPReceiver.cs:**  
  Listens for RTP packets and extracts audio payload data (placeholder implementation).

- **AudioPlayer.cs:**  
  Uses NAudio to initialize an audio output device and play buffered PCM data.

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/stef1949/WindowPlay.git

2. Restore Dependencies:
   ```bash
   dotnet restore
3. Build the Project:
   ```bash
   dotnet build
4. Run the Application:
   ```bash
   dotnet run

This will launch the WinUI 3 application window. Click the Start button to:

Advertise the AirPlay receiver service via mDNS.
Start the RTSP server.
Begin listening for RTP packets (audio streaming).

## Testing
- Using an iOS Device:
Ensure that your Windows machine and your iOS device are on the same network. On your iOS device, look for an AirPlay receiver matching "My AirPlay Receiver". Note that full connection and audio streaming may require additional RTSP and RTP handling.

- Network Monitoring:
Use tools like Wireshark or Bonjour Browser to inspect mDNS advertisements and RTSP/RTP traffic.

## Next Steps / Roadmap
### Improve RTSP Protocol Support:
Add support for additional methods (e.g., OPTIONS, PAUSE) and implement full session management.
### Enhance RTP Handling:
Implement jitter buffering, packet reordering, and (if needed) decryption.
### Integrate Audio Decoding:
Use a library such as FFmpeg.AutoGen to decode encoded audio formats (AAC/ALAC) to PCM.
### Refine TXT Records:
Adjust TXT records to more closely mimic genuine AirPlay receivers.
