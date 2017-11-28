using System;
using System.Collections.Generic;
using GameFW.Net;
using Protocol;
using Protocol.DTO;
using Protocol.DTO.Fight;
using GameData.Constans;
using Protocol.DTO.Fight.Instance;
using GameFW.Utility;
using UnityEngine;
using GameFW.Nav;
using GameData.Skill;
using GameData.Process;
using MobaServer.Task;
using GameData.Constans.AttackTree;

namespace MobaServer.Logic.Fight
{
    public class FightRoom : AbsMultiHandler, IHandler
    {
        /// <summary>
        /// 战斗单位的缓存
        /// </summary>
        public Dictionary<int, AbsFightInstance> instances = new Dictionary<int, AbsFightInstance>();
        /// <summary>
        /// 记录同步的位置
        /// </summary>
        public Dictionary<int, EntityMoveInfo> instancePos = new Dictionary<int, EntityMoveInfo>();
        /// <summary>
        /// 上一次攻击时的时间戳
        /// </summary>
        public Dictionary<int, long> lastAtkTimeStamp = new Dictionary<int, long>();

        private Dictionary<int, CachePath> reqPathsCache = new Dictionary<int, CachePath>();
        /// <summary>
        /// 目前的离线列表
        /// </summary>
        private List<int> offLineList = new List<int>();
        /// <summary>
        /// 目前的进入战场列表
        /// </summary>
        private List<int> enterList = new List<int>();

        private int heroCount;//英雄总数
        private int entityMinIndex = 0;//战斗单位的最小序号

        private FightRoomDTO fightRoomDTO = new FightRoomDTO();
        public void Init(SelectDTO[] teamOne, SelectDTO[] teamTwo)
        {
            heroCount = teamTwo.Length + teamOne.Length;

            this.instances.Clear();
            offLineList.Clear();
            enterList.Clear();

            fightRoomDTO.entities = new List<AbsFightInstance>();
            fightRoomDTO.buildingEntities = new List<BuildingInstance>();
            foreach (BuildingInstance buildingInstance in BuildingData.BuildingInstances)
            {
                CreateBuilding(buildingInstance);

                if (buildingInstance.instanceId < entityMinIndex)
                    entityMinIndex = buildingInstance.instanceId;
            }

            foreach (SelectDTO item in teamOne)
            {
                CreateHero(item, 1);
            }
            foreach (SelectDTO item in teamTwo)
            {
                CreateHero(item, 2);
            }

            //AddSoilders();

            enterList.Clear();
        }

        /// <summary>
        /// 将第一波士兵添加到初始化数据
        /// </summary>
        private void AddSoilders()
        {
            for (int i = 0; i < 9; i++)
            {
                AddSoilderByTeam(1, i / 3);
            }

            for (int i = 0; i < 9; i++)
            {
                AddSoilderByTeam(2, i / 3);
            }
        }

        private AbsFightInstance GetSoilderInstance(byte teamId, int posId)
        {
            entityMinIndex--;

            AbsFightModel soilderModel = SoilderData.SoilderModels[teamId];
            AbsFightInstance soilderInstance = new AbsFightInstance();
            soilderInstance.instanceId = entityMinIndex;
            soilderInstance.name = soilderModel.name;
            soilderInstance.fightModel = soilderModel;
            soilderInstance.teamId = teamId;
            soilderInstance.atk = soilderModel.atk;
            soilderInstance.def = soilderModel.def;
            soilderInstance.hp = soilderModel.maxHp;
            soilderInstance.atkRange = soilderModel.atkRange;
            soilderInstance.speed = soilderModel.speed;
            soilderInstance.atkSpeed = soilderModel.atkSpeed;
            soilderInstance.eyeRange = soilderModel.eyeRange;

            if (teamId == 1)
            {
                soilderInstance.posX = MapData.soilderBornPointTeamOne[posId].x;
                soilderInstance.posY = MapData.soilderBornPointTeamOne[posId].y;
                soilderInstance.posZ = MapData.soilderBornPointTeamOne[posId].z;
            }
            else
            {
                soilderInstance.posX = MapData.soilderBornPointTeamTwo[posId].x;
                soilderInstance.posY = MapData.soilderBornPointTeamTwo[posId].y;
                soilderInstance.posZ = MapData.soilderBornPointTeamTwo[posId].z;
            }

            return soilderInstance;
        }

