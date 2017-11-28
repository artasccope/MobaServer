using GameFW.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobaServer.Biz
{
    public interface IAccountBiz
    {
        /// <summary>
        /// 创建账号
        /// </summary>
        /// <param name="token"></param>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        int Create(UserToken token, string account, string password);
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="token"></param>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        int Login(UserToken token, string account, string password);
        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="token"></param>
        void Close(UserToken token);
        /// <summary>
        /// 获取账号id
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        int GetAccountId(UserToken token);
    }
}
