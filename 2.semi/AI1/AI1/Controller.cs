using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HackerInterface;
using ljqLog;

namespace AI4
{
    public class Controller : HackerInterface.IControl
    {
        //Log logger = new Log("1");
        private bool infoGot = false;
        private bool someoneIsArrested = false;
        private int index;
        private int[] mapSize;
        private List<List<List<int?>>> map = new List<List<List<int?>>>();
        private List<int[]> keysPosition = new List<int[]>();
        private List<int[]> exitPosition = new List<int[]>();
        private List<List<int[]>> elevatorPosition = new List<List<int[]>>();
        private int[] selfPosition = new int[3];
        private List<int[]> hackerPosition = new List<int[]>();
        private int[] policePosition = new int[3];
        private int policePositionUpdateSteps = 15;
        private int myKey = 0;
        List<AStarCell> track = new List<AStarCell>();
        List<int[]> pointList = new List<int[]>();

        private int[] target = { 0, 0, 0 };

        private List<int> Range(int max)
        {
            List<int> range_list = new List<int>();
            for (int i = 0; i < max; ++i)
                range_list.Add(i);
            return range_list;
        }

        private List<int> Range(int min, int max)
        {
            List<int> range_list = new List<int>();
            for (int i = min; i < max; ++i)
                range_list.Add(i);
            return range_list;
        }


        //队名录入位置
        public string GetTeamName()
        {
            return "唱跳rap篮球";
        }

        //逻辑代码编写
        public class AStarCell
        {
            public int[] pos;
            public int disStart;
            public int disEnd;
            public int disSum;
            public string direction;
            public string state = "unreached";

            public AStarCell(int[] pos_in)
            {
                pos = pos_in;
                state = "unreached";
            }
        }

        int Distance(int[] position1, int[] position2)
        {
            return Math.Abs(position1[0] - position2[0]) + Math.Abs(position1[2] - position2[2]);
        }

