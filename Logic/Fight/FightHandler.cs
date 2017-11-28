using GameFW.Net;
using Protocol;
using Protocol.DTO;
using System.Collections.Concurrent;
using MobaServer.Logic.Fight;
using CommonTools;

namespace MobaServer.Logic.Fight
{
    public class FightHandler : AbsMultiHandler, IHandler
    {
        ConcurrentDictionary<int, int> userIdToRoomId = new ConcurrentDictionary<int, int>();

        ConcurrentDictionary<int, FightRoom> roomMap = new ConcurrentDictionary<int, FightRoom>();

        ConcurrentStack<FightRoom> roomCache = new ConcurrentStack<FightRoom>();

        ConcurrentInteger index = new ConcurrentInteger(0);

        public FightHandler() {
            ServerEvents.createFight = Create;
            ServerEvents.destoryFight = Destroy;
        }

        private void Create(SelectDTO[] teamOne, SelectDTO[] teamTwo)
        {
            FightRoom room;
            if (!roomCache.TryPop(out room)) {
                room = new FightRoom();
                room.SetArea(index.GetAndAdd());
            }

            room.Init(teamOne, teamTwo);

            foreach (SelectDTO item in teamOne) {
                userIdToRoomId.TryAdd(item.userId, room.GetArea());
            }

            foreach (SelectDTO item in teamTwo) {
                userIdToRoomId.TryAdd(item.userId, room.GetArea());
            }

            roomMap.TryAdd(room.GetArea(), room);
        }

        private void Destroy(int roomId)
        {
            FightRoom room;
            if (roomMap.TryRemove(roomId, out room)) {
                int tmp = 0;
                foreach (int item in room.instances.Keys)
                {
                    if(userIdToRoomId.ContainsKey(item))
                        userIdToRoomId.TryRemove(item, out tmp);
                }

                room.Clear();
                roomCache.Push(room);
            }
        }

        public override byte GetType()
        {
            return Protocol.Protocol.TYPE_FIGHT;
        }

        public void OnClientClosed(UserToken token, string error)
        {
            int userId = GetUserId(token);
            
            if (userIdToRoomId.ContainsKey(userId)) {
                roomMap[userIdToRoomId[userId]].OnClientClosed(token, error);
            }
        }

        public void OnMessageReceived(UserToken token, SocketModel sm)
        {
            roomMap[userIdToRoomId[GetUserId(token)]].OnMessageReceived(token, sm);
        }

    }
}
