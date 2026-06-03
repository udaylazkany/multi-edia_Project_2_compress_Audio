using NAudio.Wave;
using System.Windows.Forms;

namespace project_2
{
    public class AudioPlayerSimple
    {
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        private bool isPlaying = false;
        private Button playButton;
        private Form parentForm;  // لإضافة خاصية السحب والإفلات

        // تعديل Constructor ليستقبل الفورم أيضاً
        public AudioPlayerSimple(Button btn, Form form)
        {
            playButton = btn;
            playButton.Text = "▶";
            parentForm = form;

            // تفعيل السحب والإفلات على الفورم
            EnableDragDrop();
        }

        // تفعيل خاصية السحب والإفلات
        private void EnableDragDrop()
        {
            parentForm.AllowDrop = true;
            parentForm.DragEnter += ParentForm_DragEnter;
            parentForm.DragDrop += ParentForm_DragDrop;
        }

        // حدث عند سحب ملف فوق النافذة
        private void ParentForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string file = files[0];
                string extension = System.IO.Path.GetExtension(file).ToLower();

                // السماح فقط للملفات الصوتية
                if (extension == ".mp3" || extension == ".wav" || extension == ".m4a" || extension == ".wma")
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
        }

        // حدث عند إفلات الملف على النافذة
        private void ParentForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string filePath = files[0];

            // تحميل وتشغيل الملف
            if (LoadFile(filePath))
            {
                Play();
                parentForm.Text = $"مشغل الصوت - {System.IO.Path.GetFileName(filePath)}";
                var loader = new LoadAudioInfo();
                var info = loader.GetAudioInfo(filePath);
                ((Form1)parentForm).DisplayAudioInfo(info);
            }
            else
            {
                MessageBox.Show("لا يمكن تشغيل هذا الملف", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string OpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files|*.mp3|WAV files|*.wav|All files|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }
            return null;
        }

        public bool LoadFile(string filePath)
        {
            try
            {
                if (outputDevice != null)
                {
                    outputDevice.Stop();
                    outputDevice.Dispose();
                }
                if (audioFile != null)
                {
                    audioFile.Dispose();
                }

                outputDevice = new WaveOutEvent();
                audioFile = new AudioFileReader(filePath);
                outputDevice.Init(audioFile);
                isPlaying = false;
                playButton.Text = "▶";
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Play()
        {
            if (outputDevice != null)
            {
                outputDevice.Play();
                isPlaying = true;
                playButton.Text = "⏸";
            }
        }

        public void Stop()
        {
            if (outputDevice != null)
            {
                outputDevice.Stop();
                isPlaying = false;
                playButton.Text = "▶";
            }
        }

        public void Pause()
        {
            if (outputDevice != null)
            {
                outputDevice.Pause();
                isPlaying = false;
                playButton.Text = "▶";
            }
        }

        public void Toggle()
        {
            if (outputDevice == null)
            {
                MessageBox.Show("الرجاء تحميل ملف أولاً");
                return;
            }

            if (!isPlaying)
            {
                outputDevice.Play();
                isPlaying = true;
                playButton.Text = "⏸";
            }
            else
            {
                outputDevice.Pause();
                isPlaying = false;
                playButton.Text = "▶";
            }
        }

        public bool HasFileLoaded()
        {
            return outputDevice != null && audioFile != null;
        }
    }
}