using MobaServer.Cache.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobaServer.Cache
{
    public class CacheFactory
    {
        public static readonly IAccountCache accountCache = new AccountCache();
        public static readonly IUserCache userCache = new UserCache();
    }
}
