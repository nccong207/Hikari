using System;
using System.Collections.Generic;
using System.Text;
using DevExpress;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors;
using CDTLib;
using Plugins;
using CDTDatabase;
using System.Data;
using System.Windows.Forms;
using DevExpress.Data.Filtering;

namespace QuanLyThi
{
    public class QuanLyThi:ICControl
    {
        private InfoCustomControl info = new InfoCustomControl(IDataType.Single);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();
        
        GridView gv;
        
        private DataTable dtMonThi;
        private DataTable dtXepLoai;

        #region ICControl Members

        public void AddEvent()
        {
            data.FrmMain.Shown += new EventHandler(FrmMain_Shown);
            data.FrmMain.KeyUp += new KeyEventHandler(FrmMain_KeyUp);
            gv = (data.FrmMain.Controls.Find("gcMain",true)[0] as GridControl).MainView as GridView;
        }

        void FrmMain_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.L && e.Modifiers == System.Windows.Forms.Keys.Control)
                LayDanhSachHV();
        }

        private void LayDanhSachHV()
        {
            gv.OptionsView.NewItemRowPosition = NewItemRowPosition.None;
            //Ghi chú: K: không thi, V: vắng
            fromShow frm = new fromShow();
            frm.Text = "Chọn lớp để quản lý";
            if (frm.ShowDialog() != DialogResult.OK)
                gv.OptionsBehavior.Editable = false;

            gv.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gv_CellValueChanged);

            if ((frm.blDauVao == false && frm.MaLop == "") || (frm.blDauVao == true && frm.MaNLop == ""))
                return;

            dtMonThi = frm.dtMonThi;
            string sIndex = "''";
            // Hiển thị môn thi của lớp            
            foreach (DataRow dr in dtMonThi.Rows)
            {
                sIndex += ",'" + dr["MaMT"].ToString() + "'";
            }

            // Danh sách các môn không thi
            string sCDT = string.Format(@"SELECT MaMT FROM DMMonThi WHERE MaMT NOT IN ({0})", sIndex);
            using (DataTable dtIndex = db.GetDataTable(sCDT))
            {
                // Ẩn cột điểm không thi
                foreach (DataRow drRow in dtIndex.Rows)
                {
                    gv.Columns[drRow["MaMT"].ToString()].Visible = false;
                }
            }

            // Xếp loại học lực
            string sXepLoai = string.Format(@"SELECT	XepLoai,TuDiem,DenDiem
                                            FROM	DMXepLoai
                                            WHERE	XepLoai NOT IN (N'Xuất sắc')
                                            ORDER BY TuDiem");
            dtXepLoai = db.GetDataTable(sXepLoai);

            if (frm.blDauVao)
            {
                // Thi đầu vào
                DateTime TuNgay = frm.pTuNgay, DenNgay = frm.pDenNgay;
                DataTable dt = (gv.DataSource as BindingSource).DataSource as DataTable;
                if (dt == null) return;

                DataTable dtSrc = dt.Clone();
                DataRow[] drs = dt.Select(string.Format(" NgayThi >= '{0}' AND NgayThi <= '{1}' ", TuNgay, DenNgay));

                // Filter string lọc theo display member
                gv.ActiveFilterString = string.Format(@" MaNLop = '{0}' AND KyThiID = '{1}' ", frm.MaNLop, frm.KyThi, DenNgay);

                if (frm.dtHocVien == null)
                    return;
                foreach (DataRow row in frm.dtHocVien.Rows)
                {
                    gv.AddNewRow();
                    gv.SetFocusedRowCellValue(gv.Columns["MaNLop"], frm.MaNLop);
                    gv.SetFocusedRowCellValue(gv.Columns["KyThiID"], frm.KyThiID);
                    gv.SetFocusedRowCellValue(gv.Columns["HVTVID"], row["HVTVID"].ToString());
                    gv.SetFocusedRowCellValue(gv.Columns["TenHV"], row["TenHV"].ToString());

                    gv.UpdateCurrentRow();
                }
            }
            else
            {
                // Các kỳ thi khác
                gv.ActiveFilterString = string.Format(" MaLop = '{0}' AND KyThiID = '{1}' ", frm.MaLop, frm.KyThi);

                if (gv.DataRowCount == 0)
                {
                    if (frm.dtHocVien == null)
                        return;
                    foreach (DataRow row in frm.dtHocVien.Rows)
                    {
                        gv.AddNewRow();
                        gv.SetFocusedRowCellValue(gv.Columns["MaLop"], frm.MaLop);
                        gv.SetFocusedRowCellValue(gv.Columns["MaNLop"], frm.MaNLop);
                        gv.SetFocusedRowCellValue(gv.Columns["KyThiID"], frm.KyThiID);

                        gv.SetFocusedRowCellValue(gv.Columns["NgayThi"], row["NgayThi"]);
                        gv.SetFocusedRowCellValue(gv.Columns["HVID"], row["HVID"].ToString());
                        gv.SetFocusedRowCellValue(gv.Columns["HVTVID"], row["HVTVID"].ToString());
                        gv.SetFocusedRowCellValue(gv.Columns["TenHV"], row["TenHV"].ToString());

                        gv.UpdateCurrentRow();
                    }
                }
                else
                {
                    // Fix TH dữ liệu lên không đủ hoặc bị xóa trong khi nhập liệu
                    string _sqlFix = @"SELECT  MaLop, HVID, HVTVID, TenHV, '{2}' as NgayThi 
                                        FROM    MTDK mt
                                        WHERE   MaLop = '{0}' and isNghiHoc ='0' and isBL = '0' 
		                                        AND HVID NOT IN (SELECT	HVID FROM DMKQ
						                                        WHERE	MaLop = '{0}' AND KyThiID = '{1}')";
                    using (DataTable dtFix = db.GetDataTable(string.Format(_sqlFix, frm.MaLop, frm.KyThiID, frm.NgayThi)))
                    {
                        foreach (DataRow row in dtFix.Rows)
                        {
                            gv.AddNewRow();
                            gv.SetFocusedRowCellValue(gv.Columns["MaLop"], frm.MaLop);
                            gv.SetFocusedRowCellValue(gv.Columns["MaNLop"], frm.MaNLop);
                            gv.SetFocusedRowCellValue(gv.Columns["KyThiID"], frm.KyThiID);

                            gv.SetFocusedRowCellValue(gv.Columns["NgayThi"], row["NgayThi"]);
                            gv.SetFocusedRowCellValue(gv.Columns["HVID"], row["HVID"].ToString());
                            gv.SetFocusedRowCellValue(gv.Columns["HVTVID"], row["HVTVID"].ToString());
                            gv.SetFocusedRowCellValue(gv.Columns["TenHV"], row["TenHV"].ToString());

                            gv.UpdateCurrentRow();
                        }
                    }
                }
            }

            gv.BestFitColumns();
        }

        void FrmMain_Shown(object sender, EventArgs e)
        {
            LayDanhSachHV();
        }

        void gv_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Value == null)
                return;
            if( dtMonThi == null)
                return;

            if (e.Column.ColumnEditName.ToLower().Contains("col"))
            {
                decimal dDiemTB = 0;
                decimal dDiemTong = 0;
                DataRow row =gv.GetDataRow(e.RowHandle);
                decimal iCount = 0;

                foreach (DataRow dr in dtMonThi.Rows)
                {
                    decimal Hso = dr["HeSo"] != DBNull.Value ? (decimal)dr["HeSo"] : 1;
                    iCount += Hso;
                    dDiemTong += row[dr["MaMT"].ToString()] != DBNull.Value ? (decimal)row[dr["MaMT"].ToString()] * Hso : 0;
                }

                dDiemTB = iCount != 0 ? Math.Round(dDiemTong / iCount, 2) : 0;

                gv.SetFocusedRowCellValue(gv.Columns["TongDiem"], dDiemTong);
                gv.SetFocusedRowCellValue(gv.Columns["DiemTB"], dDiemTB);
                gv.SetFocusedRowCellValue(gv.Columns["XL"], XepLoai(dDiemTong, iCount));
            }
        }

        string XepLoai(decimal dTong, decimal dHeso)
        {
            if (dHeso == 0)
                return "";

            string sXL = "";
            decimal dTB = dTong / dHeso;

            if (dTB == 100)
                return "Xuất sắc";

            foreach (DataRow dr in dtXepLoai.Rows)
            {
                decimal dTu = dr["TuDiem"] != DBNull.Value ? (decimal)dr["TuDiem"] : 0;
                decimal dDen = dr["DenDiem"] != DBNull.Value ? (decimal)dr["DenDiem"] : 0;
                if (dTu <= dTB && dTB < dDen)
                {
                    sXL = dr["XepLoai"].ToString();
                }
            }
            return sXL;
        }
       
        bool isNumber(string c)
        {
            for (int i = 0; i < c.Length;i++ )
                if (!char.IsDigit(c[i]) && c[i]!=',' && c[i]!='.' )                                   
                    return false;
            return true;
        }

        public DataCustomFormControl Data
        {
            set { data = value; }
        }

        public InfoCustomControl Info
        {
            get { return info; }
        }

        #endregion
    }
}
