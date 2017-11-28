using GameFW.Net;
using MobaServer.Dao.Model;
using Protocol.DTO;

namespace MobaServer.Cache
{
    /// <summary>
    /// 用户缓存
    /// </summary>
    public interface IUserCache
    {
        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="token"></param>
        /// <param name="name"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        bool Create(UserToken token, string name, int accountId);
        /// <summary>
        /// 账号有没有用户
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        bool HasUser(UserToken token);
        /// <summary>
        /// 根据账号id判断是否拥有用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool HasByAccountId(int id);
        /// <summary>
        /// 根据连接获取用户信息
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        USER GetUser(UserToken token);
        /// <summary>
        /// 根据用户id获取用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        USER GetUser(int id);
        /// <summary>
        /// 连接上线
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        USER Online(UserToken token, int id);
        /// <summary>
        /// 连接下线
        /// </summary>
        /// <param name="token"></param>
        void Offline(UserToken token);
        /// <summary>
        /// 根据账号id获取用户
        /// </summary>
        /// <param name="accId"></param>
        /// <returns></returns>
        USER GetByAccountId(int userId);
        /// <summary>
        /// 用户是否已经在线
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool IsOnline(int id);
        /// <summary>
        /// 根据一个用户的id得到这个用户的token
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        UserToken GetToken(int id);
        /// <summary>
        /// 得到某个用户的延迟
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        DelayAndFloating GetDelayAndFloating(UserToken token);
        /// <summary>
        /// 通过用户id得到延迟
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        DelayAndFloating GetDelayAndFloatingById(int userId);
        /// <summary>
        /// 缓存用户的延迟
        /// </summary>
        /// <param name="token"></param>
        /// <param name="delay"></param>
        void CacheDelayAndFloating(int userId, DelayAndFloating df);
    }
}
