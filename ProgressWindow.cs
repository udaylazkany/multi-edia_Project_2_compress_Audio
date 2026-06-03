using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace project_2
{
    public partial class ProgressWindow : Form
    {
        public Button btnCancel;
        public ProgressBar progressBar;
        public Label lblStatus;

        public Chart chartSpeed;
        public Chart chartRatio;

        // 🔥 عدّاد X لكل رسم
        private Dictionary<Chart, int> xCounters = new Dictionary<Chart, int>();

        public ProgressWindow()
        {
            InitializeComponent();

            this.Text = "جاري التنفيذ...";
            this.Size = new System.Drawing.Size(700, 500);

            progressBar = new ProgressBar();
            progressBar.Location = new System.Drawing.Point(20, 20);
            progressBar.Size = new System.Drawing.Size(640, 25);
            this.Controls.Add(progressBar);

            lblStatus = new Label();
            lblStatus.Location = new System.Drawing.Point(20, 55);
            lblStatus.Size = new System.Drawing.Size(640, 30);
            lblStatus.Text = "الحالة: بدء العملية...";
            this.Controls.Add(lblStatus);

            btnCancel = new Button();
            btnCancel.Text = "إلغاء";
            btnCancel.Location = new System.Drawing.Point(20, 420);
            btnCancel.Size = new System.Drawing.Size(100, 30);
            this.Controls.Add(btnCancel);

            // 🔥 رسم سرعة المعالجة
            chartSpeed = CreateStyledChart("سرعة المعالجة (عينات/ثانية)");
            chartSpeed.Location = new System.Drawing.Point(20, 100);
            this.Controls.Add(chartSpeed);
            xCounters[chartSpeed] = 0;

            // 🔥 رسم نسبة الضغط
            chartRatio = CreateStyledChart("نسبة الضغط (%)");
            chartRatio.Location = new System.Drawing.Point(20, 260);
            this.Controls.Add(chartRatio);
            xCounters[chartRatio] = 0;
        }

        private Chart CreateStyledChart(string title)
        {
            Chart chart = new Chart();
            chart.Size = new System.Drawing.Size(640, 140);

            chart.AntiAliasing = AntiAliasingStyles.All;
            chart.TextAntiAliasingQuality = TextAntiAliasingQuality.High;

            ChartArea area = new ChartArea("area");
            area.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
            area.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            area.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            area.AxisX.LineWidth = 2;
            area.AxisY.LineWidth = 2;
            chart.ChartAreas.Add(area);

            Series s = new Series("data");
            s.ChartType = SeriesChartType.Line;
            s.BorderWidth = 3;
            s.Color = System.Drawing.Color.FromArgb(52, 152, 219);
            s.MarkerStyle = MarkerStyle.Circle;
            s.MarkerSize = 4;
            s.MarkerColor = System.Drawing.Color.FromArgb(41, 128, 185);
            s["LineTension"] = "0.5";
            chart.Series.Add(s);

            Title t = new Title(title);
            t.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            chart.Titles.Add(t);

            return chart;
        }

        public void AddSpeedPoint(double speed)
        {
            AddPoint(chartSpeed, speed);
        }

        public void AddRatioPoint(double ratio)
        {
            AddPoint(chartRatio, ratio);
        }

        private void AddPoint(Chart chart, double value)
        {
            if (chart.InvokeRequired)
            {
                chart.Invoke(new Action(() => AddPoint(chart, value)));
                return;
            }

            var s = chart.Series["data"];

            // 🔥 عدّاد X الخاص بهذا الرسم
            int x = xCounters[chart];

            s.Points.AddXY(x, value);

            // زيادة عدّاد X
            xCounters[chart] = x + 1;

            // إزالة النقاط القديمة
            if (s.Points.Count > 50)
                s.Points.RemoveAt(0);

            // تحديث محور X
            var area = chart.ChartAreas["area"];
            area.AxisX.Minimum = s.Points[0].XValue;
            area.AxisX.Maximum = s.Points[s.Points.Count - 1].XValue;

            chart.ChartAreas[0].RecalculateAxesScale();
        }
    }
}
