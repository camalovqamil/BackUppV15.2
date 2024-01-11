using BackUpp.DAL;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using System.Net.Http;
using System.Threading.Tasks;

namespace BackUpp
{
    public partial class Form1 : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public Form1()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;

            CreateFile();

            SelectModel();

            LoadModel();

            LoadHistory();
        }

        public string SendMessageText = string.Empty;

        public static string RootPath = "BackUppData";
        public string ModelFullPath { get; set; } = RootPath + @"\Model.txt";

        public static string HistoryFullPath = RootPath + @"\History.txt";

        public List<Model> models=new List<Model>();

        public IList<History> history = new List<History>();

        void CreateFile()
        {
            try
            {
                if (!Directory.Exists(RootPath))
                {
                    Directory.CreateDirectory(RootPath);
                }

                if (!File.Exists(ModelFullPath))
                {
                    File.Create(ModelFullPath);
                }

                if (!File.Exists(HistoryFullPath))
                {
                    File.Create(HistoryFullPath);
                }                           
            }
            catch (Exception xx)
            {
                XtraMessageBox.Show(xx.Message, "Xəta!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        void LoadModel()
        {
            models.Clear();

            string mod = string.Empty;
            using (var sr = new StreamReader(ModelFullPath))
            {
                var line = sr.ReadToEnd();
                mod = line;
            }

            if (!string.IsNullOrEmpty(mod))
            {
                mod = Password.Decrypt(mod);
            }

            models = JsonConvert.DeserializeObject<List<Model>>(mod);

            if (models == null)
            {
                models = new List<Model>();
            }


            drpModel.Properties.DataSource = models;
            drpModel.Properties.DisplayMember = "Name";
            drpModel.Properties.ValueMember = "Oid";
        }

        void LoadHistory()
        {
            try
            {


                history.Clear();

                string his = string.Empty;
                using (var sr = new StreamReader(HistoryFullPath))
                {
                    var line = sr.ReadToEnd();
                    his = line;
                }

                if (!string.IsNullOrEmpty(his))
                {
                    his = Password.Decrypt(his);
                }

                history = JsonConvert.DeserializeObject<List<History>>(his);

                if (history == null)
                {
                    history = new List<History>();
                }

                foreach (var item in history)
                {
                    Model m = models.Where(x => x.Oid == item.Model).FirstOrDefault();

                    item.CurrentModel = m;
                }

                gridControl1.DataSource = null;
                gridControl1.DataSource = history;
            }
            catch (Exception xx)
            {
                XtraMessageBox.Show("Load History Error: "+xx.Message, "Xəta", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void SelectModel()
        {
            txtAd.Text = string.Empty;
            txtHardan.Text = string.Empty;
            txtHara.Text = string.Empty;
            drpRejm.SelectedIndex = -1;
            txtRunTime.Text = string.Empty;
            txtStart.Text = string.Empty;
            txtEnd.Text = string.Empty;
            txtInterval.Value = txtInterval.Minimum;
            txtQeyd.Text = string.Empty;
            txtTel.Text = string.Empty;
            chckSendSMS.Checked = false;

            lytRunTime.Visibility = LayoutVisibility.Never;
            lytS.Visibility = LayoutVisibility.Never;
            lytE.Visibility = LayoutVisibility.Never;
            lytI.Visibility = LayoutVisibility.Never;


            if (drpModel.EditValue != null)
            {
                Model m = models.Where(x => x.Oid == drpModel.EditValue.ToString()).FirstOrDefault();
                if (m != null)
                {
                    txtAd.Text = m.Name;
                    txtHardan.Text = m.FromFolder;
                    txtHara.Text = m.ToFolder;
                    drpRejm.SelectedIndex = m.CopyType - 1;
                    txtRunTime.EditValue = m.RunTime;
                    txtStart.EditValue = m.StartTime;
                    txtEnd.EditValue = m.EndTame;
                    txtInterval.Value = m.Interval;
                    txtQeyd.Text = m.Note;
                    txtTel.Text = m.Telephone;
                    chckSendSMS.Checked = m.SendSMS;
                }
            }
        }

        bool CheckStart()
        {
            bool result = false;

            if (Directory.Exists(txtHara.Text) && Directory.Exists(txtHardan.Text))
                result = true;
            else
                XtraMessageBox.Show("Qovluqların yollarını yoxlayın, proqram başladıla bilmir!", "Diqqət", MessageBoxButtons.OK, MessageBoxIcon.Warning);    

            return result;
        }

        async void SendSMS()
        {
            if (!chckSendSMS.Checked)
            {
                return;
            }


            string username = "fabboya_api";
            string apiKey = "2FTSK0Gs";
            
            string senderName = "OMID";
            string messageText = SendMessageText;


            string[] parts = txtTel.Text.Split(';');

            foreach (var item in parts)
            {
                string recipientNumber = item;

                string apiUrl = $"https://gw.soft-line.az/sendsms?user={username}&password={apiKey}&gsm={recipientNumber}&from={senderName}&text={messageText}";

                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(apiUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        async Task BackupDiferensial()
        {
            try
            {
                List<FileInfo> CopyFiles = new List<FileInfo>();

                string[] files = Directory.GetFiles(txtHardan.Text, "*", SearchOption.AllDirectories);
                foreach (var item in files)
                {
                    FileInfo info = new FileInfo(item);
                    CopyFiles.Add(info);
                }

                if (chckDelete.Checked)
                {
                    DeleteFiles();
                }

                long CopySize = 0;
                foreach (var item in CopyFiles)
                {
                    string path = item.FullName.Replace(txtHardan.Text, txtHara.Text);

                    if (!File.Exists(path))
                    {
                        CopySize += item.Length;
                    }
                }

                DriveInfo dDrive = new DriveInfo(txtHara.Text);
                long freeSpace = dDrive.TotalFreeSpace;

                if (CopySize > freeSpace)
                {
                    SendMessageText = "Faylı kopyalamaq üçün diskinizdə yetəri qədər boş yer yoxdur!";

                    SaveHistory(0, DateTime.Now, DateTime.Now, false, SendMessageText);

                    SendSMS();

                    return;
                }


                DateTime start = DateTime.Now;
                long c_size = 0;

                foreach (var item in CopyFiles)
                {
                    if (File.Exists(item.FullName))
                    {

                        string path = item.FullName.Replace(txtHardan.Text, txtHara.Text);

                        string dic = item.Directory.FullName.Replace(txtHardan.Text, txtHara.Text);

                        if (!Directory.Exists(dic))
                        {
                            Directory.CreateDirectory(dic);
                        }

                        if (!File.Exists(path))
                        {
                            File.Copy(item.FullName, path, true);
                            c_size += item.Length;
                        }
                        else
                        {
                            FileInfo info = new FileInfo(path);
                            if (item.Length != info.Length)
                            {
                                if (chckFileReplace.Checked)
                                {
                                    File.Copy(item.FullName, path, true);
                                    c_size += item.Length;
                                }
                                else
                                {
                                    string hedefDosyaAdi = Path.GetFileNameWithoutExtension(path);
                                    string hedefDosyaUzantisi = Path.GetExtension(path);
                                    string yeniHedefDosyaYolu = Path.Combine(Path.GetDirectoryName(path), hedefDosyaAdi + "_Copy_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + hedefDosyaUzantisi);
                                    File.Copy(item.FullName, yeniHedefDosyaYolu);
                                    c_size += item.Length;
                                }
                            }
                        }
                    }
                }

                SendMessageText = "Fayllar uğurla kopyalandı.";

                if (c_size < 1024 && c_size > 0)
                {
                    SendMessageText += "(<1KB)";
                }

                c_size = c_size / 1024;
                SaveHistory((int)c_size, start, DateTime.Now, true, SendMessageText);

                SendSMS();

            }
            catch (Exception xx)
            {
                XtraMessageBox.Show("Copy File Error: " + xx.Message, "Xəta", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void DeleteFiles()
        {
            string[] files = Directory.GetFiles(txtHara.Text, "*", SearchOption.AllDirectories);
            foreach (var item in files)
            {
                FileInfo info = new FileInfo(item);
                if (info.LastWriteTime.Date.AddDays((int)txtSaveDays.Value)<=DateTime.Now.Date)
                {
                    File.Delete(info.FullName);
                }
            }
        }

        void SaveHistory(int size, DateTime start, DateTime end, bool status, string result)
        {
            try
            {
                History H = new History();
                Model m = models.Where(x => x.Oid == drpModel.EditValue.ToString()).FirstOrDefault();

                H.Model = m.Oid;
                H.CurrentModel = m;
                H.FileSize = size;
                H.StartDate = start;
                H.EndDate = end;
                H.CopyIsSuccess = status;
                H.Result = result;

                history.Add(H);

                FileInfo fi = new FileInfo(HistoryFullPath);
                using (TextWriter txtWriter = new StreamWriter(fi.Open(FileMode.Truncate)))
                {
                    string mod = JsonConvert.SerializeObject(history, Formatting.Indented);
                    mod = Password.Encrypt(mod);
                    txtWriter.Write(mod);
                }

                LoadHistory();
            }
            catch (Exception xx)
            {

            }
        }

        private void btnLisenziya_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            LicenceForm frm = new LicenceForm();
            frm.ShowDialog();
        }

        private void btnModel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ModelForm frm = new ModelForm(models);
            frm.ShowDialog();

            LoadModel();
        }

        private void btnStart_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (CheckStart())
            {
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                IsRuning = true;
                Task.Run(()=> RunProgram());
            }
        }

        private void drpModel_EditValueChanged(object sender, EventArgs e)
        {
            SelectModel();
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

        bool IsRuning = true;

        async void RunProgram()
        {
            XtraMessageBox.Show("Proqram uğurla başladıldı!", "Məlumat", MessageBoxButtons.OK, MessageBoxIcon.Information);

            while (IsRuning)
            {
                int sleep = 1;

                try
                {
                    DateTime nov = DateTime.Now;
                    DateTime RunTime = Convert.ToDateTime(txtRunTime.Text);


                    Model m = models.Where(x => x.Oid == drpModel.EditValue.ToString()).FirstOrDefault();
                    if (m != null)
                    {
                        if (m.CopyType == 1 && RunTime.Hour == nov.Hour && RunTime.Minute == nov.Minute)
                        {
                            //XtraMessageBox.Show("StartCopy", "Məlumat", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            await BackupDiferensial();

                            //XtraMessageBox.Show("EndCopy", "Məlumat", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            sleep = 60;
                        }
                        if (m.CopyType == 2)
                        {
                            sleep = 60 * (int)txtInterval.Value;

                            DateTime Start = Convert.ToDateTime(txtStart.Text);

                            DateTime End = Convert.ToDateTime(txtEnd.Text);

                            if (Start <= nov && nov <= End)
                            {
                                await BackupDiferensial();
                            }
                        }
                    }

                }
                catch (Exception xx)
                {
                    MessageBox.Show(xx.Message);
                }

                await Task.Delay(1000 * sleep);

                //Thread.Sleep(1000 * sleep);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            XtraMessageBox.Show("Proqram uğurla başladıldı!", "Məlumat", MessageBoxButtons.OK, MessageBoxIcon.Information);

            while (IsRuning)
            {
                int sleep = 1;

                try
                {
                    DateTime nov = DateTime.Now;
                    DateTime RunTime = Convert.ToDateTime(txtRunTime.Text);
                   

                    Model m = models.Where(x => x.Oid == drpModel.EditValue.ToString()).FirstOrDefault();
                    if (m != null)
                    {
                        if (m.CopyType == 1 && RunTime.Hour==nov.Hour && RunTime.Minute==nov.Minute)
                        {
                            BackupDiferensial();

                            sleep = 60;
                        }
                        if (m.CopyType == 2)
                        {
                            sleep = 60 * (int)txtInterval.Value;

                            DateTime Start = Convert.ToDateTime(txtStart.Text);

                            DateTime End = Convert.ToDateTime(txtEnd.Text);

                            if (Start <= nov && nov <= End)
                            {
                                BackupDiferensial();
                            }
                        }
                    }
                    
                }
                catch (Exception xx)
                {

                }
                

                Thread.Sleep(1000 * sleep);
            }
        }

        private void chckDelete_CheckedChanged(object sender, EventArgs e)
        {
            lytDays.Visibility = LayoutVisibility.Never;

            if (chckDelete.Checked)
            {
                lytDays.Visibility= LayoutVisibility.Always;
            }
        }

        private void btnStop_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            IsRuning = false;
            //backgroundWorker1.CancelAsync();
        }

        private void btnBackupp_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Backupp üçün qovluq seçin";
            folderBrowserDialog.ShowNewFolderButton = true; 

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedPath = folderBrowserDialog.SelectedPath;

                string[] files = Directory.GetFiles(RootPath, "*", SearchOption.AllDirectories);
                foreach (var item in files)
                {
                    FileInfo info = new FileInfo(item);
                    File.Copy(info.FullName, selectedPath + "\\" + info.Name, true);
                }

                XtraMessageBox.Show("Proqram faylları uğurla kopyalandı.", "Məlumat", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnRestore_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Restore üçün qovluq seçin";
            folderBrowserDialog.ShowNewFolderButton = true;

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedPath = folderBrowserDialog.SelectedPath;

                try
                {
                    FileInfo info = new FileInfo(selectedPath + "\\Model.txt");
                    File.Copy(info.FullName, RootPath + "\\" + info.Name, true);

                    info = new FileInfo(selectedPath + "\\History.txt");
                    File.Copy(info.FullName, RootPath + "\\" + info.Name, true);

                    XtraMessageBox.Show("Proqram faylları uğurla kopyalandı.", "Məlumat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception xx)
                {
                    XtraMessageBox.Show(xx.Message, "Xəta", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnHide_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Hide();
            notifyIcon1.Visible = true;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            notifyIcon1.Visible = false;
        }
    }
}
