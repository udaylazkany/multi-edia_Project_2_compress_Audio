using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public class ADMDecompressor
{
    public CancellationToken Token { get; set; }
    public PerformanceMonitor Monitor { get; set; }

    private List<int> UnpackBits(byte[] bytes, int bitCount)
    {
        List<int> bits = new List<int>();

        for (int i = 0; i < bitCount; i++)
        {
            int byteIndex = i / 8;
            int bitIndex = 7 - (i % 8);

            int bit = (bytes[byteIndex] >> bitIndex) & 1;
            bits.Add(bit);
        }

        return bits;
    }

    private short[] ADM_Decompress(List<int> bits, short firstSample, int initialStep = 2)
    {
        List<short> samples = new List<short>();

        short predictor = firstSample;
        int step = initialStep;

        int lastBit = 0;
        int repeatCount = 0;

        samples.Add(predictor);

        Monitor?.Start(bits.Count, bits.Count);

        for (int i = 0; i < bits.Count; i++)
        {
            if (Token.IsCancellationRequested)
            {
                Monitor?.ReportCanceled();
                return null;
            }

            int bit = bits[i];

            if (bit == 1)
                predictor += (short)step;
            else
                predictor -= (short)step;

            // Adaptive step
            if (bit == lastBit)
            {
                repeatCount++;
                if (repeatCount >= 3 && step < 512)
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

            samples.Add(predictor);

            if (i % 1000 == 0)
                Monitor?.Update(1000, 1000);
        }

        Monitor?.Finish();
        return samples.ToArray();
    }

    // 🔥 النسخة الجديدة: ترجع PCM فقط — بدون حفظ
    public (short[] pcm, int sampleRate)? Decompress(string inputPath)
    {
        using (BinaryReader br = new BinaryReader(File.OpenRead(inputPath)))
        {
            short firstSample = br.ReadInt16();
            int sampleRate = br.ReadInt32();
            int sampleCount = br.ReadInt32();
            int step = br.ReadInt32();
            int bitBytesLength = br.ReadInt32();

            byte[] bitBytes = br.ReadBytes(bitBytesLength);

            int bitCount = sampleCount - 1;
            var bits = UnpackBits(bitBytes, bitCount);

            var samples = ADM_Decompress(bits, firstSample, step);

            if (samples == null)
                return null;

            return (samples, sampleRate);
        }
    }

    // 🔥 إذا بدك WAV جاهز (لـ ADM فقط)
    public byte[] DecompressToWavBytes(string inputPath)
    {
        var result = Decompress(inputPath);
        if (result == null)
            return null;

        var (samples, sampleRate) = result.Value;

        using (var ms = new MemoryStream())
        using (var writer = new WaveFileWriter(ms, new WaveFormat(sampleRate, 16, 1)))
        {
            foreach (short s in samples)
                writer.WriteSample(s / 32768f);

            writer.Flush();
            return ms.ToArray();
        }
    }
}
