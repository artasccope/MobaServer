using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFW.Net;
using Protocol;
using System.Collections.Concurrent;
using MobaServer.Logic.Select;
using CommonTools;

namespace MobaServer.Logic
{
    public class SelectHandler : AbsOnceHandler, IHandler
    {
        /// <summary>
        /// 玩家和房间id映射
        /// </summary>
        ConcurrentDictionary<int, int> userIdToRoomId = new ConcurrentDictionary<int, int>();

        ConcurrentDictionary<int, SelectRoom> roomIdMap = new ConcurrentDictionary<int, SelectRoom>();
        /// <summary>
        /// 回收利用过的房间对象
        /// </summary>
        ConcurrentStack<SelectRoom> roomCache = new ConcurrentStack<SelectRoom>();
        /// <summary>
        /// 房间Id
        /// </summary>
        ConcurrentInteger index = new ConcurrentInteger();

        public SelectHandler()
        {
            ServerEvents.createSelect = Create;
            ServerEvents.destorySelect = Destroy;
        }


        private void Create(List<int> teamOne, List<int> teamTwo)
        {
            SelectRoom room;
            if (!roomCache.TryPop(out room)) {
                room = new SelectRoom();
                //设置唯一区域号
                room.SetArea(index.GetAndAdd());
            }
            
            room.Init(teamOne, teamTwo);

            foreach (int i in teamOne) {
                userIdToRoomId.TryAdd(i, room.GetArea());
            }
            foreach (int i in teamTwo)
            {
                userIdToRoomId.TryAdd(i, room.GetArea());
            }
            roomIdMap.TryAdd(room.GetArea(), room);
        }


        private void Destroy(int roomId)
        {
            SelectRoom room;
            if (roomIdMap.TryRemove(roomId, out room)) {
                int temp = 0;
                foreach (int i in room.GetTeamOneIds()) {
                    userIdToRoomId.TryRemove(i, out temp);
                }
                foreach (int i in room.GetTeamTwoIds())
                {
                    userIdToRoomId.TryRemove(i, out temp);
                }
                room.Clear();
                //回收利用
                roomCache.Push(room);
            }
        }

        public void OnClientClosed(UserToken token, string error)
        {
            int userId = GetUserId(token);
            if (userIdToRoomId.ContainsKey(userId)) {
                int roomId;
                userIdToRoomId.TryRemove(userId, out roomId);

                if (roomIdMap.ContainsKey(roomId)) {
                    //通知房间这个玩家离开了
                    roomIdMap[roomId].OnClientClosed(token, error);
                }
            }
        }

        public void OnMessageReceived(UserToken token, SocketModel sm)
        {
            int userId = GetUserId(token);
            if (userIdToRoomId.ContainsKey(userId)) {
                int roomId = userIdToRoomId[userId];
                if (roomIdMap.ContainsKey(roomId)) {
                    roomIdMap[roomId].OnMessageReceived(token, sm);
                }
            }
        }
        public override byte GetType()
        {
            return Protocol.Protocol.TYPE_SELECT;
        }
    }
}
