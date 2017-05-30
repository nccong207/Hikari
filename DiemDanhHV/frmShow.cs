using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Globalization;
using CDTLib;
using CDTDatabase;

namespace DiemDanhHV
{
    public partial class frmShow : DevExpress.XtraEditors.XtraForm
    {
        public frmShow()
        {
            InitializeComponent();
        }
        Database db = Database.NewDataDatabase();
        public DataTable dtLop;
        public DataTable dtHocVien;
        public string MaLop = "";
        public DateTime dtFirst = DateTime.Today;
        DateTime dtLast = DateTime.Today;
        

        private void frmShow_Load(object sender, EventArgs e)
        {
            CultureInfo myCI = new CultureInfo("en-US");
            
            Calendar myCal = myCI.Calendar;
            CalendarWeekRule _rule= myCI.DateTimeFormat.CalendarWeekRule;
            DayOfWeek _firstDay = DayOfWeek.Monday;
                        
            while (dtFirst.DayOfWeek != DayOfWeek.Monday)
                dtFirst = dtFirst.AddDays(-1);                        
            while (dtLast.DayOfWeek != DayOfWeek.Sunday)
                dtLast = dtLast.AddDays(1);
            spTuan.Value = myCal.GetWeekOfYear(DateTime.Today, _rule, _firstDay);
            dateBegin.DateTime = dtFirst;
            dateEnd.DateTime = dtLast;
            getLopHoc();
        }

        private void getLopHoc()
        {
            // Là admin: hiển thị đầy đủ lớp
            // Là giáo viên: chỉ hiển thị lớp do mình phụ trách
            string iThang = DateTime.Today.Month.ToString(), iNam = DateTime.Today.Year.ToString();
            if (Config.GetValue("NamLamViec") != null)
                iNam = Config.GetValue("NamLamViec").ToString();
            if (Config.GetValue("KyKeToan") != null)
                iThang = Config.GetValue("KyKeToan").ToString();

            string sql = string.Format(@"DECLARE @NgayBD DATETIME
                                        DECLARE @NgayKT DATETIME
                                        SET @NgayBD = CONVERT(DATETIME,'1/{1}/{2}',103)
                                        SET @NgayKT = DATEADD(mm,1,@NgayBD)-1

                                        SELECT l.MaLop, l.TenLop, l.NgayBDKhoa, l.NgayKTKhoa
                                        FROM    DMLopHoc l
                                                LEFT OUTER JOIN GVPhuTrach gv ON l.MaLop = gv.MaLop
                                        WHERE   isKT ='0' AND MaCN = '{0}'
                                                AND l.NgayBDKhoa <= @NgayKT AND l.NgayKTKhoa >= @NgayBD"
                                        , Config.GetValue("MaCN").ToString(), iThang, iNam);

            dtLop = db.GetDataTable(sql);
            grdEditLopHoc.Properties.DataSource = dtLop;
            grdEditLopHoc.Properties.ValueMember = "MaLop";
            grdEditLopHoc.Properties.DisplayMember = "MaLop";
            grdEditLopHoc.Properties.PopupFormMinSize = new Size(600, 400);
            grdEditLopHoc.Properties.View.BestFitColumns();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (grdEditLopHoc.EditValue == null || string.IsNullOrEmpty(grdEditLopHoc.EditValue.ToString()))
                return;
            MaLop = grdEditLopHoc.EditValue.ToString();
            dtHocVien = getHocVien(MaLop);
            dtFirst = dateBegin.DateTime;
            this.DialogResult = DialogResult.OK;
        }

