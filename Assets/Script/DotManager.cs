using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotManager : MonoBehaviour {

    private Board board;

    GameObject selectedDot;
    GameObject neighborDot;

    // 각도계산
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private float swipeAngle = 0f;

	void Start () {
        board = FindObjectOfType<Board>();
	}
	

   	void Update () {

        // raycast로 마우스가 있는 위치의 object를 가져온다
		if(Input.GetMouseButtonDown(0))
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

            // Dot의 column, row를 이동시킨다
            SwipeDots();

            // 이동한 좌표에 따라 실제 Dot을 움직인다
            MoveDots(selectedDot, neighborDot);

        }
    }

    private void SwipeDots()
    {
        int row = selectedDot.GetComponent<Dot>().row;
        int column = selectedDot.GetComponent<Dot>().column;

        // Swipe With Up
        if(swipeAngle > 60 && swipeAngle < 120)
        {
            // neighborDot 지정
            neighborDot = board.allDots[column, row + 1];

            // [column, row] 이동
            neighborDot.GetComponent<Dot>().row--;
            selectedDot.GetComponent<Dot>().row++;
        }
        // Swipe with Down
        else if (swipeAngle < -60 && swipeAngle > -120) 
        {
            // neighborDot 지정
            neighborDot = board.allDots[column, row - 1];

            // [column, row] 이동
            neighborDot.GetComponent<Dot>().row++;
            selectedDot.GetComponent<Dot>().row--;
        } 
        // Swipe with Right
        else if (swipeAngle > - 30 && swipeAngle < 30)
        {
            // neighborDot 지정
            neighborDot = board.allDots[column + 1,  row];

            // [column, row] 이동
            neighborDot.GetComponent<Dot>().column--;
            selectedDot.GetComponent<Dot>().column++;
        }
        // Swipe with Left
        else if (swipeAngle > 150 || swipeAngle < -150)
        {
            // neighborDot 지정
            neighborDot = board.allDots[column - 1, row];

            // [column, row] 이동
            neighborDot.GetComponent<Dot>().column++;
            selectedDot.GetComponent<Dot>().column--;
        }
        // else : swipe각도가 대각선으로 치우친 경우 -> 초기화
        else
        {
            selectedDot = null;
            firstTouchPosition = Vector2.zero;
            return ;
        }


        return ;
    }

    // board 의 allDots[]에 column, row 가 바뀌어 있는 상태에서 좌표를 목적지로 하여 Dot 이미지를 이동시키는 메서드
    private void MoveDots(GameObject _selectedDot, GameObject _neighborDot)
    {
        Vector2 selectedVelocity = Vector2.zero;
        Vector2 neighborVelocity = Vector2.zero;

        // Dot 이동
        while (true)
        {
            _selectedDot.transform.position = Vector2.SmoothDamp(_selectedDot.transform.position, new Vector2(_selectedDot.GetComponent<Dot>().column, _selectedDot.GetComponent<Dot>().row), ref selectedVelocity, 1f, 10f, Time.deltaTime);
            _neighborDot.transform.position = Vector2.SmoothDamp(_neighborDot.transform.position, new Vector2(_neighborDot.GetComponent<Dot>().column, _neighborDot.GetComponent<Dot>().row), ref selectedVelocity, 1f, 10f, Time.deltaTime);
        
            if(Mathf.Abs(_selectedDot.transform.position.x - _selectedDot.GetComponent<Dot>().column) < 0.07f)
            {
                _selectedDot.transform.position = new Vector2(_selectedDot.GetComponent<Dot>().column, _selectedDot.GetComponent<Dot>().row);
                _neighborDot.transform.position = new Vector2(_neighborDot.GetComponent<Dot>().column, _neighborDot.GetComponent<Dot>().row);
                break;
            }
        }
    }
}
