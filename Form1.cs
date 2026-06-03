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
            lblStatus = new Label();
            lblStatus.Name = "lblStatus";
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(10, 230);
            lblStatus.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblStatus.Text = "الحالة: جاهز";
            this.Controls.Add(lblStatus);
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
        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}