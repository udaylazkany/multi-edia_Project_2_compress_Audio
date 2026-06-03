using System;
using System.Collections.Generic;
using System.Threading;

namespace project_2
{
    internal class DeltaDecompressor
    {
        private int step;

        public CancellationToken Token { get; set; }
        public PerformanceMonitor Monitor { get; set; }

        public DeltaDecompressor(int stepSize = 50)
        {
            step = stepSize;
        }

        public short[] DecompressFromFile(string filePath)
        {
            using (BinaryReader br = new BinaryReader(File.OpenRead(filePath)))
            {
                // اقرأ أول عينة
                short firstSample = br.ReadInt16();

                // اقرأ عدد العينات الأصلي
                int sampleCount = br.ReadInt32();

                // اقرأ البتات
                List<int> bits = new List<int>();
                while (br.BaseStream.Position < br.BaseStream.Length)
                    bits.Add(br.ReadByte());

                // فك الضغط الحقيقي
                return Decompress(bits, firstSample, sampleCount);
            }
        }
        public short[] Decompress(List<int> bits, short firstSample, int sampleCount)
        {
            short[] samples = new short[sampleCount];

            int predictor = firstSample;
            samples[0] = firstSample;

            Monitor?.Start(sampleCount, bits.Count);

            for (int i = 0; i < sampleCount - 1; i++)
            {
                if (Token.IsCancellationRequested)
                {
                    Monitor?.ReportCanceled();
                    return null;
                }

                if (bits[i] == 1)
                    predictor += step;
                else
                    predictor -= step;

                samples[i + 1] = (short)predictor;

                if (i % 1000 == 0)
                    Monitor?.Update(1000, 1000);
            }

            Monitor?.Finish();
            return samples;
        }

    }
}
