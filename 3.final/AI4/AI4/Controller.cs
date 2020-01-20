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
        //private int[,] keyPosition = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
        private List<int[]> exitPosition = new List<int[]>();
        private List<List<int[]>> elevatorPosition = new List<List<int[]>>();
        private int[] selfPosition = new int[3];
        private List<int[]> hackerPosition = new List<int[]>();
        List<AStarCell> track = new List<AStarCell>();
        List<int[]> pointList = new List<int[]>();
        List<List<string>> hackerMoment = new List<List<string>>();
        private int nearestHacker = 1;
        private string perferenceDirection = "UD";
        private bool[] hackerAlive = { true, true, true };
        private int?[] hackerKey = { 0, 0, 0 };
        private List<HackerInfo> hackerInfos = new List<HackerInfo>();

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

        public class HackerInfo
        {
            public bool alive;
            public int[] pos;
            public int hackerKey;
            public int[] disKey = { 999, 999, 999, 999 };
            public int[] disExit = { 999, 999, 999, 999 };
            public int[] disElevator = { 999, 999 };
            public bool[] targetList = { true, true, true, true };
            public int target = -1;
            public List<bool> targetChange = new List<bool> { false, false, false, false, false };
            public List<AStarCell> track;
            public int[] arrestPos;
            public int disCorrect = 0;
        }

        public void UpdateHackerInfo(HackerInfo info, int num)
        {
            //logger.info("-01-");
            bool targetChangeFlag = false;
            bool newKeyFlag = false;
            info.disCorrect = 0;
            info.alive = hackerAlive[num - 1];
            if (info.alive)
                info.pos = hackerPosition[num - 1];
            if (hackerKey[num - 1] == null)
                info.hackerKey = 4;
            else
            {
                if (info.hackerKey == 0 && (int)hackerKey[num - 1] != 0)
                    newKeyFlag = true;
                info.hackerKey = (int)hackerKey[num - 1];
            }
            //logger.info("-02-");

            if (info.alive)
            {
                if (info.hackerKey == 0)
                {
                    //logger.info("-02A-");
                    //foreach (int i in Range(4))
                    for (int i = 0; i < 4; ++i)
                    {
                        int dis = FindWayWholeMap(info.pos, keysPosition[i]).Count;
                        if (dis <= info.disKey[i])
                            info.disKey[i] = dis;
                        else
                        {
                            info.disKey[i] = dis;
                            info.targetList[i] = false;
                        }
                    }
                    //foreach (int i in Range(4))
                    for (int i = 0; i < 4; ++i)
                    {
                        info.disExit[i] = FindWayWholeMap(info.pos, exitPosition[i]).Count;
                    }
                    //foreach (int i in Range(2))
                    for (int i = 0; i < 2; ++i)
                    {
                        if (info.pos[1] == 0)
                            info.disElevator[i] = FindWayWholeMap(info.pos, elevatorPosition[i][0]).Count;
                        else
                            info.disElevator[i] = FindWayWholeMap(info.pos, elevatorPosition[i][1]).Count;
                    }
                    //logger.info("-02B-");
                    if (info.targetList.Count(s => s == true) == 1)
                    {
                        //foreach (int i in Range(4))
                        for (int i = 0; i < 4; ++i)
                        {
                            if (info.targetList[i] == true)
                            {
                                if (info.target != i)
                                    targetChangeFlag = true;
                                info.target = i;
                            } 
                        }
                    }
                    else if (info.targetList.Count(s => s == true) == 0)
                    {
                        //foreach (int i in Range(4))
                        for (int i = 0; i < 4; ++i)
                            info.targetList[i] = true;
                        info.target = -1;
                    }
                    /*
                    else if (info.targetList[0] == true)
                    {
                        if (info.target != 0)
                            targetChangeFlag = true;
                        info.target = 0;
                    }
                    */
                    //logger.info("-02C-");

                    if (info.target == -1)
                        info.track = null;
                    else if (info.target == 0)
                        info.track = FindWayWholeMap(info.pos, exitPosition[0]);
                    else
                        info.track = Merge(FindWayWholeMap(info.pos, keysPosition[info.target]), FindWayWholeMap(keysPosition[info.target], exitPosition[info.target]));
                    //logger.info("-02D-");
                }
                else if (info.hackerKey != 4)
                {
                    //logger.info("-02E-");
                    if (info.target == -1)
                        info.target = info.hackerKey;
                    //logger.info("-02E1-");
                    int floorAt = info.pos[1] - 1;
                    int floorNotAt = 1 - floorAt;
                    //logger.info("-02E2-");
                    int dis0 = FindWayWholeMap(info.pos, exitPosition[0]).Count;
                    //logger.info("-02E3-");
                    int disExit = FindWayWholeMap(info.pos, exitPosition[info.hackerKey]).Count;
                    //logger.info("-02E4-");
                    int disEle0 = FindWayWholeMap(info.pos, elevatorPosition[0][floorAt]).Count;
                    //logger.info("-02E5-");
                    int disEle1 = FindWayWholeMap(info.pos, elevatorPosition[1][floorAt]).Count;
                    //logger.info("-02F-");

                    if (dis0 != info.disExit[0] && disExit != info.disExit[info.hackerKey])
                    {
                        if (dis0 < info.disExit[0] && disExit > info.disExit[info.hackerKey])
                        {
                            //logger.info("-02G10-");
                            if (info.target != 0)
                                targetChangeFlag = true;
                            info.target = 0;
                            info.track = FindWayWholeMap(info.pos, exitPosition[0]);
                            //logger.info("-02G11-");
                        }
                        else if (dis0 > info.disExit[0] && disExit < info.disExit[info.hackerKey])
                        {
                            //logger.info("-02G20-");
                            if (info.target != info.hackerKey)
                                targetChangeFlag = true;
                            info.target = info.hackerKey;
                            info.track = FindWayWholeMap(info.pos, exitPosition[info.hackerKey]);
                            //logger.info("-02G21-");
                        }
                        else if (dis0 < info.disExit[0] && disExit < info.disExit[info.hackerKey])
                        {
                            //logger.info("-02G30-");
                            info.track = FindWayWholeMap(info.pos, exitPosition[info.target]);
                            //logger.info("-02G31-");
                        }
                        else if (dis0 > info.disExit[0] && disExit > info.disExit[info.hackerKey])
                        {
                            //logger.info("-02G40-");
                            //info.track = null;

                            if (floorAt != exitPosition[info.target][1])
                            {
                                if (disEle0 < info.disElevator[0] && disEle1 > info.disElevator[1])
                                    info.track = Merge(FindWayWholeMap(info.pos, elevatorPosition[0][floorAt]), FindWayWholeMap(elevatorPosition[0][floorNotAt], exitPosition[info.target]));
                                else if (disEle0 > info.disElevator[0] && disEle1 < info.disElevator[1])
                                    info.track = Merge(FindWayWholeMap(info.pos, elevatorPosition[1][floorAt]), FindWayWholeMap(elevatorPosition[1][floorNotAt], exitPosition[info.target]));
                                else if (disEle0 > info.disElevator[0] && disEle1 > info.disElevator[1])
                                    info.track = null;
                            }
                            else
                                info.track = null;
                            //logger.info("-02G41-");

                        }
                    }
                    info.disExit[0] = dis0;
                    info.disExit[info.hackerKey] = disExit;
                    info.disElevator[0] = disEle0;
                    info.disElevator[1] = disEle1;
                    

                    //info.track = FindWayWholeMap(info.pos, exitPosition[info.target]);
                }

                if (newKeyFlag)
                    targetChangeFlag = false;
                info.targetChange.Add(targetChangeFlag);
                info.targetChange.Remove(info.targetChange[0]);

                if (info.track == null)
                {
                    //logger.info("trackLen: 0");
                    info.arrestPos = info.pos;
                }
                else 
                {
                    //logger.info("trackLen: " + info.track.Count);
                    info.arrestPos = info.pos;
                    //logger.info("targetChange" + info.targetChange.Count(s => s == true));
                    if (info.targetChange.Count(s => s == true) < 2)
                    {
                        foreach (AStarCell cell in info.track)
                        {
                            int policeDis = FindWayWholeMap(selfPosition, cell.pos).Count;
                            int HackerPos = info.track.IndexOf(cell) + 1;//FindWayWholeMap(info.pos, cell.pos).Count;
                            if (policeDis * 6 <= HackerPos * 7 + 7)
                            {
                                info.arrestPos = cell.pos;
                                //logger.info("police " + policeDis * 6 + ", hacker " + HackerPos * 7);
                                break;
                            }
                            if (cell == info.track.Last())
                            {
                                //logger.info("Last: police " + policeDis * 6 + ", hacker " + HackerPos * 7);
                                info.disCorrect = 99;
                            }
                            
                        }
                    }
                }
            }
            //logger.info("-03-");
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
            //foreach (int x in Range(mapSize[0]))
            for (int x = 0; x < mapSize[0]; ++x)
            {
                List<AStarCell> nextX = new List<AStarCell>();
                //foreach (int z in Range(mapSize[2]))
                for (int z = 0; z < mapSize[2]; ++z)
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
                    //foreach (int i in Range(track.Count() - 1))
                    for (int i = 0; i < track.Count() - 1; ++i)
                    {
                        track[i].direction = track[i + 1].direction;
                    }
                    track.Remove(track.Last());
                    break;
                }
                else if (perferenceDirection == "UD")
                {
                    int[] pos = openSet[0].pos;
                    int[,] dir = new int[4, 2] { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };
                    AStarMap[pos[0]][pos[2]].state = "close";
                    openSet.Remove(openSet[0]);
                    //foreach (int i in Range(4))
                    for (int i = 0; i < 4; ++i)
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
                else
                {
                    int[] pos = openSet[0].pos;
                    int[,] dir = new int[4, 2] { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };
                    AStarMap[pos[0]][pos[2]].state = "close";
                    openSet.Remove(openSet[0]);
                    //foreach (int i in Range(4))
                    for (int i = 0; i < 4; ++i)
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
                                            AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].direction = "R";
                                            break;
                                        case 1:
                                            AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].direction = "L";
                                            break;
                                        case 2:
                                            AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].direction = "U";
                                            break;
                                        case 3:
                                            AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].direction = "D";
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
                                                AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].direction = "R";
                                                break;
                                            case 1:
                                                AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].direction = "L";
                                                break;
                                            case 2:
                                                AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].direction = "U";
                                                break;
                                            case 3:
                                                AStarMap[pos[0] + dir[i, 0]][pos[2] + dir[i, 1]].direction = "D";
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

            //foreach (int i in Range(1, 4))
            for (int i = 1; i < 4; ++i)
            {
                List<int[]> pointList = new List<int[]>();

                if (hackerPosition[i - 1] != null)
                {
                    /*
                    length[i - 1] = FindWayWholeMap(selfPosition, hackerPosition[i - 1]).Count;
                    pointList.Add(hackerPosition[i - 1]);
                    */
                    if (FindWayWholeMap(selfPosition, hackerPosition[i - 1]).Count <= 5)
                    {
                        length[i - 1] = FindWayWholeMap(selfPosition, hackerPosition[i - 1]).Count + hackerInfos[i - 1].disCorrect;
                        pointList.Add(hackerPosition[i - 1]);
                    }
                    else
                    {
                        length[i - 1] = FindWayWholeMap(selfPosition, hackerInfos[i - 1].arrestPos).Count + hackerInfos[i - 1].disCorrect;
                        pointList.Add(hackerInfos[i - 1].arrestPos);
                    }
                    
                }
                else
                {
                    pointList.Add(null);
                }
                pointListList.Add(pointList);
            }
            //logger.info("length: " + length[0] + ", " + length[1] + ", " + length[2]);

            if ((length[0] <= length[1]) && (length[0] <= length[2]))
            {
                List<int[]> pointListBack = pointListList[0];
                nearestHacker = 1;
                return pointListBack;
            }
            else if ((length[1] <= length[0]) && (length[1] <= length[2]))
            {
                List<int[]> pointListBack = pointListList[1];
                nearestHacker = 2;
                return pointListBack;
            }
            else
            {
                List<int[]> pointListBack = pointListList[2];
                nearestHacker = 3;
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
                //foreach (int x in Range(mapSize[0]))
                for (int x = 0; x < mapSize[0]; ++x)
                {
                    List<List<int?>> nextX = new List<List<int?>>();
                    //foreach (int y in Range(1, mapSize[1] + 1))
                    for (int y = 1; y < mapSize[1] + 1; ++y)
                    {
                        List<int?> nextY = new List<int?>();
                        //foreach (int z in Range(mapSize[2]))
                        for (int z = 0; z < mapSize[2]; ++z)
                        {
                            nextY.Add(police.GetMapType(x, y, z));
                        }
                        nextX.Add(nextY);
                    }
                    map.Add(nextX);
                }

                //获取钥匙位置
                int[] temp = police.GetExitPosition(0);
                keysPosition.Add(temp);
                //foreach (int i in Range(1, 4))
                for (int i = 1; i < 4; ++i)
                {
                    //string sOut = "Key" + i + "  ";
                    keysPosition.Add(police.GetKeysPosition(i));
                    //sOut += keysPosition[i][0] + "  " + keysPosition[i][1] + "  " + keysPosition[i][2];
                    //logger.info(sOut);
                }

                //获取出口位置
                //foreach (int i in Range(4))
                for (int i = 0; i < 4; ++i)
                {
                    //string sOut = "Exit" + i + "  ";
                    exitPosition.Add(police.GetExitPosition(i));
                    //sOut += exitPosition[i][0] + "  " + exitPosition[i][1] + "  " + exitPosition[i][2];
                    //logger.info(sOut);
                }

                //获取电梯位置
                //foreach (int i in Range(1, 3))
                for (int i = 1; i < 3; ++i)
                {
                    //string sOut = "Elevator" + i + "  ";
                    List<int[]> elevatorTemp = new List<int[]>();
                    int[] ele = police.GetElevatorPosition(i);
                    int[] eleF1 = { ele[0], 1, ele[1] };
                    int[] eleF2 = { ele[0], 2, ele[1] };
                    elevatorTemp.Add(eleF1);
                    elevatorTemp.Add(eleF2);
                    elevatorPosition.Add(elevatorTemp);
                    //sOut += elevatorPosition[i - 1][0][0] + "  " + elevatorPosition[i - 1][0][1] + "  " + elevatorPosition[i - 1][0][2] + "\n";
                    //sOut += elevatorPosition[i - 1][1][0] + "  " + elevatorPosition[i - 1][1][1] + "  " + elevatorPosition[i - 1][1][2] + "\n";
                    //logger.info(sOut);
                }

                //获取逃跑者位置
                //foreach (int i in Range(1, 4))
                for (int i = 1; i < 4; ++i)
                {
                    hackerPosition.Add(police.HackerPosition(i));
                    List<string> movement = new List<string>{"UD", "LR", "UD", "LR", "UD"};
                    hackerMoment.Add(movement);
                    HackerInfo newHackerInfo = new HackerInfo();
                    UpdateHackerInfo(newHackerInfo, i);
                    hackerInfos.Add(newHackerInfo);
                }

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

            //foreach (int i in Range(1, 4))
            for (int i = 1; i < 4; ++i)
            {
                int[] hackerPositionTemp = police.HackerPosition(i);
                if (hackerPositionTemp != null)
                {
                    if (hackerPositionTemp[0] != hackerPosition[i - 1][0])
                    {
                        hackerMoment[i - 1].Remove(hackerMoment[i - 1][0]);
                        hackerMoment[i - 1].Add("LR");
                    }
                    else if (hackerPositionTemp[2] != hackerPosition[i - 1][2])
                    {
                        hackerMoment[i - 1].Remove(hackerMoment[i - 1][0]);
                        hackerMoment[i - 1].Add("UD");
                    }
                }
                else
                    hackerAlive[i - 1] = false;
                hackerPosition[i - 1] = police.HackerPosition(i);
            }
            if (!police.isMoving())//不在移动
            {
                //foreach (int i in Range(1, 4))
                for (int i = 1; i < 4; ++i)
                {
                    hackerKey[i - 1] = police.GetHackerKey(i);
                    //logger.info("A" + police.GetGameTime());
                    UpdateHackerInfo(hackerInfos[i - 1], i);
                    //logger.info("B" + police.GetGameTime());
                }
                /*
                logger.info(police.GetGameTime() + "s SelfPos: ("  + selfPosition[0] + ", " + selfPosition[1] + ", " + selfPosition[2] + ") " 
                    + "[HackerInfo] Target 1: " + hackerInfos[0].target + " 2: " + hackerInfos[1].target + " 3: " + hackerInfos[2].target
                    + " Key 1: " + hackerInfos[0].hackerKey + " 2: " + hackerInfos[1].hackerKey + " 3: " + hackerInfos[2].hackerKey
                    + " Pos 1: (" + hackerInfos[0].pos[0] + ", " + hackerInfos[0].pos[1] + ", " + hackerInfos[0].pos[2] + ") "
                    + "2: (" + hackerInfos[1].pos[0] + ", " + hackerInfos[1].pos[1] + ", " + hackerInfos[1].pos[2] + ") "
                    + "3: (" + hackerInfos[2].pos[0] + ", " + hackerInfos[2].pos[1] + ", " + hackerInfos[2].pos[2] + ") "
                    + " ArrestPos 1: (" + hackerInfos[0].arrestPos[0] + ", " + hackerInfos[0].arrestPos[1] + ", " + hackerInfos[0].arrestPos[2] + ") "
                    + "2: (" + hackerInfos[1].arrestPos[0] + ", " + hackerInfos[1].arrestPos[1] + ", " + hackerInfos[1].arrestPos[2] + ") "
                    + "3: (" + hackerInfos[2].arrestPos[0] + ", " + hackerInfos[2].arrestPos[1] + ", " + hackerInfos[2].arrestPos[2] + ") ");
                */

                selfPosition = police.GetPosition();

                pointList = SearchNearestHacker();
                if (hackerMoment[nearestHacker - 1].Count(s => s == "UD") > hackerMoment[nearestHacker - 1].Count(s => s == "LR"))
                    perferenceDirection = "LR";
                else
                    perferenceDirection = "UD";

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
