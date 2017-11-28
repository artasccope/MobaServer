using GameFW.Net;
using Protocol;
using System.Collections.Concurrent;
using CommonTools;
using GameFW.Utility;

namespace MobaServer.Logic.Match
{
    public class MatchHandler : AbsOnceHandler, IHandler
    {
        /// <summary>
        /// 用户id和其对应的roomId
        /// </summary>
        ConcurrentDictionary<int, int> userIdToRoomId = new ConcurrentDictionary<int, int>();
        /// <summary>
        /// room的id和room实体的映射
        /// </summary>
        ConcurrentDictionary<int, MatchRoom> roomIdMap = new ConcurrentDictionary<int, MatchRoom>();
        /// <summary>
        /// 房间池
        /// </summary>
        ConcurrentStack<MatchRoom> roomCache = new ConcurrentStack<MatchRoom>();

        ConcurrentInteger index = new ConcurrentInteger();


        public override byte GetType()
        {
            return Protocol.Protocol.TYPE_MATCH;
        }

        public void OnClientClosed(UserToken token, string error)
        {
            Leave(token);
        }

        private void Leave(UserToken token)
        {
            int userId = GetUserId(token);
            GameFW.Utility.Tools.debuger.Log("用户取消匹配 : "+ userId);
            if (!userIdToRoomId.ContainsKey(userId)) {
                return;
            }

            int roomId = userIdToRoomId[userId];
            if (roomIdMap.ContainsKey(roomId)) {
                MatchRoom room = roomIdMap[roomId];
                userIdToRoomId.TryRemove(userId, out roomId);

                if (room.RemoveUser(userId)) {//返回为真代表房间里没有用户了
                    roomIdMap.TryRemove(roomId, out room);
                    roomCache.Push(room);
                }
            }
            //发送一个离开成功的消息
            Send(token, MatchProtocol.LEAVE_SRES);
        }

        public void OnMessageReceived(UserToken token, SocketModel sm)
        {
            switch (sm.command) {
                case MatchProtocol.ENTER_CREQ:
                    Enter(token);
                    break;
                case MatchProtocol.LEAVE_CREQ:
                    Leave(token);
                    break;
            }
        }

        private void Enter(UserToken token)
        {
            int userId = GetUserId(token);
            GameFW.Utility.Tools.debuger.Log("开始匹配，用户id : " + userId);
            if (!userIdToRoomId.ContainsKey(userId)) {//用户还没进入房间
                MatchRoom room = null;
                if (roomIdMap.Count > 0)
                {
                    foreach (MatchRoom waitRoom in roomIdMap.Values)
                    {
                        TryEnterRoom(userId, out room, waitRoom);
                        if (room != null)
                            break;
                    }
                }
                if (room == null)
                {//说明没有等待的房间或者是等待的房间全部满员了
                    GetRoom(out room);
                    room.teamOne.Add(userId);
                    roomIdMap.TryAdd(room.id, room);
                    userIdToRoomId.TryAdd(userId, room.id);
                }

                
                if (room.IsFull) {
                    //TODO 通知选人模块开始选人了, 创建选人房间
                    GameFW.Utility.Tools.debuger.Log("匹配房间已满，创建选人房间");
                    ServerEvents.createSelect(room.teamOne, room.teamTwo);
                    SendToUsers(room.teamOne.ToArray(), GetType(), 0, MatchProtocol.ENTER_SELECT_BRO, room.teamMaxUserCount);
                    SendToUsers(room.teamTwo.ToArray(), GetType(), 0, MatchProtocol.ENTER_SELECT_BRO, room.teamMaxUserCount);

                    RemoveMatchRoomMap(room);
                    room.Clear();
                    roomIdMap.TryRemove(room.id, out room);
                    roomCache.Push(room);
                    return;
                }
                //如果没有马上满,给用户发送一个排队中的消息
                GameFW.Utility.Tools.debuger.Log("排队中");
                Send(token,Protocol.Protocol.TYPE_MATCH, 0, MatchProtocol.ENTER_SRES,null);
            }
        }

        private void RemoveMatchRoomMap(MatchRoom room) {
            int i;
            foreach (int item in room.teamOne) {
                userIdToRoomId.TryRemove(item, out i);
            }
            foreach (int item in room.teamTwo)
            {
                userIdToRoomId.TryRemove(item, out i);
            }
        }

        private void GetRoom(out MatchRoom room) {
            if (roomCache.Count > 0)
            {
                roomCache.TryPop(out room);
            }
            else {
                room = new MatchRoom();
                room.id = index.GetAndAdd();
            }
        }
        /// <summary>
        /// 尝试进入一个匹配房间,如果这个房间未满,就进入这个房间并返回它;如果满了就不能进入，返回空
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="room"></param>
        /// <param name="waitRoom"></param>
        private void TryEnterRoom(int userId, out MatchRoom room, MatchRoom waitRoom) {
            if (waitRoom.teamMaxUserCount * 2 > waitRoom.teamOne.Count + waitRoom.teamTwo.Count)
            {
                if (waitRoom.teamOne.Count < waitRoom.teamMaxUserCount)
                {
                    waitRoom.teamOne.Add(userId);
                }
                else
                {
                    waitRoom.teamTwo.Add(userId);
                }
                room = waitRoom;
                userIdToRoomId.TryAdd(userId, room.id);
                return;
            }
            else {
                room = null;
                return;
            }
        }
    }
}
