using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MapGenerator : MonoBehaviour
{

    // 가로
    public int width;
    // 세로
    public int height;

    public string seed;
    public bool useRandomSeed;
    public int[] worldmapX;
    public int[] worldmapY;
    public int[,] worldmap;
    int c = 0;

    // Fill 값
    [Range(0, 100)]
    public int randomFillPercent;

    // 맵
    int[,] map;

    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
    }

    void GenerateMap()
    {
        worldmapX = new int[300];
        worldmapY = new int[300];
        map = new int[width, height];
        worldmap = new int[width, height];
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height;  j++)
            {
                worldmap[i, j] = 1;
            }
        }
        RandomFillMap();

        // i 값 = 세대. 총 5번의 변화를 거친다.
        for (int i = 0; i < 5; i++)
        {
            // 자기 자신을 포함한 9개의 셀을 판별하여 상태를 변화시킨다.
            SmoothMap();
        }

        ProcessMap();

        
        // 보더 크기. ( 가장자리 크기)
        int borderSize = 1;
        // map[,] 에서 borderedMap으로 맵 변경
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                // 가장자리 값이 아니라면
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    // map 값을 그대로 보존하여 넣는다.
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                // 지정된 값 만큼의 가장자리 값이라면
                else
                {
                    // 벽으로 만든다.
                    borderedMap[x, y] = 1;
                }
            }
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        // borderedMap 의 값을 mesh쪽으로 넘겨 Rendering을 한다.
        meshGen.GenerateMesh(borderedMap, 1);
    }
    
    
    void ProcessMap()
    {        
        // 벽 삭제
        // 벽 타일 집합들을 확인함.
        List<List<Coord>> wallRegions = GetRegions(1);
        // wall 의 최소값을 정함.
        // 50개(wallThresholdSize) 미만의 타일을 가지는 벽은 사라짐
        int wallThresholdSize = 50;

        // 전체 집합을 확인함.
        foreach (List<Coord> wallRegion in wallRegions)
        {
            // 집합의 타일 갯수가 쓰래쉬홀드보다 적다면
            if (wallRegion.Count < wallThresholdSize)
            {
                // 집합 안의 좌표값을 전부
                foreach (Coord tile in wallRegion)
                {
                    // 룸으로 만들어버림.
                    map[tile.tileX, tile.tileY] = 0;
                    worldmap[tile.tileX, tile.tileY] = 0;
                }
            }
        }
        c = 0;
        Main ma = GetComponent<Main>();
        ma.trans(worldmap, width, height);
        PlayerSpawn player = GetComponent<PlayerSpawn>();
        player.Spawn(worldmap,width,height);


        // 룸 삭제
        // 룸 타일 집합들을 확인함.
        List<List<Coord>> roomRegions = GetRegions(0);
        // room 의 최소값을 정함.
        // 50개의 (roomThres) 미만의 타일을 가지는 룸은 사라짐.
        int roomThresholdSize = 50;
        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            // 집합의 타일 갯수가 쓰래쉬 홀드보다 크다면
            // 즉 룸이 살아 남았다면
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }
        // 내림차순 정렬. 큰 방이 0번
        survivingRooms.Sort();
        // 가장 큰 방을 메인룸으로 만듬.
        survivingRooms[0].isMainRoom = true;
        survivingRooms[0].isAccessibleFromMainRoom = true;

        ConnectClosestRooms(survivingRooms);
    }

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {

        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }
            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        //Debug.DrawLine (CoordToWorldPoint (tileA), CoordToWorldPoint (tileB), Color.green, 100);

        List<Coord> line = GetLine(tileA, tileB);
        foreach (Coord c in line)
        {
            DrawCircle(c, 5);
        }
    }

    void DrawCircle(Coord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (IsInMapRange(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        // 각각 룸 또는 벽의 집합들의 좌표들의 리스트 값들을 다시 하나릐 리스트 값으로 만든다.
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        // 맵 전체를 돌아봄.
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 체크하지 않았고, 타일 타입인 경우.
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    // 벽 또는 룸인 한개의 집합체 좌표값들을 가져옴
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    // List<List<Coord>> regions 에 추가함.
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        // 집합체에 포함된 좌표들을 전부 체크 표시.
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        // 내보냄.
        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        // 좌표들의 집합
        // 룸 또는 벽의 한개 집합의 좌표들의 리스트 값.
        List<Coord> tiles = new List<Coord>();
        // 좌표를 확인했는지 체크하는 용도. 0 - no check. 1 - check
        int[,] mapFlags = new int[width, height];
        // startX, startY 의 좌표값을 tileType에 넣음.
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        // 큐에 startX, startY 좌표의 값을 집어넣음.
        queue.Enqueue(new Coord(startX, startY));

        // 시작 좌표 확인 체크.
        mapFlags[startX, startY] = 1;

        // 큐에 데이터가 남아있다면
        while (queue.Count > 0)
        {
            // 큐값을 빼내어 타일에 넣음.
            Coord tile = queue.Dequeue();
            // List<Coord> tiles에 집어넣음
            tiles.Add(tile);

            // 인접한 셀 확인
            // int x = startX - 1
            // or 인근 셀값이 됨.
            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    // 0미만의 값이 아니고, 끝 값이 아니면서 대각선이 아닌 경우에는
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        // 좌표를 아직 체크하지 않았고
                        // 맵좌표가 타일 타입과 같다면 ( 타일 타입은 입력받아진 시작 값의 타일 타입과 동일 )
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            // 현재 좌표를 큐에 추가함.
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
            // 큐에 값이 남아있는 경우. ( 인근 셀에 아직 확인하지 않은 자기자신 셀과 같은 타입이 있다는 것.)
            // 그 셀의 인근 셀을 다시 확인해야함.
            // 고로 그 셀의 값을 다시 받아서 다시 호출하게 됨.
        }
        return tiles;
    }

    bool IsInMapRange(int x, int y)
    {
        // 0 <= x < width && 0 <= y < height
        // x랑 y값이 0보다 크면서 끝 값이 아닐 경우. true.
        return x >= 0 && x < width && y >= 0 && y < height;
    }
    
    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 가장자리인 경우에는
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    // 벽으로 한다.
                    map[x, y] = 1;
                }
                else
                {
                    // 지정해준 Fill 값 보다 작은 값이 나오면 true( 벽 ) , 아니면 false ( 땅 ) 지정.
                    map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 셀의 자신 포함 주변 9개의 셀을 체크한 값을 nei_WallTile에 대입.
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                // 벽 타일이 4개보다 많았다면
                if (neighbourWallTiles > 4)
                    // 벽으로 만든다.
                    map[x, y] = 1;
                // 벽 타일이 4개보다 작았다면
                else if (neighbourWallTiles < 4)
                    // 땅으로 만든다.
                    map[x, y] = 0;
                // 벽 타일이 4개라면
                // 그대로 둔다.

            }
        }
    }
    
    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        // gridX - 1 , girdX , gridX + 1
        // 3번 loop
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            // girdY - 1, girdY, girdY + 1
            // 3번 loop
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                // nei_X와 nei_Y값이 0보다 크면서 끝 값이 아니라면
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    // nei_X값이 gridX 값이 아니거나 nei_Y값이 gridY값이 아니라면
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        // map의 nei_X, nei_Y 좌표의 값을 wallCount에 추가한다.
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    // 끝 값이거나 0보다 작은 경우에는 wallCount에 값을 추가한다.
                    wallCount++;
                }
            }
        }

        // 즉 자기 자신 셀을 포함한 총 9칸을 확인한다.
        // 가로3, 세로3.
        // 값을 확인하여 벽의 개수를 넘긴다.
        return wallCount;
    }

    // 좌표
    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }


    class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        // 벽 타일을 이웃으로 가지는 roomTile의 위치정보.
        public List<Coord> edgeTiles;
        // 연결 되어 있는 방
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        // 빈방
        public Room()
        {
        }

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edgeTiles = new List<Coord>();
            // 방의 집합체 좌표들을 쭉 둘러봄
            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        // 대각선 배제
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            // 벽이라면
                            if (map[x, y] == 1)
                            {
                                
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMainRoom()
        {
            // 메인 룸과 연결되지 않은 방이 있다면
            if (!isAccessibleFromMainRoom)
            {
                // 메인 룸과 연결되었다고 하고
                isAccessibleFromMainRoom = true;
                // 룸을 연결해?
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            // 룸 A가 메인 룸이랑 연결이 되어있다면
            if (roomA.isAccessibleFromMainRoom)
            {
                // 룸 B를 룸 A랑 연결시켜서 메인이랑 뚫어버리는건가?
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        // 방 크기 비교.
        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }

}