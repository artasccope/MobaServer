using GameFW.Net;
using Protocol;
using MobaServer.Biz;
using CommonTools.ConcurrentTask;

namespace MobaServer.Logic
{
    public class LoginHandler : AbsOnceHandler, IHandler
    {
        IAccountBiz accountBiz = BizFactory.accountBiz;


        public void OnClientClosed(UserToken token, string error)
        {
            ExecutorPool.Instance.Execute(
                delegate ()
                {
                    accountBiz.Close(token);
                }
                );
        }

        public void OnMessageReceived(UserToken token, SocketModel sm)
        {
            switch (sm.command)
            {
                case LoginProtocol.LOGIN_CREQ:
                    Login(token, sm.GetMessage<AccountInfoDTO>());
                    break;
                case LoginProtocol.REG_CREQ:
                    Reg(token, sm.GetMessage<AccountInfoDTO>());
                    break;
            }
        }

        private void Reg(UserToken token, AccountInfoDTO accountInfoDTO)
        {
            ExecutorPool.Instance.Execute(
                delegate ()
                {
                    int result = accountBiz.Create(token, accountInfoDTO.account, accountInfoDTO.password);
                    Send(token, LoginProtocol.REG_SRES, result);
                }
                );
        }

        private void Login(UserToken token, AccountInfoDTO accountInfoDTO)
        {
            ExecutorPool.Instance.Execute(
                delegate ()
                {
                    int result = accountBiz.Login(token, accountInfoDTO.account, accountInfoDTO.password);

                    Send(token, LoginProtocol.LOGIN_SRES, result);
                }
                );
        }

        public override byte GetType()
        {
            return Protocol.Protocol.TYPE_LOGIN;
        }

    }
}
