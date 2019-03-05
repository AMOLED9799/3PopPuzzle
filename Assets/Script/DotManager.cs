using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotManager : MonoBehaviour {

    public static DotManager dotManager;


    public GameObject selectedDot = null;
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
    private bool swipeHappened = false;

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

                        // Swipe 명령이 들엉온 이후 실행된 이후 한 번만 실행하는 메서드
                        if (startState)
                        {
                            // Dot의 column, row를 이동시킨다 (화면상이 아닌 데이터상에서 움직임)
                            SwipeDotsCR();

                            if (state == State.stable)
                            {
                                break;
                            }

                            // Dot을 실제로 움직이기
                            // selected Dot은 Mouse Down Input이 들어오면서 결정된다.
                            selectedDot.GetComponent<Dot_Mom>().swipeDotTF = true;
                            // neighbor Dot은 Mouse Up Input을 통한 각도처리와 함께 결정된다.
                            neighborDot.GetComponent<Dot_Mom>().swipeDotTF = true;

                            startState = false;
                        }

                        // state 탈출 조건 : 두 Dot이 모두 이동을 마쳤을 때
                        if (selectedDot.GetComponent<Dot_Mom>().swipeDotTF == false && neighborDot.GetComponent<Dot_Mom>().swipeDotTF == false)
                        {
                            swipeHappened = true;

                            // checkMatch State로 이동
                            state = State.checkMatch;
                        }

                        break;
                    }

                case State.checkMatch:
                    {

                        // 모든 매치검사를 실행
                        MatchingDot();

                        // match 결과 match가 존재할 때 => isMatched인 Dot을 Destroy 할 차례
                        if (matchExist && matchDone)
                        {
                            matchDone = false;
                            matchExist = false;

                            swipeHappened = false;
                            startState = true;
                            state = State.destroyMatch;
                        }


                        // match 결과 match가 없을 때 => Swipe한 Dot이 다시 자기 자리로 돌아가야함
                        else if (!matchExist && matchDone)
                        {
                            matchDone = false;

                            // *******************************************************************  자기자리로 돌아가는 메서드 구현해야함
                            if (swipeHappened == true)
                            {
                                state = State.swipeDot;

                                // Board 의 allDots 에서 바꿔주기
                                GameObject tempObject = Board.board.allDots[neighborDot.GetComponent<Dot_Mom>().column, neighborDot.GetComponent<Dot_Mom>().row];
                                Board.board.allDots[neighborDot.GetComponent<Dot_Mom>().column, neighborDot.GetComponent<Dot_Mom>().row] = Board.board.allDots[selectedDot.GetComponent<Dot_Mom>().column, selectedDot.GetComponent<Dot_Mom>().row];
                                Board.board.allDots[selectedDot.GetComponent<Dot_Mom>().column, selectedDot.GetComponent<Dot_Mom>().row] = tempObject;

                                // Row 와 Column 바꿔주기
                                int goBackColumn = neighborDot.GetComponent<Dot_Mom>().column;
                                int goBackRow = neighborDot.GetComponent<Dot_Mom>().row;

                                neighborDot.GetComponent<Dot_Mom>().column = selectedDot.GetComponent<Dot_Mom>().column;
                                neighborDot.GetComponent<Dot_Mom>().row = selectedDot.GetComponent<Dot_Mom>().row;

                                selectedDot.GetComponent<Dot_Mom>().column = goBackColumn;
                                selectedDot.GetComponent<Dot_Mom>().row = goBackRow;

                                // Swipe 실행하도록 TF 켜기
                                selectedDot.GetComponent<Dot_Mom>().swipeDotTF = true;
                                neighborDot.GetComponent<Dot_Mom>().swipeDotTF = true;

                                // 다시 안정화된 상태로 돌아가기
                                state = State.stable;

                            }

                            swipeHappened = false;

                            if (selectedDot != null || neighborDot != null)
                            {
                                selectedDot = null;
                                neighborDot = null;
                            }
                            state = State.stable;
                        }

                        
                        break;
                    }

                case State.destroyMatch:
                    {
                        if (startState)
                        {
                            // 모든 dot에 대해 isMatched 를 확인해 destroy 코루틴을 실행시킨다
                            foreach (GameObject dot in Board.board.allDots)
                            {
                                if (dot != null)
                                {
                                    if (dot.GetComponent<Dot_Mom>().isMatched || dot.GetComponent<Dot_Mom>().isCheckedByColorPop)
                                    {
                                        // howManyDotsDestroy 카운트를 올려 다 터졌는지 검사
                                        if(dot.GetComponent<Dot>() != null)
                                            howManyDotsDestroy++;

                                        // destroyDotTF true로 체크하여 각 Dot의 Dot Script를 통해 제거
                                        dot.GetComponent<Dot_Mom>().destroyDotTF = true;
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
                                if (dot != null && dot.GetComponent<Dot_Mom>().dot2Drop)
                                {
                                    dot.GetComponent<Dot_Mom>().dropDotTF = true;
                                }
                            }

                            startState = false;
                        }

                        if (howManyDotsNeedDrop == 0)
                        {

                            yield return new WaitForEndOfFrame();
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
        int row = selectedDot.GetComponent<Dot_Mom>().row;
        int column = selectedDot.GetComponent<Dot_Mom>().column;
        GameObject tempObject;

        // Swipe With Up
        if (swipeAngle > 60 && swipeAngle < 120)
        {
            // neighborDot 지정
            neighborDot = Board.board.allDots[column, row + 1];

            // [column, row] 이동
            neighborDot.GetComponent<Dot_Mom>().row--;
            selectedDot.GetComponent<Dot_Mom>().row++;

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
            neighborDot.GetComponent<Dot_Mom>().row++;
            selectedDot.GetComponent<Dot_Mom>().row--;

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
            neighborDot.GetComponent<Dot_Mom>().column--;
            selectedDot.GetComponent<Dot_Mom>().column++;

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
            neighborDot.GetComponent<Dot_Mom>().column++;
            selectedDot.GetComponent<Dot_Mom>().column--;

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

            state = State.stable;
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

        if(selectedDot != null && neighborDot != null && (selectedDot.tag == "ColorPop" || neighborDot.tag == "ColorPop"))
        {
            if(selectedDot.CompareTag("ColorPop"))
            {
                selectedDot.GetComponent<ColorPop_Dot>().ColorPop(neighborDot);
            }
            else if (neighborDot.CompareTag("ColorPop"))
            {
                neighborDot.GetComponent<ColorPop_Dot>().ColorPop(selectedDot);
            }


            // 탈출 조건

            if (howManyDotsMatched == 0)
            {
                matchExist = false;
            }

            else
            {
                matchExist = true;
            }

            matchDone = true;
            howManyDotsMatched = 0;
            return;
        }

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
                            Board.board.allDots[_column, _row].GetComponent<Dot_Mom>().isMatched = true;
                            Board.board.allDots[_column + 1, _row].GetComponent<Dot_Mom>().isMatched = true;
                            Board.board.allDots[_column + 2, _row].GetComponent<Dot_Mom>().isMatched = true;

                            howManyDotsMatched++;
                            // Special Dot에 대해 검사 ( 가로 4개 )
                            if ((_column + 3 < Board.board.width && Board.board.allDots[_column, _row].gameObject.CompareTag(Board.board.allDots[_column + 3, _row].gameObject.tag))) {

                                // 가로 5개
                                if (_column + 4 < Board.board.width && Board.board.allDots[_column, _row].gameObject.CompareTag(Board.board.allDots[_column + 4, _row].gameObject.tag)) {
                                  
                                  Debug.Log("5개");

                                    if(Board.board.allDots[_column + 2, _row].gameObject == selectedDot)
                                    {
                                        Debug.Log("swipe로 코루틴 실행");

                                        StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(selectedDot.tag, selectedDot.transform.position, 3));
                                    }

                                    else if (Board.board.allDots[_column + 2, _row].gameObject == neighborDot)
                                    {
                                        Debug.Log("swipe로 코루틴 실행");

                                        StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(neighborDot.tag, neighborDot.transform.position, 3));
                                    }
                                    
                                    else
                                    {
                                        Debug.Log("drop으로 코루틴 실행");

                                        StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(Board.board.allDots[_column, _row].gameObject.tag, Board.board.allDots[_column , _row].transform.position, 3));
                                    }
                                }

                                // Special Dot에 대해 검사 ( 가로 4개 )
                                else {
                                    Board.board.allDots[_column + 3, _row].GetComponent<Dot_Mom>().isMatched = true;

                                    Debug.Log("4개");

                                    // if selectedDot이 포함되어 있다면 ( Swipe 의 결과로 Special Dot이 Gen 되는거라면 )
                                    if ((Board.board.allDots[_column + 1, _row].gameObject == selectedDot || Board.board.allDots[_column + 2, _row].gameObject == selectedDot))
                                    {
                                        Debug.Log("swipe로 코루틴 실행");

                                        StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(selectedDot.tag, selectedDot.transform.position, 2));
                                    }

                                    // if neighborDot이 포함되어 있다면 ( 옆에 Dot을 swipe해서 짝을 맞춘 경우 )
                                    else if ((Board.board.allDots[_column + 1, _row].gameObject == neighborDot || Board.board.allDots[_column + 2, _row].gameObject == neighborDot))
                                    {
                                        Debug.Log("swipe로 코루틴 실행");

                                        StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(neighborDot.tag, neighborDot.transform.position, 2));
                                    }

                                    // Drop에 의해 Match 된 경우
                                    else
                                    {
                                        Debug.Log("drop으로 코루틴 실행");
                                        
                                        StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(Board.board.allDots[_column, _row].tag, Board.board.allDots[_column, _row].transform.position, 2));
                                    }
                                } 
                            }

                        }
                    }

                    if (Board.board.allDots[_column, _row + 1] != null && Board.board.allDots[_column, _row + 2] != null) {
                        if ((Board.board.allDots[_column, _row].CompareTag(Board.board.allDots[_column, _row + 1].tag)) && (Board.board.allDots[_column, _row].CompareTag(Board.board.allDots[_column, _row + 2].tag)))
                        {
                            Board.board.allDots[_column, _row].GetComponent<Dot_Mom>().isMatched = true;
                            Board.board.allDots[_column, _row + 1].GetComponent<Dot_Mom>().isMatched = true;
                            Board.board.allDots[_column, _row + 2].GetComponent<Dot_Mom>().isMatched = true;

                            howManyDotsMatched++;

                            // Special Dot에 대해 검사 ( 세로 4개 )
                            if (_row + 3 < Board.board.height && Board.board.allDots[_column, _row].CompareTag(Board.board.allDots[_column, _row + 3].gameObject.tag))
                            {
                                if (_row + 4 < Board.board.height && Board.board.allDots[_column, _row].CompareTag(Board.board.allDots[_column, _row + 4].gameObject.tag))
                                {
                                    Debug.Log("5개");

                                    if ((Board.board.allDots[_column, _row + 3].gameObject == selectedDot && selectedDot != null))
                                    {
                                        Debug.Log("swipe로 코루틴 실행");

                                        StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(selectedDot.tag, selectedDot.transform.position, 3));
                                    }
                                    else if (Board.board.allDots[_column, _row + 3].gameObject == neighborDot && selectedDot != null)
                                    {
                                        Debug.Log("swipe로 코루틴 실행");

                                        StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(neighborDot.tag, neighborDot.transform.position, 3));
                                    }
                                    else
                                    {
                                        Debug.Log("drop으로 코루틴 실행");

                                        StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(Board.board.allDots[_column, _row].gameObject.tag, Board.board.allDots[_column, _row].transform.position, 3));
                                    }
                                }

                                else
                                {
                                    Board.board.allDots[_column, _row + 3].GetComponent<Dot_Mom>().isMatched = true;

                                    Debug.Log("4개");

                                    if ((Board.board.allDots[_column, _row + 1].gameObject == selectedDot || Board.board.allDots[_column, _row + 2].gameObject == selectedDot))
                                    {
                                        Debug.Log("swipe로 코루틴 실행");

                                        StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(selectedDot.tag, selectedDot.transform.position, 1));
                                    }

                                    else if ((Board.board.allDots[_column, _row + 1].gameObject == neighborDot || Board.board.allDots[_column, _row + 2].gameObject == neighborDot))
                                    {
                                        Debug.Log("swipe로 코루틴 실행");

                                        StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(neighborDot.tag, neighborDot.transform.position, 1));
                                    }

                                    else
                                    {
                                        Debug.Log("drop으로 코루틴 실행");

                                        StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(Board.board.allDots[_column, _row].tag, Board.board.allDots[_column, _row].transform.position, 2));
                                    }
                                }
                            }
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
                            Board.board.allDots[_column, _row].GetComponent<Dot_Mom>().isMatched = true;
                            Board.board.allDots[_column + 1, _row].GetComponent<Dot_Mom>().isMatched = true;
                            Board.board.allDots[_column + 2, _row].GetComponent<Dot_Mom>().isMatched = true;

                            howManyDotsMatched++;

                            // Special Dot에 대해 검사 ( 가로 4개 )
                            if ((_column + 3 < Board.board.width && Board.board.allDots[_column, _row].gameObject.CompareTag(Board.board.allDots[_column + 3, _row].gameObject.tag)))
                            {
                                Board.board.allDots[_column + 3, _row].GetComponent<Dot_Mom>().isMatched = true;

                                Debug.Log("4개");

                                // if selectedDot이 포함되어 있다면 ( Swipe 의 결과로 Special Dot이 Gen 되는거라면 )
                                if ((Board.board.allDots[_column + 1, _row].gameObject == selectedDot || Board.board.allDots[_column + 2, _row].gameObject == selectedDot))
                                {
                                    Debug.Log("swipe로 코루틴 실행");

                                    StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(selectedDot.tag, selectedDot.transform.position, 2));
                                } 

                                else if ((Board.board.allDots[_column + 1, _row].gameObject == neighborDot || Board.board.allDots[_column + 2, _row].gameObject == neighborDot)) {
                                    Debug.Log("swipe로 코루틴 실행");

                                    StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(neighborDot.tag, neighborDot.transform.position, 2));
                                }

                                // 그냥 4개 매치되면 실행시킬거 (Row, column)이 가장 작은곳에 Spawn 시킴
                                else
                                {
                                    Debug.Log("drop으로 코루틴 실행");

                                    StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(Board.board.allDots[_column, _row].tag, Board.board.allDots[_column, _row].transform.position, 2));
                                }


                            }
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
                            Board.board.allDots[_column, _row].GetComponent<Dot_Mom>().isMatched = true;
                            Board.board.allDots[_column, _row + 1].GetComponent<Dot_Mom>().isMatched = true;
                            Board.board.allDots[_column, _row + 2].GetComponent<Dot_Mom>().isMatched = true;

                            howManyDotsMatched++;

                            // Special Dot에 대해 검사 ( 세로 4개 )
                            if (_row + 3 < Board.board.height && Board.board.allDots[_column, _row].CompareTag(Board.board.allDots[_column, _row + 3].gameObject.tag))
                            {
                                Board.board.allDots[_column, _row + 3].GetComponent<Dot_Mom>().isMatched = true;

                                Debug.Log("4개");

                                if ((Board.board.allDots[_column, _row + 1].gameObject == selectedDot || Board.board.allDots[_column, _row + 2].gameObject == selectedDot))
                                {
                                    Debug.Log("swipe로 코루틴 실행");

                                    StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(selectedDot.tag, selectedDot.transform.position, 1));
                                }

                                else if ((Board.board.allDots[_column, _row + 1].gameObject == neighborDot || Board.board.allDots[_column, _row + 2].gameObject == neighborDot))
                                {
                                    Debug.Log("swipe로 코루틴 실행");

                                    StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(neighborDot.tag, neighborDot.transform.position, 1));
                                }
                                else
                                {
                                    Debug.Log("drop으로 코루틴 실행");

                                    StartCoroutine(GenSpecialDot.genSpecialDot.GenSpecialDotCo(Board.board.allDots[_column, _row].tag, Board.board.allDots[_column, _row].transform.position, 2));
                                }
                            }
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
        howManyDotsMatched = 0;
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
                    Board.board.allDots[_column, _row].GetComponent<Dot_Mom>().row -= nullCount;
                    Board.board.allDots[_column, _row - nullCount] = Board.board.allDots[_column, _row];
                    Board.board.allDots[_column, _row - nullCount].GetComponent<Dot_Mom>().dot2Drop = true;
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
                dot.GetComponent<Dot_Mom>().column = _column;
                dot.GetComponent<Dot_Mom>().row = _dropRow - nullCount;

                howManyDotsNeedDrop++;
                Board.board.allDots[_column, _dropRow - nullCount].GetComponent<Dot_Mom>().dot2Drop = true;

            }
        }
    }
}