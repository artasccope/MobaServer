using GameFW.Net;
using System.Net.Sockets;

namespace MobaServer.Net
{
    /// <summary>
    /// 服务端的单个网络连接,对应一个用户
    /// </summary>
    public class NetCONServer : NetCONBase
    {
        public NetCONServer():base()
        {
        }
        #region 服务端实现的 消息发送后的处理、消息接收后的处理、关闭连接(在客户端和服务端有不同的实现)
        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        protected override bool CloseCON(UserToken token, string error)
        {
            if (token.HasConnection())
            {
                lock (token)
                {
                    token.Close();
                    ServerNetCenter.Instance.OnConnClosed(token, error);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// UserToken拿到消息的处理
        /// </summary>
        /// <param name="token"></param>
        /// <param name="sm"></param>
        protected override void MessageProcess(UserToken token, byte[] message)
        {
            ServerNetCenter.Instance.OnMessageRevieved(token, message);
        }
        /// <summary>
        /// UserToken发送消息之后的回调
        /// </summary>
        /// <param name="e"></param>
        protected override void MessageSended(SocketAsyncEventArgs e)
        {
            
        }
#endregion
        public UserToken Token {
            get {
                return token;
            }
        }
    }
}
