using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFW.Net;
using MobaServer.Dao.Model;
using GameFW.Utility;

namespace MobaServer.Cache.Impl
{
    /// <summary>
    /// 账号的缓存层
    /// </summary>
    public class AccountCache : IAccountCache
    {
        /// <summary>
        /// 连接token和账号的映射
        /// </summary>
        Dictionary<UserToken, string> onlineAccountMap = new Dictionary<UserToken, string>();
        /// <summary>
        /// 账号字符串和账号DAO对象的映射
        /// </summary>
        Dictionary<string, ACCOUNT> accountMap = new Dictionary<string, ACCOUNT>();

        /// <summary>
        /// 添加新账号DAO到缓存层
        /// </summary>
        /// <param name="account"></param>
        public void Init(string account) {
            if (accountMap.ContainsKey(account))
                return;

            ACCOUNT acc = new ACCOUNT(account);
            if (acc.id >= 0) {
                accountMap.Add(account, acc);
            }
        }

        public void Add(string account, string password)
        {
            ACCOUNT model = new ACCOUNT();
            model.account = account;
            model.password = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
            
            model.Add();
            GameFW.Utility.Tools.debuger.Log("创建了新账号.");
            accountMap.Add(account, model);
        }
        /// <summary>
        /// 得到缓存的token对应的Account的id
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public int GetId(UserToken token)
        {
            if (!onlineAccountMap.ContainsKey(token))
                return -1;

            return accountMap[onlineAccountMap[token]].id;
        }

        public bool HasAccount(string account)
        {
            Init(account);
            return accountMap.ContainsKey(account);
        }

        public bool IsOnline(string account)
        {
            return onlineAccountMap.ContainsValue(account);
        }

        public bool Match(string account, string password)
        {
            Init(account);
            return accountMap[account].password.Equals(Convert.ToBase64String(Encoding.UTF8.GetBytes(password)));
        }

        public void Offline(UserToken token)
        {
            if (onlineAccountMap.ContainsKey(token))
                onlineAccountMap.Remove(token);
        }

        public void Online(UserToken token, string account)
        {
            if(!onlineAccountMap.ContainsKey(token))
                onlineAccountMap.Add(token, account);
        }
    }
}
