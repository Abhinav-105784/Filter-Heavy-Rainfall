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
        }

        private void BrowseASCFILES_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog file = new OpenFileDialog())
            {
                file.Filter = "ASC Raster Files | *.asc";
                file.Title = "Select the ASC Data File";
                file.Multiselect = true;

                if (file.ShowDialog() == DialogResult.OK)
                {
                    ASCFILES.Items.Add(file.FileNames);
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

        }

        private void Close_Click(object sender, EventArgs e)
        {

        }

        private void ASCFILES_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void PolygonFIle_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
