using System;

namespace project_2
{
    internal class NonlinearQuantizerCompressor
    {
        public CancellationToken Token { get; set; }
        public PerformanceMonitor Monitor { get; set; }

        // جدول المستويات (غير خطي)
        private readonly int[] Levels =
        {
            0, 1, 2, 4, 8, 16, 32, 64,
            128, 256, 512, 1024, 2048, 4096, 8192, 16384
        };

        // ضغط عينة واحدة
        private int Quantize(int sample)
        {
            int abs = Math.Abs(sample);
            int index = 0;

            for (int i = 0; i < Levels.Length; i++)
            {
                if (abs <= Levels[i])
                {
                    index = i;
                    break;
                }
            }

            return sample >= 0 ? index : -index;
        }

        // 🔥 النسخة الجديدة: ترجع البيانات فقط — بدون UI
        public (int[] compressed, int sampleCount)? Compress(short[] samples)
        {
            int[] output = new int[samples.Length];

            Monitor?.Start(samples.Length, samples.Length * sizeof(short));

            for (int i = 0; i < samples.Length; i++)
            {
                // 🔥 الإلغاء
                if (Token.IsCancellationRequested)
                {
                    Monitor?.ReportCanceled();
                    return null;
                }

                output[i] = Quantize(samples[i]);

                // 🔥 تحديث كل 1000 عينة فقط
                if (i % 1000 == 0)
                    Monitor.Update(
        processed: 1000 * 2,   // 2000 bytes
        compressed: 1000 * 1   // 1000 bytes
    );

            }

            Monitor?.Finish();

            return (output, samples.Length);
        }
    }
}
