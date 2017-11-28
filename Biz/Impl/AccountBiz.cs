using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFW.Net;
using MobaServer.Cache;
using Protocol;
using Protocol.Result;

namespace MobaServer.Biz.Impl
{
    public class AccountBiz : IAccountBiz
    {
        IAccountCache accountCache = CacheFactory.accountCache;

        public void Close(UserToken token)
        {
            accountCache.Offline(token);
        }

        public int Create(UserToken token, string account, string password)
        {
            if (accountCache.HasAccount(account))
                return (int)AccountResult.HasAccountCantCreate;

            accountCache.Add(account, password);
            return (int)AccountResult.CreateSuccess;
        }

        public int GetAccountId(UserToken token)
        {
            return accountCache.GetId(token);
        }

        public int Login(UserToken token, string account, string password)
        {
            if (!accountCache.HasAccount(account))
                return (int)AccountResult.AccountNotExistedCantLogin;
            if (accountCache.IsOnline(account))
                return (int)AccountResult.AlreadyOnlineCantLogin;

            if (!accountCache.Match(account, password))
                return (int)AccountResult.AccountPwdNotMatch;

            accountCache.Online(token, account);
            return (int)AccountResult.LoginSuccess;
        }
    }
}
