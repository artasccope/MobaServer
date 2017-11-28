using GameFW.Net;
using MobaServer.Cache.Impl;
using MobaServer.Dao.Model;
using Protocol.DTO;

namespace MobaServer.Biz
{
    public interface IUserBiz
    {
        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="token"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        bool Create(UserToken token, string name);
        /// <summary>
        /// 获取连接对应的用户信息
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        USER GetUser(UserToken token);
        /// <summary>
        /// 通过id获取用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        USER GetUser(int id);
        /// <summary>
        /// 用户上线
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        USER Online(UserToken token);
        /// <summary>
        /// 用户下线
        /// </summary>
        /// <param name="token"></param>
        void Offline(UserToken token);
        /// <summary>
        /// 通过id获取连接对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        UserToken GetToken(int id);
        /// <summary>
        /// 通过账号的连接对象获取，仅在初始登陆验证角色时有效
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        USER GetUserByAccount(UserToken token);
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
