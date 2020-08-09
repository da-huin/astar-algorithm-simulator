/* 
 * AddWall                  벽을 추가한다.
 * RemoveWall               벽을 제거한다.
 * Find                     목적지를 찾는다.
 * UpdateFlagWithStateInit  업데이트에서 if문에 넣고 완료했으면 true를 반환하고 FindState를 초기화한다.
 * GetRoute                 성공했을 때 루트를 반환한다.
 * DebugSuccessRoute        성공한 루트를 print로 출력한다.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astar : MonoBehaviour
{

    private static Astar instance;
    public static Astar Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType(typeof(Astar)) as Astar;
            }
            return instance;
        }
    }

    /*==========================================*/
    /*                   변수                   */
    /*==========================================*/
    public enum FindState { UnFound, Finding, Success, fail }
    static FindState eFindState = FindState.UnFound;

    static float one = 1;

    List<GameObject> list_WallObject;
    List<Node> list_OpenNode;
    List<Node> list_CloseNode;
    Vector2 StartPos;
    Vector2 EndPos;

    Node lastNode;
    class Var
    {
        public static float[] way = new float[] { -one, 0, one };
        public static int limit = 500;
        public static int count = 0;
    }

    /*==========================================*/
    /*                   클래스                  */
    /*==========================================*/
    class Node
    {
        public Node prev_node;
        public float x, y;
        public int cost;

        public Node(float x, float y) { this.x = x; this.y = y; }
    }

    /*==========================================*/
    /*              사용자 함수                  */
    /*==========================================*/

    public void AddWall(GameObject AddWall)
    {
        if (list_WallObject == null)
            list_WallObject = new List<GameObject>();
        list_WallObject.Add(AddWall);
    }

    public void RemoveWall(GameObject RemoveWall)
    {
        list_WallObject.Remove(RemoveWall);
    }

    public void Find(Vector2 Start_Position, Vector2 End_Position)
    {
        StartPos = Start_Position;
        EndPos = End_Position;
        if (eFindState != FindState.Finding && IsEndPositionWall() == false)
        {
            Var.count = 0;

            if (list_OpenNode == null || list_CloseNode == null)
            {
                list_OpenNode = new List<Node>();
                list_CloseNode = new List<Node>();
            }
            else
            {
                list_OpenNode.Clear();
                list_CloseNode.Clear();
            }
            StartCoroutine("FindCoroutine");
        }
    }

    public FindState GetState()
    {
        return eFindState;
    }

    public void DebugSuccessRoute(List<Vector2> Route)
    {
        foreach (var routePiece in Route)
        {
            print(routePiece);
        }
    }
    public bool UpdateFlagWithStateInit()
    {
        if (eFindState == FindState.Success)
        {
            eFindState = FindState.UnFound;
            return true;
        }
        return false;
    }

    public void GetRoute(ref List<Vector2> Route)
    {
        Node tmpNode = lastNode;

        while (true)
        {
            Route.Insert(0, new Vector2(tmpNode.x, tmpNode.y));
            if (tmpNode.prev_node == null) break;

            tmpNode = tmpNode.prev_node;
        }
    }

    /*==========================================*/
    /*               일반 함수                   */
    /*==========================================*/

    IEnumerator FindCoroutine()
    {
        eFindState = FindState.Finding;
        Node cur_node;
        Node new_node;
        cur_node = new Node(StartPos.x, StartPos.y);
        list_OpenNode.Add(cur_node);

        while (true)
        {
            if (eFindState == FindState.Success)
            {
                break;
            }

            Var.count++;
            if (Var.limit < Var.count || list_OpenNode.Count == 0 || eFindState == FindState.UnFound)
            {
                eFindState = FindState.fail;
                break;
            }
            OpenNode_Sort();
            cur_node = list_OpenNode[0];
            list_OpenNode.RemoveAt(0);
            list_CloseNode.Add(cur_node);

            for (int A = 0; A < 3; A++)
            {
                for (int B = 0; B < 3; B++)
                {
                    if (Var.way[A] == 0 && Var.way[B] == 0) continue;

                    RaycastHit2D hit =
                        Physics2D.Raycast(new Vector2(cur_node.x, cur_node.y), new Vector2(Var.way[A], Var.way[B]), one);

                    new_node = new Node(cur_node.x + Var.way[A], cur_node.y + Var.way[B]);
                    do
                    {
                        if (IsClose(new_node))
                        {
                            break;
                        }

                        if (hit.collider != null)
                        {
                            if (IsWall(hit))
                                break;
                        }


                        if (new_node.x != cur_node.x && new_node.y != cur_node.y)
                        {
                            if (DiagonalCollider(new_node, Var.way[A], Var.way[B]))
                                break;
                        }

                        new_node.prev_node = cur_node;
                        new_node.cost = Cost_Calculate(new_node);

                        if (!OpenNode_ItsOverlap(new_node))
                            list_OpenNode.Add(new_node);

                        if (new_node.x == EndPos.x && new_node.y == EndPos.y)
                        {
                            eFindState = FindState.Success;
                            lastNode = new_node;
                            break;
                        }


                    } while (false);
                }
            }
        }
        yield return null;
    }

    int Cost_Calculate(Node node)
    {
        int start_cost = 0;
        int end_cost = 0;

        Node tmpNode = node;
        while (true)
        {
            if (tmpNode.prev_node == null) break;

            if (tmpNode.prev_node.x == tmpNode.x || tmpNode.prev_node.y == tmpNode.y)
            {
                start_cost += 10;
            }
            else
            {
                start_cost += 14;
            }

            tmpNode = tmpNode.prev_node;
        }

        end_cost = (int)((Mathf.Abs(node.x - EndPos.x) + Mathf.Abs(node.y - EndPos.y)) * 10);

        return start_cost + end_cost;
    }

    bool DiagonalCollider(Node node, float WayA, float WayB)
    {
        bool returnValue = false;
        Vector2 origin = new Vector2(node.x, node.y);
        Vector2 ToRight = new Vector2(one, 0);
        Vector2 ToLeft = new Vector2(-one, 0);
        Vector2 ToUp = new Vector2(0, one);
        Vector2 ToDown = new Vector2(0, -one);
        RaycastHit2D[] hit = new RaycastHit2D[2];

        if (WayA == -one && WayB == +one)
        {
            hit[0] = Physics2D.Raycast(origin, ToRight, one);
            hit[1] = Physics2D.Raycast(origin, ToDown, one);
        }
        else if (WayA == +one && WayB == +one)
        {
            hit[0] = Physics2D.Raycast(origin, ToLeft, one);
            hit[1] = Physics2D.Raycast(origin, ToDown, one);
        }
        else if (WayA == -one && WayB == -one)
        {
            hit[0] = Physics2D.Raycast(origin, ToRight, one);
            hit[1] = Physics2D.Raycast(origin, ToUp, one);
        }
        else if (WayA == one && WayB == -one)
        {
            hit[0] = Physics2D.Raycast(origin, ToLeft, one);
            hit[1] = Physics2D.Raycast(origin, ToUp, one);
        }
        if (hit[0].collider != null && hit[1].collider != null)
        {
            bool[] WallCheck = new bool[2];
            foreach (var wall in list_WallObject)
            {
                if (wall.name == hit[0].collider.name)
                {
                    WallCheck[0] = true;
                }
                if (wall.name == hit[1].collider.name)
                {
                    WallCheck[1] = true;
                }
                if (WallCheck[0] == true && WallCheck[1] == true)
                {
                    returnValue = true;
                    break;
                }
            }
        }

        return returnValue;
    }

    bool IsWall(RaycastHit2D hit)
    {
        bool returnValue = false;
        foreach (var wall in list_WallObject)
        {
            if (wall.name == hit.collider.name)
            {
                returnValue = true;
            }
        }

        return returnValue;
    }

    bool IsClose(Node node)
    {
        bool returnValue = false;
        foreach (var CloseNode in list_CloseNode)
        {
            if (CloseNode.x == node.x && CloseNode.y == node.y)
            {
                returnValue = true;
                break;
            }
        }
        return returnValue;
    }
    bool IsEndPositionWall()
    {
        bool returnValue = false;
        Vector2 EndPosition = EndPos;
        RaycastHit2D hit = Physics2D.Raycast(EndPosition, Vector2.zero, 0f);
        if (hit.collider != null)
        {
            returnValue = IsWall(hit);
        }
        return returnValue;
    }
    void OpenNode_Sort()
    {
        list_OpenNode.Sort(delegate (Node A, Node B)
        {
            if (A.cost > B.cost) return 1;
            else if (A.cost < B.cost) return -1;
            else return 0;
        });
    }
    bool OpenNode_ItsOverlap(Node node)
    {
        int indexCount = 0;
        bool returnValue = false;
        foreach (var OpenNode in list_OpenNode)
        {
            if (OpenNode.x == node.x && OpenNode.y == node.y)
            {
                //list_OpenNode.RemoveAt(indexCount);
                returnValue = true;
                break;
            }
            indexCount++;
        }
        return returnValue;
    }
}