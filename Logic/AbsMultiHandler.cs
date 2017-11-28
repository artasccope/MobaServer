using GameFW.Net;
using System;
using System.Collections.Generic;
using Protocol;

namespace MobaServer.Logic
{
    /// <summary>
    /// 针对一个用户群的Handler
    /// </summary>
    public abstract class AbsMultiHandler:AbsOnceHandler
    {
        public List<UserToken> tokenList = new List<UserToken>();
        /// <summary>
        /// 每个玩家的网络延迟
        /// </summary>
        private Dictionary<UserToken, int> playerDelay = new Dictionary<UserToken, int>();

        /// <summary>
        /// 用户进入当前子模块
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool Enter(UserToken token) {
            if (tokenList.Contains(token)) {
                return false;
            }
            tokenList.Add(token);
            return true;
        }
        /// <summary>
        /// 用户是否已经在此子模块了
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool IsEntered(UserToken token) {
            return tokenList.Contains(token);
        }
        /// <summary>
        /// 用户离开当前子模块
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool Leave(UserToken token) {
            if (tokenList.Contains(token)) {
                tokenList.Remove(token);
                return true;
            }

            return false;
        }

        #region 通过连接列表群发消息API
        public void brocast(int command, object message, UserToken exToken = null)
        {
            brocast(GetArea(), command, message, exToken);
        }
        public void brocast(int area, int command, object message, UserToken exToken = null)
        {
            brocast(GetType(), area, command, message, exToken);
        }
        public void brocast(byte type, int area, int command, object message, UserToken exToken = null)
        {
            byte[] value = MessageEncoder.Encode(NewSocketModel(type, area, command, message));
            foreach (UserToken item in tokenList)
            {
                if (item != exToken)
                {
                    byte[] bs = new byte[value.Length];
                    Array.Copy(value, 0, bs, 0, value.Length);
                    item.Send(bs);
                }
            }
        }

        public void brocast(byte type, int area, int command, object message, int[] exUsers = null)
        {
            byte[] value = MessageEncoder.Encode(NewSocketModel(type, area, command, message));
            foreach (UserToken item in tokenList)
            {
                bool isInExUsers = false;
                for (int i = 0; i < exUsers.Length; i++) {
                    if (GetUserId(item) == exUsers[i]) {
                        isInExUsers = true;
                        break;
                    }
                }

                if (!isInExUsers)
                {
                    byte[] bs = new byte[value.Length];
                    Array.Copy(value, 0, bs, 0, value.Length);
                    item.Send(bs);
                }
            }
        }

        #endregion
    }
}