        private void AddSoilderByTeam(byte teamId, int posId)
        {
            AbsFightInstance soilderInstance = GetSoilderInstance(teamId, posId);

            fightRoomDTO.entities.Add(soilderInstance);
            instances.Add(soilderInstance.instanceId, soilderInstance);
        }

        private int taskId = -1;

        /// <summary>
        /// 添加定时实例化士兵的任务
        /// </summary>
        private void CreateSoilderTask()
        {
            //2分钟后启动
            taskId = ScheduleUtil.Instance.Schedule(delegate
            {
                //到了2分钟后,先执行创建一波士兵
                CreateSoilders();
                //再增加下一次2分钟的任务
                CreateSoilderTask();
            }, 120 * 1000);
        }

        /// <summary>
        /// 为双方创建一波士兵
        /// </summary>
        /// <param name="team"></param>
        private void CreateSoilders()
        {
            for (int i = 0; i < 9; i++)
            {
                CreateSoilderByTeam(1, i / 3);
            }

            for (int i = 0; i < 9; i++)
            {
                CreateSoilderByTeam(2, i / 3);
            }
        }

        private void CreateSoilderByTeam(byte teamId, int posId)
        {
            AbsFightInstance soilderInstance = GetSoilderInstance(teamId, posId);
            instances.Add(soilderInstance.instanceId, soilderInstance);

            //广播新增小兵指令
            brocast(FightProtocol.CREATE_SOILDER_SCMD, soilderInstance);
        }

        private void CreateBuilding(BuildingInstance building)
        {
            building.hp = building.fightModel.maxHp;
            building.atk = building.fightModel.atk;//攻击力
            building.def = building.fightModel.def;//防御力
            building.speed = building.fightModel.speed;//移动速度
            building.atkSpeed = building.fightModel.atkSpeed;//攻击速度
            building.atkRange = building.fightModel.atkRange;//攻击范围
            building.eyeRange = building.fightModel.eyeRange;//视野范围

            fightRoomDTO.buildingEntities.Add(building);
            instances.Add(building.instanceId, building);
        }

        /// <summary>
        /// 创建英雄
        /// </summary>
        /// <param name="model"></param>
        /// <param name="team"></param>
        private void CreateHero(SelectDTO model, int team)
        {
            HeroModel heroModel = HeroData.HeroModels[model.heroId];
            HeroInstance heroInstance = new HeroInstance();

            heroInstance.instanceId = model.userId;
            heroInstance.name = model.name;
            heroInstance.heroModel = HeroData.HeroModels[model.heroId];
            heroInstance.fightModel = HeroData.HeroModels[model.heroId];
            heroInstance.teamId = (byte)team;
            heroInstance.atk = heroModel.atk;
            heroInstance.def = heroModel.def;
            heroInstance.hp = heroModel.maxHp;
            heroInstance.mp = heroModel.mp;
            heroInstance.atkRange = heroModel.atkRange;
            heroInstance.speed = heroModel.speed;
            heroInstance.atkSpeed = heroModel.atkSpeed;
            heroInstance.eyeRange = heroModel.eyeRange;
            heroInstance.atkArr = heroModel.atkArr;
            heroInstance.defArr = heroModel.atkArr;
            heroInstance.hpArr = heroModel.hpArr;
            heroInstance.mpArr = heroModel.mpArr;
            int skillCount = heroModel.skillCodes.Length;
            FightSkill[] fightSkills = new FightSkill[skillCount];
            for (int i = 0; i < skillCount; i++)
            {
                fightSkills[i] = SkillData.SkillModels[heroModel.skillCodes[i]].GetSkillLevelOne();
            }
            heroInstance.skills = fightSkills;

            if (team == 1)
            {
                heroInstance.posX = MapData.heroBornPointTeamOne.x;
                heroInstance.posY = MapData.heroBornPointTeamOne.y;
                heroInstance.posZ = MapData.heroBornPointTeamOne.z;
            }
            else
            {
                heroInstance.posX = MapData.heroBornPointTeamTwo.x;
                heroInstance.posY = MapData.heroBornPointTeamTwo.y;
                heroInstance.posZ = MapData.heroBornPointTeamTwo.z;
            }

            fightRoomDTO.heroEntities.Add(heroInstance);
            instances.Add(heroInstance.instanceId, heroInstance);
        }

        public override byte GetType()
        {
            return Protocol.Protocol.TYPE_FIGHT;
        }

