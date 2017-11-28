using GameFW.Net;

namespace MobaServer.Logic
{
    public interface IHandlerCenter
    {
        /// <summary>
        /// 客户端连接
        /// </summary>
        /// <param name="token">连接的客户端对象</param>
        void OnClientConnected(UserToken token);
        /// <summary>
        /// 收到客户端消息
        /// </summary>
        /// <param name="token">发送消息的客户端对象</param>
        /// <param name="message">消息内容</param>
        void OnMessageReceived(UserToken token, byte[] message);
        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="token">断开的客户端对象</param>
        /// <param name="error">断开的错误信息</param>
        void OnClientClosed(UserToken token, string error);
    }
}
