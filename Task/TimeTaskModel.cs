namespace MobaServer.Task
{
    public class TimeTaskModel
    {
        private TimeEvent task;
        public long Time;
        public int Id;

        public TimeTaskModel(int id, TimeEvent task, long time) {
            this.task = task;
            this.Id = id;
            this.Time = time;
        }

        public void Run()
        {
            task();
        }
    }
}