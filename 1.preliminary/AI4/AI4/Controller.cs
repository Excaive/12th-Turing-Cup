using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoliceInterface;
using ljqLog;

namespace AI4
{
    public class Controller : PoliceInterface.IControl
    {
        //Log logger = new Log("4");
        private bool infoGot = false;
        private int[] mapSize;
        private List<List<List<int?>>> map = new List<List<List<int?>>>();
        private List<int[]> keysPosition = new List<int[]>();
        private List<int[]> exitPosition = new List<int[]>();
        private List<List<int[]>> elevatorPosition = new List<List<int[]>>();
        private int[] selfPosition = new int[3];
        private List<int[]> hackerPosition = new List<int[]>();
        List<AStarCell> track = new List<AStarCell>();
        List<int[]> pointList = new List<int[]>();

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

        //代码编写位置

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
                    foreach (int i in Range(track.Count() - 1))
                    {
                        track[i].direction = track[i + 1].direction;
                    }
                    track.Remove(track.Last());
                    break;
                }
                else
                {
                    int[] pos = openSet[0].pos;

                    /*
                    string logPos = "";
                    foreach (AStarCell cell in openSet)
                    {
                        logPos += "(" + cell.pos[0] + ", " + cell.pos[1] + ", " + cell.pos[2] + " | " + cell.disSum + " | " + cell.disStart + " | " + cell.disEnd + ") ";
                    }
                    logger.info(logPos);
                    */

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
                                    if (AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].disStart > AStarMap[pos[0]][pos[2]].disStart + 1)
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


        List<int[]> SearchNearestHacker()
        {
            List<List<int[]>> pointListList = new List<List<int[]>>();
            int[] length = { 999, 999, 999 };

            foreach (int i in Range(1, 4))
            {
                List<int[]> pointList = new List<int[]>();

                if (hackerPosition[i - 1] != null)
                {
                    length[i - 1] = FindWayWholeMap(selfPosition, hackerPosition[i - 1]).Count;
                    pointList.Add(hackerPosition[i - 1]);
                }
                else
                {
                    pointList.Add(null);
                }
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

        public void Update(Police police)
        {
            //logger.info("hacker0 Key:" + police.GetHackerKey(1));

            if (!infoGot)
            {
                //获取地图尺寸
                mapSize = police.GetMapInfo();  // 20  2  22 

                //获取地图
                foreach (int x in Range(mapSize[0]))
                {
                    List<List<int?>> nextX = new List<List<int?>>();
                    foreach (int y in Range(1, mapSize[1] + 1))
                    {
                        List<int?> nextY = new List<int?>();
                        foreach (int z in Range(mapSize[2]))
                        {
                            nextY.Add(police.GetMapType(x, y, z));
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
                    keysPosition.Add(police.GetKeysPosition(i));
                    sOut += keysPosition[i][0] + "  " + keysPosition[i][1] + "  " + keysPosition[i][2];
                    //logger.info(sOut);
                }

                //获取出口位置
                foreach (int i in Range(4))
                {
                    string sOut = "Exit" + i + "  ";
                    exitPosition.Add(police.GetExitPosition(i));
                    sOut += exitPosition[i][0] + "  " + exitPosition[i][1] + "  " + exitPosition[i][2];
                    //logger.info(sOut);
                }

                //获取电梯位置
                foreach (int i in Range(1, 3))
                {
                    string sOut = "Elevator" + i + "  ";
                    List<int[]> elevatorTemp = new List<int[]>();
                    int[] ele = police.GetElevatorPosition(i);
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
                    hackerPosition.Add(police.HackerPosition(i));

                infoGot = true;

                //LOG
                /*
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
                selfPosition = police.GetPosition();
            }

            if (!police.isMoving())//不在移动
            {              
                selfPosition = police.GetPosition();

                foreach (int i in Range(1, 4))
                    hackerPosition[i - 1] = police.HackerPosition(i);

                pointList = SearchNearestHacker();

                if (pointList.Count > 0)
                {
                    track = FindWayWholeMap(selfPosition, pointList[0]);

                    if (track.Count > 0)
                    {
                        switch (track[0].direction)
                        {
                            case "U":
                                police.MoveNorth();
                                break;
                            case "D":
                                police.MoveSouth();
                                break;
                            case "L":
                                police.MoveWest();
                                break;
                            case "R":
                                police.MoveEast();
                                break;
                        }
                        track.Remove(track[0]);
                    }
                }
            }
        }
    }
}
