using NAudio.Wave;
using System;
using System.Text;

namespace project_2
{

    public partial class Form1 : Form
    {
        AudioPlayerSimple myPlayer;
        string currentFile = "";
        private Label lblInfo;
        private Label lblStatus;
        private Label lblSpeed;
        private Label lblRatio;
        private CancellationTokenSource cts;



        public Form1()
        {
            InitializeComponent();
            lblSpeed = new Label();
            lblSpeed.Name = "lblSpeed";
            lblSpeed.AutoSize = true;
            lblSpeed.Location = new Point(10, 260);   // غير المكان حسب رغبتك
            lblSpeed.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblSpeed.Text = "السرعة: 0";
            this.Controls.Add(lblSpeed);
            lblRatio = new Label();
            lblRatio.Name = "lblRatio";
            lblRatio.AutoSize = true;
            lblRatio.Location = new Point(10, 290);   // غيّر المكان حسب تصميمك
            lblRatio.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblRatio.Text = "نسبة الضغط: 0%";
            this.Controls.Add(lblRatio);



            lblInfo = new Label();
            lblInfo.Name = "lblInfo";
            lblInfo.AutoSize = false;
            lblInfo.Size = new Size(250, 120);
            lblInfo.Location = new Point(10, 100);
            lblInfo.Font = new Font("Segoe UI", 10);
            this.Controls.Add(lblInfo);


            myPlayer = new AudioPlayerSimple(btnPlayPause, this);
            string filePath = "song.mp3";
            if (File.Exists(filePath))
            {
                myPlayer.LoadFile(filePath);
                btnPlayPause.Enabled = true;
            }
            else
            {
                btnPlayPause.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // إذا كان هناك ملف محمل، قم بالتبديل بين تشغيل وإيقاف
            if (myPlayer.HasFileLoaded())
            {
                myPlayer.Toggle();  // تأكد أن هذه الدالة موجودة في AudioPlayerSimple
            }
            else
            {
                // إذا لم يوجد ملف، افتح نافذة اختيار الملف
                openToolStripMenuItem_Click(sender, e);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFile = myPlayer.OpenFileDialog();

            if (currentFile != null)
            {
                if (myPlayer.LoadFile(currentFile))
                {
                    btnPlayPause.Enabled = true;
                    myPlayer.Play();
                    this.Text = $"مشغل الصوت - {Path.GetFileName(currentFile)}";
                    var loader = new LoadAudioInfo();
                    var info = loader.GetAudioInfo(currentFile);
                    DisplayAudioInfo(info);

                }
                else
                {
                    MessageBox.Show("خطأ في تحميل الملف");
                }
            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
        public void DisplayAudioInfo(AudioInfo info)
        {
            lblInfo.Text =
                $"الحجم: {info.SizeKB:F0} KB\n" +
                $"المدة: {info.Duration:mm\\:ss}\n" +
                $"Sample Rate: {info.SampleRate} Hz\n" +
                $"القنوات: {info.Channels} ({(info.Channels == 1 ? "Mono" : "Stereo")})\n" +
                $"Bitrate: {info.Bitrate} kbps\n" +
                $"Codec: {info.Codec}";
        }


        private async void openCompressFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Compressed Audio Files|*.dm;*.adm;*.nq";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            string ext = Path.GetExtension(ofd.FileName).ToLower();
            string tempWav = Path.GetTempFileName() + ".wav";

            // نافذة التقدم
            ProgressWindow win = new ProgressWindow();
            win.Show();

            CancellationTokenSource cts = new CancellationTokenSource();

            // إعداد المراقب
            PerformanceMonitor monitor = new PerformanceMonitor();

            // ربط الأحداث
            monitor.ProgressChanged += p =>
            {
                if (win.progressBar.InvokeRequired)
                    win.progressBar.Invoke(new Action(() => win.progressBar.Value = (int)p));
                else
                    win.progressBar.Value = (int)p;
            };

            monitor.SpeedChanged += s => win.AddSpeedPoint(s);
            monitor.RatioChanged += r => win.AddRatioPoint(r);

            monitor.StatusChanged += msg =>
            {
                if (win.lblStatus.InvokeRequired)
                    win.lblStatus.Invoke(new Action(() => win.lblStatus.Text = msg));
                else
                    win.lblStatus.Text = msg;
            };

            // زر الإلغاء
            win.btnCancel.Click += (s2, e2) =>
            {
                cts.Cancel();
                win.lblStatus.Text = "تم الإلغاء...";
            };

            short[] pcm = null;

            // 🔥 فك الضغط داخل Thread الخلفية
            await Task.Run(() =>
            {
                if (ext == ".dm")
                {
                    DeltaDecompressor dec = new DeltaDecompressor(stepSize: 25);
                    dec.Monitor = monitor;
                    dec.Token = cts.Token;

                    // فك الضغط من الملف مباشرة
                    pcm = dec.DecompressFromFile(ofd.FileName);
                }
                else if (ext == ".adm")
                {
                    ADMDecompressor dec = new ADMDecompressor();
                    byte[] wavBytes = dec.DecompressToWavBytes(ofd.FileName);

                    File.WriteAllBytes(tempWav, wavBytes);
                    pcm = null; // ADM جاهز WAV
                }
                else if (ext == ".nq")
                {
                    int[] compressed;

                    using (BinaryReader br = new BinaryReader(File.OpenRead(ofd.FileName)))
                    {
                        int count = br.ReadInt32();
                        compressed = new int[count];

                        for (int i = 0; i < count; i++)
                            compressed[i] = br.ReadInt16();
                    }

                    NonlinearQuantizerDecompressor dec = new NonlinearQuantizerDecompressor();
                    dec.Monitor = monitor;
                    dec.Token = cts.Token;

                    pcm = dec.Decompress(compressed);
                }
            });

            // إذا تم الإلغاء
            if (cts.IsCancellationRequested)
            {
                win.Close();
                return;
            }

            // 🔥 حفظ WAV (DM + NQ فقط)
            if (pcm != null)
            {
                using (var writer = new WaveFileWriter(tempWav, new WaveFormat(44100, 16, 1)))
                {
                    foreach (short s in pcm)
                        writer.WriteSample((float)s / 32768f);
                }
            }

            win.Close();

            // تشغيل الملف
            myPlayer.LoadFile(tempWav);
            btnPlayPause.Enabled = true;
            myPlayer.Play();
            this.Text = $"مشغل الصوت - {Path.GetFileName(ofd.FileName)} ({ext.ToUpper()})";
        }
        private async void aDMSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentFile))
            {
                MessageBox.Show("لم يتم تحميل أي ملف صوتي.");
                return;
            }

            // 🔥 أولاً: نطلب من المستخدم مكان الحفظ قبل الضغط
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "ADM File|*.adm";

            if (sfd.ShowDialog() != DialogResult.OK)
                return; // المستخدم لغى العملية

            // نافذة التقدم
            ProgressWindow win = new ProgressWindow();
            win.Show();

            cts = new CancellationTokenSource();

            ADMCompressor compressor = new ADMCompressor();
            compressor.Monitor = new PerformanceMonitor();
            compressor.Token = cts.Token;

            // زر الإلغاء
            win.btnCancel.Click += (s2, e2) =>
            {
                cts.Cancel();
                win.lblStatus.Text = "تم الإلغاء...";
            };

            // التقدم
            compressor.Monitor.ProgressChanged += p =>
            {
                int safe = Math.Min(100, Math.Max(0, (int)p));

                if (win.progressBar.InvokeRequired)
                    win.progressBar.Invoke(new Action(() => win.progressBar.Value = safe));
                else
                    win.progressBar.Value = safe;
            };

            compressor.Monitor.SpeedChanged += s => win.AddSpeedPoint(s);
            compressor.Monitor.RatioChanged += r => win.AddRatioPoint(r);

            compressor.Monitor.StatusChanged += msg =>
            {
                if (win.lblStatus.InvokeRequired)
                    win.lblStatus.Invoke(new Action(() => win.lblStatus.Text = msg));
                else
                    win.lblStatus.Text = msg;
            };

            // 🔥 تشغيل الضغط
            var result = await Task.Run(() => compressor.CompressFile(currentFile));

            if (result == null)
            {
                win.Close();
                return; // تم الإلغاء
            }

            var (firstSample, sampleRate, bitBytes, sampleCount) = result.Value;

            // 🔥 حفظ الملف مباشرة (لأن المستخدم اختار المكان مسبقاً)
            using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(sfd.FileName)))
            {
                bw.Write(firstSample);
                bw.Write(sampleRate);
                bw.Write(sampleCount);
                bw.Write(2); // step size
                bw.Write(bitBytes.Length);
                bw.Write(bitBytes);
            }

