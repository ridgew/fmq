using System;
using Webot.Common;
using Webot.SiteMagic;

namespace Webot.WebUIPackage
{
    public class TrackUnit
    {
        /// <summary>
        /// 跟踪检查实例
        /// </summary>
        public TrackUnit(string tKey, string dTime)
        {
            DataRef = string.Concat(tKey, "!", dTime);
            if (tKey.ToUpper().StartsWith("T"))
            {
                ResTable = "Tbl_Templets";
                this.UpdateByTemplet = true;
            }
            else
            {
                ResTable = "Tbl_HtmlBlock";
            }
            ResID = int.Parse(tKey.Substring(1));
            this.SaveTime = Convert.ToInt64(dTime);
        }

        private OleDbHelper hp = FanmaquerOleDbModule.GetOleDbInstance(FanmaquarConfig.ConnectionKey);

        private int ResID = 0;
        private string ResTable = "", DataRef = "";

        private long _saveTime = 0;
        /// <summary>
        /// 保存时间数据
        /// </summary>
        public long SaveTime
        {
            get { return _saveTime; }
            set { _saveTime = value; }
        }


        private bool _updateByTemplet = false;
        /// <summary>
        /// 是否由模板激发，优先顺序：模板>内容块
        /// </summary>
        public bool UpdateByTemplet
        {
            get { return _updateByTemplet; }
            set { _updateByTemplet = value; }
        }

        private bool _needUpdate = false;
        /// <summary>
        /// 是否已经更新
        /// </summary>
        public bool IsUpdatable()
        {
            string field = (this.UpdateByTemplet == true) ? "TimeFlag" : "PublishDate";
            string IDField = (this.UpdateByTemplet == true) ? "TempletID" : "BlockID";
            object updateTime = hp.ExecuteScalar(string.Format("select top 1 {0} from {2} where {1}={3}",
                field,
                IDField,
                ResTable,
                ResID));

            if (updateTime != null)
            {
                DateTime timeflag = Convert.ToDateTime(updateTime);
                this._needUpdate = (Convert.ToInt64(timeflag.ToString("yyyyMMddHHmmss")) > SaveTime);
                return this._needUpdate;
            }
            return false;
        }

        /// <summary>
        /// 设置某个页面的更新跟踪信息
        /// </summary>
        /// <param name="trackPath">页面基于跟目录的路径</param>
        public void SetRecord(string trackPath)
        {
            int SiteID = 0;
            trackPath = trackPath.Replace("'", "");
            string sql = String.Format("select count(TrackID) from Tbl_TrackDat where SiteID={0} and TrackPath='{1}' ",
                SiteID, trackPath);

            int count = Convert.ToInt32(hp.ExecuteScalar(sql));
            string sqlformat = "insert into Tbl_TrackDat(LastIP,IsUpdatable,UpdateByTemplet,DataRef,SiteID,TrackPath,TimeFlag) values('{0}',{1},{2},'{3}',{4},'{5}',Now())";
            if (count > 0)
            {
                sqlformat = "update Tbl_TrackDat set LastIP='{0}',IsUpdatable={1},UpdateByTemplet={2},TimeFlag=Now(),DataRef='{3}' where SiteID={4} and TrackPath='{5}'";
            }
            sql = string.Format(sqlformat, Util.GetIP(),
                    this._needUpdate.ToString(),
                    UpdateByTemplet.ToString(),
                    DataRef, SiteID, trackPath);
            //Util.Debug(false, DataRef);
            hp.ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 移除需要更新的记录
        /// </summary>
        /// <param name="SiteID">站点编号</param>
        /// <param name="trackPath">页面基于跟目录的路径</param>
        public static void RemoveUpdateRecord(int SiteID, string trackPath)
        {
            FanmaquerOleDbModule.GetOleDbInstance(FanmaquarConfig.ConnectionKey)
                .ExecuteNonQuery(string.Format("delete from [Tbl_TrackDat] where SiteID={0} and TrackPath='{1}'",
                    SiteID,
                    trackPath.Replace("'", "")));
        }

    }

}
