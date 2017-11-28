using GameFW.Nav;
using MobaServer.Logic;
using MobaServer.Net;
using Protocol;
using System;
using UnityEngine;

namespace MobaServer
{
    class Program
    {
        static void Main(string[] args)
        {
            GameFW.Utility.Tools.debuger = GameFW.Utility.ServerDebuger.Instance;


            ServerSettings.maxClient = 9000;
            ServerSettings.gameServerPort = 6630;
            ServerNetCenter.Instance.SetHandlerCenter(new HandlerCenter(MessageEncoder.Decode));
            ServerNetCenter.Instance.Start();
            Console.WriteLine("服务器启动成功");
            while (true) { }
        }
    }
}
