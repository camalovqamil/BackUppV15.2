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
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using DevExpress.XtraEditors;

namespace BackUpp
{
    public partial class LicenceForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public LicenceForm()
        {
            InitializeComponent();
        }

        private void btnAddLicence_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                string bilgisayarAdi = Environment.MachineName;
                string macAdres = GetMacAddress();
                DateTime tarih = DateTime.Now.Date;

                string guid = GenerateGuid(bilgisayarAdi, macAdres, tarih);

                if (guid == txtKey.Text.Trim())
                {
                    string path = "Licence.txt";
                }
                else
                {
                    XtraMessageBox.Show("Lisenziya alına bilmədi, KEY düzgün deyil!", "Xəta!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch (Exception xx)
            {
                XtraMessageBox.Show(xx.Message, "Xəta!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static string GetMacAddress()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            string macAddress = "";
            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    macAddress = networkInterface.GetPhysicalAddress().ToString();
                    break;
                }
            }
            return macAddress;
        }

        static string GenerateGuid(string computerName, string macAddress, DateTime date)
        {
            string inputText = $"{computerName}-{macAddress}-{date.ToString()}";

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputText));

                Guid generatedGuid = new Guid(hashedBytes.Take(16).ToArray());

                return generatedGuid.ToString();
            }
        }
    }
}