using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour {

	private Vector2 firstTouchPosition;
	private Vector2 finalTouchPosition;
	private GameObject otherDot;
	private Board board;
	private Vector2 targetPosition;

	public float swipeAngle = 0;
	public int column;
	public int row;
	public float swipeResist = 1f;
	public bool swipeing;
	private Vector2 velocity = Vector2.zero;

	// Use this for initialization
	void Start () {
		board = FindObjectOfType<Board> ();
		swipeing = false;
	}

	// Update is called once per fra
	void Update () {
		if (swipeing) {
			transform.position = Vector2.SmoothDamp (transform.position, new Vector2 (column, row), ref velocity, 0.3f, 10f, Time.deltaTime);
			if (transform.position.x == column && transform.position.y == row) {
				swipeing = false;
			}
		}
	} 

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
		swipeing = true;
		otherDot.GetComponent<Dot> ().swipeing = true;
	}
}
