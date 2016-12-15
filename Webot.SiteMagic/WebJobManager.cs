using System;
using System.Collections;

namespace Webot.SiteMagic
{
    public class WebJobManager : SupportProgressBase
    {
        public WebJobManager()
        {

        }

        private Queue jobQueue = new Queue();

        /// <summary>
        /// 是否还有未完成的任务
        /// </summary>
        public bool HasUnfinishedJob()
        {
            //jobQueue.TrimToSize();
            return (jobQueue.Count > 0);
        }

        /// <summary>
        /// 添加一个任务
        /// </summary>
        public void EnqueJob(WebJob job)
        {
            ++TargetPosition;
            jobQueue.Enqueue(job);
        }

        /// <summary>
        /// 取出一个任务
        /// </summary>
        public WebJob DequeJob()
        {
            ++CurrentPosition;
            return (WebJob)jobQueue.Dequeue();
        }

        public override void ShowMessage(string msg)
        { 
            
        }

    }

}
