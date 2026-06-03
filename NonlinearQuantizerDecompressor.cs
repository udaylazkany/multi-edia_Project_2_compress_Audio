using System;
using System.Threading;

namespace project_2
{
    internal class NonlinearQuantizerDecompressor
    {
        public CancellationToken Token { get; set; }
        public PerformanceMonitor Monitor { get; set; }

        // نفس جدول المستويات المستخدم في الضغط
        private readonly int[] Levels =
        {
            0, 1, 2, 4, 8, 16, 32, 64,
            128, 256, 512, 1024, 2048, 4096, 8192, 16384
        };

        // فك ضغط عينة واحدة
        private short Dequantize(int q)
        {
            int index = Math.Abs(q);

            if (index >= Levels.Length)
                index = Levels.Length - 1;

            int value = Levels[index];
            return (short)(q >= 0 ? value : -value);
        }

        // فك ضغط مصفوفة كاملة (مع مراقبة الأداء)
        public short[] Decompress(int[] compressed)
        {
            short[] output = new short[compressed.Length];

            Monitor?.Start(compressed.Length, compressed.Length);

            for (int i = 0; i < compressed.Length; i++)
            {
                // 🔥 الإلغاء
                if (Token.IsCancellationRequested)
                {
                    Monitor?.ReportCanceled();
                    return null;
                }

                output[i] = Dequantize(compressed[i]);

                // 🔥 تحديث كل 1000 عينة فقط
                if (i % 1000 == 0)
                    Monitor?.Update(1000, 1000);
            }

            Monitor?.Finish();
            return output;
        }
    }
}
