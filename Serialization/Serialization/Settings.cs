using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Serialization
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            GetValues();
        }

        private void GetValues()
        {
            ConfigurationManager.RefreshSection("appSettings");

            string connection = ConfigurationManager.AppSettings.Get("conn");

            if(connection!=null)
            {
                box.Text = connection;
            } else
            {
                MessageBox.Show("Prišlo je do napake...");
            }
        }

        private void save_Click(object sender, EventArgs e)
        {
            string edited = box.Text;

            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings["conn"].Value = edited;
            configuration.Save(ConfigurationSaveMode.Modified);
            Application.Restart();
        }
    }
}
