using NAudio.Wave;
using System;
using System.IO;

namespace project_2
{
    internal class LoadAudioInfo
    {
        public AudioInfo GetAudioInfo(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            long fileSizeBytes = fileInfo.Length;
            double fileSizeMB = fileSizeBytes / (1024.0 * 1024.0);

            using (var reader = new AudioFileReader(filePath))
            {
                var duration = reader.TotalTime;
                var sampleRate = reader.WaveFormat.SampleRate;
                var channels = reader.WaveFormat.Channels;
                var bitsPerSample = reader.WaveFormat.BitsPerSample;

                int bitrate = sampleRate * bitsPerSample * channels;
                string codec = reader.WaveFormat.Encoding.ToString();

                return new AudioInfo
                {
                    SizeKB = fileSizeBytes / 1024.0,

                    Duration = duration,
                    SampleRate = sampleRate,
                    Channels = channels,
                    Bitrate = bitrate / 1000,
                    Codec = codec
                };
            }
        }
    }

    public class AudioInfo
    {
        public double SizeKB { get; set; }

        public TimeSpan Duration { get; set; }
        public int SampleRate { get; set; }
        public int Channels { get; set; }
        public int Bitrate { get; set; }
        public string Codec { get; set; }
    }
}
