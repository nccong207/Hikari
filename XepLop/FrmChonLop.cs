using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTDatabase;
using CDTLib;

namespace XepLop
{
    public partial class FrmChonLop : DevExpress.XtraEditors.XtraForm
    {
        private Database db = Database.NewDataDatabase();
        public DataRow DrLop = null;
        public DateTime NgayDK;

        public FrmChonLop()
        {
            InitializeComponent();
        }

        private void FrmChonLop_Load(object sender, EventArgs e)
        {
            deNgayDK.DateTime = DateTime.Today;

            gluGioHoc.Properties.DataSource = db.GetDataTable("select MaGioHoc, DienGiai, SoTiet from DMNgayGioHoc");
            gluGioHoc.Properties.DisplayMember = "MaGioHoc";
            gluGioHoc.Properties.ValueMember = "MaGioHoc";
            gluGioHoc.Properties.View.BestFitColumns();

            gluLopHoc.Properties.DataSource = db.GetDataTable("select MaCN, MaLop, TenLop, MaNLop, MaGioHoc, NgayBDKhoa, NgayKTKhoa, SoBuoi from DMLopHoc where isKT = 0");
            gluLopHoc.Properties.DisplayMember = "MaLop";
            gluLopHoc.Properties.ValueMember = "MaLop";
            gluLopHoc.Properties.View.Columns["NgayBDKhoa"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            gluLopHoc.Properties.View.Columns["NgayBDKhoa"].DisplayFormat.FormatString = "dd/MM/yyyy";
            gluLopHoc.Properties.View.Columns["NgayKTKhoa"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            gluLopHoc.Properties.View.Columns["NgayKTKhoa"].DisplayFormat.FormatString = "dd/MM/yyyy";
            gluLopHoc.Properties.View.Columns["SoBuoi"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gluLopHoc.Properties.View.Columns["SoBuoi"].DisplayFormat.FormatString = "##0";
            gluLopHoc.Properties.PopupFormMinSize = new Size(700, 300);
            gluLopHoc.Properties.View.BestFitColumns();

            gluNLop.Properties.DataSource = db.GetDataTable("select MaNLop, TenNLop, SoBuoi from DMNhomLop");
            gluNLop.Properties.DisplayMember = "MaNLop";
            gluNLop.Properties.ValueMember = "TenNLop";
            gluNLop.Properties.View.BestFitColumns();
        }

        private void gluLopHoc_Popup(object sender, EventArgs e)
        {
            gluLopHoc.Properties.View.ClearColumnsFilter();
            string s = "";
            if (!ceKhacCN.Checked)
                s += "MaCN = '" + Config.GetValue("MaCN").ToString() + "'";
            if (gluNLop.Text != "")
                s += (s == "" ? "" : " and ") + "MaNLop = '" + gluNLop.Text + "'";
            if (gluGioHoc.Text != "")
                s += (s == "" ? "" : " and ") + "MaGioHoc = '" + gluGioHoc.Text + "'";
            if (s != "")
                gluLopHoc.Properties.View.ActiveFilterString = s;
        }

        private void btnXepLop_Click(object sender, EventArgs e)
        {
            if (gluLopHoc.Text == "")
            {
                XtraMessageBox.Show("Chưa chọn lớp học", Config.GetValue("PackageName").ToString());
                return;
            }
            if (deNgayDK.EditValue == null)
            {
                XtraMessageBox.Show("Chưa nhập ngày xếp lớp", Config.GetValue("PackageName").ToString());
                return;
            }
            DataTable dt = gluLopHoc.Properties.DataSource as DataTable;
            DataRow[] drs = dt.Select("MaLop = '" + gluLopHoc.Text + "'");
            if (drs.Length == 0)
            {
                XtraMessageBox.Show("Chọn lớp học chưa đúng", Config.GetValue("PackageName").ToString());
                return;
            }
            DrLop = drs[0];
            NgayDK = deNgayDK.DateTime;
            this.DialogResult = DialogResult.OK;
        }

        private void btnBoQua_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}