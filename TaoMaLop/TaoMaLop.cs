using System;
using System.Collections.Generic;
using System.Text;
using DevExpress;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using Plugins;
using CDTDatabase;
using CDTLib;
using System.Windows.Forms;
using System.Data;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Utils;
using DevExpress.XtraGrid.Views.Grid;

namespace TaoMaLop
{
    public class TaoMaLop : ICControl
    {
        #region ICControl Members

        private InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();
        private int getSTT = 1;
        LayoutControl lc;
        GridView gvNgayNghi;
        CheckEdit chkIsCT;
        DataTable dtCTGioHoc = new DataTable();
        DataTable dtNgayNghi = new DataTable();
        DataRow drMaster;
        DateEdit dateNgayBD;
        DateEdit dateNgayKT;

        public void AddEvent()
        {
            if (data.BsMain == null)
                return;
            TextEdit teMaLop = data.FrmMain.Controls.Find("MaLop", true)[0] as TextEdit;
            teMaLop.KeyPress += new KeyPressEventHandler(teMaLop_KeyPress);
            lc = data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            drMaster = (data.BsMain.Current as DataRowView).Row;
            //// Nút tính ngày kết thúc
            //SimpleButton btnNgayKT = new SimpleButton();
            //btnNgayKT.Name = "btnNgayKT";   //phai co name cua control
            //btnNgayKT.Text = "Tính ngày KT";
            //LayoutControlItem lci1 = lc.AddItem("", btnNgayKT);
            //lci1.Name = "cusNgayKT"; //phai co name cua item, bat buoc phai co "cus" phai truoc
            //btnNgayKT.Click += new EventHandler(btnNgayKT_Click);
            // -----------------------
            // Nút lấy dữ liệu ngày nghỉ
            SimpleButton btnNapDL = new SimpleButton();
            btnNapDL.Name = "btnNapDL";   //phai co name cua control
            btnNapDL.Text = "Nạp lịch nghỉ";
            LayoutControlItem lci2 = lc.AddItem("", btnNapDL);
            lci2.Name = "cusNapDL"; //phai co name cua item, bat buoc phai co "cus" phai truoc
            btnNapDL.Click += new EventHandler(btnNapDL_Click);
            // -----------------------

            if (drMaster == null)
                return;
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            data.FrmMain.Shown += new EventHandler(FrmMain_Shown);
            chkIsCT = data.FrmMain.Controls.Find("IsCT", true)[0] as CheckEdit;
            if (chkIsCT != null)
                chkIsCT.EditValueChanged += new EventHandler(chkIsCT_EditValueChanged);

            dateNgayKT = data.FrmMain.Controls.Find("NgayKTKhoa", true)[0] as DateEdit;
            dateNgayBD = data.FrmMain.Controls.Find("NgayBDKhoa", true)[0] as DateEdit;
            gvNgayNghi = (data.FrmMain.Controls.Find("TLNgayNghiLop", true)[0] as GridControl).MainView as GridView;

            data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(data.BsMain, new EventArgs());
        }

        void teMaLop_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
            if (!(sender as TextEdit).Properties.ReadOnly)
                lc.FocusHelper.GetNextControl(sender).Focus();
        }

