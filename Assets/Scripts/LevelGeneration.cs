using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static System.String;


public class LevelGeneration : MonoBehaviour
{
  public GameObject prefab_room;
  public GameObject teleporterPrefab;
  public GameObject treasurePrefab;
  public GameObject prefab_room2;
  public GameObject prefab_room3;
  public GameObject prefabPoint;
  public GameObject lockedDoorPrefab;
  public GameObject goblinPrefab;
  public ArrayList rooms;
  public GameObject keyPrefab;
  public ArrayList corridors;
  bool constructionComplete = false;
  public GameObject prefabCorridor;
  public GameObject prefabPlayer;
  public int[] gameArea = new int[2];
  public int lockedAreas;
  Room startRoom;

  class BFSTableValue {
    public int distance;
    public Room previousNode;
  }

  class BFSQValue {
    public Room from;
    public Room to;
    public int distance;
  }

  public class Vertex {
    public float[] pos;
    public Room room;
    public ArrayList neighbours = new ArrayList();
    public Vertex() {
      this.neighbours.Add(this);
    }
  }

  class Side {
    public Vertex pointA;
    public Vertex pointB;
  }

  class Corridor : Side {
    public string direction;
    public ArrayList corridorObjects = new ArrayList();
  }

  class Triangle {
    public Vertex a = new Vertex();
    public Vertex b = new Vertex();
    public Vertex c = new Vertex();

    public Vertex[] vertices() {
      return new Vertex[3] {this.a,this.b,this.c};
    }

    public float[] circumcenter() {
      float[] a = this.a.pos;
      float[] b = this.b.pos;
      float[] c = this.c.pos;

      float d = 2*((a[0]*(b[1]-c[1])) + (b[0]*(c[1]-a[1])) + (c[0]*(a[1]-b[1]))
      );

      float ux = (1/d) * (
      (Mathf.Pow(a[0],2)+Mathf.Pow(a[1],2))*(b[1]-c[1]) +
      (Mathf.Pow(b[0],2)+Mathf.Pow(b[1],2))*(c[1]-a[1]) +
      (Mathf.Pow(c[0],2)+Mathf.Pow(c[1],2))*(a[1]-b[1])
      );

      float uy = (1/d) * (
      (Mathf.Pow(a[0],2)+Mathf.Pow(a[1],2))*(c[0]-b[0]) +
      (Mathf.Pow(b[0],2)+Mathf.Pow(b[1],2))*(a[0]-c[0]) +
      (Mathf.Pow(c[0],2)+Mathf.Pow(c[1],2))*(b[0]-a[0])
      );

      return new float[] {ux,uy};
    }

    public Side[] sides() {
      Side[] sides = new Side[3];
      sides[0] = (new Side{
        pointA = this.a, pointB = this.b
      });
      sides[1] = (new Side{
        pointA = this.b, pointB = this.c
      });
      sides[2] = (new Side{
        pointA = this.a, pointB = this.c
      });
      return sides;
    }
  }


  class Leaf {
    public float leftExt;
    public float rightExt;
    public float topExt;
    public float bottomExt;
    Vertex room_center = new Vertex();
    int keyarea;

    public Vertex center() {
      float centerX = (this.rightExt + this.leftExt)/2;
      float centerY = (this.topExt + this.bottomExt)/2;
      this.room_center.pos = new float[] {centerX, centerY};
      return room_center;
    }

    public float width() {
      float width = rightExt - leftExt;
      return width;
    }

    public float height() {
      float height = topExt - bottomExt;
      return height;
    }
  }

  public class Room {
    public float leftExt;
    public float rightExt;
    public float topExt;
    public float bottomExt;
    public Room topcor = null;
    public Room bottomcor = null;
    public Room leftcor = null;
    public Room rightcor = null;
    Vertex room_center = new Vertex();
    public int keyarea; 
    public int nodestr;
    public int doorRoom = -1;
    public ArrayList lockedDoors = new ArrayList();
    public bool startRoom = false;
    public ArrayList neighbours = new ArrayList();

    public Vertex center() {
      float centerX = (this.rightExt + this.leftExt)/2;
      float centerY = (this.topExt + this.bottomExt)/2;
      this.room_center.pos = new float[] {centerX, centerY};
      this.room_center.room = this;
      return room_center;
    }

    public float width() {
      float width = rightExt - leftExt;
      return width;
    }

