using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public class ADMCompressor
{
    public CancellationToken Token { get; set; }
    public PerformanceMonitor Monitor { get; set; }

    private short[] ReadSamples(string filePath)
    {
        using (var reader = new AudioFileReader(filePath))
        {
            List<short> samples = new List<short>();
            float[] buffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];

            int read;
            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < read; i++)
                {
                    short s = (short)(buffer[i] * short.MaxValue);
                    samples.Add(s);
                }
            }

            return samples.ToArray();
        }
    }

    private List<int> ADM_Compress(short[] samples, int initialStep = 2)
    {
        List<int> bits = new List<int>();

        short predictor = samples[0];
        int step = initialStep;

        int lastBit = 0;
        int repeatCount = 0;

        Monitor?.Start(samples.Length, samples.Length * sizeof(short));

        for (int i = 1; i < samples.Length; i++)
        {
            if (Token.IsCancellationRequested)
            {
                Monitor?.ReportCanceled();
                return null;
            }

            int bit;

            if (samples[i] > predictor)
            {
                bit = 1;
                predictor += (short)step;
            }
            else
            {
                bit = 0;
                predictor -= (short)step;
            }

            bits.Add(bit);

            if (bit == lastBit)
            {
                repeatCount++;
                if (repeatCount >= 2 && step < 1024)
                    step *= 2;
            }
            else
            {
                repeatCount = 0;
                if (step > 4)
                    step /= 2;
            }

            lastBit = bit;

            if (predictor > 32767) predictor = 32767;
            if (predictor < -32768) predictor = -32768;

            if (i % 1000 == 0)
                Monitor.Update(2000, 125);

        }

        Monitor?.Finish();
        return bits;
    }

    private byte[] PackBits(List<int> bits)
    {
        int byteCount = (bits.Count + 7) / 8;
        byte[] bytes = new byte[byteCount];

        for (int i = 0; i < bits.Count; i++)
        {
            int byteIndex = i / 8;
            int bitIndex = 7 - (i % 8);

            if (bits[i] == 1)
                bytes[byteIndex] |= (byte)(1 << bitIndex);
        }

        return bytes;
    }

    // 🔥 النسخة الجديدة: ترجع البيانات فقط — بدون SaveFileDialog
    public (short firstSample, int sampleRate, byte[] bitBytes, int sampleCount)? CompressFile(string inputPath)
    {
        var samples = ReadSamples(inputPath);
        var bits = ADM_Compress(samples);

        if (bits == null)
            return null;

        var reader = new AudioFileReader(inputPath);
        int sampleRate = reader.WaveFormat.SampleRate;

        byte[] bitBytes = PackBits(bits);

        return (samples[0], sampleRate, bitBytes, samples.Length);
    }
}
