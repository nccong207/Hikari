using System;
using System.Collections.Generic;
using System.Text;
using DevExpress;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Data;
using DevExpress.XtraGrid.Views.Grid;

namespace SuaTenHV
{
    public class SuaTenHV:ICData
    {
        private InfoCustomData _info;
        private DataCustomData _data;
        Database db = Database.NewDataDatabase();        
        public SuaTenHV()
        {
            _info = new InfoCustomData(IDataType.MasterDetailDt);
            //_info = new InfoCustomData(IDataType.Single);
        }

       
        #region ICData Members

        public DataCustomData Data
        {
            set { _data = value; }
        }
         
        public void ExecuteAfter()
        {

        }

        public void ExecuteBefore()
        {
            update();
            
        }

        private void insertQTHT()
        {
            
        }

        private void update()
        {
            if (_data.CurMasterIndex < 0)
                return;
            if (_data.DsData == null)
                return;
            DataRow row = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (row == null)
                return;
            if(row.RowState != DataRowState.Modified)
                return;
            
            // Thay đổi nguồn học viên
            if (row["MaNguon", DataRowVersion.Original].ToString() != row["MaNguon", DataRowVersion.Current].ToString())
            {
                DataRow drw = _data.DsData.Tables[1].NewRow();
                drw["HVTVID"] = row["HVTVID"];
                drw["TuNgay"] = DateTime.Today;
                drw["Nguon"] = row["MaNguon", DataRowVersion.Current].ToString();
                drw["MoTa"] = string.Format("Chuyển nguồn học viên: {0} sang {1}", row["MaNguon", DataRowVersion.Original].ToString(), row["MaNguon", DataRowVersion.Current].ToString());
                drw["NhomDK"] = "CNHV";
                _data.DsData.Tables[1].Rows.Add(drw);                
            }

            //Thay đổi tên học viên
            if (row["TenHV", DataRowVersion.Original].ToString() != row["TenHV", DataRowVersion.Current].ToString())
            {
                string code = row["HVTVID"].ToString();
                string newName = row["TenHV"].ToString();
                string MaHV = row["MaHV"].ToString();
                ChangeName(code, newName, MaHV);
            }
        }

        private void ChangeName(string code, string newName, string MaHV)
        {            
            // HV tư vấn, hv đăng ký, dm khách hàng, hv chuyển lớp, phiếu thu, phiếu chi, blvt, bltk.
            string sql = "";
            sql += "Update MTDK set TenHV = N'" + newName + "' where HVTVID = '" + code + "';";// and MaHV = '" + MaHV + "';";
            sql += "Update DMKH set TenKH = N'" + newName + "' where MaKH = '" + MaHV + "';";
            sql += "Update MTChuyenLop set TenHV = N'" + newName + "' where MaHV = '" + MaHV + "';";
            sql += "Update MT11 set TenKH = N'" + newName + "' where MaKH = '" + MaHV + "';";
            sql += "Update DT11 set TenKHCt = N'" + newName + "' where MaKhCt = '" + MaHV + "';";
            sql += "Update MT12 set TenKH = N'" + newName + "' where MaKH = '" + MaHV + "';";
            sql += "Update DT12 set TenKHCt = N'" + newName + "' where MaKhCt = '" + MaHV + "';";
            sql += "Update BLVT set TenKH = N'" + newName + "' where MaKH = '" + MaHV + "';";
            sql += "Update BLTK set TenKH = N'" + newName + "' where MaKH = '" + MaHV + "';";
            sql += "Update DMKQ set TenHV = N'" + newName + "' where HVTVID = '" + code + "';";
            sql += "Update MT32 set TenKH = N'" + newName + "' where MaKH = '" + MaHV + "';";
            db.UpdateByNonQuery(sql);
            
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