    public float height() {
      float height = topExt - bottomExt;
      return height;
    }
  }

  ArrayList merge(ArrayList array1, ArrayList array2) {
    ArrayList finalArray = new ArrayList();
    for (int i = 0 ; i < array1.Count ; i++) {
      finalArray.Add(array1[i]);
    }
    for (int i = 0 ; i < array2.Count ; i++) {
      finalArray.Add(array2[i]);
    }
    return finalArray;
  }

  float distance(float[] point1, float[] point2) {
    float distance = Mathf.Pow(
      Mathf.Pow(point2[0]-point1[0], 2f) + Mathf.Pow(point2[1]-point1[1],2f), 0.5f
    );
    return distance;
  }

  bool sidecheck(Side side1, Side side2) {
    if ((Enumerable.SequenceEqual(side1.pointA.pos, side2.pointA.pos) && Enumerable.SequenceEqual(side1.pointB.pos, side2.pointB.pos)) | (Enumerable.SequenceEqual(side1.pointA.pos, side2.pointB.pos) && Enumerable.SequenceEqual(side2.pointA.pos, side1.pointB.pos))) {
      return true;
    } else {
      return false;
    }
  }
      //Next function based from the theories shown within this video Breadth First Search - Finding Shortest Paths in Unweighted Graphs 2021. Available at: https://www.youtube.com/watch?v=T_m27bhVQQQ [Accessed: 23 September 2022].
    
  void BFSExit(Room startRoom, ArrayList rooms) {

    Dictionary<Room,BFSTableValue> BFSTable = new Dictionary<Room,BFSTableValue>();

    Queue<BFSQValue> BFSQueue = new Queue<BFSQValue>();
    ArrayList BFSQueue2 = new ArrayList();

    foreach(Room room in rooms) {
      BFSTable.Add(room,null);
    }
    
    foreach(Room neighbour in startRoom.neighbours) {
      
      BFSQueue.Enqueue(new BFSQValue() {
        from = startRoom,
        to = neighbour,
        distance = 1
      });
    }

    while(BFSTable.ContainsValue(null) && BFSQueue.Count != 0){
  
      //remove item from queue
      BFSQValue item = BFSQueue.Dequeue();

      //check if there is already date for removed route. 
      if(BFSTable[item.to] == null) {
        //add route data to table
        BFSTable[item.to] = new BFSTableValue() {
          distance = item.distance,
          previousNode = item.from
        };
        //for each neighbour add route to queue. 
        foreach(Room neighbour in item.to.neighbours) {
          if(BFSTable[neighbour] == null) {
            BFSQueue.Enqueue(new BFSQValue() {
              from = item.to,
              to = neighbour,
              distance = (BFSTable[item.to].distance+1)
            });
          }
        }
      }
    }
    int largestRoom = 0;
    Room targetRoom = null;
    foreach(KeyValuePair<Room, BFSTableValue> pair in BFSTable){
      // print(pair.Key);
      if(pair.Value != null) {
        if(pair.Value.distance > largestRoom && pair.Key.keyarea != startRoom.keyarea) {
          targetRoom = pair.Key;
          largestRoom = pair.Value.distance;
        }
      }
    }
    GameObject teleporter = Instantiate(teleporterPrefab, new Vector3(
    (targetRoom.rightExt + targetRoom.leftExt)/2,
    1,
    (targetRoom.bottomExt + targetRoom.topExt)/2
    ),Quaternion.identity);
  
    GameObject goblin = Instantiate(goblinPrefab, new Vector3(0,0,0),Quaternion.identity);
    
  }
  //Next function based on the theories shown within this video Breadth First Search - Finding Shortest Paths in Unweighted Graphs 2021. Available at: https://www.youtube.com/watch?v=T_m27bhVQQQ [Accessed: 23 September 2022].
    