        public void OnClientClosed(UserToken token, string error)
        {
            Leave(token);
            int userId = GetUserId(token);
            if (instances.ContainsKey(userId))
            {
                if (!offLineList.Contains(userId))
                {
                    offLineList.Add(userId);
                }
            }

            if (offLineList.Count == heroCount)
            {
                GameFW.Utility.Tools.debuger.Log("销毁房间 id: " + GetArea());
                if (taskId != -1)
                {
                    ScheduleUtil.Instance.RemoveMission(taskId);
                }

                ServerEvents.destoryFight(GetArea());
            }
        }

        public void OnMessageReceived(UserToken token, SocketModel sm)
        {
            switch (sm.command)
            {
                case FightProtocol.ENTER_CREQ:
                    EnterBattle(token);
                    break;
                case FightProtocol.POS_SYNC_CREQ:
                    Move(token, sm.GetMessage<PosSyncDTO>());
                    break;
                case FightProtocol.MOVE_CREQ:
                    ReturnNextTargetToClient(token, sm.GetMessage<PathRequestDTO>());
                    break;

                case FightProtocol.ATTACK_CREQ:
                    Attack(sm.GetMessage<AttackDTO>());

                    break;
                case FightProtocol.DAMAGE_CREQ:
                    Damage(token, sm.GetMessage<DamageDTO>());

                    break;
                case FightProtocol.SKILL_UP_CREQ:
                    SkillUp(token, sm.GetMessage<int>());
                    break;
                case FightProtocol.SKILL_CREQ:
                    Skill(token, sm.GetMessage<SkillAttackDTO>());
                    break;
                case FightProtocol.REQ_ATTACK_POS_CREQ:
                    PosDTO posDTO = sm.GetMessage<PosDTO>();
                    int teamId = instances[posDTO.instanceId].teamId;
                    Vector3 atkPos = AtkTreeMgr.Instance.GetAtkTree(teamId).GetNearestAttackablePos(new Vector3(posDTO.posX, posDTO.posY, posDTO.posZ));
                    posDTO.posX = atkPos.x;
                    posDTO.posY = atkPos.y;
                    posDTO.posZ = atkPos.z;
                    posDTO.timeStamp = DateTime.Now.Ticks;
                    Send(token, FightProtocol.ATK_TO_POS_SRES, posDTO);

                    break;

                case FightProtocol.SMALL_TARGET_ARRIVE_CREQ:
                    ReturnNextSmallTarget(token, sm.GetMessage<int>());
                    break;
                case FightProtocol.IDLE_CREQ:
                    int instanceId = sm.GetMessage<int>();
                    if (instancePos.ContainsKey(instanceId))
                        instancePos[instanceId].moveStatus = MoveStatus.Still;
                    else
                        instancePos.Add(instanceId, new EntityMoveInfo(new Vector3(0, 0, 0), false));
                    brocast(FightProtocol.IDLE_BRO, instanceId, token);

                    break;
            }
        }

        /// <summary>
        /// 返回一个移动点，当申请时
        /// </summary>
        /// <param name="token"></param>
        /// <param name="pathRequestDTO"></param>
        private void ReturnNextTargetToClient(UserToken token, PathRequestDTO pathRequestDTO)
        {
            Vector2 srcVec = new Vector3(pathRequestDTO.srcX, pathRequestDTO.srcZ);
            Vector2 tarVec = new Vector3(pathRequestDTO.tarX, pathRequestDTO.tarZ);
            int srcIndex = PathFinder.Instance.GetPolyByPos(srcVec);
            int tarIndex = PathFinder.Instance.GetPolyByPos(tarVec);
            if (srcIndex == tarIndex)
            {
                Send(token, FightProtocol.MOVE_TARGET_SRES, new PosDTO(pathRequestDTO.instanceId, pathRequestDTO.tarX, pathRequestDTO.tarY, pathRequestDTO.tarZ, DateTime.Now.Ticks));
            }
            else
            {
                List<int> path = PathFinder.Instance.GetPaths(srcVec, tarVec);
                if (!reqPathsCache.ContainsKey(pathRequestDTO.instanceId))
                {
                    reqPathsCache.Add(pathRequestDTO.instanceId, new CachePath(path));
                }
                else
                {
                    reqPathsCache[pathRequestDTO.instanceId].path = path;
                    reqPathsCache[pathRequestDTO.instanceId].lastReturnNodeIndex = 0;
                }
                ReturnNextSmallTarget(token, pathRequestDTO.instanceId);
            }
        }

