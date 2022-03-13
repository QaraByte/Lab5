using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace WindowsFormsApp5
{
    public partial class Form1 : Form
    {

        private SQLiteConnection SQLiteConn;
        private DataTable dTable;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SQLiteConn = new SQLiteConnection();
            dTable = new DataTable();
        }
        private bool OpenDBFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.Filter = "Текстовые файлы (*.sqlite)|*.sqlite| Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog(this)==DialogResult.OK)
            {
                SQLiteConn = new SQLiteConnection("Data Source =" + openFileDialog.FileName + ";Version = 3;");
                SQLiteConn.Open();
                SQLiteCommand command = new SQLiteCommand();
                command.Connection = SQLiteConn;
                return true;
            }
            else return false;
        }
        private void ShowTable()
        {
            string SQLQue;
            string SQLQuery = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name;";
            SQLiteCommand command = new SQLiteCommand(SQLQuery, SQLiteConn);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                SQLQue = "SELECT * FROM [" + reader[0].ToString() + "] order by 1";

                dTable.Clear();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(SQLQue, SQLiteConn);
                adapter.Fill(dTable);

                dataGridView1.Columns.Clear();
                dataGridView1.Rows.Clear();

                for (int col = 0; col < dTable.Columns.Count; col++)
                {
                    string ColName = dTable.Columns[col].ColumnName;
                    dataGridView1.Columns.Add(ColName, ColName);
                    dataGridView1.Columns[col].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
                for (int row = 0; row < dTable.Rows.Count; row++)
                {
                    dataGridView1.Rows.Add(dTable.Rows[row].ItemArray);
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (OpenDBFile() == true)
            {
                ShowTable();
            }
        }
        private double MaxValue()
        {

            double value1, value2,value,max=0;
            int i = 0, j=1;
            while (j< dTable.Rows.Count)
            {
                value1 = Convert.ToDouble(dTable.Rows[i].ItemArray[1]);
                value2 = Convert.ToDouble(dTable.Rows[j].ItemArray[1]);
                value = Math.Abs(value1 - value2);
                if (value > max) { max = value; }
                i++;
                j = i + 1;
            }
            return max;

        }
        private void button2_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            double rmax,rmin;
            rmax = Convert.ToDouble(dTable.Rows[0].ItemArray[1])+MaxValue();
            rmin = Convert.ToDouble(dTable.Rows[0].ItemArray[1]) - MaxValue();
            Random random = new Random();
            double r;
            List<string> values = new List<string>();
            string SQLQuery = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name;";
            SQLiteCommand command = new SQLiteCommand(SQLQuery, SQLiteConn);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            { string sqlBegin = "INSERT INTO ["+reader[0].ToString()+"] (";
                string sqlBody = "";
                for (int i = 0; i < dTable.Columns.Count; i++)
                {
                    r = random.NextDouble() * (rmax - rmin) + rmin;
                    if (i == 0) values.Add((dTable.Rows.Count).ToString());
                    else values.Add(Math.Round(r, 4).ToString());
                    if (i != 0 && i >= dTable.Columns.Count - 1)
                        sqlBody += "\"" + dTable.Columns[i].ColumnName + "\")";
                    else sqlBody += "\"" + dTable.Columns[i].ColumnName + "\",";
                }

                try
                {
                    SQLiteCommand com = new SQLiteCommand(SQLiteConn);
                    com.CommandText = sqlBegin + sqlBody + "VALUES (" + String.Join(",", values) + ")";
                    com.ExecuteNonQuery();

                    ShowTable();
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                } 
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string SQLQuery = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name;";
            SQLiteCommand command = new SQLiteCommand(SQLQuery, SQLiteConn);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                    SQLiteCommand sqlEnd = new SQLiteCommand(SQLiteConn);
                    sqlEnd.CommandText = "DELETE  FROM [" + reader[0].ToString() + "] WHERE Эпоха  = \"" + (dTable.Rows.Count - 1).ToString() + "\";";
                    sqlEnd.ExecuteNonQuery();
                    ShowTable();

            }
        }

        private void btnPutImg_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter= "jpg files (*.jpg)|*.jpg|All files (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string sql = "INSERT INTO [test] (img) values ('img\\camry10.jpg')";

                try
                {
                    Image img = Image.FromFile(dlg.FileName);
                    byte[] photo = ImageToByteArray(img);

                    SQLiteCommand com = new SQLiteCommand(SQLiteConn);
                    com.CommandText = "INSERT INTO test (img) VALUES (@photo)";
                    com.Parameters.Add("@photo", DbType.Binary, 20).Value = photo;
                    com.ExecuteNonQuery();


                    string SQLQuery = "SELECT img FROM test WHERE id=1;";
                    SQLiteCommand command = new SQLiteCommand(SQLQuery, SQLiteConn);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] buffer = GetBytes(reader);

                            using (var ms = new MemoryStream(buffer))
                            {
                                pictureBox1.Image =Image.FromStream(ms);
                            }
                        }
                    }
                    SQLiteConn.Close();
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }

        static byte[] GetBytes(SQLiteDataReader reader)
        {
            const int CHUNK_SIZE = 2 * 1024;
            byte[] buffer = new byte[CHUNK_SIZE];
            long bytesRead;
            long fieldOffset = 0;
            using (MemoryStream stream = new MemoryStream())
            {
                while ((bytesRead = reader.GetBytes(0, fieldOffset, buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, (int)bytesRead);
                    fieldOffset += bytesRead;
                }
                return stream.ToArray();
            }
        }
    }
}
