using System;
using System.IO;
using System.Diagnostics;

namespace project_2
{
    internal class CompressionReport
    {
        private Stopwatch sw;
        private long originalSize;

        public void Start(long sampleCount)
        {
            originalSize = sampleCount * 2; // short = 2 bytes
            sw = Stopwatch.StartNew();
        }

        public void Finish(string filePath, string algorithm, int samplingRate, int bitDepth, int stepSize = -1)
        {
            sw.Stop();

            long compressedSize = new FileInfo(filePath).Length;
            double ratio = (1 - (double)compressedSize / originalSize) * 100;

            string stepInfo = stepSize > 0 ? $"🔹 Step Size: {stepSize}\n" : "";

            string report =
                $"📌 تفاصيل عملية الضغط:\n\n" +
                $"🔹 الخوارزمية: {algorithm}\n" +
                stepInfo +
                $"🔹 Sampling Rate: {samplingRate} Hz\n" +
                $"🔹 Bit Depth: {bitDepth}-bit\n\n" +
                $"📦 حجم الملف قبل الضغط: {originalSize / 1024.0:F2} KB\n" +
                $"📦 حجم الملف بعد الضغط: {compressedSize / 1024.0:F2} KB\n" +
                $"💾 نسبة التوفير: {ratio:F2}%\n\n" +
                $"⏱ الزمن المستغرق: {sw.Elapsed.TotalMilliseconds:F2} ms";

            System.Windows.Forms.MessageBox.Show(report, "نتائج عملية الضغط");
        }
    }
}