        void chkIsCT_EditValueChanged(object sender, EventArgs e)
        {
            CheckEdit chkEdit = sender as CheckEdit;
            if (chkEdit.Checked)
            {
                lc.Items.FindByName("lciDiaDiem").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciNgayHD").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciNgayKTHD").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciGhiChu").Visibility = LayoutVisibility.Always; 
            }
            else
            {
                lc.Items.FindByName("lciDiaDiem").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciNgayHD").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciNgayKTHD").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciGhiChu").Visibility = LayoutVisibility.Never;
            }
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            if (!chkIsCT.Checked)
            {
                lc.Items.FindByName("lciDiaDiem").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciNgayHD").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciNgayKTHD").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciGhiChu").Visibility = LayoutVisibility.Never;
            }
            else
            {
                lc.Items.FindByName("lciDiaDiem").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciNgayHD").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciNgayKTHD").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciGhiChu").Visibility = LayoutVisibility.Always; 
            }
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null)
                return;
            ds.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(TaoMaLop_ColumnChanged);
        }

        void TaoMaLop_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted)
                return;
            if (e.Column.ColumnName.ToUpper().Equals("MACN") || e.Column.ColumnName.ToUpper().Equals("MANLOP") 
                || e.Column.ColumnName.ToUpper().Equals("MAGIOHOC"))
            {
                if (e.Row["MaCN"].ToString() != "" && e.Row["MaNLop"].ToString() != "" && e.Row["MaGioHoc"].ToString() != "")
                {
                    string malop = CreateMaLop(e.Row["MaCN"].ToString(), e.Row["MaNLop"].ToString(), e.Row["MaGioHoc"].ToString());
                    if (malop != "")
                        e.Row["MaLop"] = malop;
                    e.Row["TenLop"] = CreateTenLop(e.Row["MaNLop"].ToString(), e.Row["MaCN"].ToString());
                    e.Row.EndEdit();
                }
            }

            if (e.Column.ColumnName.ToUpper().Equals("ISCT"))
            {
                AnTTin((bool)e.Row[e.Column.ColumnName]);
            }
        }

        private string CreateMaLop(string MaCN, string MaNhomLop, string MaGioHoc)
        {
            string MaLop = "";
            // Mã lớp học = Mã CN + Mã khóa học + Mã nhóm lớp + thời gian học            
            string sYear = Config.GetValue("NamLamViec").ToString().Substring(2, 2);
            string sMonth = Config.GetValue("KyKeToan").ToString();
            sMonth = sMonth.Length < 2 ? "0" + sMonth : sMonth;
            string Left = MaCN + sYear + sMonth;
            string Right = MaNhomLop + MaGioHoc;

            string sql = string.Format(@" DECLARE @LEFT varchar(16)
                            DECLARE @RIGHT varchar(16)
                            SET @LEFT = '{0}'
                            SET @RIGHT = '{1}'
                            SELECT	LEFT(MaLop,LEN(@LEFT)) [LEFT]
		                            , MAX(CAST(SUBSTRING(MaLop, LEN(@LEFT)+1, LEN(MaLop) - LEN(@LEFT) - LEN(@RIGHT)) AS INT)) [STT]
		                            , RIGHT(MaLop,LEN(@RIGHT)) [RIGHT]
                            FROM	DMLopHoc
                            WHERE	LEFT(MaLop,LEN(@LEFT)) = @LEFT AND RIGHT(MaLop,LEN(@RIGHT)) = @RIGHT
		                            AND ISNUMERIC(SUBSTRING(MaLop, LEN(@LEFT)+1, LEN(MaLop) - LEN(@LEFT) - LEN(@RIGHT))) = 1 
                            GROUP BY LEFT(MaLop,LEN(@LEFT)), RIGHT(MaLop,LEN(@RIGHT))", Left, Right);

            DataTable dt = db.GetDataTable(sql);
            if (dt.Rows.Count == 0)
                MaLop = Left + "1" + Right;
            else
            {
                int stt = dt.Rows[0]["STT"] != DBNull.Value ? (int)dt.Rows[0]["STT"] : 0;
                if (stt <= 0)
                {
                    XtraMessageBox.Show("Tạo mã lớp không thành công!", Config.GetValue("PackageName").ToString());
                    return null;
                }
                else
                {
                    stt += 1;
                    MaLop = Left + stt.ToString() + Right;
                    getSTT = stt;
                }
            }

            if (MaLop.Length > 14)
            {
                XtraMessageBox.Show("Mã lớp được tạo vượt quá 14 ký tự quy định!", Config.GetValue("PackageName").ToString());
                return null;
            }
            else
                return MaLop;
        }

        private string CreateTenLop(string MaNhomLop, string MaCN)
        {
            string TenNLop = "";
            if (MaNhomLop == "" || MaCN == "")
                return TenNLop;
            string sql = "select TenNLop from DMNhomLop where MaNLop = '" + MaNhomLop + "'";
            DataTable dt = db.GetDataTable(sql);
            if (dt.Rows.Count == 0)
                return TenNLop;
            TenNLop = dt.Rows[0]["TenNLop"].ToString();            
            TenNLop += " - " + getSTT.ToString();
            return TenNLop;
        }

        private void AnTTin(bool IsCT)
        {
            lc = data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            if (IsCT)
            {
                lc.Items.FindByName("lciDiaDiem").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciNgayHD").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciNgayKTHD").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciGhiChu").Visibility = LayoutVisibility.Always;                
            }
            else
            {
                lc.Items.FindByName("lciDiaDiem").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciNgayHD").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciNgayKTHD").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciGhiChu").Visibility = LayoutVisibility.Never;
            }
            //
        }

        public DataCustomFormControl Data
        {
            set { data = value; }
        }

        public InfoCustomControl Info
        {
            get { return info; }
        }
                
        #region Tính Ngày KT Khóa

        void btnNapDL_Click(object sender, EventArgs e)
        {
            DateTime dNgayBD;
            if (gvNgayNghi == null || dateNgayBD == null)
                return;

            if (string.IsNullOrEmpty(dateNgayBD.Text))
            {
                XtraMessageBox.Show("Ngày bắt đầu khóa không được rỗng", Config.GetValue("PackageName").ToString());
                return;
            }
            else
            {
                dNgayBD = dateNgayBD.DateTime;
            }

            string iNam = Config.GetValue("NamLamViec").ToString();
            string sql = string.Format(@"SELECT	NgayNghi, DenNgay , DienGiai
                                        FROM	TLNgayNghi
                                        WHERE	YEAR('{0}') BETWEEN YEAR(NgayNghi) AND YEAR(DenNgay)
                                                AND DenNgay >= '{0}'
                                        ORDER BY NgayNghi", dNgayBD);
            DataTable _dtNgayNghi = db.GetDataTable(sql);

            foreach (DataRow row in _dtNgayNghi.Rows)
            {
                gvNgayNghi.AddNewRow();
                gvNgayNghi.SetFocusedRowCellValue(gvNgayNghi.Columns["NgayNghi"], row["NgayNghi"]);
                gvNgayNghi.SetFocusedRowCellValue(gvNgayNghi.Columns["DenNgay"], row["DenNgay"]);
                gvNgayNghi.SetFocusedRowCellValue(gvNgayNghi.Columns["DienGiai"], row["DienGiai"]);
                gvNgayNghi.UpdateCurrentRow();
            }

        }

        void btnNgayKT_Click(object sender, EventArgs e)
        {
            if (data.BsMain.Current == null)
                return;
            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster["MaGioHoc"] != DBNull.Value && drMaster["SoBuoi"] != DBNull.Value
                && drMaster["NgayBDKhoa"] != DBNull.Value && (decimal)drMaster["SoBuoi"] != 0)
            {
                DataRow[] drCTGioHoc = dtCTGioHoc.Select(string.Format(" MaGioHoc = '{0}' ", drMaster["MaGioHoc"].ToString()));
                if (drCTGioHoc.Length < 1)
                {
                    XtraMessageBox.Show("Thời gian học chưa thiết lập", Config.GetValue("PackageName").ToString());
                    return;
                }
                decimal dSoBuoi = dSobuoi((decimal)drMaster["SoBuoi"]);
                DateTime NgayKT = TinhNgayKT(drMaster["MaGioHoc"].ToString(), (DateTime)drMaster["NgayBDKhoa"]
                                        , Convert.ToInt32(dSoBuoi), drCTGioHoc);
                drMaster["NgayKTKhoa"] = NgayKT;
                dateNgayKT.DateTime = NgayKT;
            }
        }

        private DateTime TinhNgayKT(string MaGioHoc, DateTime NgayBD, int SoBuoi, DataRow[] drCTGioHoc)
        {
            DataTable dtDayOff = new DataTable();
            dtDayOff.Columns.Add("DayOff", typeof(DateTime));

            dtNgayNghi = (data.BsMain.DataSource as DataSet).Tables[2];
            DataRow[] drs = dtNgayNghi.Select(string.Format("MaLop = '{0}'", drMaster["MaLop"].ToString()));
            if (drs.Length != 0)
            {
                foreach (DataRow _dr in drs)
                {
                    DateTime TuNgay = (DateTime)_dr["NgayNghi"];

                    while (TuNgay <= (DateTime)_dr["DenNgay"])
                    {
                        DataRow drNew = dtDayOff.NewRow();
                        drNew["DayOff"] = TuNgay;
                        dtDayOff.Rows.Add(drNew);
                        TuNgay = TuNgay.AddDays(1);
                    }
                }
            }

            DataTable disctDayOff = dtDayOff.DefaultView.ToTable(true, "DayOff");
            int iCount = 1;
            int iOff = 0;// Số buổi trùng với lịch nghỉ
            DateTime NgayKT = NgayBD;

            while (iCount <= SoBuoi)
            {
                NgayKT = NgayKT.AddDays(1);
                foreach (DataRow dr in drCTGioHoc)
                {
                    if (NgayKT.DayOfWeek == OfWeek(dr["Value"].ToString()))
                    {
                        foreach (DataRow drOff in disctDayOff.Rows)
                        {
                            DateTime _date = (DateTime)drOff["DayOff"];
                            if (_date == NgayKT)
                            {
                                iOff++;
                                break;
                            }
                        }
                        iCount++;
                        break;
                    }
                }
            }
            // Cộng thêm số buổi trùng với lịch nghỉ
            while (iOff != 0)
            {
                NgayKT = NgayKT.AddDays(1);
                foreach (DataRow dr in drCTGioHoc)
                {
                    if (NgayKT.DayOfWeek == OfWeek(dr["Value"].ToString()))
                    {
                        //Ngày tiếp theo có buổi trùng với lịch nghỉ
                        foreach (DataRow drOff in disctDayOff.Rows)
                        {
                            DateTime _date = (DateTime)drOff["DayOff"];
                            if (_date == NgayKT)
                            {
                                iOff++;
                                break;
                            }
                        }
                        iOff--;
                        break;
                    }
                }
            }

            return NgayKT;
        }

        private decimal dSobuoi(decimal iSotiet)
        {
            decimal sobuoi = 0;

            return sobuoi;
        }

        private DayOfWeek OfWeek(string Value)
        {
            DayOfWeek _DayOfWeek = DayOfWeek.Monday;
            switch (Value)
            {
                case "2":
                    _DayOfWeek = DayOfWeek.Monday;
                    break;
                case "3":
                    _DayOfWeek = DayOfWeek.Tuesday;
                    break;
                case "4":
                    _DayOfWeek = DayOfWeek.Wednesday;
                    break;
                case "5":
                    _DayOfWeek = DayOfWeek.Thursday;
                    break;
                case "6":
                    _DayOfWeek = DayOfWeek.Friday;
                    break;
                case "7":
                    _DayOfWeek = DayOfWeek.Saturday;
                    break;
                case "1":
                    _DayOfWeek = DayOfWeek.Sunday;
                    break;
            }
            return _DayOfWeek;
        }

        #endregion

        #endregion
    }
}
