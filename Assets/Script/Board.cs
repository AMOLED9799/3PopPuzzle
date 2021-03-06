﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {

    public static Board board;

	public int width;
	public int height;
	public GameObject tilePrefab;
	public BackgroundTile[,] allTiles;
	public bool needChecking = false;

	public GameObject[] dots;
	public GameObject [,] allDots;
    public GameObject[,] allRefillDots;

    private void Awake()
    {
        board = this;
    }

    void Start () {
		allTiles = new BackgroundTile[width, height];
		allDots = new GameObject[width, height];
        allRefillDots = new GameObject[width, height];
		SetUp ();

	}
	
	private void SetUp() {
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				Vector2 tempPosition = new Vector2 (i , j);
				GameObject backgroundTile = Instantiate (tilePrefab, tempPosition, Quaternion.identity) as GameObject;
				backgroundTile.transform.parent = this.transform;
				backgroundTile.name = "( " + i + ", " + j + " )";

				int dotToUse = Random.Range (0, dots.Length);

				while (MatchesAt (i, j, dots[dotToUse])) {		//조건에 맞는 색이 나올 때 까지 반복 
					dotToUse = Random.Range (0, dots.Length);
				}

              GameObject dot = Instantiate (dots [dotToUse], tempPosition, Quaternion.identity);
                
				dot.transform.parent = this.transform;
				dot.name = "dot";
				allDots [i, j] = dot;
				dot.GetComponent<Dot_Mom> ().column = i;
				dot.GetComponent<Dot_Mom> ().row = j;
			}
		}
	}

	// 생성 당시 겹치는 게 없도록 설정
	private bool MatchesAt(int column, int row, GameObject piece) {
			if (column > 1 && allDots [column - 1, row].tag == piece.tag && allDots [column - 2, row].tag == piece.tag) {
				return true;
			}

			if (row > 1 && allDots [column, row - 1].tag == piece.tag && allDots [column, row - 2].tag == piece.tag) {
				return true;
			}

		return false;
	}
}
