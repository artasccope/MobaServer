using Protocol.DTO;
using System.Collections.Generic;

/// <summary>
/// 创建选人模块事件
/// </summary>
/// <param name="teamOne"></param>
/// <param name="teamTwo"></param>
public delegate void CreateSelect(List<int> teamOne, List<int> teamTwo);
/// <summary>
/// 移除选人模块事件  选人房间关闭
/// </summary>
/// <param name="roomId"></param>
public delegate void DestorySelect(int roomId);

/// <summary>
/// 创建战斗模块事件
/// </summary>
/// <param name="teamOne"></param>
/// <param name="teamTwo"></param>
public delegate void CreateFight(SelectDTO[] teamOne, SelectDTO[] teamTwo);
/// <summary>
/// 战斗结束事件
/// </summary>
/// <param name="roomId"></param>
public delegate void DestoryFight(int roomId);
namespace MobaServer
{
    public class ServerEvents
    {
        public static CreateSelect createSelect;
        public static DestorySelect destorySelect;

        public static CreateFight createFight;
        public static DestoryFight destoryFight;
    }
}

