using GameFW.Net;
using MobaServer.Biz;
using MobaServer.Dao.Model;
using Protocol;
using System;


namespace MobaServer.Logic
{
    /// <summary>
    /// 单个处理
    /// </summary>
    public abstract class AbsOnceHandler
    {
        public IUserBiz userBiz = BizFactory.userBiz;


        protected byte type;
        protected int area;

        public abstract new byte GetType();

        public void SetArea(int area) {
            this.area = area;
        }

        public virtual int GetArea() {
            return area;
        }
        /// <summary>
        /// 通过连接对象获取用户
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public USER GetUser(UserToken token) {
            return userBiz.GetUser(token);
        }
        /// <summary>
        /// 通过id获取用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public USER GetUser(int id) {
            return userBiz.GetUser(id);
        }
        /// <summary>
        /// 通过连接获取用户id
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public int GetUserId(UserToken token) {
            USER user = GetUser(token);
            if (user == null)
                return -1;

            return user.id;
        }
        /// <summary>
        /// 通过用户id获取连接
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UserToken GetToken(int id) {
            return userBiz.GetToken(id);
        }



        #region 通过连接对象发送
        public void Send(UserToken token, int command) {
            Send(token, GetArea(), command, null);
        }

        public void Send(UserToken token, int command, object message) {
            Send(token, GetArea(), command, message);
        }

        public void Send(UserToken token, int area, int command, object message) {
            Send(token, GetType(), area, command, message);
        }

        public void Send(UserToken token, byte type, int area, int command, object message) {
            token.Send(MessageEncoder.Encode(NewSocketModel(type, area, command, message)));
        }
        #endregion

        public SocketModel NewSocketModel(byte type, int area, int command, object message) {
            return new SocketModel(type, area, command, message);
        }

        #region 通过id来发送

        public void Send(int id, int command)
        {
            Send(id, GetArea(), command, null);
        }

        public void Send(int id, int command, object message)
        {
            Send(id, GetType(), GetArea(), command, message);
        }

        public void Send(int id, int area, int command, object message)
        {
            Send(id, GetType(), area, command, message);
        }

        public void Send(int id, byte type, int area, int command, object message)
        {
            UserToken token = GetToken(id);
            if (token == null)
                return;
            token.Send(MessageEncoder.Encode(NewSocketModel(type, area, command, message)));
        }

        public void SendToUsers(int[] users, byte type, int area, int command, object message) {
            byte[] value = MessageEncoder.Encode(NewSocketModel(type, area, command, message));

            foreach (int userId in users) {
                UserToken token = userBiz.GetToken(userId);

                if (token == null)
                    continue;

                byte[] bs = new byte[value.Length];
                Array.Copy(value, 0, bs, 0, value.Length);
                token.Send(bs);
            }
        }
        #endregion

    }
}
