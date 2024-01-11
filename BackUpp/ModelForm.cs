using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraBars;
using BackUpp.DAL;
using DevExpress.XtraLayout.Utils;
using DevExpress.XtraEditors;
using System.IO;
using Newtonsoft.Json;

namespace BackUpp
{
    public partial class ModelForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public List<Model> models = new List<Model>();

        public ModelForm(List<Model> mods)
        {
            InitializeComponent();

            models = mods;

            gridControl1.DataSource = mods;

            drpRejm.SelectedIndex = 0;
        }

        private void drpRejm_SelectedIndexChanged(object sender, EventArgs e)
        {
            lytRunTime.Visibility = LayoutVisibility.Never;
            lytS.Visibility = LayoutVisibility.Never;
            lytE.Visibility = LayoutVisibility.Never;
            lytI.Visibility = LayoutVisibility.Never;

            if (drpRejm.SelectedIndex == 0)
            {
                lytRunTime.Visibility = LayoutVisibility.Always;
            }

            if (drpRejm.SelectedIndex == 1)
            {
                lytS.Visibility = LayoutVisibility.Always;
                lytE.Visibility = LayoutVisibility.Always;
                lytI.Visibility = LayoutVisibility.Always;
            }
        }

        private void btnSave_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (string.IsNullOrEmpty(txtAd.Text))
            {
                XtraMessageBox.Show("Modelin adın daxil edin!", "Diqqət!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtHardan.Text) || !Directory.Exists(txtHardan.Text))
            {
                XtraMessageBox.Show("Hansı qovluqdan xanasının düzgünlüyünü yoxlayın!", "Diqqət!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtHara.Text) || !Directory.Exists(txtHara.Text))
            {
                XtraMessageBox.Show("Hansı qovluğa xanasının düzgünlüyünü yoxlayın!", "Diqqət!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtStart.Text==txtEnd.Text && drpRejm.SelectedIndex==1)
            {
                XtraMessageBox.Show("Başlama və bitmə saatları eyni ola bilməz", "Diqqət!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtTel.Text) && chckSendSMS.Checked)
            {
                XtraMessageBox.Show("SMS bildirişi göndərmək üçün ən azı 1 nömrə daxil edin!", "Diqqət!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Model m = new Model();

            m.Name = txtAd.Text;
            m.FromFolder = txtHardan.Text;
            m.ToFolder = txtHara.Text;
            m.CopyType = drpRejm.SelectedIndex + 1;
            m.RunTime = txtRunTime.Text;
            m.StartTime = txtStart.Text;
            m.EndTame = txtEnd.Text;
            m.Interval = (int)txtInterval.Value;
            m.Note = txtQeyd.Text;
            m.Telephone = txtTel.Text;
            m.SendSMS = chckSendSMS.Checked;

            models.Add(m);
            string mod = string.Empty;
            Form1 frm = new Form1();
            FileInfo fi = new FileInfo(frm.ModelFullPath);

            using (TextWriter txtWriter = new StreamWriter(fi.Open(FileMode.Truncate)))
            {
                mod = JsonConvert.SerializeObject(models, Formatting.Indented);
                mod = Password.Encrypt(mod);
                txtWriter.Write(mod);
            }

            txtAd.Text=string.Empty;
            txtHardan.Text = string.Empty;
            txtHara.Text = string.Empty;
            drpRejm.SelectedIndex =0;
            txtRunTime.Text = string.Empty;
            txtStart.Text = string.Empty;
            txtEnd.Text = string.Empty;
            txtInterval.Value= txtInterval.Minimum;
            txtQeyd.Text = string.Empty;
            txtTel.Text = string.Empty;
            chckSendSMS.Checked=false;

            gridControl1.DataSource = null;
            gridControl1.DataSource = models;
        }

        private void barButtonItem3_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}