  void BFSKey(Room startRoom, ArrayList rooms) {

    Dictionary<Room,BFSTableValue> BFSTable = new Dictionary<Room,BFSTableValue>();
    Queue<BFSQValue> BFSQueue = new Queue<BFSQValue>();
    ArrayList KeyOptions = new ArrayList();
  
    foreach(Room room in rooms) {
      BFSTable.Add(room,null);
    }
    
    foreach(Room neighbour in startRoom.neighbours) {
      if(neighbour.keyarea == startRoom.keyarea) {
        BFSQueue.Enqueue(new BFSQValue() {
          from = startRoom,
          to = neighbour,
          distance = 1
        });
      }
    }
    // //if BFS Table has empty info or queue is empty stop loop;
    while(BFSTable.ContainsValue(null) && BFSQueue.Count != 0){
  
      //remove item from queue
      BFSQValue item = BFSQueue.Dequeue();

      //check if there is already date for removed route. 
      if(BFSTable[item.to] == null) {
        //add route data to table
        BFSTable[item.to] = new BFSTableValue() {
          distance = item.distance,
          previousNode = item.from
        };
        //for each neighbour add route to queue. 
        foreach(Room neighbour in item.to.neighbours) {
          if(item.to.keyarea == neighbour.keyarea) {
            if(BFSTable[neighbour] == null) {
              BFSQueue.Enqueue(new BFSQValue() {
                from = item.to,
                to = neighbour,
                distance = (BFSTable[item.to].distance+1)
              });
            }
          }
        }
      }
    }
    
    foreach(KeyValuePair<Room, BFSTableValue> pair in BFSTable){
      if(pair.Value != null) {
        KeyOptions.Add(pair.Key);
      }
    }
    
    Room keyRoom = (Room)(KeyOptions[Random.Range(0,KeyOptions.Count)]);
    Instantiate(keyPrefab, new Vector3(
          (keyRoom.rightExt + keyRoom.leftExt)/2,
          1,
          (keyRoom.bottomExt + keyRoom.topExt)/2
          ),Quaternion.identity);
  }
  //This implementation was adapted from psuedocode listen on the wikipedia page for the bowyer watson algoirithm: 
  // https://en.wikipedia.org/wiki/Bowyer%E2%80%93Watson_algorithm#/media/File:Bowyer-Watson_1.png
  ArrayList triangulation(ArrayList rooms) {
    ArrayList triangulation = new ArrayList();
    ArrayList badTriangles = new ArrayList();
    ArrayList supercleanup = new ArrayList();
    ArrayList polygon;
    Triangle superTriangle = new Triangle() {
      a = new Vertex {pos = new float[] {-1000, -1000}},
      b = new Vertex {pos = new float[] {1000, -1000}},
      c = new Vertex {pos = new float[] {0,     1000}},

    };
    triangulation.Add(superTriangle);

    foreach (Room room in rooms) {
      badTriangles = new ArrayList();
      polygon = new ArrayList();
      // print(polygon.Count);
      foreach (Triangle tri in triangulation) {
        if (distance(room.center().pos,tri.circumcenter()) <= distance(tri.c.pos, tri.circumcenter())) {
          badTriangles.Add(tri);
        }
      }

      foreach (Triangle badtri in badTriangles) {
        foreach (Side side in badtri.sides()) {
          bool appendside = true;
          foreach (Triangle badtri2 in badTriangles) {
            if (badtri2 == badtri) {
              continue;
            }
            foreach (Side side2 in badtri2.sides()) {
              if (sidecheck(side, side2)) {
                appendside = false;
              }
            }
          }
          if (appendside == true) {
            polygon.Add(side);
          }
        }
      }
      foreach (Triangle badtri in badTriangles) {
        triangulation.Remove(badtri);
      }
    // triangulate point
    foreach (Side side in polygon) {
      Triangle newTri = new Triangle();
      newTri.a = side.pointA;
      newTri.b = side.pointB;
      newTri.c = room.center();
      triangulation.Add(newTri);
      }
    }
  // cleanup super triangle : works why??
    foreach (Triangle tri in triangulation) {
      foreach (Vertex vertex in tri.vertices()){
        if (vertex == superTriangle.a | vertex == superTriangle.b | vertex == superTriangle.c) {
          supercleanup.Add(tri);
        }
      }
    }
    foreach (Triangle tri in supercleanup) {
      triangulation.Remove(tri);
    }
    foreach (Triangle tri in triangulation) {
      tri.a.neighbours.Add(tri.b);
      tri.a.neighbours.Add(tri.c);
      tri.b.neighbours.Add(tri.a);
      tri.b.neighbours.Add(tri.c);
      tri.c.neighbours.Add(tri.a);
      tri.c.neighbours.Add(tri.b);
    }
    return triangulation;
  }

