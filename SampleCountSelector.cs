using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace project_2
{
    public partial class SampleCountSelector : Form
    {
        public int SelectedCount { get; private set; }

        public SampleCountSelector(List<int> options)
        {
            InitializeComponent();

            ComboBox combo = new ComboBox();
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.Location = new Point(20, 20);
            combo.Width = 150;

            foreach (var n in options)
                combo.Items.Add(n == -1 ? "All" : n.ToString());

            combo.SelectedIndex = 0;
            this.Controls.Add(combo);

            Button ok = new Button();
            ok.Text = "موافق";
            ok.Location = new Point(20, 60);
            ok.Click += (s, e) =>
            {
                if (combo.SelectedItem.ToString() == "All")
                    SelectedCount = -1;
                else
                    SelectedCount = int.Parse(combo.SelectedItem.ToString());

                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(ok);

            this.Text = "اختيار عدد العينات";
            this.Size = new Size(220, 150);
        }
    }
}
