using GameFW.Net;
using MobaServer.Dao.Model;
using MobaServer.Cache;
using GameFW.Utility;
using Protocol.DTO;

namespace MobaServer.Biz.Impl
{
    public class UserBiz : IUserBiz
    {
        IAccountBiz accountBiz = BizFactory.accountBiz;
        IUserCache userCache = CacheFactory.userCache;

        public void CacheDelayAndFloating(int userId, DelayAndFloating delay)
        {
            userCache.CacheDelayAndFloating(userId, delay);
        }

        public bool Create(UserToken token, string name)
        {
            int accountId = accountBiz.GetAccountId(token);
            if (accountId == -1)
                return false;
            if (userCache.HasByAccountId(accountId))
                return false;

            return userCache.Create(token, name, accountId);
        }

        public DelayAndFloating GetDelayAndFloating(UserToken token)
        {
            return userCache.GetDelayAndFloating(token);
        }

        public DelayAndFloating GetDelayAndFloatingById(int userId)
        {
            return userCache.GetDelayAndFloatingById(userId);
        }

        public UserToken GetToken(int id)
        {
            return userCache.GetToken(id);
        }

        public USER GetUser(UserToken token)
        {
            return userCache.GetUser(token);
        }

        public USER GetUser(int id)
        {
            return userCache.GetUser(id);
        }

        public USER GetUserByAccount(UserToken token)
        {
            int accountId = accountBiz.GetAccountId(token);
            if (accountId == -1) {
                GameFW.Utility.Tools.debuger.Log("账号没上线");
                return null;
            }
            return userCache.GetByAccountId(accountId);
        }

        public void Offline(UserToken token)
        {
            userCache.Offline(token);
        }

        public USER Online(UserToken token)
        {
            int accountId = accountBiz.GetAccountId(token);
            if (accountId == -1)
                return null;
            USER user = userCache.GetByAccountId(accountId);
            if (userCache.IsOnline(user.id))
                return null;
            userCache.Online(token, user.id);
            return user;
        }
    }
}
