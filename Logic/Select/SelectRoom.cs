using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameFW.Net;
using Protocol;
using System.Collections.Concurrent;
using Protocol.DTO;
using GameFW.Utility;
using MobaServer.Dao.Model;
using MobaServer.Task;

namespace MobaServer.Logic.Select
{
    public class SelectRoom : AbsMultiHandler, IHandler
    {
        private  ConcurrentDictionary<int, SelectDTO> teamOne = new ConcurrentDictionary<int, SelectDTO>();
        private ConcurrentDictionary<int, SelectDTO> teamTwo = new ConcurrentDictionary<int, SelectDTO>();
        //目前房间里进入的人数
        int enterCount = 0;
        //当前定时任务Id
        int missionId = -1;

        private List<int> readyList = new List<int>();

        public void Init(List<int> teamOne, List<int> teamTwo) {
            this.teamOne.Clear();
            this.teamTwo.Clear();
            enterCount = 0;

            AddUserToTeam(teamOne, ref this.teamOne);
            AddUserToTeam(teamTwo, ref this.teamTwo);

            foreach (SelectDTO item in this.teamOne.Values) {
                GameFW.Utility.Tools.debuger.Log("队伍一的玩家:" + item.name);
            }

            foreach (SelectDTO item in this.teamTwo.Values)
            {
                GameFW.Utility.Tools.debuger.Log("队伍二的玩家:" + item.name);
            }

            GameFW.Utility.Tools.debuger.Log("初始化一个房间");
            missionId = ScheduleUtil.Instance.Schedule(delegate {
                if (enterCount < this.teamOne.Count + this.teamTwo.Count)
                {//30秒后看是不是全员进入，如果不是，就解散房间
                    Destroy();
                }
                else {//如果全员进入了,就再启动30秒选人
                    missionId = ScheduleUtil.Instance.Schedule(delegate
                    {
                        bool allSelected = true;
                        foreach (SelectDTO item in this.teamOne.Values) {
                            if (item.heroId == -1) {
                                allSelected = false;
                                break;
                            }
                        }
                        if (allSelected) {
                            foreach (SelectDTO item in this.teamTwo.Values)
                            {
                                if (item.heroId == -1)
                                {
                                    allSelected = false;
                                    break;
                                }
                            }
                        }
                        if (allSelected)
                        {
                            StartFight();
                        }
                        else {
                            Destroy();
                        }

                    }, 30 * 1000);
                }
            }, 30*1000);
        }

        private void StartFight()
        {
            if (missionId != -1) {
                ScheduleUtil.Instance.RemoveMission(missionId);
                missionId = -1;
            }

            ServerEvents.createFight(teamOne.Values.ToArray(), teamTwo.Values.ToArray());
            brocast(SelectProtocol.FIGHT_BRO, null);
            //回收房间
            ServerEvents.destorySelect(GetArea());
        }
        /// <summary>
        /// 解散房间
        /// </summary>
        private void Destroy()
        {
            brocast(SelectProtocol.DESTORY_BRO, null);
            if (missionId != -1)
            {
                ScheduleUtil.Instance.RemoveMission(missionId);
            }
            ServerEvents.destorySelect(GetArea());
        }

        private void AddUserToTeam(List<int> userIds, ref ConcurrentDictionary<int, SelectDTO> team) {
            foreach (int i in userIds)
            {
                SelectDTO select = new SelectDTO();
                select.userId = i;
                select.name = GetUser(i).name;
                select.heroId = -1;
                select.isEnter = false;
                select.isReady = false;
                select.heroList = GetUser(i).heroList;
                team.TryAdd(i, select);
            }
        }

        public override byte GetType()
        {
            return Protocol.Protocol.TYPE_SELECT; 
        }

        public void OnClientClosed(UserToken token, string error)
        {
            Leave(token);
            Destroy();
        }

        public void OnMessageReceived(UserToken token, SocketModel sm)
        {
            switch (sm.command) {
                case SelectProtocol.ENTER_CREQ:
                    Enter(token);
                    break;
                case SelectProtocol.SELECT_CREQ:
                    Select(token, sm.GetMessage<int>());
                    break;
                case SelectProtocol.TALK_CREQ:
                    Talk(token, sm.GetMessage<string>());
                    break;
                case SelectProtocol.READY_CREQ:
                    Ready(token);
                    break;
            }
        }

        private void Talk(UserToken token, string v)
        {
            if (!IsEntered(token)) {
                return;
            }
            USER user = GetUser(token);
            brocast(SelectProtocol.TALK_BRO, new StringBuilder(user.name).Append(":").Append(v).ToString());
        }

        private void Ready(UserToken token) {
            if (!IsEntered(token))
                return;
            int userId = GetUserId(token);

            if (readyList.Contains(userId))
                return;

            SelectDTO sm = null;
            if (teamOne.ContainsKey(userId))
            {
                sm = teamOne[userId];
            }
            else {
                sm = teamTwo[userId];
            }

            if (sm.heroId == -1) {
                return;
            }

            sm.isReady = true;
            brocast(SelectProtocol.READY_BRO, sm);
            readyList.Add(userId);
            if (readyList.Count >= teamOne.Count + teamTwo.Count) {
                StartFight();
            }
        }

        private void Select(UserToken token, int v)
        {
            if (!IsEntered(token))
                return;
            USER user = GetUser(token);
            //判断玩家是否拥有此英雄
            if (!user.heroList.Contains(v)) {
                Send(token, SelectProtocol.SELECT_SRES, null);
                return;
            }

            SelectDTO select = null;
            if (teamOne.ContainsKey(user.id))
            {
                foreach (SelectDTO item in teamOne.Values)
                {
                    if (item.heroId == v)
                        return;
                }
                select = teamOne[user.id];
            }
            else {
                foreach (SelectDTO item in teamTwo.Values) {
                    if (item.heroId == v)
                        return;
                }
                select = teamTwo[user.id];
            }

            select.heroId = v;
            //广播更新的玩家选人数据
            brocast(SelectProtocol.SELECT_BRO, select);
        }

        private new void Enter(UserToken token)
        {
            int userId = GetUserId(token);
            if (teamOne.ContainsKey(userId))
            {
                teamOne[userId].isEnter = true;
            }
            else if (teamTwo.ContainsKey(userId))
            {
                teamTwo[userId].isEnter = true;
            }
            else {
                return;
            }

            if (base.Enter(token)) {
                enterCount++;
            }

            SelectRoomDTO dto = new SelectRoomDTO();
            dto.SelfUserId = userId;
            dto.teamOne = teamOne.Values.ToArray();
            dto.teamTwo = teamTwo.Values.ToArray();
            Send(token, SelectProtocol.ENTER_SRES, dto);//向申请登录的玩家发送整个房间的信息
            brocast(SelectProtocol.ENTER_EXBRO, userId, token);//向其他人发送这个玩家登录的信息
        }

        public ICollection<int> GetTeamOneIds() {
            return teamOne.Keys;
        }

        public ICollection<int> GetTeamTwoIds()
        {
            return teamTwo.Keys;
        }

        public void Clear() {
            this.teamOne.Clear();
            this.teamTwo.Clear();
            this.readyList.Clear();
            this.tokenList.Clear();
            this.enterCount = 0;
            this.missionId = -1;
        }
    }
}
