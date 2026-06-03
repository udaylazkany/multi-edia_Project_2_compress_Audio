using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace project_2
{
    internal class Preview
    {
        private const int SampleRate = 44100;
        private const int PreviewSeconds = 60;

        // تخزين آخر معاينة للتشغيل
        public static byte[] LastPreviewWav { get; private set; }
        public static string LastPreviewAlgorithm { get; private set; }
        public static bool HasPreview { get; private set; }
        public static string LastPreviewTempFile { get; private set; }  // مسار الملف المؤقت

        // معاينة ADM
        public async Task<bool> PreviewADM(string filePath, IProgress<PreviewProgress> progress, CancellationToken token)
        {
            return await PerformPreview(filePath, "ADM", progress, token);
        }

        // معاينة DM
        public async Task<bool> PreviewDM(string filePath, IProgress<PreviewProgress> progress, CancellationToken token)
        {
            return await PerformPreview(filePath, "DM", progress, token);
        }

        // معاينة NQ
        public async Task<bool> PreviewNQ(string filePath, IProgress<PreviewProgress> progress, CancellationToken token)
        {
            return await PerformPreview(filePath, "NQ", progress, token);
        }

        // دالة المعاينة الأساسية
        private async Task<bool> PerformPreview(string filePath, string algorithm, IProgress<PreviewProgress> progress, CancellationToken token)
        {
            try
            {
                // 1. تحميل العينات
                progress?.Report(new PreviewProgress { Status = $"جاري تحميل الملف...", Percentage = 0 });

                int previewCount = SampleRate * PreviewSeconds;
                short[] originalSamples = await LoadSamples(filePath, previewCount, token);

                if (originalSamples == null || originalSamples.Length == 0)
                    return false;

                // 2. ضغط وفك ضغط
                progress?.Report(new PreviewProgress { Status = $"جاري معاينة {algorithm}...", Percentage = 30 });

                short[] decompressed = null;

                await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();

                    switch (algorithm)
                    {
                        case "ADM":
                            decompressed = PreviewADMInternal(originalSamples, token);
                            break;
                        case "DM":
                            decompressed = PreviewDMInternal(originalSamples, token);
                            break;
                        case "NQ":
                            decompressed = PreviewNQInternal(originalSamples, token);
                            break;
                    }
                }, token);

                if (decompressed == null || decompressed.Length == 0)
                    return false;

                // 3. تخزين المعاينة في الذاكرة وكتابتها كملف مؤقت
                progress?.Report(new PreviewProgress { Status = "تجهيز المعاينة...", Percentage = 80 });

                byte[] wavData = ConvertToWav(decompressed, SampleRate);

                // تخزين البيانات في متغيرات static
                LastPreviewWav = wavData;
                LastPreviewAlgorithm = algorithm;
                HasPreview = true;

                // ✅ حفظ كملف WAV مؤقت للتشغيل عبر myPlayer
                SavePreviewToTempFile(wavData);

                progress?.Report(new PreviewProgress { Status = $"اكتملت معاينة {algorithm}", Percentage = 100 });

                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في معاينة {algorithm}: {ex.Message}", "خطأ",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // ========== دوال المعاينة الداخلية ==========

        // معاينة DM
        private short[] PreviewDMInternal(short[] samples, CancellationToken token)
        {
            DeltaCompressor compressor = new DeltaCompressor(stepSize: 25);
            var compressed = compressor.Compress(samples);

            token.ThrowIfCancellationRequested();

            if (compressed == null) return null;

            var compressedValue = compressed.Value;
            short firstSample = compressedValue.firstSample;
            var bits = compressedValue.bits;
            int sampleCount = compressedValue.sampleCount;

            List<int> bitsList = new List<int>();
            foreach (var bit in bits)
            {
                bitsList.Add(Convert.ToInt32(bit));
            }

            DeltaDecompressor decompressor = new DeltaDecompressor(stepSize: 25);
            decompressor.Token = token;
            decompressor.Monitor = new PerformanceMonitor();

            return decompressor.Decompress(bitsList, firstSample, sampleCount);
        }

        // معاينة NQ
        private short[] PreviewNQInternal(short[] samples, CancellationToken token)
        {
            int[] compressed = samples.Select(s => s / 256).ToArray();

            token.ThrowIfCancellationRequested();

            NonlinearQuantizerDecompressor decompressor = new NonlinearQuantizerDecompressor();
            decompressor.Token = token;
            decompressor.Monitor = new PerformanceMonitor();

            return decompressor.Decompress(compressed);
        }

        // معاينة ADM (تعمل في الذاكرة)
        // معاينة ADM (نسخة آمنة)
        // معاينة ADM الحقيقية (تعمل مع الملفات المؤقتة)
        // معاينة ADM (نسخة مصححة)
        private short[] PreviewADMInternal(short[] samples, CancellationToken token)
        {
            string tempWav = Path.GetTempFileName() + ".wav";
            string tempAdm = Path.GetTempFileName() + ".adm";

            try
            {
                // 1. حفظ العينات في ملف WAV مؤقت
                using (var writer = new WaveFileWriter(tempWav, new WaveFormat(SampleRate, 16, 1)))
                {
                    foreach (short s in samples)
                        writer.WriteSample((float)s / 32768f);
                }

                token.ThrowIfCancellationRequested();

                // 2. ضغط ADM
                ADMCompressor compressor = new ADMCompressor();
                compressor.Token = token;
                compressor.Monitor = new PerformanceMonitor();

                var compressed = compressor.CompressFile(tempWav);
                if (compressed == null) return null;

                token.ThrowIfCancellationRequested();

                // 3. حفظ الملف بنفس التنسيق الذي يتوقعه ADMDecompressor
                using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(tempAdm)))
                {
                    bw.Write(compressed.Value.firstSample);  // short firstSample
                    bw.Write(compressed.Value.sampleRate);   // int sampleRate
                    bw.Write(compressed.Value.sampleCount);  // int sampleCount
                    bw.Write(2);                              // int step (step size)
                    bw.Write(compressed.Value.bitBytes.Length); // int bitBytesLength
                    bw.Write(compressed.Value.bitBytes);     // byte[] bitBytes
                }

                // 4. فك الضغط
                ADMDecompressor decompressor = new ADMDecompressor();
                byte[] wavBytes = decompressor.DecompressToWavBytes(tempAdm);

                // 5. التحقق من صحة البيانات
                if (wavBytes == null || wavBytes.Length < 44)
                {
                    MessageBox.Show("فشل فك ضغط ADM", "خطأ",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return samples;
                }

                return ReadSamplesFromWav(wavBytes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في ADM: {ex.Message}", "خطأ",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                return samples;
            }
            finally
            {
                try { if (File.Exists(tempWav)) File.Delete(tempWav); } catch { }
                try { if (File.Exists(tempAdm)) File.Delete(tempAdm); } catch { }
            }
        }
        // حفظ المعاينة كملف WAV مؤقت
        private static void SavePreviewToTempFile(byte[] wavData)
        {
            // حذف الملف المؤقت السابق إذا وجد
            CleanupTempFile();

            // إنشاء ملف مؤقت جديد
            string tempFile = Path.GetTempFileName() + ".wav";
            File.WriteAllBytes(tempFile, wavData);
            LastPreviewTempFile = tempFile;
        }

        // تنظيف الملف المؤقت
        public static void CleanupTempFile()
        {
            if (!string.IsNullOrEmpty(LastPreviewTempFile) && File.Exists(LastPreviewTempFile))
            {
                try
                {
                    File.Delete(LastPreviewTempFile);
                }
                catch { }
                LastPreviewTempFile = null;
            }
        }

        // الحصول على مسار ملف المعاينة (للاستخدام مع myPlayer)
        public static string GetPreviewFilePath()
        {
            return HasPreview && LastPreviewTempFile != null && File.Exists(LastPreviewTempFile)
                ? LastPreviewTempFile
                : null;
        }

        // ========== دوال مساعدة ==========

        // تحميل العينات من الملف
        private async Task<short[]> LoadSamples(string filePath, int maxSamples, CancellationToken token)
        {
            return await Task.Run(() =>
            {
                using (var reader = new MediaFoundationReader(filePath))
                using (var resampler = new MediaFoundationResampler(reader, new WaveFormat(SampleRate, 16, 1)))
                {
                    List<short> samples = new List<short>();
                    byte[] buffer = new byte[4096];
                    int bytesRead;

                    while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) > 0 && samples.Count < maxSamples)
                    {
                        token.ThrowIfCancellationRequested();

                        for (int i = 0; i < bytesRead / 2 && samples.Count < maxSamples; i++)
                            samples.Add(BitConverter.ToInt16(buffer, i * 2));
                    }

                    return samples.ToArray();
                }
            }, token);
        }

        // تحويل العينات إلى WAV bytes
        private byte[] ConvertToWav(short[] samples, int sampleRate)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + samples.Length * 2);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(sampleRate);
                writer.Write(sampleRate * 2);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(samples.Length * 2);

                foreach (short sample in samples)
                    writer.Write(sample);

                return ms.ToArray();
            }
        }

        // قراءة العينات من WAV bytes
        private short[] ReadSamplesFromWav(byte[] wavData)
        {
            using (MemoryStream ms = new MemoryStream(wavData))
            using (WaveFileReader reader = new WaveFileReader(ms))
            {
                List<short> samples = new List<short>();
                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < bytesRead / 2; i++)
                        samples.Add(BitConverter.ToInt16(buffer, i * 2));
                }

                return samples.ToArray();
            }
        }
    }

    // كلاس لتتبع التقدم
    public class PreviewProgress
    {
        public string Status { get; set; }
        public int Percentage { get; set; }
    }
}