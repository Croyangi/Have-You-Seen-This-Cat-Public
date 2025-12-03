using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    
    /*
    [Header("Settings")] [SerializeField] private int roomCount;

    [Range(0.1f, 1f)] [SerializeField] private float branchPathsChance;
    [Range(0f, 1f)] [SerializeField] private float roomAdjacentConnectionChance;

    [SerializeField] private int seed;

    [SerializeField] private List<Room> rooms;
    
    private System.Random rng;

    private void Start()
    {
        Generate(seed); // Example seed
    }

    private void Temp()
    {
        
    }
    
    private bool IsRoomPositionValid(Room room)
    {
        foreach (BoxCollider box in room.boundingBoxes)
        {
            Vector3 center = box.transform.TransformPoint(box.center);
            Vector3 halfExtents = box.size / 2f;
            Quaternion rotation = box.transform.rotation;

            Collider[] results = new Collider[10];
            if (Physics.OverlapBoxNonAlloc(center, halfExtents, results, rotation, LayerMask.GetMask("Room")) > 0)
            {
                return false;
            }
        }

        return true;
    }


    private void Generate(int seed)
    {
        rng = new System.Random(seed);

        Vector2Int start = Vector2Int.zero;
        MarkOccupied(start, roomOccupancySize);
        placedRooms.Add(start);
        Instantiate(roomPrefab, new Vector3(start.x, 0, start.y), Quaternion.identity, transform);

        Vector2Int current = start;
        int placed = 1;

        while (placed < roomCount)
        {
            Vector2Int direction = GetRandomDirection();
            Vector2Int next = current + direction * roomOccupancySize;

            if (CanPlaceRoom(next, roomOccupancySize))
            {
                MarkOccupied(next, roomOccupancySize);
                placedRooms.Add(next);

                Vector3 pos = new Vector3(next.x * (roomSize + roomPaddingSize), 0,
                    next.y * (roomSize + roomPaddingSize));
                Instantiate(roomPrefab, pos, Quaternion.identity, transform);

                int rot = 0;
                if (direction == Vector2Int.up || direction == Vector2Int.down) rot = 90;
                float offset = (roomSize + roomPaddingSize) / 2f;
                Vector3 offsetPos = pos - new Vector3(direction.x, 0, direction.y) * offset;
                //Instantiate(connectorPrefab, offsetPos, Quaternion.Euler(0, rot, 0), transform);

                current = next;
                placed++;
            }
            else if (rng.NextDouble() < branchPathsChance)
            {
                current = placedRooms[rng.Next(placedRooms.Count)];
            }
        }

        ConnectionPass();
    }

    private HashSet<(Vector2Int, Vector2Int)> createdConnections = new HashSet<(Vector2Int, Vector2Int)>();

    private bool TryAddConnection(Vector2Int a, Vector2Int b)
    {
        var pair = a.x < b.x || (a.x == b.x && a.y < b.y) ? (a, b) : (b, a); // ensures consistent ordering

        if (createdConnections.Contains(pair))
            return false;

        createdConnections.Add(pair);
        return true;
    }
    
    private void ConnectionPass()
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var room in placedRooms)
        {
            foreach (var dir in directions)
            {
                Vector2Int neighbor = room + dir * roomOccupancySize;

                if (rng.NextDouble() < roomAdjacentConnectionChance && placedRooms.Contains(neighbor) && TryAddConnection(room, neighbor))
                {
                    // Connect them with a connector
                    Vector3 roomPos = new Vector3(room.x, 0, room.y) * (roomSize + roomPaddingSize);
                    float offset = (roomSize + roomPaddingSize) / 2f;
                    Vector3 connPos = roomPos + new Vector3(dir.x, 0, dir.y) * offset;

                    int rot = (dir == Vector2Int.up || dir == Vector2Int.down) ? 90 : 0;
                    //Instantiate(connectorPrefab, connPos, Quaternion.Euler(0, rot, 0), transform);

                    // Optionally: mark connection so it's not duplicated later
                }
            }
        }
    }

    private void WallPass()
    {
        
    }

    
    private void MarkOccupied(Vector2Int center, Vector2Int size)
    {
        Vector2Int half = size / 2;
        for (int x = -half.x; x <= half.x; x++)
        {
            for (int y = -half.y; y <= half.y; y++)
            {
                occupied.Add(center + new Vector2Int(x, y));
            }
        }
    }

    private Vector2Int GetRandomDirection()
    {
        int roll = rng.Next(4);
        switch (roll)
        {
            case 0: return Vector2Int.up;
            case 1: return Vector2Int.down;
            case 2: return Vector2Int.left;
            case 3: return Vector2Int.right;
        }
        return Vector2Int.zero;
    }
    
    
    /*
    using System.Collections.Generic;
    using UnityEngine;
    
    public class ProceduralGeneration : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int roomCount;
        [SerializeField] private int seed;
    
        [Range(0.1f, 1f)]
        [SerializeField] private float branchChance;
    
        [SerializeField] private Room startingRoomPrefab;
        [SerializeField] private List<Room> roomPrefabs;
    
        private System.Random rng;
        private HashSet<Vector3> occupiedDoorwayPositions = new HashSet<Vector3>();
        private List<Room> placedRooms = new List<Room>();
    
        private void Start()
        {
            Generate(seed);
        }
    
        private void Generate(int seed)
        {
            rng = new System.Random(seed);
    
            Room startRoom = Instantiate(startingRoomPrefab, Vector3.zero, Quaternion.identity, transform);
            placedRooms.Add(startRoom);
            MarkDoorways(startRoom);
    
            Queue<Room> openRooms = new Queue<Room>();
            openRooms.Enqueue(startRoom);
    
            while (placedRooms.Count < roomCount && openRooms.Count > 0)
            {
                Room current = openRooms.Dequeue();
    
                List<Transform> availableDoorways = current.GetAvailableDoorways();
                Shuffle(availableDoorways);
    
                foreach (Transform currentDoor in availableDoorways)
                {
                    if (occupiedDoorwayPositions.Contains(currentDoor.position))
                        continue;
    
                    Room newRoom = Instantiate(GetRandomRoomPrefab(), transform);
                    List<Transform> newRoomDoors = newRoom.GetAvailableDoorways();
                    Shuffle(newRoomDoors);
    
                    bool placed = false;
    
                    foreach (Transform newDoor in newRoomDoors)
                    {
                        foreach (int angle in new int[] { 0, 90, 180, 270 })
                        {
                            newRoom.transform.rotation = Quaternion.Euler(0, angle, 0);
    
                            Vector3 offset = currentDoor.position - newDoor.position;
                            newRoom.transform.position += offset;
    
                            if (IsRoomPositionValid(newRoom))
                            {
                                placedRooms.Add(newRoom);
                                MarkDoorways(newRoom);
                                ConnectDoors(currentDoor, newDoor);
    
                                if (rng.NextDouble() < branchChance)
                                    openRooms.Enqueue(newRoom);
    
                                placed = true;
                                break;
                            }
                            else
                            {
                                newRoom.transform.position -= offset; // revert
                            }
                        }
    
                        if (placed) break;
                    }
    
                    if (placed) break;
                    else Destroy(newRoom.gameObject);
                }
            }
        }
    
        private void MarkDoorways(Room room)
        {
            foreach (Transform door in room.GetAvailableDoorways())
            {
                occupiedDoorwayPositions.Add(door.position);
            }
        }
    
        private void ConnectDoors(Transform a, Transform b)
        {
            a.gameObject.SetActive(false);
            b.gameObject.SetActive(false);
        }
    
        private Room GetRandomRoomPrefab()
        {
            return roomPrefabs[rng.Next(roomPrefabs.Count)];
        }
    
        private bool IsRoomPositionValid(Room room)
        {
            foreach (BoxCollider box in room.boundingBoxes)
            {
                Vector3 center = box.transform.TransformPoint(box.center);
                Vector3 halfExtents = box.size / 2f;
                Quaternion rotation = box.transform.rotation;
    
                Collider[] results = new Collider[10];
                int count = Physics.OverlapBoxNonAlloc(center, halfExtents, results, rotation, LayerMask.GetMask("Room"));
    
                for (int i = 0; i < count; i++)
                {
                    if (results[i].gameObject != box.gameObject)
                        return false;
                }
            }
            return true;
        }
    
        private void Shuffle<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = rng.Next(i, list.Count);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    } 
    
    // Assume Room.cs has:
    // public List<Transform> GetAvailableDoorways();
    // public List<BoxCollider> boundingBoxes;
    */
} 