        /// <summary>
        /// 当客户端到达小目标点时的处理
        /// </summary>
        /// <param name="token"></param>
        /// <param name="instanceId"></param>
        private void ReturnNextSmallTarget(UserToken token, int instanceId)
        {
            int nextNodeIndex = NextSmallTargetToClient(instanceId);
            if (nextNodeIndex == -1)
            {
                //告诉Client没路了
                Send(token, FightProtocol.NO_PATH_TO_TARGET_SRES, instanceId);
            }
            else
            {
                Vector2 pos = PathFinder.Instance.GetPolyCenter(nextNodeIndex);
                if (pos != Vector2.zero)
                    Send(token, FightProtocol.MOVE_TARGET_SRES, new PosDTO(instanceId, pos.x, 0f, pos.y, DateTime.Now.Ticks));
                else
                {
                    //告诉Client没路了
                    Send(token, FightProtocol.NO_PATH_TO_TARGET_SRES, instanceId);
                }
            }
        }

        /// <summary>
        /// 将下一个移动地点的索引返回给client
        /// </summary>
        /// <param name="instanceId"></param>
        private int NextSmallTargetToClient(int instanceId)
        {
            if (!reqPathsCache.ContainsKey(instanceId))
                return -1;

            List<int> path = reqPathsCache[instanceId].path;
            int newIndex = Mathf.Min(path.Count - 1, reqPathsCache[instanceId].lastReturnNodeIndex + 3);

            reqPathsCache[instanceId].lastReturnNodeIndex = newIndex;
            return path[newIndex];
        }

        /// <summary>
        /// 攻击
        /// </summary>
        /// <param name="attackDTO"></param>
        private void Attack(AttackDTO attackDTO)
        {
            if (instances.ContainsKey(attackDTO.attackerId) && instances.ContainsKey(attackDTO.targetId))
            {
                bool couldAtk = false;
                //1.验证时间上能否攻击
                float atkSpeed = instances[attackDTO.attackerId].atkSpeed;
                couldAtk = (GetAtkTimeStamp(attackDTO.attackerId) + atkSpeed * 10000000 >= attackDTO.timeStamp);

                //TODO 2.验证位置是否能够攻击

                //3.允许攻击
                if (couldAtk)
                {
                    SetAtkTimeStamp(attackDTO.attackerId, attackDTO.timeStamp);
                    brocast(FightProtocol.ATTACK_BRO, attackDTO);
                }
            }
        }

        private long GetAtkTimeStamp(int instanceId)
        {
            if (lastAtkTimeStamp.ContainsKey(instanceId))
                return lastAtkTimeStamp[instanceId];
            else
                return 0;
        }

        private void SetAtkTimeStamp(int instanceId, long timeStamp)
        {
            if (!lastAtkTimeStamp.ContainsKey(instanceId))
                lastAtkTimeStamp.Add(instanceId, timeStamp);
            else
                lastAtkTimeStamp[instanceId] = timeStamp;
        }



        private void Skill(UserToken token, SkillAttackDTO skillAttackDTO)
        {
            skillAttackDTO.userId = GetUserId(token);
            brocast(FightProtocol.SKILL_BRO, skillAttackDTO);
        }

