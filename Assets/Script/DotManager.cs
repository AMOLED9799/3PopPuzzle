using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotManager : MonoBehaviour {

    public static DotManager instance;

    private Board board;


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

    // 
    private void Awake()
    {
        instance = this;
    }

    public enum State
    {
        stable, swipeDot, checkMatch, destroyMatch, dropDot
    }

    public State state;

    void Start() {
        board = FindObjectOfType<Board>();
        state = State.stable;

    }


    void Update() {
        switch (state)
        {
            case State.stable:
                // 안정된 상태. 
                // 마우스 swipe를 감지하면 바로 checking state로 이동한다.
                {
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
                    }
                }
                break;

            case State.swipeDot:
                {
                    // Dot의 column, row를 이동시킨다 (화면상이 아닌 데이터상에서 움직임)
                    SwipeDotsCR();

                    // Dot을 실제로 움직이기
                    // selected Dot은 Mouse Down Input이 들어오면서 결정된다.
                    selectedDot.GetComponent<Dot>().swipeMove = true;
                    // neighbor Dot은 Mouse Up Input을 통한 각도처리와 함께 결정된다.
                    neighborDot.GetComponent<Dot>().swipeMove = true;

                    state = State.checkMatch;
                }
                break;

            case State.checkMatch:
                {
                    // 모든 매치검사를 실행
                    MatchingDot();
                }
                break;

            case State.destroyMatch:
                {

                }
                break;

            case State.dropDot:
                {

                }
                break;

        }

    }

    // 메서드 정의 부분 -----------------------------------

    private void SwipeDotsCR()
    {
        int row = selectedDot.GetComponent<Dot>().row;
        int column = selectedDot.GetComponent<Dot>().column;
        GameObject tempObject;

        // Swipe With Up
        if (swipeAngle > 60 && swipeAngle < 120)
        {
            // neighborDot 지정
            neighborDot = board.allDots[column, row + 1];

            // [column, row] 이동
            neighborDot.GetComponent<Dot>().row--;
            selectedDot.GetComponent<Dot>().row++;

            tempObject = board.allDots[column, row];

            board.allDots[column, row] = neighborDot;
            board.allDots[column, row + 1] = tempObject;
        }
        // Swipe with Down
        else if (swipeAngle < -60 && swipeAngle > -120)
        {
            // neighborDot 지정
            neighborDot = board.allDots[column, row - 1];

            // [column, row] 이동
            neighborDot.GetComponent<Dot>().row++;
            selectedDot.GetComponent<Dot>().row--;

            tempObject = board.allDots[column, row];

            board.allDots[column, row] = neighborDot;
            board.allDots[column, row - 1] = tempObject;
        }
        // Swipe with Right
        else if (swipeAngle > -30 && swipeAngle < 30)
        {
            // neighborDot 지정
            neighborDot = board.allDots[column + 1, row];

            // [column, row] 이동
            neighborDot.GetComponent<Dot>().column--;
            selectedDot.GetComponent<Dot>().column++;

            tempObject = board.allDots[column, row];

            board.allDots[column, row] = neighborDot;
            board.allDots[column + 1, row] = tempObject;
        }
        // Swipe with Left
        else if (swipeAngle > 150 || swipeAngle < -150)
        {
            // neighborDot 지정
            neighborDot = board.allDots[column - 1, row];

            // [column, row] 이동
            neighborDot.GetComponent<Dot>().column++;
            selectedDot.GetComponent<Dot>().column--;

            tempObject = board.allDots[column, row];

            board.allDots[column, row] = neighborDot;
            board.allDots[column - 1, row] = tempObject;
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


    // board 의 allDots[]에 column, row 가 바뀌어 있는 상태에서 좌표를 목적지로 하여 Dot 이미지를 이동시키는 메서드



    private void MatchingDot()
    {
        Debug.Log("HAppy");
        for (int _row = 0; _row < board.height - 2; _row++)
        {
            Debug.Log("YWW");

            for (int _column = 0; _column < board.width - 2; _column++)
            {
                if (board.allDots[_column, _row] != null)
                {
                    if (board.allDots[_column + 1, _row] != null && board.allDots[_column + 2, _row] != null)
                    {
                        if ((board.allDots[_column, _row].gameObject.CompareTag(board.allDots[_column + 1, _row].gameObject.tag)) && ((board.allDots[_column, _row].CompareTag(board.allDots[_column + 2, _row].gameObject.tag))))
                        {
                            board.allDots[_column, _row].GetComponent<Dot>().isMatched = true;
                            board.allDots[_column + 1, _row].GetComponent<Dot>().isMatched = true;
                            board.allDots[_column + 2, _row].GetComponent<Dot>().isMatched = true;
                            howManyDotsMatched++;

                        }
                    }

                    if (board.allDots[_column, _row + 1] != null && board.allDots[_column, _row + 2] != null) {
                        if ((board.allDots[_column, _row].CompareTag(board.allDots[_column, _row + 1].tag)) && (board.allDots[_column, _row].CompareTag(board.allDots[_column, _row + 2].tag)))
                        {
                            board.allDots[_column, _row].GetComponent<Dot>().isMatched = true;
                            board.allDots[_column, _row + 1].GetComponent<Dot>().isMatched = true;
                            board.allDots[_column, _row + 2].GetComponent<Dot>().isMatched = true;
                            howManyDotsMatched++;

                        }
                    }
                }
            }
        }

        // 예외 지역 처리

        for (int _row = board.height - 2; _row < board.height; _row++)
        {
            for (int _column = 0; _column < board.width - 2; _column++)
            {
                if (board.allDots[_column, _row] != null)
                {
                    if (board.allDots[_column + 1, _row] != null && board.allDots[_column + 2, _row] != null)
                    {

                        if ((board.allDots[_column, _row].tag == board.allDots[_column + 1, _row].tag) && (board.allDots[_column, _row].tag == board.allDots[_column + 2, _row].tag))
                        {
                            board.allDots[_column, _row].GetComponent<Dot>().isMatched = true;
                            board.allDots[_column + 1, _row].GetComponent<Dot>().isMatched = true;
                            board.allDots[_column + 2, _row].GetComponent<Dot>().isMatched = true;
                            howManyDotsMatched++;

                        }
                    }
                }
            }
        }

        for (int _column = board.width - 2; _column < board.width; _column++)
        {
            for (int _row = 0; _row < board.height - 2; _row++)
            {
                if (board.allDots[_column, _row] != null)
                {
                    if (board.allDots[_column, _row + 1] != null && board.allDots[_column, _row + 2] != null)
                    {

                        if ((board.allDots[_column, _row].tag == board.allDots[_column, _row + 1].tag) && (board.allDots[_column, _row].tag == board.allDots[_column, _row + 2].tag))
                        {
                            board.allDots[_column, _row].GetComponent<Dot>().isMatched = true;
                            board.allDots[_column, _row + 1].GetComponent<Dot>().isMatched = true;
                            board.allDots[_column, _row + 2].GetComponent<Dot>().isMatched = true;

                            howManyDotsMatched++;
                        }
                    }
                }
            }
        }

        return;
    }

}
    /*
    private IEnumerator DestroyingDotsCo()
    {
        while(true)
        {
            if(state == State.matchDone)
            {
                Debug.Log(state.ToString());

                state = State.destroy;

                foreach(GameObject dot in board.allDots)
                {
                    if(dot != null && dot.GetComponent<Dot>().isMatched)
                    {
                        SpriteRenderer spriteRenderer = dot.GetComponent<SpriteRenderer>();
                        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);

                        Destroy(dot, 0.2f);

                        board.allDots[dot.GetComponent<Dot>().column, dot.GetComponent<Dot>().row] = null;
                    }
                }

                state = State.destoryDone;
            }

            yield return null;
        }
    }

    private void CountNulls()
    {
        for (int _column = 0; _column < board.width; _column++)
        {
            int nullCount = 0;

            for (int _row = 0; _row < board.height; _row++)
            {
                if (board.allDots[_column, _row] == null)
                {
                    nullCount++;
                }
                else if(nullCount != 0)
                {
                    board.allDots[_column, _row].GetComponent<Dot>().nullCount = nullCount;
                    howManyDotsNeedDrop++;
                }
            }
        }

        if(howManyDotsNeedDrop == 0)
        {
            state = State.dropDone;
        } else
        {
            state = State.countDone;

        }
    }

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

                for (int _column = 0; _column < board.width; _column++)
                {
                    Debug.Log("Yaho");

                    for (int _row = 0; _row < board.height; _row++)
                    {
                        if (board.allDots[_column, _row] != null)
                        {
                            if (board.allDots[_column, _row].GetComponent<Dot>().nullCount != 0)
                            {
                                board.allDots[_column, _row].transform.Translate(new Vector2(0f, -0.2f));

                                if ((board.allDots[_column, _row].transform.position.y - (board.allDots[_column, _row].GetComponent<Dot>().row) + board.allDots[_column, _row].GetComponent<Dot>().nullCount) < 0.05f)
                                {
                                    Debug.Log("hi im elfo");

                                    board.allDots[_column, _row].transform.position = new Vector2(board.allDots[_column, _row].GetComponent<Dot>().column, board.allDots[_column, _row].GetComponent<Dot>().row - board.allDots[_column, _row].GetComponent<Dot>().nullCount);
                                    board.allDots[_column, _row].GetComponent<Dot>().row = board.allDots[_column, _row].GetComponent<Dot>().row - board.allDots[_column, _row].GetComponent<Dot>().nullCount;

                                    //초기화
                                    int tempNullCount = board.allDots[_column, _row].GetComponent<Dot>().nullCount;

                                    board.allDots[_column, _row - tempNullCount] = board.allDots[_column, _row];

                                    board.allDots[_column, _row].GetComponent<Dot>().nullCount = 0;
                                    board.allDots[_column, _row] = null;

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