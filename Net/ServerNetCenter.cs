using System;
using System.Net.Sockets;
using CommonTools;
using System.Threading;
using System.Net;
using GameFW.Net;
using MobaServer.Logic;
using GameFW.Utility;

namespace MobaServer.Net
{
    class ServerNetCenter
    {
        private ObjectPool<UserToken> tokenPool;
        private Socket listenSocket;
        private Semaphore acceptedClientCount;
        private ConcurrentInteger cInteger = new ConcurrentInteger(-1);

        #region 单例
        public static ServerNetCenter Instance
        {
            get
            {
                return Nested.instance;
            }
        }
        private class Nested
        {
            static Nested() { }

            internal readonly static ServerNetCenter instance = new ServerNetCenter();
        }

        #endregion

        #region ctor
        private ServerNetCenter() {

        }
        #endregion

        #region 上层消息处理接口

        private IHandlerCenter handlerCenter;
        public void SetHandlerCenter(IHandlerCenter center) {
            this.handlerCenter = center;
        }
#endregion

        /// <summary>
        /// 启动服务器
        /// </summary>
        public void Start() {
            //新建连接池
            tokenPool = new ObjectPool<UserToken>(ServerSettings.maxClient);
            //设置信号量
            acceptedClientCount = new Semaphore(ServerSettings.maxClient, ServerSettings.maxClient);
            //新建连接
            for (int i = 0; i < ServerSettings.maxClient; i++) {
                NetCONServer conn = new NetCONServer();
                //放入连接池
                tokenPool.PutObj(conn.Token);
            }

            //新建监听Socket
            IPAddress[] addressList = Dns.GetHostEntry(Environment.MachineName).AddressList;
            IPAddress iPAddress = null;
            foreach (IPAddress ip in addressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    iPAddress = ip;
                    break;
                }
            }

            IPEndPoint localEndPoint = new IPEndPoint(iPAddress, ServerSettings.gameServerPort);
            Console.WriteLine(localEndPoint.Address);
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEndPoint);

            try
            {
                //开始监听服务端口
                listenSocket.Listen(ServerSettings.backlog);
                Console.WriteLine(listenSocket.AddressFamily.ToString());
                GameFW.Utility.Tools.debuger.Log("网络监听成功");
                //开始接收
                StartAccept(null);
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 按传来的对象接收连接
        /// </summary>
        /// <param name="e"></param>
        private void StartAccept(SocketAsyncEventArgs e) {
            if (e == null)//如果e为空就新建一个，并设置它的完成事件
            {
                e = new SocketAsyncEventArgs();
                e.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptCompleted);
            }
            else {//重置e
                e.AcceptSocket = null;
            }
            acceptedClientCount.WaitOne();

            GameFW.Utility.Tools.debuger.Log("有连接,当前连接数为:"+ cInteger.GetAndAdd());
            //接收连接
            bool result = listenSocket.AcceptAsync(e);
            if (!result) {//如果不是马上挂起就马上处理掉
                ProcessAccepted(e);
            }
        }
        /// <summary>
        /// 接收完成回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AcceptCompleted(object sender, SocketAsyncEventArgs e) {
            ProcessAccepted(e);
        }
        /// <summary>
        /// 处理接收到连接的情况
        /// </summary>
        /// <param name="e"></param>
        public void ProcessAccepted(SocketAsyncEventArgs e) {
            UserToken token = tokenPool.GetObj();
            token.Connection = e.AcceptSocket;

            //通知上层,客户端连接
            handlerCenter.OnClientConnected(token);
            token.StartReceive();

            //尾递归,接收完成了继续接收
            StartAccept(e);
        }
        /// <summary>
        /// 连接断开的处理
        /// </summary>
        /// <param name="conn"></param>
        public void OnConnClosed(UserToken token, string error) {
            GameFW.Utility.Tools.debuger.Log("有客户断开,当前连接数为:"+ cInteger.GetAndReduce());
            handlerCenter.OnClientClosed(token, error);
            acceptedClientCount.Release();
            tokenPool.PutObj(token);
        }

        public void OnMessageRevieved(UserToken token, byte[] message) {
            handlerCenter.OnMessageReceived(token, message);
        }
    }
}
