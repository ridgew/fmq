using System;

namespace Webot.SiteMagic
{
    /// <summary>
    /// 支持进度接口
    /// </summary>
    public interface ISupportProgress
    {
        /// <summary>
        /// 当前位置
        /// </summary>
        int CurrentPosition { get; set; }

        /// <summary>
        /// 运行百分比
        /// </summary>
        float GetPercent();

        /// <summary>
        /// 目标位置
        /// </summary>
        int TargetPosition { get; set; }

        /// <summary>
        /// 开始时间UTC时刻
        /// </summary>
        DateTime UTCDateBegin { get; set; }

        /// <summary>
        /// 重置运行状态(开始)
        /// </summary>
        void ResetState();

        /// <summary>
        /// 查询是否支持暂停
        /// </summary>
        bool IsSupportPause();

        /// <summary>
        /// 设置为暂停
        /// </summary>
        void SetPause();

        /// <summary>
        /// 继续运行
        /// </summary>
        void Continue();

        /// <summary>
        /// 输出消息方法
        /// </summary>
        /// <param name="msg">消息</param>
        void ShowMessage(string msg);
    }
}
