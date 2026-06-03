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
    internal class CompressedSaver
    {
        private const int SampleRate = 44100;

        /// <summary>
        /// حفظ المعاينة كملف مضغوط
        /// </summary>
        public static async Task<bool> SavePreviewAsCompressed(
            byte[] previewWavData,
            string previewAlgorithm,
            IProgress<SaveProgress> progress,
            CancellationToken token)
        {
            if (previewWavData == null || previewWavData.Length < 44)
            {
                MessageBox.Show("لا توجد معاينة صالحة للحفظ.", "تنبيه",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // اختيار صيغة الضغط
            string algorithm = ShowAlgorithmDialog();
            if (algorithm == null) return false;

            // اختيار مكان الحفظ
            string filePath = ShowSaveDialog(algorithm);
            if (string.IsNullOrEmpty(filePath)) return false;

            // تحويل WAV bytes إلى short[]
            short[] samples = ReadSamplesFromWav(previewWavData);
            if (samples == null || samples.Length == 0) return false;

            // حفظ حسب الخوارزمية
            switch (algorithm)
            {
                case "DM":
                    await SaveAsDM(samples, filePath, progress, token);
                    break;
                case "NQ":
                    await SaveAsNQ(samples, filePath, progress, token);
                    break;
                case "ADM":
                    await SaveAsADM(samples, filePath, progress, token);
                    break;
                default:
                    return false;
            }

            MessageBox.Show($"✅ تم حفظ المعاينة كملف {algorithm} مضغوط بنجاح!\n{filePath}\n\n" +
                           "لا يمكن فتح هذا الملف إلا من خلال برنامج فك الضغط.",
                           "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return true;
        }

        // ========== دوال الضغط ==========

        private static async Task SaveAsDM(short[] samples, string filePath, IProgress<SaveProgress> progress, CancellationToken token)
        {
            await Task.Run(() =>
            {
                progress?.Report(new SaveProgress { Status = "جاري ضغط DM...", Percentage = 50 });

                DeltaCompressor compressor = new DeltaCompressor(stepSize: 25);
                var compressed = compressor.Compress(samples);

                if (compressed == null) throw new Exception("فشل ضغط DM");

                using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(filePath)))
                {
                    bw.Write(compressed.Value.firstSample);
                    bw.Write(compressed.Value.sampleCount);
                    foreach (var bit in compressed.Value.bits)
                        bw.Write((byte)bit);
                }

                progress?.Report(new SaveProgress { Status = "تم الحفظ", Percentage = 100 });
            }, token);
        }

        private static async Task SaveAsNQ(short[] samples, string filePath, IProgress<SaveProgress> progress, CancellationToken token)
        {
            await Task.Run(() =>
            {
                progress?.Report(new SaveProgress { Status = "جاري ضغط NQ...", Percentage = 50 });

                int[] compressed = samples.Select(s => s / 256).ToArray();

                using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(filePath)))
                {
                    bw.Write(compressed.Length);
                    foreach (int q in compressed)
                        bw.Write((short)q);
                }

                progress?.Report(new SaveProgress { Status = "تم الحفظ", Percentage = 100 });
            }, token);
        }

        private static async Task SaveAsADM(short[] samples, string filePath, IProgress<SaveProgress> progress, CancellationToken token)
        {
            string tempWav = Path.GetTempFileName() + ".wav";

            try
            {
                // حفظ العينات في ملف WAV مؤقت
                using (var writer = new WaveFileWriter(tempWav, new WaveFormat(SampleRate, 16, 1)))
                {
                    foreach (short s in samples)
                        writer.WriteSample((float)s / 32768f);
                }

                progress?.Report(new SaveProgress { Status = "جاري ضغط ADM...", Percentage = 30 });

                token.ThrowIfCancellationRequested();

                ADMCompressor compressor = new ADMCompressor();
                compressor.Token = token;
                compressor.Monitor = new PerformanceMonitor();

                var compressed = compressor.CompressFile(tempWav);
                if (compressed == null) throw new Exception("فشل ضغط ADM");

                progress?.Report(new SaveProgress { Status = "جاري حفظ الملف...", Percentage = 70 });

                using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(filePath)))
                {
                    bw.Write(compressed.Value.firstSample);
                    bw.Write(compressed.Value.sampleRate);
                    bw.Write(compressed.Value.sampleCount);
                    bw.Write(2); // step
                    bw.Write(compressed.Value.bitBytes.Length);
                    bw.Write(compressed.Value.bitBytes);
                }

                progress?.Report(new SaveProgress { Status = "تم الحفظ", Percentage = 100 });
            }
            finally
            {
                if (File.Exists(tempWav)) File.Delete(tempWav);
            }
        }

        // ========== دوال مساعدة ==========

        private static string ShowAlgorithmDialog()
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "اختر صيغة الضغط للحفظ:\n\n" +
                "1) DM (Delta Modulation) - نسبة ضغط عالية\n" +
                "2) NQ (Nonlinear Quantization) - نسبة ضغط عالية جداً\n" +
                "3) ADM (Adaptive Delta Modulation) - جودة عالية\n\n" +
                "أدخل الرقم (1-3):",
                "حفظ المعاينة بشكل مضغوط",
                "1"
            );

            switch (input)
            {
                case "1": return "DM";
                case "2": return "NQ";
                case "3": return "ADM";
                default: return null;
            }
        }

        private static string ShowSaveDialog(string algorithm)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            switch (algorithm)
            {
                case "DM":
                    sfd.Filter = "DM Compressed File|*.dm";
                    sfd.Title = "حفظ المعاينة كملف DM مضغوط";
                    break;
                case "NQ":
                    sfd.Filter = "NQ Compressed File|*.nq";
                    sfd.Title = "حفظ المعاينة كملف NQ مضغوط";
                    break;
                case "ADM":
                    sfd.Filter = "ADM Compressed File|*.adm";
                    sfd.Title = "حفظ المعاينة كملف ADM مضغوط";
                    break;
            }

            return sfd.ShowDialog() == DialogResult.OK ? sfd.FileName : null;
        }

        private static short[] ReadSamplesFromWav(byte[] wavData)
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

    // كلاس لتتبع تقدم الحفظ
    public class SaveProgress
    {
        public string Status { get; set; }
        public int Percentage { get; set; }
    }
}