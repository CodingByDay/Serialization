using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Serialization
{
    public partial class Form1 : Form
    {

        public static string ConnectionString = @"";
        private SqlConnection conn;

        public Form1()
        {
            this.Name = "Prenos na centralni sistem.";
            InitializeComponent();
            ConfigurationManager.RefreshSection("appSettings");
            ConnectionString = ConfigurationManager.AppSettings.Get("conn");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Click += Button1_Click;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            int size = -1;
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) 
            {
                string file = openFileDialog1.FileName;
                try
                {
                    MessageBox.Show("Pritisnite ok da se fajl serializira v bazo.");
                    XDocument doc = XDocument.Load(openFileDialog1.FileName);
                    AddDashboard(doc, openFileDialog1.SafeFileName.Replace(" ", string.Empty).Replace(".xml", string.Empty));
                    MessageBox.Show("Uspešno serializiran dokument");                   
                }
                catch (IOException)
                {
                }
            }
            Console.WriteLine(size); // <-- Shows file size in debugging mode.
            Console.WriteLine(result); // <-- For debugging use.
        }


        private void InsertPermision(string dashboardName)
        {
            conn = new SqlConnection(ConnectionString);
            conn.Open();
            SqlCommand cmd = new SqlCommand($"ALTER TABLE permisions_user ADD {dashboardName} int not null default(0);", conn);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch 
            {
            }
            cmd.Dispose();
            conn.Close();
        }
        public string AddDashboard(XDocument document, string dashboardName)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                MemoryStream stream = new MemoryStream();
                document.Save(stream);
                stream.Position = 0;

                SqlCommand InsertCommand = new SqlCommand(
                    "INSERT INTO Dashboards (Dashboard, Caption) " +
                    "output INSERTED.ID " +
                    "VALUES (@Dashboard, @Caption)");
                string stripped = String.Concat(dashboardName.ToString().Where(c => !Char.IsWhiteSpace(c))).Replace("-", "");
                InsertCommand.Parameters.Add("Caption", SqlDbType.NVarChar).Value = stripped.Replace(".xml", string.Empty);
                InsertCommand.Parameters.Add("Dashboard", SqlDbType.VarBinary).Value = stream.ToArray();
                InsertCommand.Connection = connection;
                string ID = InsertCommand.ExecuteScalar().ToString();
                connection.Close();
                InsertPermision(stripped.Replace(".xml", string.Empty));
                return ID;
            }
        }

        private void bazaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Settings
            Form form = new Settings();

            form.ShowDialog();
        }

        
    }
}
