using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour {

	private Vector2 firstTouchPosition;
	private Vector2 finalTouchPosition;
	private GameObject otherDot;
	private GameObject tempDot;
	private Board board;
	private Vector2 targetPosition;

	public float swipeAngle = 0;
	public int column;
	public int row;
	public float swipeResist = 1f;

	public bool swipeing = false;
	public bool dropping = false;

	public int nullUnderMe = 0;

	private Vector2 velocity = Vector2.zero;

	public bool isMatched = false;

	void Start () {
		board = FindObjectOfType<Board> ();
	}

	//*****************************************
	void Update () {

		// swipe position of Dots
		if (swipeing) {		
			transform.position = Vector2.SmoothDamp (transform.position, new Vector2 (column, row), ref velocity, 0.3f, 10f, Time.deltaTime);
			if (Mathf.Abs (transform.position.x - column) < 0.05f && Mathf.Abs (transform.position.y - row) < 0.05f) {
				transform.position = new Vector2 (column, row);
				swipeing = false;
			}
		} 
		
		// drop Dot to bottom way

		if (!swipeing) {
			CheckMatch ();
		}

		DestoryDot ();
		 
	}
	//*****************************************

	private void OnMouseDown() {
		firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		//Debug.Log (firstTouchPosition);
	}

	private void OnMouseUp() {
		finalTouchPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		CalculateAngle ();
	} 

	void CalculateAngle() {
		if ((Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist) || (Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)) {
			swipeAngle = Mathf.Atan2 (finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 100 / Mathf.PI;
			MovePoint();	// column and row
		}
	}

	void MovePoint() {
		if (swipeAngle > -35 && swipeAngle <= 35 && column < board.width-1) {			// move to Right
			otherDot = board.allDots[column + 1, row];
			otherDot.GetComponent<Dot> ().column -= 1;
			column += 1;
		} else if (swipeAngle > 35 && swipeAngle <= 65 && row < board.height-1) {	// move to Upward
			otherDot = board.allDots[column, row + 1];
			otherDot.GetComponent<Dot> ().row -= 1;
			row += 1;
		} else if (swipeAngle > -65 && swipeAngle <= -35 && row > 0) {	// move to Downward
			otherDot = board.allDots[column, row-1];
			otherDot.GetComponent<Dot> ().row += 1;
			row -= 1;
		} else if ((swipeAngle < -65 || swipeAngle > 65) && column > 0) {	// move to Left
			otherDot = board.allDots[column - 1, row];
			otherDot.GetComponent<Dot> ().column += 1;
			column -= 1;
		}

		tempDot = otherDot;
		board.allDots [column, row] = this.gameObject;
		board.allDots [otherDot.GetComponent<Dot> ().column, otherDot.GetComponent<Dot> ().row] = tempDot;
		tempDot = null;

		swipeing = true;
		otherDot.GetComponent<Dot> ().swipeing = true;
	}

	private void CheckMatch()
	{
		if (column < board.width - 2) {
			if (board.allDots [column + 1, row] != null && board.allDots [column + 2, row] != null) {
				if (transform.tag == board.allDots [column + 1, row].tag && transform.tag == board.allDots [column + 2, row].tag) {
					isMatched = true;
					board.allDots [column + 1, row].GetComponent<Dot> ().isMatched = true;
					board.allDots [column + 2, row].GetComponent<Dot> ().isMatched = true;
				}
			}
		}

		if (row < board.height - 2) {
			if (board.allDots [column, row + 1] != null && board.allDots [column, row + 2] != null) {
				if (transform.tag == board.allDots [column, row + 1].tag && transform.tag == board.allDots [column, row + 2].tag) {
					isMatched = true;
					board.allDots [column, row + 1].GetComponent<Dot> ().isMatched = true;
					board.allDots [column, row + 2].GetComponent<Dot> ().isMatched = true;
				}
			}
		}
	}

	private void DestoryDot() 
	{
		if (isMatched) {
			board.allDots [column, row] = null;
			board.desTriggered = true;
			Destroy(this.gameObject,0.3f);
		}
	}
}