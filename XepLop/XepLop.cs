using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using CDTDatabase;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;

namespace XepLop
{
    public class XepLop : ICData
    {
        private DataCustomData _data;
        private InfoCustomData _info = new InfoCustomData(IDataType.Single);
        private Database db;

        #region ICData Members

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public void ExecuteAfter()
        {
            db = _data.DbData;
            //kiem tra co chon lop khong?
            DataView dv = new DataView(_data.DsData.Tables[0]);
            dv.RowFilter = "Chon = 1";
            if (dv.Count == 0)
                return;
            //hien form lay ma lop - neu bo qua thi khong cho luu du lieu
            FrmChonLop frm = new FrmChonLop();
            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                XtraMessageBox.Show("Danh sách học viên đã chọn chưa được xếp lớp,\nvì vậy số liệu này vẫn chưa lưu",
                    Config.GetValue("PackageName").ToString());
                _info.Result = false;
                return;
            }
            //insert vao bang hoc vien dang ky 
            //tim nguon phu hop
            DataRow drLop = frm.DrLop;
            string malop = drLop["MaLop"].ToString();
            string manlop = drLop["MaNLop"].ToString();
            string macn = drLop["MaCN"].ToString();
            string magh = drLop["MaGioHoc"].ToString();
            string sb = drLop["SoBuoi"].ToString();
            DateTime ngaydk = frm.NgayDK;
            DataTable dt = TaoBang();
            foreach (DataRowView drv in dv)
            { 
                string hvtvid = drv["MaHV"].ToString();
                string cndk = drv["MaCN"].ToString();
                DataRow drNguon = NguonHV(hvtvid);
                int nguon = 0;
                decimal tienbl = 0;
                decimal tiencl = 0;
                if (drNguon != null)
                {
                    tienbl = decimal.Parse(drNguon["BLSoTien"].ToString());
                    tiencl = decimal.Parse(drNguon["ConLai"].ToString());
                    if (tienbl == 0)
                        nguon = 1;
                    else
                        nguon = 2;
                }
                DataRow dr = dt.NewRow();
                dr["NgayDK"] = ngaydk;
                dr["HVTVID"] = hvtvid;
                dr["MaLop"] = malop;
                dr["MaNLop"] = manlop;
                dr["MaGioHoc"] = magh;
                dr["SoBuoi"] = sb;
                dr["MaCNDK"] = cndk;
                dr["MaCNHoc"] = macn;
                dr["TongHP"] = drv["HocPhi"];
                dr["KhuyenHoc"] = drv["KhuyenHoc"];
                dr["GiamHP"] = drv["TLGiam"];
                dr["TienHP"] = drv["HPThuc"];
                dr["ConLaiNL"] = drv["ConNo"];
                dr["NguonHV"] = nguon;
                if (drNguon != null)
                    dr["MaHVDK"] = drNguon["MaHV"];
                dr["ConLai"] = tiencl;
                dr["BLSoTien"] = tienbl;
                dr["SoPT"] = db.GetValue("select SoPT from MTNL where MTNLID = '" + drv["HVID"].ToString() + "'");
                dr["MTNLID"] = drv["HVID"];
                dt.Rows.Add(dr);
            }
            FrmDSDK frmdk = new FrmDSDK(dt, _data);
            if (frmdk.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                XtraMessageBox.Show("Danh sách học viên đã chọn chưa được xếp lớp,\nvì vậy số liệu này vẫn chưa lưu",
                    Config.GetValue("PackageName").ToString());
                _info.Result = false;
                return;
            }
            //tu dong xoa ra khoi dsdata
            for (int i = dv.Count - 1; i >= 0; i--)
                dv[i].Row.Delete();
            _data.DsData.AcceptChanges();
        }

        private DataRow NguonHV(string hvtvid)
        {
            DataTable dt = db.GetDataTable("select MaHV, ConLai, BLSoTien from MTDK where HVTVID = " + hvtvid + " order by NgayDK desc");
            if (dt.Rows.Count == 0)
                return null;
            return dt.Rows[0];
        }

        public void ExecuteBefore()
        {
        }

        private DataTable TaoBang()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("MTNLID", typeof(Guid));
            dt.Columns.Add("NgayDK", typeof(DateTime));
            dt.Columns.Add("HVTVID", typeof(Int32));
            dt.Columns.Add("MaLop");
            dt.Columns.Add("MaNLop");
            dt.Columns.Add("MaGioHoc");
            dt.Columns.Add("MaCNDK");
            dt.Columns.Add("MaCNHoc");
            dt.Columns.Add("TongHP", typeof(Decimal));
            dt.Columns.Add("KhuyenHoc", typeof(Int32));
            dt.Columns.Add("GiamHP", typeof(Decimal));
            dt.Columns.Add("TienHP", typeof(Decimal));
            dt.Columns.Add("ConLaiNL", typeof(Decimal));
            dt.Columns.Add("ThucThu", typeof(Decimal), "TienHP - ConLaiNL");
            dt.Columns.Add("NguonHV", typeof(Int32));
            dt.Columns.Add("MaHVDK");
            dt.Columns.Add("SoPT");
            dt.Columns.Add("ConLai", typeof(Decimal));
            dt.Columns.Add("BLSoTien", typeof(Decimal));
            dt.Columns.Add("SoBuoi", typeof(Decimal));
            dt.Columns.Add("PhaiNop", typeof(Decimal), "ConLaiNL - BLSoTien + ConLai");
            return dt;
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
