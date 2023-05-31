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
    public partial class Software : Form
    {

        public static string ConnectionString = @"";
        private SqlConnection conn;

        public Software()
        {
            InitializeComponent();
            ConfigurationManager.RefreshSection("appSettings");
            ConnectionString = ConfigurationManager.AppSettings.Get("conn");
            this.Text = "Serializacija";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Click += Button1_Click;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = openFileDialog1.FileName;
                try
                {
                    var updateBoolean = SeeIfItsAnUpdate(openFileDialog1.SafeFileName.Replace(" ", string.Empty).Replace(".xml", string.Empty));
                    if (updateBoolean)
                    {
                        MessageBox.Show("Ta analiza že obstaja. Pritisnite ok da jo posodobite.");
                        XDocument doc = XDocument.Load(openFileDialog1.FileName);
                        UpdateDashboard(openFileDialog1.SafeFileName.Replace(" ", string.Empty).Replace(".xml", string.Empty), doc);
                        MessageBox.Show("Uspešno posodobljen dokument");
                    }
                    else
                    {
                        MessageBox.Show("Ta analiza še ne obstaja. Pritisnite ok da se fajl serializira v bazo.");
                        XDocument doc = XDocument.Load(openFileDialog1.FileName);
                        AddDashboard(doc, openFileDialog1.SafeFileName.Replace(" ", string.Empty).Replace(".xml", string.Empty));
                        MessageBox.Show("Uspešno serializiran dokument");
                    }
                }
                catch (IOException)
                {
                    MessageBox.Show("Napaka");
                }
            }
        }

        private void UpdateDashboard(string name, XDocument document)
        {
            string stripped = String.Concat(name.ToString().Where(c => !Char.IsWhiteSpace(c))).Replace("-", "");
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                MemoryStream stream = new MemoryStream();
                document.Save(stream);
                stream.Position = 0;

                SqlCommand InsertCommand = new SqlCommand(
                    $"UPDATE Dashboards SET Dashboard = @Dashboard WHERE Caption = @Caption ");
                InsertCommand.Parameters.Add("Dashboard", SqlDbType.VarBinary).Value = stream.ToArray();
                InsertCommand.Parameters.Add("Caption", SqlDbType.VarChar).Value = stripped;

                InsertCommand.Connection = connection;
                InsertCommand.ExecuteNonQuery();
                connection.Close();
  
            }
        }

        private bool SeeIfItsAnUpdate(string name)
        {


            string stripped = String.Concat(name.ToString().Where(c => !Char.IsWhiteSpace(c))).Replace("-", "");
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();
                    // Create SqlCommand to select pwd field from users table given supplied userName.
                    var sqlCmd = new SqlCommand($"select count(*) as singular from Dashboards where Caption = '{stripped}';", conn); /// Intepolation or the F string. C# > 5.0       
                    // Execute command and fetch pwd field into lookupPassword string.
                    try
                    {
                        int ok = (int)sqlCmd.ExecuteScalar();
                        if (ok == 1) { return true; } else { return false; }

                    } catch
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
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
            catch (Exception error)
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
