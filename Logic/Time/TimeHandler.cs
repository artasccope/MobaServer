using System;
using System.Collections.Generic;
using System.Linq;
using GameFW.Net;
using Protocol;
using Protocol.DTO;

namespace MobaServer.Logic
{
    /// <summary>
    /// 用户登录时，向server发起一次对时请求，之后每10分钟用户请求一次对时
    /// </summary>
    public class TimeHandler : AbsOnceHandler, IHandler
    {
        public override byte GetType()
        {
            return Protocol.Protocol.TYPE_TIME;
        }

        public void OnClientClosed(UserToken token, string error)
        {
            
        }

        public void OnMessageReceived(UserToken token, SocketModel sm)
        {
            switch (sm.command) {
                case TimeProtocol.CHECK_CREQ://客户端发起对时请求
                    DelayCheckDTO delayCheckDTO = sm.GetMessage<DelayCheckDTO>();
                    if (delayCheckDTO.timeStamps.Count < delayCheckDTO.checkNum * 2)
                    {
                        delayCheckDTO.timeStamps.Add(DateTime.Now.Ticks);
                        Send(token, TimeProtocol.CHECK_SRES, delayCheckDTO);
                    }
                    else
                    {//对时次数够了，计算时差和延迟,这里注意最后一个时间戳是client发来的，因此需要再把现在的时间加上进行计算
                        delayCheckDTO.timeStamps.Add(DateTime.Now.Ticks);
                        DelayAndFloating df = GetDelayAndFloatingEven(delayCheckDTO.timeStamps);
                        userBiz.CacheDelayAndFloating(GetUserId(token), df);
                    }
                    break;

                case TimeProtocol.CHECK_CRES://客户端的回应
                    DelayCheckDTO delayCheck = sm.GetMessage<DelayCheckDTO>();
                    if (delayCheck.timeStamps.Count <= delayCheck.checkNum * 2)
                    {//对时次数不够，就继续对够
                        delayCheck.timeStamps.Add(DateTime.Now.Ticks);
                        Send(token,TimeProtocol.CHECK_SREQ, delayCheck);
                    }

                    if (delayCheck.timeStamps.Count == delayCheck.checkNum * 2 + 1)
                    {//对时次数够了，计算时差和延迟
                        DelayAndFloating df = GetDelayAndFloatingOdd(delayCheck.timeStamps);
                        userBiz.CacheDelayAndFloating(GetUserId(token), df);
                    }
                    break;
            }
        }

        private DelayAndFloating GetDelayAndFloatingEven(List<long> timeStamps)
        {
            long delay = 0;
            long floating = 0;
            for (int i = timeStamps.Count - 1; i >= 2; i -= 2)
            {
                delay += (timeStamps[i] - timeStamps[i - 2]);
            }
            for (int j = timeStamps.Count - 1; j >= 1; j -= 2) {
                floating += (timeStamps[j] - timeStamps[j - 1]);//server-client
            }

            delay /= ((timeStamps.Count -1)/ 2);
            floating = floating / (timeStamps.Count / 2) - delay;
            DelayAndFloating delayAndFloating = new DelayAndFloating(delay, floating);

            return delayAndFloating;
        }

        private DelayAndFloating GetDelayAndFloatingOdd(List<long> timeStamps)
        {
            long delay = 0;
            long floating = 0;
            for (int i = timeStamps.Count - 1; i >= 2; i -= 2)
            {
                delay += (timeStamps[i] - timeStamps[i - 2]);
                floating += (timeStamps[i] - timeStamps[i - 1]);
            }
            delay /= ((timeStamps.Count - 1) / 2);
            floating = floating / (timeStamps.Count / 2) - delay;//server-client
            DelayAndFloating delayAndFloating = new DelayAndFloating(delay, floating);

            return delayAndFloating;
        }
    }
}
