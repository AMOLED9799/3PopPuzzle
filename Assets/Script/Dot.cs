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
		
        	 
	}
	//*****************************************

}