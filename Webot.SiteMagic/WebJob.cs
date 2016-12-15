using System;

namespace Webot.SiteMagic
{
    /// <summary>
    /// Web自定义任务
    /// </summary>
    public class WebJob : SupportProgressBase
    {
        public WebJob(string jobname)
        {
            this.JobName = jobname;
        }

        private string _jobName = "";
        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName
        {
            get { return _jobName; }
            set { _jobName = value; }
        }

        private IResourceDependency _resDependency = null;
        /// <summary>
        /// 获取该标签所依赖的资源
        /// </summary>
        public IResourceDependency GetResourceDependency()
        {
            return this._resDependency;
        }

        /// <summary>
        /// 设置该标签所依赖的资源
        /// </summary>
        public void SetResourceDependency(IResourceDependency value)
        {
            this._resDependency = value;
        }

        /// <summary>
        /// 运行任务
        /// </summary>
        /// <param name="job">相关任务</param>
        public delegate void Execute(WebJob job);


        public override void ShowMessage(string msg)
        {
            base.ShowMessage(msg);
        }

    }
}
