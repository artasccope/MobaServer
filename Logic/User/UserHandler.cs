using GameFW.Net;
using Protocol;
using GameFW.Utility;
using CommonTools.ConcurrentTask;
using MobaServer.Dao.Model;
using Protocol.DTO;

namespace MobaServer.Logic.User
{
    public class UserHandler : AbsOnceHandler, IHandler
    {
        public void OnClientClosed(UserToken token, string error)
        {
            userBiz.Offline(token);
        }

        public void OnMessageReceived(UserToken token, SocketModel sm)
        {
            switch (sm.command) {
                case RoleProtocol.CREATE_CREQ:
                    Create(token, sm.GetMessage<string>());
                    break;
                case RoleProtocol.INFO_CREQ:
                    Info(token);
                    break;
                case RoleProtocol.ONLINE_CREQ:
                    Online(token);
                    break;
            }
        }

        private void Online(UserToken token)
        {
            ExecutorPool.Instance.Execute(
                delegate ()
                {
                    Send(token, UserProtocol.ONLINE_SRES, Convert(userBiz.Online(token)));
                }
                ); ;
        }

        private void Info(UserToken token)
        {
            ExecutorPool.Instance.Execute(
                delegate ()
                {
                    Send(token, UserProtocol.INFO_SRES, Convert(userBiz.GetUserByAccount(token)));
                }
                );
        }

        private void Create(UserToken token, string message)
        {
            GameFW.Utility.Tools.debuger.Log("创建用户");
            ExecutorPool.Instance.Execute(
                delegate () {
                    Send(token, UserProtocol.CREATE_SRES, userBiz.Create(token, message));
                }
                );
        }

        private UserDTO Convert(USER user) {
            if (user == null)
                return null;
            return new UserDTO(user.name, user.id, user.level, user.winCount, user.loseCount, user.runCount, user.heroList);
        }
        public override byte GetType()
        {
            return Protocol.Protocol.TYPE_USER;
        }
    }
}
