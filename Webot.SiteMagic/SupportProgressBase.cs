using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Webot.SiteMagic
{
    public abstract class SupportProgressBase : ISupportProgress
    {
        public SupportProgressBase() { }

        #region ISupportProgress Members
        private int _currentPos = 0;
        /// <summary>
        /// 当前位置
        /// </summary>
        public int CurrentPosition
        {
            get { return _currentPos; }
            set { _currentPos = value; }
        }

        private int _targetPos = 0;
        /// <summary>
        /// 目标位置
        /// </summary>
        public int TargetPosition
        {
            get { return _targetPos; }
            set { _targetPos = value; }
        }

        /// <summary>
        /// 运行百分比
        /// </summary>
        public float GetPercent()
        {
            if (CurrentPosition != 0 && TargetPosition != 0)
            {
                return (float)CurrentPosition / (float)TargetPosition;
            }
            else
            {
                return 1.00F;
            }
        }

        private DateTime _beginUTCDate;
        /// <summary>
        /// 开始时间UTC时刻
        /// </summary>
        public DateTime UTCDateBegin
        {
            get { return _beginUTCDate; }
            set { _beginUTCDate = value; }
        }

        private ShowMessageHandler _msgHandler;
        /// <summary>
        /// 设置输出消息委托
        /// </summary>
        public ShowMessageHandler MsgHandler
        {
            get { return _msgHandler; }
            set { _msgHandler = value; }
        }
 

        /// <summary>
        /// 输出消息方法
        /// </summary>
        /// <param name="msg">消息</param>
        public virtual void ShowMessage(string msg)
        {
            if (this.MsgHandler != null)
            {
                MsgHandler(msg);
            }
        }

        /// <summary>
        /// 显示消息状态委托
        /// </summary>
        public delegate void ShowMessageHandler(string msg);

        /// <summary>
        /// 重置运行状态(开始)
        /// </summary>
        public virtual void ResetState()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 查询是否支持暂停
        /// </summary>
        public virtual bool IsSupportPause()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 设置为暂停
        /// </summary>
        public virtual void SetPause()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 继续运行
        /// </summary>
        public virtual void Continue()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
