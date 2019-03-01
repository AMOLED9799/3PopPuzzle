using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotManager : MonoBehaviour {

    public static DotManager dotManager;


    GameObject selectedDot = null;
    GameObject neighborDot = null;
    GameObject _selectedDot = null;
    GameObject _neighborDot = null;

    // SmoothDamp Velocity
    Vector2 selectedVelocity = Vector2.zero;
    Vector2 neighborVelocity = Vector2.zero;
    Vector2 dropVelocity = Vector2.zero;

    // 각도계산
    private Vector2 firstTouchPosition = Vector2.zero;
    private Vector2 finalTouchPosition = Vector2.zero;
    private float swipeAngle = 0f;

    public int howManyDotsNeedDrop = 0;
    public int howManyDotsMatched = 0;
    public int howManyDotsDestroy = 0;

    private bool matchExist = false;
    private bool startState = false;
    private bool matchDone = false;

    // 
    private void Awake()
    {
        dotManager = this;
    }

    public enum State
    {
        stable, swipeDot, checkMatch, destroyMatch, dropDot, RefillDot
    }

    public State state;

    void Start() {
        state = State.stable;
        StartCoroutine(SwitchStateCo());
    }


    void Update() {

    }

    // 메서드 정의 부분 -----------------------------------

    private IEnumerator SwitchStateCo()
    {
        Debug.Log("START SWITCHSTATECOROUTINE");

        while (true)
        {
            switch (state)
            {
                case State.stable:
                    {
                        Debug.Log("Stable State");
                        // 안정된 상태. 
                        // 마우스 swipe를 감지하면 바로 checking state로 이동한다.

                        // raycast로 마우스가 있는 위치의 object를 가져온다
                        if (Input.GetMouseButtonDown(0))
                        {
                            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            Ray2D ray = new Ray2D(firstTouchPosition, Vector2.zero);
                            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
                            
                            // Dot을 클릭
                            if (hit.collider != null)
                            {
                                selectedDot = hit.collider.gameObject;
                            }

                            // Dot을 클릭 X
                            else
                            {
                                selectedDot = null;
                                firstTouchPosition = Vector2.zero;

                            }
                        }

                        // 마우스 클릭을 떼면, 각도를 계산하여 움직이기를 실행한다
                        if (Input.GetMouseButtonUp(0) && (selectedDot != null))
                        {
                            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                            // 각도 : 2-1-4-3분면 순서로 180 ~ -180
                            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) / Mathf.PI * 180;

                            state = State.swipeDot;
                            startState = true;

                        }

                        break;
                    }

                case State.swipeDot:
                    {
                        Debug.Log("swipeDot State");

                        // Swipe 명령이 들엉온 이후 실행된 이후 한 번만 실행하는 메서드
                        if (startState)
                        {
                            // Dot의 column, row를 이동시킨다 (화면상이 아닌 데이터상에서 움직임)
                            SwipeDotsCR();

                            // Dot을 실제로 움직이기
                            // selected Dot은 Mouse Down Input이 들어오면서 결정된다.
                            selectedDot.GetComponent<Dot>().swipeDotTF = true;
                            // neighbor Dot은 Mouse Up Input을 통한 각도처리와 함께 결정된다.
                            neighborDot.GetComponent<Dot>().swipeDotTF = true;

                            startState = false;
                        }

                        // state 탈출 조건 : 두 Dot이 모두 이동을 마쳤을 때
                        if (selectedDot.GetComponent<Dot>().swipeDotTF == false && neighborDot.GetComponent<Dot>().swipeDotTF == false)
                        {
                            Debug.Log("Swipe Done");

                            // checkMatch State로 이동
                            state = State.checkMatch;
                        }

                        break;
                    }

                case State.checkMatch:
                    {
                        Debug.Log("checkMatch State");

                        // 모든 매치검사를 실행
                        MatchingDot();

                        // match 결과 match가 존재할 때 => isMatched인 Dot을 Destroy 할 차례
                        if (matchExist && matchDone)
                        {
                            matchDone = false;
                            startState = true;
                            state = State.destroyMatch;
                        }

                        // match 결과 match가 없을 때 => Swipe한 Dot이 다시 자기 자리로 돌아가야함
                        else if (!matchExist && matchDone) 
                        {
                            matchDone = false;
                            // *******************************************************************  자기자리로 돌아가는 메서드 구현해야함
                            state = State.stable;
                        }

                    }
                    break;

                case State.destroyMatch:
                    {
                        Debug.Log("destroyMatch Executed");
                        if (startState)
                        {
                            // 모든 dot에 대해 isMatched 를 확인해 destroy 코루틴을 실행시킨다
                            foreach (GameObject dot in Board.board.allDots)
                            {
                                if (dot != null)
                                {
                                    if (dot.GetComponent<Dot>().isMatched)
                                    {
                                        Debug.Log("destroy");
                                        // howManyDotsDestroy 카운트를 올려 다 터졌는지 검사
                                        howManyDotsDestroy++;

                                        // destroyDotTF true로 체크하여 각 Dot의 Dot Script를 통해 제거
                                        dot.GetComponent<Dot>().destroyDotTF = true;
                                    }
                                }
                            }
                            startState = false;
                        }

                        if (howManyDotsDestroy == 0)
                        {
                            state = State.dropDot;
                            startState = true;
                        }
                    }
                    break;

                case State.dropDot:
                    {
                        // drop 실행할 때 조건형성을 위해 한 번만 실행할 메서드
                        if (startState)
                        {
                            // 자기 아래에 null 개수를 확인
                            // 자기가 갈 위치에 미리 Column, Row를 옮겨놓음
                            // Refill할 Dot까지 미리 Positioning 해 놓기

                            CountNullsIndividual();

                            // 모든 dot에 대해 drop 코루틴을 실행시킨다.
                            foreach (GameObject dot in Board.board.allDots)
                            {
                                if (dot != null)
                                {
                                    dot.GetComponent<Dot>().dropDotTF = true;
                                }
                            }

                            startState = false;
                        }



                        if (howManyDotsNeedDrop == 0)
                        {
                                                                                        // Refill State 를 만들어 넣어줘야함 
                            state = State.checkMatch;
                            startState = true;
                        }
                    }
                    break;
            }

            // yield return null waitfor.. 로 변경하면 Stable State 의 Swipe 동작이 인식되지 않는 현상이 보이므로 수정 X 할 것.
            yield return null;
        }
    }

    private void SwipeDotsCR()
    {
        int row = selectedDot.GetComponent<Dot>().row;
        int column = selectedDot.GetComponent<Dot>().column;
        GameObject tempObject;

        // Swipe With Up
        if (swipeAngle > 60 && swipeAngle < 120)
        {
            // neighborDot 지정
            neighborDot = Board.board.allDots[column, row + 1];

            // [column, row] 이동
            neighborDot.GetComponent<Dot>().row--;
            selectedDot.GetComponent<Dot>().row++;

            tempObject = Board.board.allDots[column, row];

            Board.board.allDots[column, row] = neighborDot;
            Board.board.allDots[column, row + 1] = tempObject;
        }
        // Swipe with Down
        else if (swipeAngle < -60 && swipeAngle > -120)
        {
            // neighborDot 지정
            neighborDot = Board.board.allDots[column, row - 1];

            // [column, row] 이동
            neighborDot.GetComponent<Dot>().row++;
            selectedDot.GetComponent<Dot>().row--;

            tempObject = Board.board.allDots[column, row];

            Board.board.allDots[column, row] = neighborDot;
            Board.board.allDots[column, row - 1] = tempObject;
        }
        // Swipe with Right
        else if (swipeAngle > -30 && swipeAngle < 30)
        {
            // neighborDot 지정
            neighborDot = Board.board.allDots[column + 1, row];

            // [column, row] 이동
            neighborDot.GetComponent<Dot>().column--;
            selectedDot.GetComponent<Dot>().column++;

            tempObject = Board.board.allDots[column, row];

            Board.board.allDots[column, row] = neighborDot;
            Board.board.allDots[column + 1, row] = tempObject;
        }
        // Swipe with Left
        else if (swipeAngle > 150 || swipeAngle < -150)
        {
            // neighborDot 지정
            neighborDot = Board.board.allDots[column - 1, row];

            // [column, row] 이동
            neighborDot.GetComponent<Dot>().column++;
            selectedDot.GetComponent<Dot>().column--;

            tempObject = Board.board.allDots[column, row];

            Board.board.allDots[column, row] = neighborDot;
            Board.board.allDots[column - 1, row] = tempObject;
        }

        // else : swipe각도가 대각선으로 치우친 경우 -> 초기화
        else
        {
            selectedDot = null;
            firstTouchPosition = Vector2.zero;
            Debug.Log("swipe 각도가 애매하여 예외로 처리함");

            return;
        }

        firstTouchPosition = Vector2.zero;
        finalTouchPosition = Vector2.zero;
        swipeAngle = 0;
        return;
    }


    // Board.board 의 allDots[]에 column, row 가 바뀌어 있는 상태에서 좌표를 목적지로 하여 Dot 이미지를 이동시키는 메서드



    private void MatchingDot()
    {
        howManyDotsMatched = 0;
    
        for (int _row = 0; _row < Board.board.height - 2; _row++)
        {
            for (int _column = 0; _column < Board.board.width - 2; _column++)
            {
                if (Board.board.allDots[_column, _row] != null)
                {
                    if (Board.board.allDots[_column + 1, _row] != null && Board.board.allDots[_column + 2, _row] != null)
                    {
                        if ((Board.board.allDots[_column, _row].gameObject.CompareTag(Board.board.allDots[_column + 1, _row].gameObject.tag)) && ((Board.board.allDots[_column, _row].CompareTag(Board.board.allDots[_column + 2, _row].gameObject.tag))))
                        {
                            Board.board.allDots[_column, _row].GetComponent<Dot>().isMatched = true;
                            Board.board.allDots[_column + 1, _row].GetComponent<Dot>().isMatched = true;
                            Board.board.allDots[_column + 2, _row].GetComponent<Dot>().isMatched = true;

                            howManyDotsMatched++;

                        }
                    }

                    if (Board.board.allDots[_column, _row + 1] != null && Board.board.allDots[_column, _row + 2] != null) {
                        if ((Board.board.allDots[_column, _row].CompareTag(Board.board.allDots[_column, _row + 1].tag)) && (Board.board.allDots[_column, _row].CompareTag(Board.board.allDots[_column, _row + 2].tag)))
                        {
                            Board.board.allDots[_column, _row].GetComponent<Dot>().isMatched = true;
                            Board.board.allDots[_column, _row + 1].GetComponent<Dot>().isMatched = true;
                            Board.board.allDots[_column, _row + 2].GetComponent<Dot>().isMatched = true;

                            howManyDotsMatched++;

                        }
                    }
                }
            }
        }

        // 예외 지역 처리

        for (int _row = Board.board.height - 2; _row < Board.board.height; _row++)
        {
            for (int _column = 0; _column < Board.board.width - 2; _column++)
            {
                if (Board.board.allDots[_column, _row] != null)
                {
                    if (Board.board.allDots[_column + 1, _row] != null && Board.board.allDots[_column + 2, _row] != null)
                    {

                        if ((Board.board.allDots[_column, _row].tag == Board.board.allDots[_column + 1, _row].tag) && (Board.board.allDots[_column, _row].tag == Board.board.allDots[_column + 2, _row].tag))
                        {
                            Board.board.allDots[_column, _row].GetComponent<Dot>().isMatched = true;
                            Board.board.allDots[_column + 1, _row].GetComponent<Dot>().isMatched = true;
                            Board.board.allDots[_column + 2, _row].GetComponent<Dot>().isMatched = true;

                            howManyDotsMatched++;

                        }
                    }
                }
            }
        }

        for (int _column = Board.board.width - 2; _column < Board.board.width; _column++)
        {
            for (int _row = 0; _row < Board.board.height - 2; _row++)
            {
                if (Board.board.allDots[_column, _row] != null)
                {
                    if (Board.board.allDots[_column, _row + 1] != null && Board.board.allDots[_column, _row + 2] != null)
                    {

                        if ((Board.board.allDots[_column, _row].tag == Board.board.allDots[_column, _row + 1].tag) && (Board.board.allDots[_column, _row].tag == Board.board.allDots[_column, _row + 2].tag))
                        {
                            Board.board.allDots[_column, _row].GetComponent<Dot>().isMatched = true;
                            Board.board.allDots[_column, _row + 1].GetComponent<Dot>().isMatched = true;
                            Board.board.allDots[_column, _row + 2].GetComponent<Dot>().isMatched = true;

                            howManyDotsMatched++;
                        }
                    }
                }
            }
        }

        if(howManyDotsMatched == 0)
        {
            matchExist = false;
        }

        else
        {
            matchExist = true;
        }

        matchDone = true;
        return;
    }


    private void CountNullsIndividual()
    {
        howManyDotsNeedDrop = 0;

        for (int _column = 0; _column < Board.board.width; _column++)
        {
            int nullCount = 0;

            for (int _row = 0; _row < Board.board.height; _row++)
            {
                // 자기가 가야할 좌표로 미리 값을 바꿔놓음
                if (Board.board.allDots[_column, _row] == null)
                {
                    nullCount++;
                }
                else if (nullCount != 0)
                {
                    Board.board.allDots[_column, _row].GetComponent<Dot>().row -= nullCount;
                    Board.board.allDots[_column, _row - nullCount] = Board.board.allDots[_column, _row];
                    Board.board.allDots[_column, _row] = null;

                    howManyDotsNeedDrop++;
                }
            }

            // Refill 을 위한 dot을 미리 만들어 놓기

            for (int _dropRow = Board.board.height; _dropRow < Board.board.height + nullCount; _dropRow++)
            {
                GameObject dot = Instantiate(Board.board.dots[Random.Range(0, Board.board.dots.Length)], new Vector2(_column, _dropRow), Quaternion.identity);
                dot.name = "dotNew";
                dot.transform.parent = Board.board.transform;

                Board.board.allDots[_column, _dropRow - nullCount] = dot;
                dot.GetComponent<Dot>().column = _column;
                dot.GetComponent<Dot>().row = _dropRow - nullCount;

                howManyDotsNeedDrop++;

            }
        }
    }
}