        private void SkillUp(UserToken token, int value)
        {
            int userId = GetUserId(token);
            HeroInstance player = instances[userId] as HeroInstance;

            if (player.skillFree > 0)
            {
                //遍历角色技能列表 判断是否有此技能
                foreach (FightSkill item in player.skills)
                {
                    if (item.code == value)
                    {
                        //判断玩家等级 是否达到技能提升等级 -1说明无法升级
                        if (item.nextLevel != -1 && item.nextLevel <= player.level)
                        {
                            player.skillFree -= 1;
                            int level = item.level + 1;
                            SkillLevelData data = SkillData.SkillModels[value].levels[level];
                            item.nextLevel = data.nextLevel;
                            item.range = data.range;
                            item.preloadTime = data.preloadTime;
                            item.bootTime = data.bootTime;
                            item.cdTime = data.cdTime;
                            item.level = level;
                            Send(token, FightProtocol.SKILL_UP_SRES, item);
                        }
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 计算伤害、存储结果、并向客户端发送消息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="damageDTO"></param>
        private void Damage(UserToken token, DamageDTO damageDTO)
        {
            int userId = GetUserId(token);
            AbsFightInstance atkInstance;
            int skillLevel = 0;
            //判断攻击者是玩家英雄 还是小兵
            if (damageDTO.userId >= 0)//是英雄
            {
                if (damageDTO.userId != userId) return;
                atkInstance = instances[userId];
                if (damageDTO.skill > 0)
                {
                    skillLevel = (atkInstance as HeroInstance).SkillLevel(damageDTO.skill);
                    if (skillLevel == -1)
                    {
                        return;
                    }
                    else
                    {

                    }
                }
            }
            else//是兵或者建筑
            {
                //TODO:
                atkInstance = instances[userId];
            }

            if (!SkillData.SkillModels.ContainsKey(damageDTO.skill))
                return;

            //1.获取技能方法
            ISkill skill = SkillProcessMap.Get(damageDTO.skill);
            List<int[]> damages = new List<int[]>();
            //2.根据技能定义，获取目标数据和攻击者数据，计算伤害值
            foreach (int[] item in damageDTO.targetDamages)
            {
                AbsFightInstance target = instances[item[0]];
                //计算伤害值并存储
                skill.Damage(skillLevel, ref atkInstance, ref target, ref damages);
                if (target.hp == 0)
                {
                    switch (target.fightModel.category)
                    {
                        case (byte)ModelType.Hero:
                            //击杀英雄
                            //奖励玩家
                            //启动定时任务 指定时间之后发送英雄复活信息 并且将英雄数据设置为满状态


                            break;
                        case (byte)ModelType.Creature:
                            //发送消息
                            //TODO 给钱、经验
                            //移除生物数据

                            break;
                        case (byte)ModelType.Building:
                            //摧毁建筑 给钱，通知进攻树变更

                            break;
                    }
                }
            }
            damageDTO.targetDamages = damages.ToArray();
            brocast(FightProtocol.DAMAGE_BRO, damageDTO);
        }

        /// <summary>
        /// 根据预测进行移动
        /// </summary>
        /// <param name="token"></param>
        /// <param name="moveDTO"></param>
        private void Move(UserToken token, PosSyncDTO moveDTO)
        {
            DelayAndFloating delayAndFloating = userBiz.GetDelayAndFloating(token);
            //time: floating = server - client
            //server = floating + client
            long time = delayAndFloating.floating + moveDTO.timeStamp;//将client的发送时间，转换为server时间
            float timeBtwClientSendAndNow = (DateTime.Now.Ticks - time) * 0.0000001f;
            float speed = instances[moveDTO.instanceId].speed;
            if (timeBtwClientSendAndNow < 0)
            {
                Console.WriteLine("time floating is negative : "+ timeBtwClientSendAndNow.ToString());
                DelayCheckDTO delayCheckDTO = new DelayCheckDTO(5);
                delayCheckDTO.timeStamps.Add(DateTime.Now.Ticks);
                Send(token, Protocol.Protocol.TYPE_TIME, area,TimeProtocol.CHECK_SREQ, delayCheckDTO);
            }
            else
            {
                Console.WriteLine("time floating is : "+ timeBtwClientSendAndNow.ToString());
                //TODO 如果位置与之前存储的位置相差太大，就发送位置校正消息

                Vector3 posNow = GetFuturePos(new Vector3(moveDTO.x, moveDTO.y, moveDTO.z), new Vector3(moveDTO.dirX, moveDTO.dirY, moveDTO.dirZ), timeBtwClientSendAndNow, speed);
                if (!instancePos.ContainsKey(moveDTO.instanceId))
                    instancePos.Add(moveDTO.instanceId, new EntityMoveInfo(posNow, true));
                else {
                    instancePos[moveDTO.instanceId].cVector3.x = posNow.x;
                    instancePos[moveDTO.instanceId].cVector3.y = posNow.y;
                    instancePos[moveDTO.instanceId].cVector3.z = posNow.z;
                    instancePos[moveDTO.instanceId].moveStatus = MoveStatus.Moving;
                }

                PosSyncDTO posSyncDTO = new PosSyncDTO(moveDTO.instanceId, posNow.x, posNow.y, posNow.z, moveDTO.dirX, moveDTO.dirY, moveDTO.dirZ, DateTime.Now.Ticks);
                brocast(FightProtocol.POS_SYNC_BRO, posSyncDTO, token);
            }
        }

        /// <summary>
        /// 根据现在的位置，方向，将要流逝的时间长度，单位移动速度，推算出将来的位置
        /// </summary>
        /// <param name="posNow"></param>
        /// <param name="dir"></param>
        /// <param name="time"></param>
        /// <param name="Speed"></param>
        /// <returns></returns>
        public static Vector3 GetFuturePosAllOriented(Vector3 posNow, Vector3 dir, float time, float Speed)
        {
            float dirX = (dir.x / 180f) * Mathf.PI;
            float dirY = (dir.y / 180f) * Mathf.PI;
            float dirZ = (dir.z / 180f) * Mathf.PI;
            float x = posNow.x + (Mathf.Sin(dirY) + Mathf.Cos(dirZ - Mathf.PI * 0.5f)) * Speed * time;
            float y = posNow.y + (Mathf.Cos(dirX) + Mathf.Sin(dirZ - Mathf.PI * 0.5f)) * Speed * time;
            float z = posNow.z + (Mathf.Sin(dirX) + Mathf.Cos(dirY)) * Speed * time;

            return new Vector3(x, y, z);
        }

        public static Vector3 GetFuturePos(Vector3 posNow, Vector3 dir, float time, float Speed)
        {
            float dirY = ((90f - dir.y) / 180f) * Mathf.PI;
            float x = posNow.x + Mathf.Cos(dirY) * Speed * time;
            float y = posNow.y;
            float z = posNow.z + Mathf.Sin(dirY) * Speed * time;

            return new Vector3(x, y, z);
        }

        private void EnterBattle(UserToken token)
        {
            int userId = GetUserId(token);
            if (IsEntered(token))
                return;
            base.Enter(token);
            if (!enterList.Contains(userId))
            {
                enterList.Add(userId);
            }

            if (enterList.Count == heroCount)
            {
                GameFW.Utility.Tools.debuger.Log("开始战斗");
                fightRoomDTO.isHost = false;
                long minDelayTeamOne = long.MaxValue;
                long minDelayTeamTwo = long.MaxValue;
                int minDelayHostTeamOne = -1;
                int minDelayHostTeamTwo = -1;

                foreach (int i in enterList)
                {
                    AbsFightInstance instance = instances[i];
                    long delay = userBiz.GetDelayAndFloatingById(i).delay;

                    if (instance.teamId == 1)
                    {
                        if (delay < minDelayTeamOne)
                        {
                            minDelayTeamOne = delay;
                            minDelayHostTeamOne = i;
                        }
                    }
                    else
                    {
                        if (delay < minDelayTeamTwo)
                        {
                            minDelayTeamTwo = delay;
                            minDelayHostTeamTwo = i;
                        }
                    }
                }

                brocast(Protocol.Protocol.TYPE_FIGHT, area, FightProtocol.START_BRO, fightRoomDTO, new int[2] { minDelayHostTeamOne, minDelayHostTeamTwo });

                fightRoomDTO.isHost = true;//最后在双方各找一个延迟最低的客户端来做运行AI的主机
                Send(minDelayHostTeamOne, FightProtocol.START_BRO, fightRoomDTO);
                Send(minDelayHostTeamTwo, FightProtocol.START_BRO, fightRoomDTO);
                //CreateSoilderTask();
            }
        }

        private bool CheckHasRepeat()
        {
            HashSet<int> ids = new HashSet<int>();
            foreach (AbsFightInstance fightInstance in fightRoomDTO.entities)
            {
                if (ids.Contains(fightInstance.instanceId))
                {
                    return true;
                }
                else
                {
                    ids.Add(fightInstance.instanceId);
                }
            }

            foreach (BuildingInstance bInstance in fightRoomDTO.buildingEntities)
            {
                if (ids.Contains(bInstance.instanceId))
                {
                    return true;
                }
                else
                {
                    ids.Add(bInstance.instanceId);
                }
            }

            return false;
        }

        internal void Clear()
        {
            instances.Clear();
            offLineList.Clear();
            enterList.Clear();
            heroCount = 0;
            entityMinIndex = 0;
            fightRoomDTO.entities.Clear();
            fightRoomDTO.buildingEntities.Clear();
            fightRoomDTO.heroEntities.Clear();
            reqPathsCache.Clear();
        }
    }
}
