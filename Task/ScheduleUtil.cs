using CommonTools;
using GameFW.Utility;
using System;
using System.Collections.Generic;
using System.Timers;

namespace MobaServer.Task
{
    public delegate void TimeEvent();

    public class ScheduleUtil
    {
        #region 单例
        private static ScheduleUtil instance;
        public static ScheduleUtil Instance { get { if (instance == null) { instance = new ScheduleUtil(); } return instance; } }
        #endregion

        private Timer timer;
        private ConcurrentInteger index = new ConcurrentInteger();
        private Dictionary<int, TimeTaskModel> mission = new Dictionary<int, TimeTaskModel>();
        private List<int> removeList = new List<int>();

        private ScheduleUtil()
        {
            timer = new Timer(14);
            timer.Elapsed += OnTimeElapsed;
            timer.Start();
        }

        /// <summary>
        /// 执行事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimeElapsed(object sender, ElapsedEventArgs e)
        {
            lock (mission)
            {
                lock (removeList)
                {
                    foreach (TimeTaskModel item in mission.Values)
                    {
                        if (item.Time <= DateTime.Now.Ticks)
                        {
                            removeList.Add(item.Id);
                        }
                    }

                    foreach (int executeItem in removeList) {
                        mission[executeItem].Run();
                    }

                    foreach (int item in removeList) {
                        mission.Remove(item);
                    }
                    removeList.Clear();
                }
            }
        }

        /// <summary>
        /// 任务调用 毫秒
        /// </summary>
        /// <param name="task"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public int Schedule(TimeEvent task, long delay)
        {
            GameFW.Utility.Tools.debuger.Log("启用了一个任务， 延时为 : "+ delay);
            return ScheduleTicks(task, delay * 10000);//1tick为100毫微秒(10^-7),1毫秒为10^-3
        }
        /// <summary>
        /// 任务调用 100毫微秒
        /// </summary>
        /// <param name="task"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public int ScheduleTicks(TimeEvent task, long delay)
        {
            lock (mission)
            {
                int id = index.GetAndAdd();
                TimeTaskModel model = new TimeTaskModel(id, task, DateTime.Now.Ticks + delay);
                mission.Add(id, model);
                return id;
            }
        }
        /// <summary>
        /// 移除任务
        /// </summary>
        /// <param name="id"></param>
        public void RemoveMission(int id)
        {
            lock (removeList)
            {
                removeList.Add(id);
            }
        }
        /// <summary>
        /// 指定日期事件调用
        /// </summary>
        /// <param name="task"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public int Schedule(TimeEvent task, DateTime time)
        {
            long t = time.Ticks - DateTime.Now.Ticks;
            t = Math.Abs(t);
            return ScheduleTicks(task, t);
        }
    }
}
