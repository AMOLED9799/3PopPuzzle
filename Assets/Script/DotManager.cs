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

    // 
    private void Awake()
    {
        dotManager = this;
    }

    public enum State
    {
        stable, swipeDot, checkMatch, destroyMatch, dropDot
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
                            Debug.Log("High");

                            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            Ray2D ray = new Ray2D(firstTouchPosition, Vector2.zero);
                            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                            Debug.Log(hit.collider.gameObject.name);

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
                        }

                        break;
                    }

                case State.swipeDot:
                    {
                        // Dot의 column, row를 이동시킨다 (화면상이 아닌 데이터상에서 움직임)
                        SwipeDotsCR();

                        // Dot을 실제로 움직이기
                        // selected Dot은 Mouse Down Input이 들어오면서 결정된다.
                        selectedDot.GetComponent<Dot>().swipeDotTF = true;
                        // neighbor Dot은 Mouse Up Input을 통한 각도처리와 함께 결정된다.
                        neighborDot.GetComponent<Dot>().swipeDotTF = true;

                        if (selectedDot.GetComponent<Dot>().swipeDotTF == false && !neighborDot.GetComponent<Dot>().swipeDotTF == false)
                        {
                            state = State.checkMatch;
                        }
                    }
                    break;

                case State.checkMatch:
                    {
                        // 모든 매치검사를 실행
                        MatchingDot();

                        Debug.Log(matchExist.ToString());

                        if (matchExist)
                        {
                            state = State.destroyMatch;

                        }
                        else
                        {
                            state = State.stable;
                        }
                    }
                    break;

                case State.destroyMatch:
                    {
                        // 모든 dot에 대해 isMatched 를 확인해 destroy 코루틴을 실행시킨다
                        foreach (GameObject dot in Board.board.allDots)
                        {
                            if (dot != null)
                            {
                                if (dot.GetComponent<Dot>().isMatched)
                                {
                                    dot.GetComponent<Dot>().destroyDotTF = true;
                                }
                            }
                        }
                        state = State.dropDot;

                    }
                    break;

                case State.dropDot:
                    {
                        // 자기 아래에 null 개수를 확인
                        CountNulls();

                        // 모든 dot에 대해 drop 코루틴을 실행시킨다.
                        foreach (GameObject dot in Board.board.allDots)
                        {
                            if (dot != null)
                            {
                                if (dot.GetComponent<Dot>().nullCount != 0)
                                {
                                    dot.GetComponent<Dot>().firstSet = true;
                                    dot.GetComponent<Dot>().dropDotTF = true;
                                }
                            }
                        }

                        if (howManyDotsNeedDrop == 0)
                        {
                            state = State.checkMatch;
                        }
                    }
                    break;
            }
            yield return new WaitForEndOfFrame();
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

        state = State.checkMatch;
        return;
    }


    // Board.board 의 allDots[]에 column, row 가 바뀌어 있는 상태에서 좌표를 목적지로 하여 Dot 이미지를 이동시키는 메서드



    private void MatchingDot()
    {
        Debug.Log("Match Execute");
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

        Debug.Log(howManyDotsMatched);
        if(howManyDotsMatched == 0)
        {
            matchExist = false;
                  }
        else
        {
            matchExist = true;

        }

        return;
    }


    private void CountNulls()
    {
        howManyDotsNeedDrop = 0;

        for (int _column = 0; _column < Board.board.width; _column++)
        {
            int nullCount = 0;

            for (int _row = 0; _row < Board.board.height; _row++)
            {
                if (Board.board.allDots[_column, _row] == null)
                {
                    nullCount++;
                }
                else if (nullCount != 0)
                {
                    Board.board.allDots[_column, _row].GetComponent<Dot>().nullCount = nullCount;
                    howManyDotsNeedDrop++;
                }
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