/*
    private IEnumerator DropDotCo()
    {
        while(true)
        {
            Debug.Log("Yaloo");

            // Destory가 끝난 직후 Drop시키기 위한 Null의 개수를 count

            if (state == State.destoryDone)
            {
                Debug.Log("Hi");

                state = State.count;

                CountNulls();

                yield return new WaitForEndOfFrame();
            }

            // count 가 끝난 후 drop을 직접 수행 + drop 수행중인 경우 계속 진입하여 drop 시킴
            if (state == State.countDone || state == State.drop)
            {
                Debug.Log("Hello");

                state = State.drop;

                for (int _column = 0; _column < Board.board.width; _column++)
                {
                    Debug.Log("Yaho");

                    for (int _row = 0; _row < Board.board.height; _row++)
                    {
                        if (Board.board.allDots[_column, _row] != null)
                        {
                            if (Board.board.allDots[_column, _row].GetComponent<Dot>().nullCount != 0)
                            {
                                Board.board.allDots[_column, _row].transform.Translate(new Vector2(0f, -0.2f));

                                if ((Board.board.allDots[_column, _row].transform.position.y - (Board.board.allDots[_column, _row].GetComponent<Dot>().row) + Board.board.allDots[_column, _row].GetComponent<Dot>().nullCount) < 0.05f)
                                {
                                    Debug.Log("hi im elfo");

                                    Board.board.allDots[_column, _row].transform.position = new Vector2(Board.board.allDots[_column, _row].GetComponent<Dot>().column, Board.board.allDots[_column, _row].GetComponent<Dot>().row - Board.board.allDots[_column, _row].GetComponent<Dot>().nullCount);
                                    Board.board.allDots[_column, _row].GetComponent<Dot>().row = Board.board.allDots[_column, _row].GetComponent<Dot>().row - Board.board.allDots[_column, _row].GetComponent<Dot>().nullCount;

                                    //초기화
                                    int tempNullCount = Board.board.allDots[_column, _row].GetComponent<Dot>().nullCount;

                                    Board.board.allDots[_column, _row - tempNullCount] = Board.board.allDots[_column, _row];

                                    Board.board.allDots[_column, _row].GetComponent<Dot>().nullCount = 0;
                                    Board.board.allDots[_column, _row] = null;

                                    howManyDotsNeedDrop--;

                                    if(howManyDotsNeedDrop == 0)
                                    {
                                        state = State.dropDone;
                                    }

                                    yield return null;

                                }
                            }
                        }
                    }
                }

            }


            yield return null;
        }
        
    }

}
*/