        List<AStarCell> FindWay(int[] startPosition, int[] endPosition)  //, List<List<List<int?>>> map, int[] mapSize
        {
            int floor = startPosition[1];
            List<AStarCell> track = new List<AStarCell>();

            List<List<AStarCell>> AStarMap = new List<List<AStarCell>>();
            foreach (int x in Range(mapSize[0]))
            {
                List<AStarCell> nextX = new List<AStarCell>();
                foreach (int z in Range(mapSize[2]))
                {
                    nextX.Add(new AStarCell(new int[] { x, floor, z }));
                }
                AStarMap.Add(nextX);
            }

            List<AStarCell> openSet = new List<AStarCell>();
            AStarMap[startPosition[0]][startPosition[2]].state = "start";
            AStarMap[startPosition[0]][startPosition[2]].disStart = 0;
            AStarMap[startPosition[0]][startPosition[2]].disEnd = Distance(startPosition, endPosition);
            AStarMap[startPosition[0]][startPosition[2]].disSum = 0 + Distance(startPosition, endPosition);
            openSet.Add(AStarMap[startPosition[0]][startPosition[2]]);

            while (openSet != null)
            {
                openSet = openSet.OrderBy(m => m.disSum).ToList();
                if ((openSet[0].pos[0] == endPosition[0]) && (openSet[0].pos[1] == endPosition[1]) && (openSet[0].pos[2] == endPosition[2]))
                {
                    //logger.info("[FIND!!!]");
                    AStarCell cellBack = openSet[0];
                    track.Add(cellBack);
                    while (cellBack.disStart != 0)
                    {
                        switch (cellBack.direction)
                        {
                            case "U":
                                cellBack = AStarMap[cellBack.pos[0]][cellBack.pos[2] - 1];
                                track.Add(cellBack);
                                break;
                            case "D":
                                cellBack = AStarMap[cellBack.pos[0]][cellBack.pos[2] + 1];
                                track.Add(cellBack);
                                break;
                            case "R":
                                cellBack = AStarMap[cellBack.pos[0] - 1][cellBack.pos[2]];
                                track.Add(cellBack);
                                break;
                            case "L":
                                cellBack = AStarMap[cellBack.pos[0] + 1][cellBack.pos[2]];
                                track.Add(cellBack);
                                break;
                        }
                    }
                    track.Reverse();
                    foreach(int i in Range(track.Count() - 1))
                    {
                        track[i].direction = track[i + 1].direction;
                    }
                    track.Remove(track.Last());
                    break;    
                }
                else
                {
                    int[] pos = openSet[0].pos;                    
                    int[,] dir = new int[4, 2] { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };
                    AStarMap[pos[0]][pos[2]].state = "close";
                    openSet.Remove(openSet[0]);
                    foreach (int i in Range(4))
                    {
                        if ((pos[0] + dir[i, 0] >= 0) && (pos[0] + dir[i, 0] < mapSize[0])
                            && (pos[2] + dir[i, 1] >= 0) && (pos[2] + dir[i, 1] < mapSize[2]))
                        {
                            if (map[pos[0] + dir[i, 0]][floor - 1][pos[2] + dir[i, 1]] != 1)
                            {
                                if (AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].state == "unreached")
                                {
                                    int[] posRound = { pos[0] + dir[i, 0], floor, pos[2] + dir[i, 1] };
                                    AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].state = "open";
                                    openSet.Add(AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]]);
                                    AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].disStart = AStarMap[pos[0]][pos[2]].disStart + 1;
                                    AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].disEnd = Distance(posRound, endPosition);
                                    AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].disSum = AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].disStart + AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].disEnd;
                                    switch (i)
                                    {
                                        case 0:
                                            AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].direction = "U";
                                            break;
                                        case 1:
                                            AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].direction = "D";
                                            break;
                                        case 2:
                                            AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].direction = "R";
                                            break;
                                        case 3:
                                            AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].direction = "L";
                                            break;
                                    }
                                }
                                
                                else if (AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].state == "open")
                                {
                                    if ((AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].disStart > AStarMap[pos[0]][pos[2]].disStart + 1)
                                        || ((AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].disStart == AStarMap[pos[0]][pos[2]].disStart + 1) && (Distance(AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].pos, policePosition) < Distance(AStarMap[pos[0]][pos[2]].pos, policePosition))))
                                    {
                                        int[] posRound = { pos[0] + dir[i, 0], floor, pos[2] + dir[i, 1] };
                                        AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].disStart = AStarMap[pos[0]][pos[2]].disStart + 1;
                                        AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].disEnd = Distance(posRound, endPosition);
                                        AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].disSum = AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].disStart + AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].disEnd;
                                        switch (i)
                                        {
                                            case 0:
                                                AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].direction = "U";
                                                break;
                                            case 1:
                                                AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].direction = "D";
                                                break;
                                            case 2:
                                                AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].direction = "R";
                                                break;
                                            case 3:
                                                AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].direction = "L";
                                                break;
                                        }
                                    }
                                }
                            }    
                        }
                    }
                }
            }
            return track;
        }

        List<AStarCell> Merge(List<AStarCell> list1, List<AStarCell> list2)
        {
            foreach (AStarCell cell in list2)
                list1.Add(cell);
            return list1;
        }


        List<AStarCell> FindWayWholeMap(int[] startPosition, int[] endPosition)
        {
            int floorStart = startPosition[1] - 1;
            int floorEnd = endPosition[1] - 1;

            if (floorStart == floorEnd)
                return FindWay(startPosition, endPosition);
            else
            {
                List<AStarCell> way_Start_0 = FindWay(startPosition, elevatorPosition[0][floorStart]);
                List<AStarCell> way_0_End = FindWay(elevatorPosition[0][floorEnd], endPosition);
                List<AStarCell> way_Start_1 = FindWay(startPosition, elevatorPosition[1][floorStart]);
                List<AStarCell> way_1_End = FindWay(elevatorPosition[1][floorEnd], endPosition);

                int length_0 = way_Start_0.Count + way_0_End.Count;
                int length_1 = way_Start_1.Count + way_1_End.Count;

                if (length_0 < length_1)
                    return Merge(way_Start_0, way_0_End);
                else
                    return Merge(way_Start_1, way_1_End);
            }
        }


        List<AStarCell> FindWaySafe(int[] startPosition, int[] endPosition)
        {
            int floorStart = startPosition[1] - 1;
            int floorEnd = endPosition[1] - 1;

            if (floorStart == floorEnd)
                return FindWay(startPosition, endPosition);
            else
            {
                List<AStarCell> way_Start_0 = FindWay(startPosition, elevatorPosition[0][floorStart]);
                List<AStarCell> way_0_End = FindWay(elevatorPosition[0][floorEnd], endPosition);
                List<AStarCell> way_Start_1 = FindWay(startPosition, elevatorPosition[1][floorStart]);
                List<AStarCell> way_1_End = FindWay(elevatorPosition[1][floorEnd], endPosition);

                List<AStarCell> way0 = Merge(way_Start_0, way_0_End);
                List<AStarCell> way1 = Merge(way_Start_1, way_1_End);

                int way0_RiskDegree = 0;
                int way1_RiskDegree = 0;

                if (policePositionUpdateSteps >= 15)
                {
                    way0_RiskDegree = RiskDegree(way0);
                    way1_RiskDegree = RiskDegree(way1);
                }
                else
                {
                    foreach (int safeLength in Range(way0.Count))
                    {
                        double policeDistance = FindWayWholeMap(policePosition, way0[safeLength].pos).Count / 3.5 * 3.0;
                        if (policeDistance < safeLength)
                        {
                            way0_RiskDegree = -safeLength;
                            break;
                        }
                        if (safeLength == way0.Count - 1)
                        {
                            way0_RiskDegree = -1000 + way0.Count;
                        }
                    }

                    foreach (int safeLength in Range(way1.Count))
                    {
                        double policeDistance = FindWayWholeMap(policePosition, way1[safeLength].pos).Count / 3.5 * 3.0;
                        if (policeDistance < safeLength)
                        {
                            way1_RiskDegree = -safeLength;
                            break;
                        }
                        if (safeLength == way1.Count - 1)
                        {
                            way1_RiskDegree = -1000 + way1.Count;
                        }
                    }
                }

                if (way0_RiskDegree < way1_RiskDegree)
                    return way0;
                else if (way0_RiskDegree > way1_RiskDegree)
                    return way1;
                else if (way0.Count < way1.Count)
                    return way0;
                else
                    return way1;
            }
        }

        int RiskDegree(List<AStarCell> way)
        {
            int degree = 0;
            foreach (AStarCell cell in way)
                degree += CellRiskDegree(cell);
            return degree;
        }

        int CellRiskDegree(AStarCell cell)
        {
            int safeDistance = 6;
            
            if (someoneIsArrested)
                safeDistance = 16;
            
            if (cell.pos[1] != policePosition[1])
                return 0;
            else if (Distance(cell.pos, policePosition) >= safeDistance)
                return 0;
            else
                return safeDistance - Distance(cell.pos, policePosition);
        }

        public class TargetInfo
        {
            public int[] target;
            public int riskDegree;
            public int length;

            public TargetInfo(int[] target_in)
            {
                target = target_in;
            }
        }

        int[] ChooseTarget()
        {
            if (myKey == 0)
            {
                List<TargetInfo> targetInfoList = new List<TargetInfo>();
                foreach (int i in Range(1, 4))
                {
                    if (keysPosition[i] != null)
                    {
                        TargetInfo info = new TargetInfo(keysPosition[i]);
                        info.riskDegree = RiskDegree(FindWaySafe(selfPosition, info.target)) + RiskDegree(FindWaySafe(info.target, exitPosition[i]));
                        info.length = FindWaySafe(selfPosition, info.target).Count + FindWaySafe(info.target, exitPosition[i]).Count;
                        targetInfoList.Add(info);
                    }
                }
                TargetInfo infoExit0 = new TargetInfo(exitPosition[0]);
                infoExit0.riskDegree = RiskDegree(FindWaySafe(selfPosition, infoExit0.target));
                infoExit0.length = FindWaySafe(selfPosition, infoExit0.target).Count;
                targetInfoList.Add(infoExit0);

                targetInfoList = targetInfoList.OrderBy(m => m.length).ToList();
                targetInfoList = targetInfoList.OrderBy(m => m.riskDegree).ToList();

                return targetInfoList[0].target;
            }
            else
            {
                List<TargetInfo> targetInfoList = new List<TargetInfo>();
                TargetInfo info = new TargetInfo(exitPosition[myKey]);
                info.riskDegree = RiskDegree(FindWaySafe(selfPosition, info.target));
                info.length = FindWaySafe(selfPosition, info.target).Count;
                targetInfoList.Add(info);
                TargetInfo infoExit0 = new TargetInfo(exitPosition[0]);
                infoExit0.riskDegree = RiskDegree(FindWaySafe(selfPosition, infoExit0.target));
                infoExit0.length = FindWaySafe(selfPosition, infoExit0.target).Count;
                targetInfoList.Add(infoExit0);

                targetInfoList = targetInfoList.OrderBy(m => m.length).ToList();
                targetInfoList = targetInfoList.OrderBy(m => m.riskDegree).ToList();

                return targetInfoList[0].target;
            }
        }

        int[] ChooseTargetV2()
        {
            if (myKey == 0)
            {
                List<TargetInfo> targetInfoList = new List<TargetInfo>();
                foreach (int i in Range(1, 4))
                {
                    if (keysPosition[i] != null)
                    {
                        TargetInfo info = new TargetInfo(keysPosition[i]);
                        info.riskDegree = i;
                        info.length = FindWaySafe(selfPosition, info.target).Count + FindWaySafe(info.target, exitPosition[i]).Count;
                        targetInfoList.Add(info);
                    }
                }
                TargetInfo infoExit0 = new TargetInfo(exitPosition[0]);
                infoExit0.riskDegree = 0;
                infoExit0.length = FindWaySafe(selfPosition, infoExit0.target).Count;
                targetInfoList.Add(infoExit0);
                
                foreach (TargetInfo tarInfo in targetInfoList)
                {
                    List<AStarCell> tarWay = new List<AStarCell>();
                    if (tarInfo.riskDegree != 0)
                    {
                        tarWay = Merge(FindWaySafe(selfPosition, tarInfo.target), FindWaySafe(tarInfo.target, exitPosition[tarInfo.riskDegree]));
                    }
                    else
                    {
                        tarWay = FindWaySafe(selfPosition, tarInfo.target);
                    }
                    int number = tarInfo.riskDegree;

                    foreach (int safeLength in Range(tarWay.Count))
                    {
                        double policeDistance = FindWayWholeMap(policePosition, tarWay[safeLength].pos).Count / 3.5 * 3.0;
                        if (policeDistance < safeLength)
                        {
                            tarInfo.riskDegree = -safeLength;
                            break;
                        }
                        if (safeLength == tarWay.Count - 1)
                        {
                            tarInfo.riskDegree = -1000 + tarWay.Count;
                        }
                    }
                    //logger.info("[Target] (" + tarInfo.target[0] + ", " + tarInfo.target[1] + ", " + tarInfo.target[2] + ") riskDegree:" + tarInfo.riskDegree + " tarwayLen:" + tarWay.Count + " number:" + number);

                }
                targetInfoList = targetInfoList.OrderBy(m => m.length).ToList();
                targetInfoList = targetInfoList.OrderBy(m => m.riskDegree).ToList();
                return targetInfoList[0].target;
            }
            else
            {
                List<TargetInfo> targetInfoList = new List<TargetInfo>();
                TargetInfo info = new TargetInfo(exitPosition[myKey]);
                info.length = FindWaySafe(selfPosition, info.target).Count;
                targetInfoList.Add(info);
                TargetInfo infoExit0 = new TargetInfo(exitPosition[0]);
                infoExit0.length = FindWaySafe(selfPosition, infoExit0.target).Count;
                targetInfoList.Add(infoExit0);

                foreach (TargetInfo tarInfo in targetInfoList)
                {
                    List<AStarCell> tarWay = FindWaySafe(selfPosition, tarInfo.target);

                    foreach (int safeLength in Range(tarWay.Count))
                    {
                        double policeDistance = FindWayWholeMap(policePosition, tarWay[safeLength].pos).Count / 3.5 * 3.0;
                        if (policeDistance < safeLength)
                        {
                            tarInfo.riskDegree = -safeLength;
                            break;
                        }
                        if (safeLength == tarWay.Count - 1)
                        {
                            tarInfo.riskDegree = -1000 + tarWay.Count;
                        }
                    }
                    //logger.info("[Target2] (" + tarInfo.target[0] + ", " + tarInfo.target[1] + ", " + tarInfo.target[2] + ") riskDegree:" + tarInfo.riskDegree + " tarwayLen:" + tarWay.Count);
                }
                targetInfoList = targetInfoList.OrderBy(m => m.length).ToList();
                targetInfoList = targetInfoList.OrderBy(m => m.riskDegree).ToList();
                return targetInfoList[0].target;
            }
        }


        List<int[]> SearchShortest()
        {
            List<List<int[]>> pointListList = new List<List<int[]>>();
            int[] length = { 0, 0, 0 };

            foreach (int i in Range(1, 4))
            {
                List<int[]> pointList = new List<int[]>();
                length[i - 1] = FindWayWholeMap(selfPosition, keysPosition[i]).Count + FindWayWholeMap(keysPosition[i], exitPosition[i]).Count;
                pointList.Add(keysPosition[i]);
                pointList.Add(exitPosition[i]);
                pointListList.Add(pointList);
            }
            if ((length[0] < length[1]) && (length[0] < length[2]))
            {
                List<int[]> pointListBack = pointListList[0];
                return pointListBack;
            }
            else if ((length[1] < length[0]) && (length[1] < length[2]))
            {
                List<int[]> pointListBack = pointListList[1];
                return pointListBack;
            }
            else
            {
                List<int[]> pointListBack = pointListList[2];
                return pointListBack;
            }
        }


        string HackerInfo(int[] hackerPos)
        {
            if (hackerPos == null)
            {
                return "[null]";
            }
            else
            {
                string str = "(";
                foreach(int i in Range(hackerPos.Length))
                {
                    str += hackerPos[i] + " ";
                }
                str += ") ";
                return str;
            }
        }

        public void Update(Hacker hacker)
        {
            //bool updateTargetFlag = false;
            if (!infoGot)
            {
                //获取当前角色序号
                index = hacker.GetIndex();

                //获取地图尺寸
                mapSize = hacker.GetMapInfo();  // 20  2  22 

                //获取地图
                foreach (int x in Range(mapSize[0]))
                {
                    List<List<int?>> nextX = new List<List<int?>>();
                    foreach (int y in Range(1, mapSize[1] + 1))
                    {
                        List<int?> nextY = new List<int?>();
                        foreach (int z in Range(mapSize[2]))
                        {
                            nextY.Add(hacker.GetMapType(x, y, z));
                        }
                        nextX.Add(nextY);
                    }
                    map.Add(nextX);
                }

                //获取钥匙位置
                int[] temp = { 0, 0, 0 };
                keysPosition.Add(temp);
                foreach (int i in Range(1, 4))
                {
                    string sOut = "Key" + i + "  ";
                    keysPosition.Add(hacker.GetKeysPosition(i));
                    sOut += keysPosition[i][0] + "  " + keysPosition[i][1] + "  " + keysPosition[i][2];
                    //logger.info(sOut);
                }

                //获取出口位置
                foreach (int i in Range(4))
                {
                    string sOut = "Exit" + i + "  ";
                    exitPosition.Add(hacker.GetExitPosition(i));
                    sOut += exitPosition[i][0] + "  " + exitPosition[i][1] + "  " + exitPosition[i][2];
                    //logger.info(sOut);
                }

                //获取电梯位置
                foreach (int i in Range(1, 3))
                {
                    string sOut = "Elevator" + i + "  ";
                    List<int[]> elevatorTemp = new List<int[]>();
                    int[] ele = hacker.GetElevatorPosition(i);
                    int[] eleF1 = { ele[0], 1, ele[1] };
                    int[] eleF2 = { ele[0], 2, ele[1] };
                    elevatorTemp.Add(eleF1);
                    elevatorTemp.Add(eleF2);
                    elevatorPosition.Add(elevatorTemp);
                    sOut += elevatorPosition[i - 1][0][0] + "  " + elevatorPosition[i - 1][0][1] + "  " + elevatorPosition[i - 1][0][2] + "\n";
                    sOut += elevatorPosition[i - 1][1][0] + "  " + elevatorPosition[i - 1][1][1] + "  " + elevatorPosition[i - 1][1][2] + "\n";
                    //logger.info(sOut);
                }

                //获取逃跑者位置
                foreach (int i in Range(1, 4))
                    hackerPosition.Add(hacker.HackerPosition(i));

                //估计抓捕者位置
                policePosition = exitPosition[0];
                //updateTargetFlag = true;
                selfPosition = hacker.GetPosition();
                target = ChooseTargetV2();

                infoGot = true;

                //LOG
                /*
                logger.info("index  " + index);
                logger.info("mapSize  " + mapSize[0] + "  " + mapSize[1] + "  " + mapSize[2] + "  ");

                for (int z = mapSize[2] - 1; z >= 0; z--)
                {
                    string sOut = "[1]";
                    for (int x = 0; x < mapSize[0]; x++)
                    {
                        sOut += map[x][0][z].ToString() + ",";
                    }
                    logger.info(sOut);
                }
                logger.info(" ");

                for (int z = mapSize[2] - 1; z >= 0; z--)
                {
                    string sOut = "[2]";
                    for (int x = 0; x < mapSize[0]; x++)
                    {
                        sOut += map[x][1][z].ToString() + ",";
                    }
                    logger.info(sOut);
                }
                logger.info(" ");
                */

                //初始化
                selfPosition = hacker.GetPosition();
                
                
                string log2 = "POINTLIST ";
                pointList = SearchShortest();
                
                foreach (int[] point in pointList)
                {
                    log2 += "(" + point[0] + ", " + point[1] + ", " + point[2] + ") ";
                }
                //logger.info(log2);
                
            }

            //实时更新逃跑者位置与抓捕者(可能的)位置
            foreach (int i in Range(1, 4))
            {
                int[] tempPosition = hacker.HackerPosition(i);

                if ((tempPosition == null) && (hackerPosition[i - 1] != null))
                {
                    bool someoneIsArrestedTemp = true;
                    foreach (int j in Range(4))
                    {
                        /*
                        if (FindWayWholeMap(hackerPosition[i - 1], exitPosition[j]).Count > 1)
                        {
                            policePosition = hackerPosition[i - 1];
                            someoneIsArrested = true;
                            updateTargetFlag = true;
                            logger.info("Hacker " + i + " is arrested!  PolicePosition: (" + policePosition[0] + ", " + policePosition[1] + ", " + policePosition[2] + ")");
                            break;
                        }
                        */
                        if (FindWayWholeMap(hackerPosition[i - 1], exitPosition[j]).Count <= 1)
                            someoneIsArrestedTemp = false;
                        //logger.info("DISTANCE[" + j + "]: " + FindWayWholeMap(hackerPosition[i - 1], exitPosition[j]).Count);
                    }
                    if (someoneIsArrestedTemp)
                    {
                        policePosition = hackerPosition[i - 1];
                        someoneIsArrested = true;
                        //updateTargetFlag = true;
                        if (myKey != 0)
                            target = ChooseTargetV2();
                        //logger.info("Hacker " + i + " is arrested!  PolicePosition: (" + policePosition[0] + ", " + policePosition[1] + ", " + policePosition[2] + "), " + "policePositionUpdateSteps: " + policePositionUpdateSteps);
                    }
                }
                hackerPosition[i - 1] = hacker.HackerPosition(i);
            }

            if (hacker.Warning())
            {
                int[] policeNewPosition = hacker.GetPolicePosition();
                if (!(policeNewPosition[0] == policePosition[0] && policeNewPosition[1] == policePosition[1] && policeNewPosition[2] == policePosition[2]))
                    target = ChooseTargetV2();
                policePosition = hacker.GetPolicePosition();
                //updateTargetFlag = true;
                //target = ChooseTargetV2();
                policePositionUpdateSteps = 0;
                //logger.info("Warning!  PolicePosition: (" + policePosition[0] + ", " + policePosition[1] + ", " + policePosition[2] + ")" + ", SelfPosition: (" + selfPosition[0] + ", " + selfPosition[1] + ", " + selfPosition[2] + "), " + "policePositionUpdateSteps: " + policePositionUpdateSteps);
            }
            


            //实时更新自己拥有的钥匙
            int newMyKey = hacker.haveKey();
            if ((myKey == 0) && (newMyKey != 0))
            {
                //logger.info("GET KEY " + newMyKey + "!");
                //updateTargetFlag = true;
                myKey = newMyKey;
                target = ChooseTargetV2();
            }
            myKey = newMyKey;

            if (!hacker.isMoving())//不在移动
            {
                selfPosition = hacker.GetPosition();

                if (FindWaySafe(selfPosition, target).Count == 0)
                    target = ChooseTargetV2();

                //int[] target = ChooseTarget();
                /*
                if (updateTargetFlag == true)
                {
                    target = ChooseTargetV2();
                }
                */

                track = FindWaySafe(selfPosition, target);

                if (track.Count > 0)
                {
                    switch (track[0].direction)
                    {
                        case "U":
                            hacker.MoveNorth();
                            break;
                        case "D":
                            hacker.MoveSouth();
                            break;
                        case "L":
                            hacker.MoveWest();
                            break;
                        case "R":
                            hacker.MoveEast();
                            break;
                    }
                    track.Remove(track[0]);
                }
                int timeRest = hacker.GetGameTime();
                //logger.info("timeRest: " + timeRest);

                if (!hacker.Warning())
                    policePositionUpdateSteps += 1;
            }
        }
    }
}
