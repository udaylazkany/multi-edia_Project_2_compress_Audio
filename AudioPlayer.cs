using NAudio.Wave;
using System;
using System.Windows.Forms;

namespace project_2
{
    public class AudioPlayer
    {
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        private Button playButton;
        private bool isPlaying = false;
        private string currentFilePath = "";

        // حدث يتم استدعاؤه عند تغيير حالة التشغيل
        public event Action<bool> PlayStateChanged;

        // حدث يتم استدعاؤه عند تحميل ملف جديد
        public event Action<string> FileLoaded;

        public AudioPlayer(Button playPauseButton)
        {
            playButton = playPauseButton;
            playButton.Text = "▶";
            playButton.Click += TogglePlayPause;
        }

        // فتح ملف صوتي
        public bool OpenFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "ملفات الصوت|*.mp3;*.wav;*.m4a;*.wma|MP3 files|*.mp3|WAV files|*.wav|All files|*.*";
                openFileDialog.Title = "اختر ملفاً صوتياً";
                openFileDialog.Multiselect = false;
                openFileDialog.CheckFileExists = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return LoadFile(openFileDialog.FileName);
                }
            }
            return false;
        }

        // تحميل ملف من مسار محدد
        public bool LoadFile(string filePath)
        {
            try
            {
                // إيقاف التشغيل الحالي
                Stop();

                // تحرير الموارد القديمة
                if (audioFile != null)
                {
                    audioFile.Dispose();
                    audioFile = null;
                }

                if (outputDevice != null)
                {
                    outputDevice.Dispose();
                    outputDevice = null;
                }

                // تحميل الملف الجديد
                audioFile = new AudioFileReader(filePath);
                outputDevice = new WaveOutEvent();
                outputDevice.Init(audioFile);

                currentFilePath = filePath;
                isPlaying = false;
                playButton.Text = "▶";

                // إشعار بتحميل الملف
                FileLoaded?.Invoke(System.IO.Path.GetFileName(filePath));

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الملف: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // تشغيل أو إيقاف مؤقت
        private void TogglePlayPause(object sender, EventArgs e)
        {
            if (outputDevice == null || audioFile == null)
            {
                MessageBox.Show("الرجاء اختيار ملف صوتي أولاً", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!isPlaying)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

        // تشغيل
        public void Play()
        {
            if (outputDevice != null && audioFile != null)
            {
                outputDevice.Play();
                isPlaying = true;
                playButton.Text = "⏸";
                PlayStateChanged?.Invoke(true);
            }
        }

        // إيقاف مؤقت
        public void Pause()
        {
            if (outputDevice != null)
            {
                outputDevice.Pause();
                isPlaying = false;
                playButton.Text = "▶";
                PlayStateChanged?.Invoke(false);
            }
        }

        // إيقاف كامل
        public void Stop()
        {
            if (outputDevice != null)
            {
                outputDevice.Stop();
                isPlaying = false;
                playButton.Text = "▶";
                PlayStateChanged?.Invoke(false);
            }
        }

        // إغلاق وتحرير الموارد
        public void Close()
        {
            Stop();

            if (audioFile != null)
            {
                audioFile.Dispose();
                audioFile = null;
            }

            if (outputDevice != null)
            {
                outputDevice.Dispose();
                outputDevice = null;
            }
        }

        // الحصول على حالة التشغيل
        public bool IsPlaying => isPlaying;

        // الحصول على اسم الملف الحالي
        public string CurrentFileName => System.IO.Path.GetFileName(currentFilePath);

        // الحصول على المسار الكامل للملف الحالي
        public string CurrentFilePath => currentFilePath;

        // التحقق من وجود ملف محمل
        public bool HasFileLoaded => audioFile != null && outputDevice != null;
    }
}