            MessageBox.Show("تم ضغط الملف بنجاح!");

            win.Close();
        }

        private async void dMSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentFile))
            {
                MessageBox.Show("لم يتم تحميل أي ملف صوتي.");
                return;
            }

            // نافذة التقدم
            ProgressWindow win = new ProgressWindow();
            win.Show();

            CancellationTokenSource cts = new CancellationTokenSource();

            // إعداد المراقب
            PerformanceMonitor monitor = new PerformanceMonitor();

            // ربط الأحداث
            monitor.ProgressChanged += p =>
            {
                if (win.progressBar.InvokeRequired)
                    win.progressBar.Invoke(new Action(() => win.progressBar.Value = (int)p));
                else
                    win.progressBar.Value = (int)p;
            };

            monitor.SpeedChanged += s => win.AddSpeedPoint(s);
            monitor.RatioChanged += r => win.AddRatioPoint(r);

            monitor.StatusChanged += msg =>
            {
                if (win.lblStatus.InvokeRequired)
                    win.lblStatus.Invoke(new Action(() => win.lblStatus.Text = msg));
                else
                    win.lblStatus.Text = msg;
            };

            // زر الإلغاء
            win.btnCancel.Click += (s2, e2) =>
            {
                cts.Cancel();
                win.lblStatus.Text = "تم الإلغاء...";
            };

            // 🔥 قراءة العينات
            short[] samples;
            using (var reader = new MediaFoundationReader(currentFile))
            using (var resampler = new MediaFoundationResampler(reader, new WaveFormat(44100, 16, 1)))
            {
                resampler.ResamplerQuality = 60;

                List<short> list = new List<short>();
                byte[] buffer = new byte[2];

                int bytesRead;
                while ((bytesRead = resampler.Read(buffer, 0, 2)) > 0)
                    list.Add(BitConverter.ToInt16(buffer, 0));

                samples = list.ToArray();
            }

            // 🔥 قائمة الخيارات
            List<int> sampleOptions = new List<int> { 1000, 5000, 10000, 20000, -1 };

            string menu = "اختر عدد العينات:\n";
            menu += "1) 1000\n";
            menu += "2) 5000\n";
            menu += "3) 10000\n";
            menu += "4) 20000\n";
            menu += "5) ALL\n";

            string input = Microsoft.VisualBasic.Interaction.InputBox(
                menu,
                "عدد العينات",
                "3"
            );

            if (!int.TryParse(input, out int choice) || choice < 1 || choice > 5)
            {
                MessageBox.Show("اختيار غير صالح.");
                return;
            }

            int sampleLimit = sampleOptions[choice - 1];

            if (sampleLimit != -1 && samples.Length > sampleLimit)
                samples = samples.Take(sampleLimit).ToArray();

            // 🔥 تقرير الضغط
            CompressionReport report = new CompressionReport();
            report.Start(samples.Length);

            // 🔥 تشغيل الضغط داخل Thread الخلفية
            DeltaCompressor compressor = new DeltaCompressor(stepSize: 2);
            compressor.Monitor = monitor;
            compressor.Token = cts.Token;

            var result = await Task.Run(() => compressor.Compress(samples));

            if (result == null)
            {
                win.Close();
                return;
            }

            var (firstSample, bits, sampleCount) = result.Value;

            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Delta Modulation File (*.dm)|*.dm";

            if (save.ShowDialog() == DialogResult.OK)
            {
                using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(save.FileName)))
                {
                    bw.Write(firstSample);
                    bw.Write(sampleCount);

                    foreach (var bit in bits)
                        bw.Write((byte)bit);
                }

                MessageBox.Show("تم حفظ ملف DM بنجاح!");

                // 🔥 عرض تقرير الضغط
                report.Finish(
                    save.FileName,
                    algorithm: "Delta Modulation (DM)",
                    samplingRate: 44100,
                    bitDepth: 16,
                    stepSize: 2
                );
            }

            win.Close();
        }
        private async void nQSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentFile))
            {
                MessageBox.Show("لم يتم تحميل أي ملف صوتي.");
                return;
            }

            // نافذة التقدم
            ProgressWindow win = new ProgressWindow();
            win.Show();

            CancellationTokenSource cts = new CancellationTokenSource();

            // إعداد المراقب
            PerformanceMonitor monitor = new PerformanceMonitor();

            // ربط الأحداث
            monitor.ProgressChanged += p =>
            {
                int safeValue = (int)p;
                if (safeValue > 100) safeValue = 100;
                if (win.progressBar.InvokeRequired)
                    win.progressBar.Invoke(new Action(() => win.progressBar.Value = safeValue));
                else
                    win.progressBar.Value = (int)p;
            };

            monitor.SpeedChanged += s => win.AddSpeedPoint(s);
            monitor.RatioChanged += r => win.AddRatioPoint(r);

            monitor.StatusChanged += msg =>
            {
                if (win.lblStatus.InvokeRequired)
                    win.lblStatus.Invoke(new Action(() => win.lblStatus.Text = msg));
                else
                    win.lblStatus.Text = msg;
            };

            // زر الإلغاء
            win.btnCancel.Click += (s2, e2) =>
            {
                cts.Cancel();
                win.lblStatus.Text = "تم الإلغاء...";
            };

            // 🔥 قراءة العينات
            short[] samples;
            using (var reader = new MediaFoundationReader(currentFile))
            using (var resampler = new MediaFoundationResampler(reader, new WaveFormat(44100, 16, 1)))
            {
                resampler.ResamplerQuality = 60;

                List<short> list = new List<short>();
                byte[] buffer = new byte[2];

                int bytesRead;
                while ((bytesRead = resampler.Read(buffer, 0, 2)) > 0)
                    list.Add(BitConverter.ToInt16(buffer, 0));

                samples = list.ToArray();
            }

            // 🔥 تشغيل الضغط داخل Thread الخلفية
            NonlinearQuantizerCompressor compressor = new NonlinearQuantizerCompressor();
            compressor.Monitor = monitor;
            compressor.Token = cts.Token;

            var result = await Task.Run(() => compressor.Compress(samples));

            // إذا تم الإلغاء
            if (result == null)
            {
                win.Close();
                return;
            }

            // استخراج البيانات
            var (compressed, sampleCount) = result.Value;

            // 🔥 الآن فقط نفتح SaveFileDialog على UI Thread
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Nonlinear Quantization File (*.nq)|*.nq";

            if (save.ShowDialog() == DialogResult.OK)
            {
                using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(save.FileName)))
                {
                    bw.Write(compressed.Length);
                    foreach (var q in compressed)
                        bw.Write((short)q);
                }

                MessageBox.Show("تم حفظ ملف NQ بنجاح!");
            }

            win.Close();
        }

        private byte[] PcmToWavBytes(short[] samples, int sampleRate = 44100)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                int byteRate = sampleRate * 2; // 16-bit mono
                int subchunk2Size = samples.Length * 2;

                // RIFF header
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + subchunk2Size);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));

                // fmt chunk
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);              // Subchunk1Size
                writer.Write((short)1);        // AudioFormat = PCM
                writer.Write((short)1);        // NumChannels = 1 (Mono)
                writer.Write(sampleRate);      // SampleRate
                writer.Write(byteRate);        // ByteRate
                writer.Write((short)2);        // BlockAlign
                writer.Write((short)16);       // BitsPerSample

                // data chunk
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(subchunk2Size);

                foreach (short s in samples)
                    writer.Write(s);

                return ms.ToArray();
            }
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // دالة عامة للمعاينة لتجنب تكرار الكود
        // دالة عامة للمعاينة (معدلة لتشغيل الصوت)
        private async Task PerformPreview(string algorithm, Func<string, IProgress<PreviewProgress>, CancellationToken, Task<bool>> previewFunc)
        {
            if (string.IsNullOrEmpty(currentFile))
            {
                MessageBox.Show("الرجاء تحميل ملف صوتي أولاً.", "تنبيه",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ProgressWindow win = new ProgressWindow();
            win.Text = $"معاينة {algorithm}";
            win.lblStatus.Text = "جاري التحضير...";
            win.Show();

            Preview preview = new Preview();

            var progress = new Progress<PreviewProgress>(p =>
            {
                if (win.lblStatus.InvokeRequired)
                {
                    win.lblStatus.Invoke(new Action(() => win.lblStatus.Text = p.Status));
                    win.progressBar.Invoke(new Action(() => win.progressBar.Value = p.Percentage));
                }
                else
                {
                    win.lblStatus.Text = p.Status;
                    win.progressBar.Value = p.Percentage;
                }
            });

            CancellationTokenSource cts = new CancellationTokenSource();
            win.btnCancel.Click += (s, ev) =>
            {
                cts.Cancel();
                win.lblStatus.Text = "جاري الإلغاء...";
            };

            try
            {
                bool success = await previewFunc(currentFile, progress, cts.Token);
                win.Close();

                if (success)
                {
                    // ✅ تشغيل الصوت بعد المعاينة
                    string previewFile = Preview.GetPreviewFilePath();
                    if (previewFile != null && File.Exists(previewFile))
                    {
                        // إيقاف التشغيل الحالي
                        myPlayer.Stop();

                        // تحميل ملف المعاينة
                        myPlayer.LoadFile(previewFile);

                        // تشغيل
                        myPlayer.Play();
                        btnPlayPause.Text = "⏸";
                        

                        MessageBox.Show($"✅ تمت معاينة {algorithm} بنجاح!\n يتم تشغيل المعاينة الآن...", "معاينة",
                                       MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"✅ تمت معاينة {algorithm} بنجاح!", "معاينة",
                                       MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (cts.IsCancellationRequested)
                {
                    MessageBox.Show("تم إلغاء المعاينة.", "ملغي",
                                   MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("فشلت المعاينة. تأكد من الملف.", "خطأ",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                win.Close();
                MessageBox.Show($"خطأ: {ex.Message}", "خطأ",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // استخدام الدالة العامة
        private async void aDMPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await PerformPreview("ADM", async (file, progress, token) =>
            {
                Preview p = new Preview();
                return await p.PreviewADM(file, progress, token);
            });
        }

        private async void dMPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await PerformPreview("DM", async (file, progress, token) =>
            {
                Preview p = new Preview();
                return await p.PreviewDM(file, progress, token);
            });
        }

        private async void nQPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await PerformPreview("NQ", async (file, progress, token) =>
            {
                Preview p = new Preview();
                return await p.PreviewNQ(file, progress, token);
            });
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 1. التحقق من وجود ملف أصلي
            if (string.IsNullOrEmpty(currentFile))
            {
                MessageBox.Show("لا يوجد ملف أصلي للرجوع إليه.", "تنبيه",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. التحقق من وجود الملف على القرص
            if (!File.Exists(currentFile))
            {
                MessageBox.Show("الملف الأصلي غير موجود.", "خطأ",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // 3. إيقاف التشغيل الحالي
                myPlayer.Stop();

                // 4. تحميل الملف الأصلي
                if (myPlayer.LoadFile(currentFile))
                {
                    // 5. تشغيل الملف الأصلي (اختياري)
                    myPlayer.Play();
                    btnPlayPause.Text = "⏸";

                    // 6. تحديث واجهة المستخدم

                    this.Text = $"مشغل الصوت - {Path.GetFileName(currentFile)}";

                    // 7. عرض معلومات الملف الأصلي (اختياري)
                    var loader = new LoadAudioInfo();
                    var info = loader.GetAudioInfo(currentFile);
                    DisplayAudioInfo(info);

                    // 8. تنظيف المعاينة (اختياري)
                    Preview.CleanupTempFile();

                    MessageBox.Show("تم الرجوع إلى الملف الأصلي بنجاح!", "إعادة تعيين",
                                   MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("فشل في تحميل الملف الأصلي.", "خطأ",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إعادة التعيين: {ex.Message}", "خطأ",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 1. التحقق من وجود معاينة
            if (!Preview.HasPreview || Preview.LastPreviewWav == null)
            {
                MessageBox.Show("لا توجد معاينة للحفظ. قم بإجراء معاينة أولاً (اختر ADM/DM/NQ Preview).",
                               "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. تحديد الامتداد واسم الخوارزمية
            string algorithm = Preview.LastPreviewAlgorithm;
            string extension = algorithm.ToLower();

            // 3. اختيار مكان الحفظ
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = $"{algorithm} Compressed File|*.{extension}";
            sfd.Title = $"حفظ المعاينة كملف {algorithm} مضغوط";
            sfd.FileName = $"preview_{algorithm}_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}";

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            // 4. نافذة التقدم
            ProgressWindow win = new ProgressWindow();
            win.Text = "جاري حفظ المعاينة...";
            win.Show();

            var progress = new Progress<SaveProgress>(p =>
            {
                if (win.lblStatus.InvokeRequired)
                {
                    win.lblStatus.Invoke(new Action(() => win.lblStatus.Text = p.Status));
                    win.progressBar.Invoke(new Action(() => win.progressBar.Value = p.Percentage));
                }
                else
                {
                    win.lblStatus.Text = p.Status;
                    win.progressBar.Value = p.Percentage;
                }
            });

            CancellationTokenSource cts = new CancellationTokenSource();
            win.btnCancel.Click += (s, ev) => cts.Cancel();

            try
            {
                // 5. تحويل WAV bytes إلى short[]
                short[] samples = ReadSamplesFromWav(Preview.LastPreviewWav);
                if (samples == null || samples.Length == 0)
                {
                    win.Close();
                    MessageBox.Show("فشل في قراءة بيانات المعاينة.", "خطأ",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 6. الضغط والحسب حسب الخوارزمية
                bool success = false;

                switch (algorithm)
                {
                    case "DM":
                        success = await SaveAsDM(samples, sfd.FileName, progress, cts.Token);
                        break;
                    case "NQ":
                        success = await SaveAsNQ(samples, sfd.FileName, progress, cts.Token);
                        break;
                    case "ADM":
                        success = await SaveAsADM(samples, sfd.FileName, progress, cts.Token);
                        break;
                    default:
                        MessageBox.Show($"خوارزمية غير معروفة: {algorithm}", "خطأ",
                                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }

                win.Close();

                if (success)
                {
                    MessageBox.Show($"✅ تم حفظ المعاينة كملف {algorithm} مضغوط بنجاح!\n{sfd.FileName}\n\n" +
                                   "لا يمكن فتح هذا الملف إلا من خلال برنامج فك الضغط.",
                                   "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (!cts.IsCancellationRequested)
                {
                    MessageBox.Show("فشل في حفظ المعاينة.", "خطأ",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                win.Close();
                MessageBox.Show($"خطأ: {ex.Message}", "خطأ",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ========== دوال الحفظ ==========

        private async Task<bool> SaveAsDM(short[] samples, string filePath, IProgress<SaveProgress> progress, CancellationToken token)
        {
            return await Task.Run(() =>
            {
                try
                {
                    progress?.Report(new SaveProgress { Status = "جاري ضغط DM...", Percentage = 50 });

                    DeltaCompressor compressor = new DeltaCompressor(stepSize: 25);
                    var compressed = compressor.Compress(samples);

                    if (compressed == null) return false;

                    using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(filePath)))
                    {
                        bw.Write(compressed.Value.firstSample);
                        bw.Write(compressed.Value.sampleCount);
                        foreach (var bit in compressed.Value.bits)
                            bw.Write((byte)bit);
                    }

                    progress?.Report(new SaveProgress { Status = "تم الحفظ", Percentage = 100 });
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في ضغط DM: {ex.Message}", "خطأ",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }, token);
        }

        private async Task<bool> SaveAsNQ(short[] samples, string filePath, IProgress<SaveProgress> progress, CancellationToken token)
        {
            return await Task.Run(() =>
            {
                try
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
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في ضغط NQ: {ex.Message}", "خطأ",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }, token);
        }

        private async Task<bool> SaveAsADM(short[] samples, string filePath, IProgress<SaveProgress> progress, CancellationToken token)
        {
            string tempWav = Path.GetTempFileName() + ".wav";
            string tempAdm = Path.GetTempFileName() + ".adm";

            try
            {
                // 1. حفظ العينات في ملف WAV مؤقت
                using (var writer = new WaveFileWriter(tempWav, new WaveFormat(44100, 16, 1)))
                {
                    foreach (short s in samples)
                        writer.WriteSample((float)s / 32768f);
                }

                progress?.Report(new SaveProgress { Status = "جاري ضغط ADM...", Percentage = 30 });

                token.ThrowIfCancellationRequested();

                // 2. ضغط ADM
                ADMCompressor compressor = new ADMCompressor();
                compressor.Token = token;
                compressor.Monitor = new PerformanceMonitor();

                var compressed = compressor.CompressFile(tempWav);
                if (compressed == null)
                {
                    MessageBox.Show("فشل ضغط ADM", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                progress?.Report(new SaveProgress { Status = "جاري حفظ الملف...", Percentage = 70 });

                // 3. حفظ بنفس التنسيق الناجح من PreviewADMInternal
                using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(filePath)))
                {
                    bw.Write(compressed.Value.firstSample);      // short
                    bw.Write(compressed.Value.sampleRate);       // int
                    bw.Write(compressed.Value.sampleCount);      // int
                    bw.Write(2);                                 // int step
                    bw.Write(compressed.Value.bitBytes.Length);  // int bitBytesLength
                    bw.Write(compressed.Value.bitBytes);         // byte[]
                }

                progress?.Report(new SaveProgress { Status = "تم الحفظ", Percentage = 100 });
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في ضغط ADM: {ex.Message}", "خطأ",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                try { if (File.Exists(tempWav)) File.Delete(tempWav); } catch { }
                try { if (File.Exists(tempAdm)) File.Delete(tempAdm); } catch { }
            }
        }
        // دالة قراءة العينات من WAV bytes
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
}