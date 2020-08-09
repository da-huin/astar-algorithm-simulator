/* 
  =================================
  LeftMouseButton : 벽 생성 및 삭제
  Enter : 시작 및 정지
  BackSpace : 벽을 제외하고 초기화
  =================================
 */
/* 
 =================================
 문제 해결할 것 : 
 코드정리
 =================================
  */


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Manage : MonoBehaviour
{
    /* ===========================================================================*/
    /*                                 변수                                      */
    /* ===========================================================================*/
    /* 유니티 입력변수 */
    public GameObject ob_start;
    public GameObject ob_end;
    public GameObject ob_open;
    public GameObject ob_close;
    public GameObject ob_wall;

    /* 일반 전역변수 */
    List<_Node> Open_Node;
    List<_Node> Close_Node;

    _Node new_node;
    _Node cur_node;
    _Node success_node;

    /* static 변수 */
    static List<_Node> Wall_Node;

    static bool OneTimeFlag;
    static bool OneNextFlag;

    static float startX;
    static float startY;

    static float endX;
    static float endY;


    /* UPDATE 전역변수 */
    bool play = true; // 실제 루틴
    bool start_find;  // 찾기 실행 (Enter 눌렀을 때)
    bool InstWall, DeleteWall;
    bool MoveStart, MoveEnd;

    /* Routine 전역변수 */
    int limit = 1500, count = 0;


    class _Node
    {
        public _Node prev_node; // 이전노드
        public float x, y; // 좌표
        public int cost; // 비용

        public _Node(float X, float Y) { x = X; y = Y; }
    }

    /* ===========================================================================*/
    /*                           유니티 실행 함수                                  */
    /* ===========================================================================*/

    void Start()
    {
        Open_Node = new List<_Node>();
        Close_Node = new List<_Node>();

        /*                   한 번 만 실행                  */
        if (OneTimeFlag == false)
        {
            Wall_Node = new List<_Node>();
            startX = ob_start.transform.position.x;
            startY = ob_start.transform.position.y;
            endX = ob_end.transform.position.x;
            endY = ob_end.transform.position.y;

            _Node start_node = new _Node(startX, startY);
            start_node.cost = (int)(Mathf.Abs(endX - startX) + Mathf.Abs(endY - startY)) * 10;
            Open_Node.Add(start_node);
            OneTimeFlag = true;
        }
    }


    void Update()
    {
        /* ==========================================*/
        /* 마우스 입력                               */
        /* ==========================================*/
        if (Input.GetMouseButton(0))
        {
            Vector3 MousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // 반올림
            MousePos.x = Mathf.Round(MousePos.x);
            MousePos.y = Mathf.Round(MousePos.y);
            MousePos.z = -1;

            RaycastHit2D hit = Physics2D.Raycast(MousePos, Vector2.zero);


            // 시작 위치 변경
            if(MousePos.x == ob_start.transform.position.x && MousePos.y == ob_start.transform.position.y || MoveStart == true)
            {
                ob_start.transform.position = new Vector3(MousePos.x, MousePos.y, ob_start.transform.position.z);
                startX = ob_start.transform.position.x;
                startY = ob_start.transform.position.y;
                Open_Node[0].x = startX;
                Open_Node[0].y = startY;
                MoveStart = true;
            }

            // 목적지 위치 변경
            else if(MousePos.x == ob_end.transform.position.x && MousePos.y == ob_end.transform.position.y || MoveEnd == true)
            {
                ob_end.transform.position = new Vector3(MousePos.x, MousePos.y, ob_end.transform.position.z);
                endX = ob_end.transform.position.x;
                endY = ob_end.transform.position.y;
                MoveEnd = true;
            }

            // 아무것도 없을 시 벽 생성
            else if ((hit.collider == null) && DeleteWall == false)
            {
                if (Input.GetMouseButtonDown(0))
                    InstWall = true;

                Instantiate<GameObject>(ob_wall, MousePos, new Quaternion());
            }
            // 벽이 있을 경우 벽 삭제
            else if (hit.collider != null)
            {
                if ((hit.collider.tag == ob_wall.tag) && InstWall == false)
                {
                    if (Input.GetMouseButtonDown(0))
                        DeleteWall = true;

                    Destroy(hit.collider.gameObject);
                }
            }
        }
        // 마우스 떼었을 때 bool 변수 초기화
        else if (Input.GetMouseButtonUp(0))
        {
            InstWall = false;
            DeleteWall = false;
            MoveStart = false;
            MoveEnd = false;
        }
        /* ==========================================*/
        /* 키 입력                                   */
        /* ==========================================*/
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // 시작과 재시작
            if (start_find == false)
                start_find = true;
            else if (start_find == true)
            {
                start_find = false;
            }
        }
        // 백스페이스 눌렀을 때, OneNextFlag가 true인 이유는 한 텀 후 벽을 생성하기 위해서
        else if (Input.GetKeyDown(KeyCode.Backspace) || OneNextFlag == true)
        {
            // 새로운 벽 생성
            OnInitialize();
        }

        /* ==========================================*/
        /* 목적지 찾기                               */
        /* ==========================================*/
        if (start_find == true) // 엔터를 눌렀을 때
        {
            if (play)
            {
                string Result = Routine();

                if (Result != "Continue") // 종료
                {
                    if (Result == "Success") // 찾기 성공
                    {
                        print("success");
                    }
                    else if (Result == "Fail") // 찾기 실패
                    {
                        print("fail");
                    }
                    play = false;
                    start_find = false;
                }
            }
        }


    }
    /* ===========================================================================*/
    /*                                     함수                                   */
    /* ===========================================================================*/


    // 1. Routine
    string Routine()
    {
        // limit 이상 시도시 탈출
        if (limit < count) return "Fail";
        count++;
        int[] way = new int[3] { -1, 0, 1 };

        // 노드가 전혀 없을 시 탈출
        if (Open_Node.Count == 0)
        {
            return "Fail";
        }

        // 0 번 인덱스 소환
        cur_node = Open_Node[0];

        // 0번 인덱스 삭제
        Open_Node.RemoveAt(0);

        // 현재 노드 Close에 추가 및 이미지 생성
        Close_Node.Add(cur_node);
        Vector3 close_inst = new Vector3(cur_node.x, cur_node.y, -1);
        Instantiate<GameObject>(ob_close, close_inst, new Quaternion());
        // 현재 노드 위치
        Vector2 ori = new Vector2(cur_node.x, cur_node.y);
        // 성공 확인 변수
        bool success = false;

        for (int a = 0; a < 3; a++)
        {
            for (int b = 0; b < 3; b++)
            {
                // way = -1, 0, 1
                // cur_node 와 new_node 위치가 같을 때 넘기기
                if (way[a] == 0 && way[b] == 0) continue;

                // 8방향 위치 반복
                Vector2 dir = new Vector2(way[a], way[b]);

                // 레이캐스트로 확인
                RaycastHit2D hit = Physics2D.Raycast(ori, dir, 1f);
                new_node = new _Node(cur_node.x + way[a], cur_node.y + way[b]);

                // 레이캐스트로 보낸 곳이 빈 공간일 때
                if (hit.collider == null)
                {

                    // 클로즈 노드인지 확인한다.
                    bool fail = false;
                    foreach (var close_node in Close_Node)
                    {
                        if (close_node.x == new_node.x && close_node.y == new_node.y)
                        {
                            fail = true;
                            break;
                        }
                    }
                    if (fail == true) continue;

                    // 대각선 노드 일 때 양 옆에 장애물이 있는지 확인한다
                    if (new_node.x != cur_node.x && new_node.y != cur_node.y)
                    {
                        if (DiagonalCollider(new_node, way[a], way[b]))
                        {
                            continue;
                        }
                    }



                    // 도착지인지 확인한다. 확인 하고도 다음 8방향 노드 확인 ( 이미지 생성 위해서 )
                    if (new_node.x == endX && new_node.y == endY)
                    {
                        success_node = new_node;
                        success = true;
                    }

                    /*       오픈 노드에 추가한다         */


                    // 중복 노드 삭제
                    int tmpCount = 0;
                    bool ItsOverlap = false;
                    foreach(var open_node in Open_Node)
                    {
                        if(open_node.x == new_node.x && open_node.y == new_node.y)
                        {
                            //Open_Node.RemoveAt(tmpCount); // test
                            ItsOverlap = true;
                            break;
                        }

                        tmpCount++;
                    }

                    // prev 연결
                    new_node.prev_node = cur_node;
                    // 비용 추가
                    new_node.cost = Cost_Calculate(new_node);

                    if (ItsOverlap == false)
                    {

                        Open_Node.Add(new_node);

                        // 현재 노드 이미지 생성
                        Vector3 open_inst = new Vector3(new_node.x, new_node.y, -1);
                        Instantiate<GameObject>(ob_open, open_inst, new Quaternion());
                    }
                    // Open_Node에 추가
                    //  print("x : " + new_node.x + "y : " + new_node.y + "cost : " + new_node.cost);

                }
            }
        }


        // 오픈노드 노드 정렬(cost별)
        OpenNode_CostSort();
        if (success == true)
        {
            // 성공했을 때 라인 그리기
            OnSuccess();
            return "Success";
        }
        else
            return "Continue";
    }

    // 2. 성공했을 때 
    void OnSuccess()
    {
        _Node tmp_node = success_node;
        int count = 0;
        // 노드 카운터 세기 ( numPosition에 카운트 넣기 위해서 )

        while (true)
        {
            count++;
            if (tmp_node.prev_node == null) break;
            tmp_node = tmp_node.prev_node;
        }
        // 라인 설정
        LineRenderer Line = GetComponent<LineRenderer>();
        Line.startColor = Color.black;
        Line.endColor = Color.white;
        Line.startWidth = 0.1f;
        Line.endWidth = 0.1f;
        Line.numPositions = count;

        // 실제 라인 그리기
        tmp_node = success_node;
        count = 0;
        while (true)
        {
            Line.SetPosition(count, new Vector3(tmp_node.x, tmp_node.y, -2));
            count++;
            // 노드의 끝에서 break
            if (tmp_node.prev_node == null) break;
            // 이전 노드 따라가기
            tmp_node = tmp_node.prev_node;
        }
    }

    // 3. 노드 정렬
    void OpenNode_CostSort()
    {
        Open_Node.Sort(delegate (_Node A, _Node B)
        {
            if (A.cost > B.cost) return 1;
            else if (A.cost < B.cost) return -1;
            return 0;
        });
    }

    // 4. 노드 비용계산
    int Cost_Calculate(_Node node)
    {
        int cost_start = 0, cost_end;
        _Node tmp_node = node;


        while (true)
        {
            if (tmp_node.prev_node == null) break;
            if (tmp_node.prev_node.x == tmp_node.x || tmp_node.prev_node.y == tmp_node.y)
            {
                cost_start += 10; // 직선 10
            }
            else
            {
                cost_start += 14; // 대각선 14
            }
            tmp_node = tmp_node.prev_node;
        }

        cost_end = (int)((Mathf.Abs(endX - node.x) + Mathf.Abs(endY - node.y)) * 10); // 직선 거리로만 계산
        return cost_start + cost_end;
    }

    // 5. 대각선 불가능 설정
    bool DiagonalCollider(_Node node, int a, int b)
    {
        // 현재 위치
        Vector2 new_ori = new Vector2(node.x, node.y);
        // 레이캐스트 할 방향
        Vector2 ToRight = new Vector2(1, 0);
        Vector2 ToLeft = new Vector2(-1, 0);
        Vector2 ToUp = new Vector2(0, 1);
        Vector2 ToDown = new Vector2(0, -1);
        RaycastHit2D[] hit = new RaycastHit2D[2];

        // 실제로 레이캐스트
        if (a == -1 && b == 1)
        {
            hit[0] = Physics2D.Raycast(new_ori, ToRight);
            hit[1] = Physics2D.Raycast(new_ori, ToDown);
        }
        else if (a == 1 && b == 1)
        {
            hit[0] = Physics2D.Raycast(new_ori, ToLeft);
            hit[1] = Physics2D.Raycast(new_ori, ToDown);
        }
        else if (a == -1 && b == -1)
        {
            hit[0] = Physics2D.Raycast(new_ori, ToRight);
            hit[1] = Physics2D.Raycast(new_ori, ToUp);
        }
        else if (a == 1 && b == -1)
        {
            hit[0] = Physics2D.Raycast(new_ori, ToLeft);
            hit[1] = Physics2D.Raycast(new_ori, ToUp);
        }

        // 충돌체가 양옆에 있을때 true
        if ((hit[0].collider != null) && (hit[1].collider != null))
        {
            return true;
        }

        // 클린할 때 false
        return false;
    }

    // 6. 초기화시 제한
    void OnInitialize()
    {
        // 생성한 벽 정보 받아온다.
        GameObject[] Walls = GameObject.FindGameObjectsWithTag("wall");
        // 시작 위치 끝 위치 저장





        // 처음 루틴(OneNextFlag == false)
        if (OneNextFlag == false)
        {
            startX = ob_start.transform.position.x;
            startY = ob_start.transform.position.y;

            endX = ob_end.transform.position.x;
            endY = ob_end.transform.position.y;

            // 벽에 대한 정보 static 노드리스트에 추가 ( Wall_Node )
            Wall_Node.Clear();
            foreach (var wall in Walls)
            {
                Wall_Node.Add(new _Node(wall.transform.position.x, wall.transform.position.y));
            }
            // 씬 불러오기 (static을 제외한 모든 변수 초기화 및 스크립트 처음으로 돌아감)
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main_Game");
            // 두번 째 루틴 준비(OneNextFlag = true)
            OneNextFlag = true;
        }
        // 두번 째 루틴(OneNextFlag == true)
        else if (OneNextFlag == true)
        {
            // Wall_Node에 저장해둔 x, y값으로 벽 다시 생성
            foreach (var wall_node in Wall_Node)
            {
                Instantiate(ob_wall, new Vector3(wall_node.x, wall_node.y, -1), new Quaternion());
            }
            
            // 포지션 변경
            ob_start.transform.position = new Vector3(startX, startY, ob_start.transform.position.z);
            ob_end.transform.position = new Vector3(endX, endY, ob_end.transform.position.z);


            _Node start_node = new _Node(startX, startY);
            start_node.cost = (int)(Mathf.Abs(endX - startX) + Mathf.Abs(endY - startY)) * 10;
            Open_Node.Add(start_node);

            // 첫 번째 루틴 준비(OneNextFlag == false)
            OneNextFlag = false;
        }
    }
}
