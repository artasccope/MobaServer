using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFW.Net;
using MobaServer.Dao.Model;
using GameFW.Utility;
using Protocol.DTO;

namespace MobaServer.Cache.Impl
{
    /// <summary>
    /// 角色的缓存层
    /// </summary>
    public class UserCache : IUserCache
    {
        /// <summary>
        /// 用户id和用户的映射
        /// </summary>
        Dictionary<int, USER> idToUser = new Dictionary<int, USER>();
        /// <summary>
        /// 账号和用户的绑定
        /// </summary>
        Dictionary<int, int> accToUid = new Dictionary<int, int>();

        /// <summary>
        /// 用户id和token的映射
        /// </summary>
        Dictionary<int, UserToken> uidToToken = new Dictionary<int, UserToken>();
        /// <summary>
        /// token和用户id的映射
        /// </summary>
        Dictionary<UserToken, int> tokenToUid = new Dictionary<UserToken, int>();
        /// <summary>
        /// 用户id和用户延迟之间的映射
        /// </summary>
        Dictionary<int, DelayAndFloating> uIdToDelayFloating = new Dictionary<int, DelayAndFloating>();

        public void CacheDelayAndFloating(int userId, DelayAndFloating df)
        {
            if (df.delay < 0)
                return;

            if (!uIdToDelayFloating.ContainsKey(userId))
                uIdToDelayFloating.Add(userId, df);
            else
                uIdToDelayFloating[userId] = df;
        }

        public bool Create(UserToken token, string name, int accountId)
        {
            if (USER.Exists(name))
                return false;

            USER user = new USER();
            user.name = name;
            user.accountId = accountId;
            List<int> list = new List<int>();
            for (int i = 1; i < 10; i++) {
                list.Add(i);
            }
            user.heroList = list.ToArray();
            user.Add();

            accToUid.Add(accountId, user.id);
            idToUser.Add(user.id, user);
            return true;
        }

        

        public USER GetByAccountId(int accId)
        {
            Init(accId);
            if (!accToUid.ContainsKey(accId))
                return null;

            return idToUser[accToUid[accId]];
        }

        public DelayAndFloating GetDelayAndFloating(UserToken token)
        {
            if (!tokenToUid.ContainsKey(token))
                return null;

            if (uIdToDelayFloating.ContainsKey(tokenToUid[token])) {
                return uIdToDelayFloating[tokenToUid[token]];
            }

            return null;
        }

        public DelayAndFloating GetDelayAndFloatingById(int userId)
        {
            if (!uIdToDelayFloating.ContainsKey(userId))
                return null;

            return uIdToDelayFloating[userId];
        }

        public void RemoveDelayAndFloating(int userId)
        {
            if(uIdToDelayFloating.ContainsKey(userId))
                uIdToDelayFloating.Remove(userId);
        }

        public UserToken GetToken(int id)
        {
            return uidToToken[id];
        }

        public USER GetUser(UserToken token)
        {
            if (!HasUser(token))
                return null;

            return idToUser[tokenToUid[token]];
        }

        public USER GetUser(int id)
        {
            return idToUser[id];
        }

        public bool HasByAccountId(int id)
        {
            Init(id);
            return accToUid.ContainsKey(id);
        }

        public bool HasUser(UserToken token)
        {
            return tokenToUid.ContainsKey(token);
        }

        public bool IsOnline(int id)
        {
            return uidToToken.ContainsKey(id);
        }

        public void Offline(UserToken token)
        {
            if (tokenToUid.ContainsKey(token)) {
                if (uidToToken.ContainsKey(tokenToUid[token])) {
                    uidToToken.Remove(tokenToUid[token]);
                    RemoveDelayAndFloating(tokenToUid[token]);
                }

                tokenToUid.Remove(token);
            }

        }

        public USER Online(UserToken token, int id)
        {
            uidToToken.Add(id, token);
            tokenToUid.Add(token, id);
            return idToUser[id];
        }
        /// <summary>
        /// 根据账号id来查询DAO层是否存在账号，存在就放入缓存
        /// </summary>
        /// <param name="accountId"></param>
        void Init(int accountId) {
            if (accToUid.ContainsKey(accountId))
                return;

            USER user = new USER(accountId);
            if (user.id >= 0)
            {
                accToUid.Add(accountId, user.id);
                idToUser.Add(user.id, user);
            }
            else {
                GameFW.Utility.Tools.debuger.Log("数据库没这个角色");
            }
        }
    }
}