        private DataTable getHocVien(string _MaLop)
        {
            string dBegin = dateBegin.DateTime.ToString();
            string dEnd = dateEnd.DateTime.ToString();

            string sql = "";
            sql = string.Format(@" DECLARE @NgayBD DATETIME
                            DECLARE @NgayKT DATETIME
                            SET	@NgayBD = '{0}'
                            SET	@NgayKT = '{1}'

                            SELECT	MaLop, MaHV [HVID]
                            FROM	DiemDanhHV
                            WHERE	MaLop = '{2}' AND Ngay BETWEEN @NgayBD AND @NgayKT ", dBegin, dEnd, _MaLop);
            DataTable dtSub = db.GetDataTable(sql);

            if (dtSub.Rows.Count == 0)
            {
                sql = string.Format(@"DECLARE @NgayBD DATETIME
                        DECLARE @NgayKT DATETIME
                        SET	@NgayBD = '{0}'
                        SET	@NgayKT = '{1}'
                        
                        SELECT	DISTINCT l.MaLop, hv.HVID, hv.TenHV, tv.MaHV
                                , cc.Ngay, tv.MaNguon
                        FROM	DMLopHoc l 
	                            INNER JOIN ChamCongGV cc ON l.MaLop = cc.MaLop
	                            INNER JOIN MTDK hv ON l.MaLop = hv.MaLop
                                INNER JOIN DMHVTV tv ON hv.HVTVID = tv.HVTVID
                        WHERE	l.MaLop = '{2}'
                                AND cc.Ngay BETWEEN @NgayBD AND @NgayKT
	                            AND l.NgayBDKhoa < @NgayKT AND @NgayBD <= l.NgayKTKhoa	
	                            AND IsKT = 0 AND hv.NgayDK <= cc.Ngay AND hv.NgayDK <= @NgayKT
                                AND (IsBL = 0 OR (IsBL = 1 AND NgayBL > cc.Ngay))
	                            AND (IsNghiHoc = 0 OR (IsNghiHoc = 1 AND NgayNghi > cc.Ngay)) ", dBegin, dEnd, _MaLop);
                dtSub = db.GetDataTable(sql);
                if (dtSub.Rows.Count == 0)
                {
                    XtraMessageBox.Show("Chưa tạo lịch dạy cho lớp trong thời gian này\nKhông có học viên đăng ký trong thời gian này", Config.GetValue("PackageName").ToString());
                }
            }
            else
            {
                // Trường hợp đã có dữ liệu
                // Add thêm những học viên 03.04.2013
                sql = string.Format(@" DECLARE @NgayBD DATETIME
                                DECLARE @NgayKT DATETIME
                                DECLARE @MaLop VARCHAR(16)
                                SET	@NgayBD = '{0}'
                                SET	@NgayKT = '{1}'
                                SET @MaLop = '{2}'
                                SELECT	DISTINCT l.MaLop, hv.HVID, hv.TenHV, tv.MaHV, cc.Ngay, tv.MaNguon
                                FROM	DMLopHoc l 
                                        INNER JOIN ChamCongGV cc ON l.MaLop = cc.MaLop
                                        INNER JOIN MTDK hv ON l.MaLop = hv.MaLop
                                        INNER JOIN DMHVTV tv ON hv.HVTVID = tv.HVTVID
                                WHERE	l.MaLop = @MaLop AND l.IsKT = 0 
                                        AND cc.Ngay BETWEEN @NgayBD AND @NgayKT
                                        AND l.NgayBDKhoa < @NgayKT AND @NgayBD <= l.NgayKTKhoa	
                                        AND hv.NgayDK <= cc.Ngay AND hv.NgayDK <= @NgayKT
                                        AND (IsBL = 0 OR (IsBL = 1 AND NgayBL > cc.Ngay)) 
                                        AND (IsNghiHoc = 0 OR (IsNghiHoc = 1 AND NgayNghi > cc.Ngay))
		                                AND HVID NOT IN (	SELECT	MaHV
							                                FROM	DiemDanhHV 
							                                WHERE	MaLop = l.MaLop AND cc.Ngay = Ngay
									                                AND Ngay BETWEEN @NgayBD AND @NgayKT)"
                            , dBegin, dEnd, _MaLop);
                dtSub = db.GetDataTable(sql);
                
                if(dtSub.Rows.Count >0)
                using (DataView dv = new DataView(dtSub))
                {
                    dv.RowFilter = " MaHV IS NULL ";
                    if (dv.Count > 0)
                    {
                        if (XtraMessageBox.Show("Tồn tại học viên chưa có mã học viên!\nThêm học viên chưa có mã học viên ?"
                                    , Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.No)
                        {
                            dtSub = null;
                        }
                    }
                }
            }

            return dtSub;
        }

        private void spTuan_EditValueChanged(object sender, EventArgs e)
        {
            int iWeek = Convert.ToInt32(spTuan.Value);
            DateTime _date = new DateTime(dtFirst.Year, 1, 1).AddDays((iWeek - 1) * 7);
            dtFirst = dtLast = _date;
            while (dtFirst.DayOfWeek != DayOfWeek.Monday)
                dtFirst = dtFirst.AddDays(-1);
            while (dtLast.DayOfWeek != DayOfWeek.Sunday)
                dtLast = dtLast.AddDays(1);

            dateBegin.DateTime = dtFirst;
            dateEnd.DateTime = dtLast;
        }

        //private void lookUpLop_Popup(object sender, EventArgs e)
        //{
        //    lookUpLop.Properties.PopupFormMinSize = new Size(400, 600);
        //    lookUpLop.Properties.BestFit();
        //}
       
    }
}