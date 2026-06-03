using System;
using System.Collections.Generic;

namespace project_2
{
    internal class DeltaCompressor
    {
        private int step;

        public CancellationToken Token { get; set; }
        public PerformanceMonitor Monitor { get; set; }

        public DeltaCompressor(int stepSize = 50)
        {
            step = stepSize;
        }

        // 🔥 نعيد: أول عينة + البتات + عدد العينات
        public (short firstSample, List<int> bits, int sampleCount)? Compress(short[] samples)
        {
            List<int> bits = new List<int>();

            short firstSample = samples[0];
            int predictor = firstSample;

            Monitor?.Start(samples.Length, samples.Length * sizeof(short));

            for (int i = 1; i < samples.Length; i++)
            {
                // 🔥 الإلغاء
                if (Token.IsCancellationRequested)
                {
                    Monitor?.ReportCanceled();
                    return null;
                }

                if (samples[i] > predictor)
                {
                    bits.Add(1);
                    predictor += step;
                }
                else
                {
                    bits.Add(0);
                    predictor -= step;
                }

                // 🔥 تحديث كل 1000 عينة فقط
                if (i % 1000 == 0)
                {
                    float progress = (i * 100f) / bits.Count;
                    Monitor?.ReportProgress(progress);
                }

            }

            Monitor?.Finish();

            return (firstSample, bits, samples.Length);
        }
    }
}