  ArrayList BSP(float x, float x2, float y, float y2, int depth,int  originWidth, int originHeight) {
    float direction = Random.Range(0f,1f);
    float width = x2-x;
    float height = y2-y;
    Leaf newLeaf;

    // Restrict the zone of the random slice to avoid very thin sections.
    float xSplit = Random.Range((x+width/3),(x+(width/3)*2));
    float ySplit = Random.Range((y+height/3),(y+(height/3)*2));
    depth += 1;
    if (depth < 6 ) {
      // Dont split nodes which are too small in relations to the original shape.
      if (direction >= 0.5f & width/originWidth > 0.15) {
        ArrayList newShape1 = BSP(x,      xSplit, y,  y2, depth, originWidth, originHeight);
        ArrayList newShape2 = BSP(xSplit, x2,     y,  y2, depth, originWidth, originHeight);
        return merge(newShape1, newShape2);
      }else if (height/originHeight > 0.15){
        ArrayList newShape1 = BSP(x, x2,  y,      ySplit, depth, originWidth, originHeight);
        ArrayList newShape2 = BSP(x, x2,  ySplit, y2,     depth,originWidth, originHeight);
        return merge(newShape1, newShape2);
      }else {
        newLeaf = new Leaf(){
          leftExt = x,
          rightExt = x2,
          bottomExt = y,
          topExt = y2
        };
        ArrayList newLeafArray = new ArrayList();
        newLeafArray.Add(newLeaf);
        return newLeafArray;
      }
    }else {
      newLeaf =   new Leaf(){
        leftExt = x,
        rightExt = x2,
        bottomExt = y,
        topExt = y2
      };
      ArrayList newRoom = new ArrayList();
      newRoom.Add(newLeaf);
      return newRoom;
    }
  }

    ArrayList GenerateRooms(int xArea, int yArea) {
      ArrayList testSpace = BSP(0,xArea,0,yArea,0,xArea,yArea);
      ArrayList culledTriangulation = new ArrayList();
      ArrayList rooms = new ArrayList();
      bool append;

      //Generate rooms that fit in leafs 
      foreach (Leaf leaf in testSpace) {
        float roomWidth  = Random.Range(leaf.width()*0.4f,leaf.width()*0.8f);
        float roomHeight = Random.Range(leaf.height()*0.4f,leaf.height()*0.8f);

        if(roomHeight > roomWidth*2) {
          roomHeight = roomHeight/2;
        }
        if(roomWidth > roomHeight*2) {
          roomWidth = roomWidth/2;
        }

        float roomY = Random.Range(leaf.topExt - (roomHeight/2), leaf.bottomExt + (roomHeight/2));
        float roomX = Random.Range(leaf.rightExt - (roomWidth/2), leaf.leftExt + (roomWidth/2));

        Room room = new Room() {leftExt = roomX-(roomWidth/2), rightExt = roomX+(roomWidth/2), topExt = roomY+(roomHeight/2), bottomExt = roomY-(roomHeight/2)};

        rooms.Add(room);
      }

      ArrayList testTriangulation = triangulation(rooms);

      //append unique vertices to new list
      ArrayList culledVertices = new ArrayList();
      ArrayList vertices = new ArrayList();
      foreach (Triangle tri in testTriangulation) {
        foreach (Vertex vertex in tri.vertices()) {
          if (vertices.Contains(vertex) == false) {
            vertices.Add(vertex);
          }
          culledVertices.Add(vertex);
        }
      }
      //generate corridors;
      ArrayList corridors = new ArrayList();
  
      
      foreach(Vertex vertex in vertices) {
        foreach(Vertex neighbour in vertex.neighbours) {
          if (vertex != neighbour) {
            if (vertex.room.leftExt < neighbour.room.rightExt && vertex.room.rightExt > neighbour.room.leftExt) {
              if (vertex.pos[1] < neighbour.pos[1]) {
                if(vertex.room.topcor == null && neighbour.room.bottomcor == null) {
                  vertex.room.topcor = neighbour.room;
                  neighbour.room.bottomcor = vertex.room;
                  corridors.Add(new Corridor() {pointA = vertex, pointB = neighbour, direction = "top"});
                  vertex.room.neighbours.Add(neighbour.room);
                  neighbour.room.neighbours.Add(vertex.room);
                }
              }
            }
            if (vertex.room.bottomExt < neighbour.room.topExt && vertex.room.topExt > neighbour.room.bottomExt) {
              if (vertex.pos[0] < neighbour.pos[0]) {
                if(vertex.room.leftcor == null && neighbour.room.rightcor == null) {
                    vertex.room.leftcor = neighbour.room;
                    neighbour.room.rightcor = vertex.room;
                    corridors.Add(new Corridor() {pointA = vertex, pointB = neighbour, direction = "left"});
                    vertex.room.neighbours.Add(neighbour.room);
                    neighbour.room.neighbours.Add(vertex.room);
                }
              }
            }
          }
        }
      }

      ArrayList badRooms = new ArrayList();

      foreach(Room room in rooms) {
        if(room.leftcor == null && room.topcor == null && room.rightcor == null && room.bottomcor == null) {
          badRooms.Add(room);
        }
      }
      foreach(Room room in badRooms) {
        rooms.Remove(room);
      }

      for(int i=0;i<lockedAreas;i++){
        if(i == 0) {
          foreach(Room room in rooms) {
            room.keyarea = 0;
          }
        }else{
          foreach(Room room in rooms) {
            if(room.center().pos[0] < 100/(2*i) && room.center().pos[1] < 50) {
              room.keyarea = i;
            }
          }
        }
      }

              // Finding Neighbours
    foreach (Room room in rooms) {
      if(room.keyarea == 0) {
        foreach(Room neighbour in room.neighbours) {
          if(neighbour.keyarea == 1) {
            room.doorRoom = 1;
            if(neighbour == room.leftcor) {
              room.lockedDoors.Add('r');
            }else if(neighbour == room.rightcor) {
              room.lockedDoors.Add('l');
            }else if(neighbour == room.topcor) {
              room.lockedDoors.Add('t');
            }else if (neighbour == room.bottomcor) {
              room.lockedDoors.Add('b');
            }
          }
        }
      }
    }

    ArrayList roomsAndCorridors = new ArrayList() {
      rooms,
      corridors
    };

    return roomsAndCorridors;
  }

