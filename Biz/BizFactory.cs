using GameFW.Utility;
using MobaServer.Biz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobaServer.Biz
{
    public class BizFactory
    {
        public static readonly IAccountBiz accountBiz =new AccountBiz();
        public static readonly IUserBiz userBiz = new UserBiz();
    }
}
