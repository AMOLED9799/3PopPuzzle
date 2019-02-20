using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour {


	private Board board;


    public int column;
	public int row;

    public bool isMatched = false;

	void Start () {
		board = FindObjectOfType<Board> ();
	}

	//*****************************************
	void Update () {
		
        	 
	}
	//*****************************************

}