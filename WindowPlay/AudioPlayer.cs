using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Makaretu.Dns;
using NAudio.Wave;

namespace WindowPlay
{
    public class AudioPlayer
    {
        private WaveOutEvent waveOut;
        private BufferedWaveProvider waveProvider;

        public AudioPlayer(int sampleRate = 44100, int channels = 2)
        {
            // Create a wave format matching your expected PCM data.
            var waveFormat = new WaveFormat(sampleRate, 16, channels);
            waveProvider = new BufferedWaveProvider(waveFormat)
            {
                DiscardOnBufferOverflow = true
            };
            waveOut = new WaveOutEvent();
            waveOut.Init(waveProvider);
            waveOut.Play();
        }

        public void AddSamples(byte[] samples, int offset, int count)
        {
            waveProvider.AddSamples(samples, offset, count);
        }

        internal void AddSamples(byte[] pcmData)
        {
            throw new NotImplementedException();
        }
    }
}
