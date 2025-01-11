using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OpenDental
{
    public class TableTimeBar : ContrTable
    {
        private IContainer components = null;
        
        public TableTimeBar()
        {
            InitializeComponent();
            MaxRows = 40;
            MaxCols = 1;
            ShowScroll = false;
            FieldsArePresent = false;
            HeadingIsPresent = false;
            InstantClassesPar();
            SetRowHeight(0, 39, 14);
            ColWidth[0] = 13;
            ColAlign[0] = HorizontalAlignment.Center;
            SetGridColor(Color.LightGray);
            /*TopBorder[0,6]=Color.Black;
            TopBorder[0,12]=Color.Black;
            TopBorder[0,18]=Color.Black;
            TopBorder[0,24]=Color.Black;
            TopBorder[0,30]=Color.Black;
            TopBorder[0,36]=Color.Black;*/
            LayoutTables();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #region Designer generated code

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // TableTimeBar
            // 
            this.Name = "TableTimeBar";
            this.Load += new System.EventHandler(this.TableTimeBar_Load);
            this.ResumeLayout(false);
        }

        #endregion

        private void TableTimeBar_Load(object sender, EventArgs e)
        {
            LayoutTables();
        }
    }
}