  void ConstructRooms(ArrayList rooms, ArrayList corridors) {
    ArrayList badRooms = new ArrayList();
    foreach(Room room in rooms) {
      if(room.leftcor == null && room.topcor == null && room.rightcor == null && room.bottomcor == null) {
        badRooms.Add(room);
      }
    }
    foreach(Room room in badRooms) {
      rooms.Remove(room);
    }

    foreach(Corridor corridor in corridors) {
    // GameObject line = Instantiate(prefabLineRender2, new Vector3(0f,0f,0f), Quaternion.identity);
    //   LineRenderer linerenderer = line.GetComponent<LineRenderer>();
      // linerenderer.SetPosition(0, new Vector3(corridor.pointA.pos[0],0f,corridor.pointA.pos[1]));
      // linerenderer.SetPosition(1, new Vector3(corridor.pointB.pos[0],0f,corridor.pointB.pos[1]));

      if (corridor.direction == "left") {
        float midPointX = (corridor.pointB.room.leftExt + corridor.pointA.room.rightExt) / 2;
        float midpointY = (corridor.pointB.pos[1] + corridor.pointA.pos[1])/2;
        float thirdSize = (corridor.pointB.room.leftExt - corridor.pointA.room.rightExt)/3;
        float halfSize = (corridor.pointB.room.leftExt - corridor.pointA.room.rightExt)/2;
        
        GameObject corridorMid = Instantiate(prefabCorridor, new Vector3(midPointX,0f,midpointY), Quaternion.identity);
      
        GameObject corridorLeft = Instantiate(prefabCorridor, new Vector3(corridor.pointA.room.rightExt+halfSize/2,0f,corridor.pointA.pos[1]), Quaternion.identity);
        
        GameObject corridorRight = Instantiate(prefabCorridor, new Vector3(corridor.pointB.room.leftExt-halfSize/2,0f,corridor.pointB.pos[1]), Quaternion.identity);


        CorridorMeta corridorMidMeta = corridorMid.GetComponent<CorridorMeta>();
        corridorMidMeta.horizontal = true;
        corridorMidMeta.centerPiece = true;
        corridorMidMeta.room1 = corridor.pointA.room;
        corridorMidMeta.room2 = corridor.pointB.room;
        corridorMidMeta.pairedCorridors.Add(corridorMid);
        corridorMidMeta.pairedCorridors.Add(corridorLeft);
        corridorMidMeta.pairedCorridors.Add(corridorRight);
        
        CorridorMeta corridorLeftMeta = corridorLeft.GetComponent<CorridorMeta>();
        corridorLeftMeta.horizontal = true;
        corridorLeftMeta.room1 = corridor.pointA.room;
        corridorLeftMeta.room2 = corridor.pointB.room;
        corridorLeftMeta.pairedCorridors.Add(corridorMid);
        corridorLeftMeta.pairedCorridors.Add(corridorLeft);
        corridorLeftMeta.pairedCorridors.Add(corridorRight);


        CorridorMeta corridorRightMeta = corridorRight.GetComponent<CorridorMeta>();
        corridorRightMeta.horizontal = true;
        corridorRightMeta.room1 = corridor.pointA.room;
        corridorRightMeta.room2 = corridor.pointB.room;
        corridorRightMeta.pairedCorridors.Add(corridorMid);
        corridorRightMeta.pairedCorridors.Add(corridorLeft);
        corridorRightMeta.pairedCorridors.Add(corridorRight);


        corridor.corridorObjects.Add(corridorMid);
        corridor.corridorObjects.Add(corridorRight);
        corridor.corridorObjects.Add(corridorLeft);

        
        if(corridor.pointA.pos[1]<corridor.pointB.pos[1]) {
          corridorMid.transform.localScale = new Vector3(2f, 1, (corridor.pointB.pos[1]-corridor.pointA.pos[1])+2f);
        }else{
          corridorMid.transform.localScale = new Vector3(2f, 1, (corridor.pointA.pos[1]-corridor.pointB.pos[1])+2f);
        }
        corridorLeft.transform.localScale = new Vector3(halfSize, 1, 2f);
        corridorRight.transform.localScale = new Vector3(halfSize, 1, 2f);
      }

      if (corridor.direction == "top") {
        
        float midPointY = (corridor.pointB.room.bottomExt + corridor.pointA.room.topExt) / 2;
        float midPointX = (corridor.pointB.pos[0] + corridor.pointA.pos[0])/2;
        float halfSize = (corridor.pointB.room.bottomExt - corridor.pointA.room.topExt)/2;

        GameObject corridorTop = Instantiate(prefabCorridor, new Vector3(corridor.pointA.pos[0],0f,corridor.pointA.room.topExt+halfSize/2), Quaternion.identity);
        GameObject corridorBottom = Instantiate(prefabCorridor, new Vector3(corridor.pointB.pos[0],0f,corridor.pointB.room.bottomExt-halfSize/2), Quaternion.identity);

        GameObject corridorMid = Instantiate(prefabCorridor, new Vector3(midPointX,0f,midPointY), Quaternion.identity);

        CorridorMeta corridorMidMeta = corridorMid.GetComponent<CorridorMeta>();
        corridorMidMeta.horizontal = false;
        corridorMidMeta.centerPiece = true;
        corridorMidMeta.room1 = corridor.pointA.room;
        corridorMidMeta.room2 = corridor.pointB.room;
        corridorMidMeta.pairedCorridors.Add(corridorMid);
        corridorMidMeta.pairedCorridors.Add(corridorTop);
        corridorMidMeta.pairedCorridors.Add(corridorBottom);
        
        CorridorMeta corridorTopMeta = corridorTop.GetComponent<CorridorMeta>();
        corridorTopMeta.horizontal = false;
        corridorTopMeta.room1 = corridor.pointA.room;
        corridorTopMeta.room2 = corridor.pointB.room;
        corridorTopMeta.pairedCorridors.Add(corridorMid);
        corridorTopMeta.pairedCorridors.Add(corridorTop);
        corridorTopMeta.pairedCorridors.Add(corridorBottom);


        CorridorMeta corrdorBottomMeta = corridorBottom.GetComponent<CorridorMeta>();
        corrdorBottomMeta.horizontal = false;
        corrdorBottomMeta.room1 = corridor.pointA.room;
        corrdorBottomMeta.room2 = corridor.pointB.room;
        corrdorBottomMeta.pairedCorridors.Add(corridorMid);
        corrdorBottomMeta.pairedCorridors.Add(corridorTop);
        corrdorBottomMeta.pairedCorridors.Add(corridorBottom);


        corridor.corridorObjects.Add(corridorMid);
        corridor.corridorObjects.Add(corridorTop);
        corridor.corridorObjects.Add(corridorBottom);
        
        if(corridor.pointA.pos[0]<corridor.pointB.pos[0]) {
          corridorMid.transform.localScale = new Vector3((corridor.pointB.pos[0]-corridor.pointA.pos[0])+2f, 1, 2f);
        }else{
          corridorMid.transform.localScale = new Vector3( (corridor.pointA.pos[0]-corridor.pointB.pos[0])+2f, 1, 2f);
        }
        corridorTop.transform.localScale = new Vector3(2f, 1, halfSize);
        corridorBottom.transform.localScale = new Vector3(2f, 1, halfSize);
      }
    }
  
    GameObject gameObjectRoom;
    GameObject topWall = prefab_room;
    GameObject topWall1 = prefab_room;
    GameObject topWall2 = prefab_room;

    //Generating Rooms
    foreach (Room room in rooms) {
      if(room.keyarea == 0) {
        gameObjectRoom = Instantiate(prefab_room, new Vector3(
        (room.rightExt + room.leftExt)/2,
        0,
        (room.bottomExt + room.topExt)/2
        ),Quaternion.identity);
      }else if(room.keyarea == 1){
        gameObjectRoom = Instantiate(prefab_room2, new Vector3(
        (room.rightExt + room.leftExt)/2,
        0,
        (room.bottomExt + room.topExt)/2
        ),Quaternion.identity);
      }else{
        gameObjectRoom = Instantiate(prefab_room3, new Vector3(
        (room.rightExt + room.leftExt)/2,
        0,
        (room.bottomExt + room.topExt)/2
        ),Quaternion.identity);
      }

      gameObjectRoom.transform.localScale = new Vector3(room.width(),1f,room.height());
    }  

    //Generating Room Walls
    foreach(Room room in rooms) {
      if(room.topcor == null) {
        topWall = Instantiate(prefab_room3, new Vector3(
        (room.rightExt+room.leftExt)/2,
        1f,
        room.topExt
        ),Quaternion.identity);

        topWall.transform.localScale = new Vector3(room.width(),3f,0.5f);
      }else {
        topWall1 = Instantiate(prefab_room3, new Vector3(
        room.leftExt+((room.rightExt-room.leftExt)-2f)/4,
        1f,
        room.topExt
        ),Quaternion.identity);
        topWall2 = Instantiate(prefab_room3, new Vector3(
        room.rightExt - ((room.rightExt-room.leftExt)-2f)/4,
        1f,
        room.topExt
        ),Quaternion.identity);
        
        topWall1.transform.localScale = new Vector3(((room.width()-2f)/2),4f,0.5f);
        topWall2.transform.localScale = new Vector3(((room.width()-2f)/2),4f,0.5f);
      }

      if(room.bottomcor == null) {
        topWall = Instantiate(prefab_room3, new Vector3(
          (room.rightExt+room.leftExt)/2,
          1f,
          room.bottomExt
          ),Quaternion.identity);
    
          topWall.transform.localScale = new Vector3(room.width(),4f,0.5f);
      }else {
        topWall1 = Instantiate(prefab_room3, new Vector3(
          room.leftExt+((room.rightExt-room.leftExt)-2f)/4,
          1f,
          room.bottomExt
          ),Quaternion.identity);
        topWall2 = Instantiate(prefab_room3, new Vector3(
        room.rightExt - ((room.rightExt-room.leftExt)-2f)/4,
        1f,
        room.bottomExt
        ),Quaternion.identity);
        
        topWall1.transform.localScale = new Vector3(((room.width()-2f)/2),4f,0.5f);
        topWall2.transform.localScale = new Vector3(((room.width()-2f)/2),4f,0.5f);
      }
      
      if(room.rightcor == null) {
        topWall = Instantiate(prefab_room3, new Vector3(
          room.leftExt,
          1f,
          (room.topExt+room.bottomExt)/2
          ),Quaternion.identity);
          
          topWall.transform.localScale = new Vector3(0.5f,4f,room.height());
      }else {
        topWall1 = Instantiate(prefab_room3, new Vector3(
          room.leftExt,
          1f,
          room.bottomExt+((room.topExt-room.bottomExt)-2f)/4
          ),Quaternion.identity);
        topWall2 = Instantiate(prefab_room3, new Vector3(
          room.leftExt,
          1f,
          room.topExt - ((room.topExt-room.bottomExt)-2f)/4
        ),Quaternion.identity);
        
        topWall1.transform.localScale = new Vector3(0.5f,4f,(room.height()-2f)/2);
        topWall2.transform.localScale = new Vector3(0.5f,4f,(room.height()-2f)/2);
      }

      if(room.leftcor == null) {
        topWall = Instantiate(prefab_room3, new Vector3(
          room.rightExt,
          1f,
          (room.topExt+room.bottomExt)/2
          ),Quaternion.identity);
          
          topWall.transform.localScale = new Vector3(0.5f,4f,room.height());
      }else {
        topWall1 = Instantiate(prefab_room3, new Vector3(
          room.rightExt,
          1f,
          room.bottomExt+((room.topExt-room.bottomExt)-2f)/4
          ),Quaternion.identity);
        topWall2 = Instantiate(prefab_room3, new Vector3(
          room.rightExt,
          1f,
          room.topExt - ((room.topExt-room.bottomExt)-2f)/4
        ),Quaternion.identity);
        
        topWall1.transform.localScale = new Vector3(0.5f,4f,(room.height()-2f)/2);
        topWall2.transform.localScale = new Vector3(0.5f,4f,(room.height()-2f)/2);
      }
    }

    GameObject lockedDoor;
    foreach(Room room in rooms){
      if(room.doorRoom == 1) {
        if(room.lockedDoors.Contains('l')) {
          lockedDoor = Instantiate(lockedDoorPrefab, new Vector3(
            room.leftExt,
            1,
            (room.bottomExt + room.topExt)/2
            ),Quaternion.identity);
          lockedDoor.transform.localScale = new Vector3(0.5f,4f,2f);  
            
        }
        if(room.lockedDoors.Contains('r')) {
          lockedDoor = Instantiate(lockedDoorPrefab, new Vector3(
            room.rightExt,
            1,
            (room.bottomExt + room.topExt)/2
            ),Quaternion.identity);
          lockedDoor.transform.localScale = new Vector3(0.5F,4f,2f);                
        } 
        if(room.lockedDoors.Contains('t')) {
          lockedDoor = Instantiate(lockedDoorPrefab, new Vector3(
            (room.rightExt + room.leftExt)/2,
            1,
            room.topExt
            ),Quaternion.identity);
          lockedDoor.transform.localScale = new Vector3(2f,4f,0.5f);
        }
        if(room.lockedDoors.Contains('b')) {
          lockedDoor = Instantiate(lockedDoorPrefab, new Vector3(
            (room.rightExt + room.leftExt)/2,
            1,
            room.bottomExt
            ),Quaternion.identity);
          lockedDoor.transform.localScale = new Vector3(2f,4f,0.5f);  
        }
      }   
    }

    foreach(Room room in rooms) {
      if(room.keyarea == 0) {
        startRoom = room;
        Instantiate(prefabPlayer, new Vector3(
          (room.rightExt + room.leftExt)/2,
          1,
          (room.bottomExt + room.topExt)/2
          ),Quaternion.identity);
        break;
      }
    }

 
  }

  void Start()
  {
    ArrayList roomsAndCorridors = GenerateRooms(gameArea[0],gameArea[1]);
    corridors = (ArrayList)roomsAndCorridors[1];
    rooms = (ArrayList)roomsAndCorridors[0];
  }

  void Update()
  {
    if(constructionComplete == false) {
      ConstructRooms(rooms, corridors);
      BFSExit(startRoom, rooms);
      BFSKey(startRoom, rooms);
      constructionComplete = true;
      // foreach(Room room in rooms) {
      // int rngGoblin = Random.Range(0,1);
      // if(rngGoblin == 0){
      //        Instantiate(goblinPrefab, new Vector3(
      //     (room.rightExt + room.leftExt)/2,
      //     1,
      //     (room.bottomExt + room.topExt)/2
      //     ),Quaternion.identity);

      // }else{
      //     Instantiate(treasurePrefab, new Vector3(
      //     (room.rightExt + room.leftExt)/2,
      //     1,
      //     (room.bottomExt + room.topExt)/2
      //     ),Quaternion.identity);
      // }


      
    // }
    }
    
  }
}
