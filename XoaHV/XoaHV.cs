using CDTDatabase;
using Plugins;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace XoaHV
{
    public class XoaHV: ICData
    {
        private InfoCustomData _info;
        private DataCustomData _data;
        Database db = Database.NewDataDatabase();

        public XoaHV()
        {
            _info = new InfoCustomData(IDataType.MasterDetailDt);
        }

        #region ICData Members
        public void ExecuteAfter()
        {
            //update();
        }

        public void ExecuteBefore()
        {
            update();
        }

        public DataCustomData Data
        {
            set { _data = value; }
        }
        public InfoCustomData Info
        {
            get { return _info; }
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
            if (row.RowState != DataRowState.Deleted)
                return;
            string hvid = row["HVID", DataRowVersion.Original].ToString();
           
            string updateLopQuery = "update MTNL set isXL = 0 from mtdk dk, DTNL dt where dk.HVTVID = MTNL.HVTVID and dk.MaNhomLop = dt.MaNLop and " +
            "dk.MaCNDK = MTNL.MaCN and dt.MTNLID = MTNL.MTNLID and isXL = 1 and dk.HVID = '{0}'";

            string updateSelectedQuery = "update HVChoLop set Chon = 0 from mtdk dk where dk.HVTVID = HVChoLop.MaHV" +
                " and dk.MaNhomLop = MaNLop and dk.MaCNDK = MaCN and Chon = 1 and dk.HVID = '{0}'";
            if (!string.IsNullOrEmpty(hvid)) {
                _data.DbData.UpdateByNonQuery(string.Format(updateLopQuery, hvid));
                _data.DbData.UpdateByNonQuery(string.Format(updateSelectedQuery, hvid));
            }
        }
        #endregion
    }
}
