using GameFW.Net;
using Protocol;

namespace MobaServer.Logic
{
    /// <summary>
    /// 各个消息的Handler
    /// </summary>
    public interface IHandler
    {
        void OnClientClosed(UserToken token, string error);
        void OnMessageReceived(UserToken token, SocketModel sm);
    }
}
