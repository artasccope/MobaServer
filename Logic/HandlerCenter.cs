using GameFW.Net;
using Protocol;
using GameFW.Utility;
using MobaServer.Logic.Match;
using MobaServer.Logic.User;
using MobaServer.Logic.Fight;

namespace MobaServer.Logic
{
    public class HandlerCenter : IHandlerCenter
    {
        private MessageDecode mDecode;
        public HandlerCenter(MessageDecode decode) {
            if (decode == null) {
                ServerDebuger.Instance.Log("没有设置消息解码，服务器启动失败！");
            }

            mDecode = decode;
        }

        public void OnClientConnected(UserToken token)
        {

        }

        #region 消息分发
        private LoginHandler loginHandler = new LoginHandler();
        private SelectHandler selectHandler = new SelectHandler();
        private MatchHandler matchHandler = new MatchHandler();
        private UserHandler userHandler = new UserHandler();
        private FightHandler fightHandler = new FightHandler();
        private TimeHandler timeHandler = new TimeHandler();

        public void OnMessageReceived(UserToken token, byte[] message)
        {
            SocketModel sm = mDecode(message);
            if (sm != null) {
                GameFW.Utility.Tools.debuger.Log("收到了消息， 类型为："+ sm.type + ", 命令为:" + sm.command);
                switch (sm.type) {
                    case Protocol.Protocol.TYPE_USER:
                        userHandler.OnMessageReceived(token, sm);
                        break;
                    case Protocol.Protocol.TYPE_SELECT:
                        selectHandler.OnMessageReceived(token, sm);
                        break;
                    case Protocol.Protocol.TYPE_MATCH:
                        matchHandler.OnMessageReceived(token, sm);
                        break;
                    case Protocol.Protocol.TYPE_LOGIN:
                        loginHandler.OnMessageReceived(token, sm);
                        break;
                    case Protocol.Protocol.TYPE_FIGHT:
                        fightHandler.OnMessageReceived(token, sm);
                        break;
                    case Protocol.Protocol.TYPE_TIME:
                        timeHandler.OnMessageReceived(token, sm);
                        break;
                    default:
                        break;
                }
            }
        }

        public void OnClientClosed(UserToken token, string error)
        {
            ServerDebuger.Instance.Log("有客户端断开连接了");

            fightHandler.OnClientClosed(token, error);
            selectHandler.OnClientClosed(token, error);
            matchHandler.OnClientClosed(token, error);
            loginHandler.OnClientClosed(token, error);
            timeHandler.OnClientClosed(token, error);
            userHandler.OnClientClosed(token, error);
        }
        #endregion
    }
}
