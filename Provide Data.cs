using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Filtering_Rainfall_Asc
{
    public partial class Provide_Data : Form
    {
        public Provide_Data()
        {
            InitializeComponent();
            textBox1.Text = "3";
        }

        private void BrowseASCFILES_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog file = new OpenFileDialog())
            {
                file.Filter = "Raster Files(*.tif) | *.tif";
                file.Title = "Select the Raster Data Files";
                file.Multiselect = true;

                if (file.ShowDialog() == DialogResult.OK)
                {
                    foreach (string s in file.FileNames) 
                    {
                        ASCFILES.Items.Add(s);
                    }
                }
            }
        }

        private void BrowsePolygon_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog file = new OpenFileDialog())
            {
                file.Filter = "Polygon Shape File | *.shp";
                file.Title = "Select the Polygon shapefile";

                if (file.ShowDialog() == DialogResult.OK)
                {
                    PolygonFIle.Text = file.FileName;
                }
            }
        }

        private void Run_Click(object sender, EventArgs e)
        {
            if(!int.TryParse(textBox1.Text, out int number) || int.Parse(textBox1.Text) > ASCFILES.Items.Count)
            {
                MessageBox.Show("Please give a valid number for filtering observations");
                return;
            }
            if (!double.TryParse(Duration.Text, out double durationTime))
            {
                MessageBox.Show("Please give a valid duration for filtering observations in hours");
                return;
            }
            if(PolygonFIle.Text == string.Empty)
            {
                MessageBox.Show("Please provide a valid Polygon shape file");
                return;
            }

            if(ASCFILES.Items.Count == 0)
            {
                MessageBox.Show("Please provide some raster files to begin");
                return;
            }
            List<string> list = new List<string>();
              foreach(var item in ASCFILES.Items)
            {
                list.Add(item.ToString());
            }
            Process_Files.Process(list, PolygonFIle.Text, int.Parse(textBox1.Text), double.Parse(Duration.Text));
            Close();
        }

        private void Close_Click(object sender, EventArgs e)
        {
            this.FindForm()?.Close();
        }

        private void ASCFILES_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void PolygonFIle_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void Duration_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
