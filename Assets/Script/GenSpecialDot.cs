﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenSpecialDot : MonoBehaviour {

	public static GenSpecialDot genSpecialDot;

	public GameObject[] horizontalDots;
	public GameObject[] verticalDots;
    public GameObject[] specialDots;

	void Awake() {
		genSpecialDot = this;
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public IEnumerator GenSpecialDotCo(string dotTag, Vector2 genPosition, int direction) {
		// direction _ 1 : horizontal, 2 : vertical
		while(true) {
			if(Board.board.allDots[(int)genPosition.x, (int)genPosition.y] == null) {
                switch (direction)
                {
                    case 1: // Horizontal
                        {
                            int dotNumber = 0;
                            for (dotNumber = 0; dotNumber < horizontalDots.Length; dotNumber++)
                            {
                                if (horizontalDots[dotNumber].CompareTag(dotTag))
                                {
                                    break;
                                    
                                }
                            }

                            // Debug용, 삭제요망
                            if (dotNumber == horizontalDots.Length)
                                break;

                            GameObject dot = Instantiate(horizontalDots[dotNumber], genPosition, Quaternion.identity);

                            dot.name = "horizontal_Dot";

                            Board.board.allDots[(int)genPosition.x, (int)genPosition.y] = dot;
                            dot.GetComponent<Dot_Mom>().column = (int)genPosition.x;
                            dot.GetComponent<Dot_Mom>().row = (int)genPosition.y;

                            break;
                        }

                    case 2: // Vertical
                        {
                            int dotNumber = 0;
                            for (dotNumber = 0; dotNumber < horizontalDots.Length; dotNumber++)
                            {
                                if (verticalDots[dotNumber].CompareTag(dotTag))
                                {
                                    break;
                                }
                            }

                            // Debug 용, 삭제예정
                            if (dotNumber == horizontalDots.Length)
                                break;

                            GameObject dot = Instantiate(verticalDots[dotNumber], genPosition, Quaternion.identity);

                            dot.name = "Vertical_Dot";

                            Board.board.allDots[(int)genPosition.x, (int)genPosition.y] = dot;
                            dot.GetComponent<Dot_Mom>().column = (int)genPosition.x;
                            dot.GetComponent<Dot_Mom>().row = (int)genPosition.y;

                            break;
                        }

                    case 3: // ColorPop
                        {
                            GameObject dot = Instantiate(specialDots[0], genPosition, Quaternion.identity);

                            dot.name = "ColorPop_Dot";

                            Board.board.allDots[(int)genPosition.x, (int)genPosition.y] = dot;
                            dot.GetComponent<Dot_Mom>().column = (int)genPosition.x;
                            dot.GetComponent<Dot_Mom>().row = (int)genPosition.y;

                            break;
                        }
                }
                break;

            }
            yield return new WaitForEndOfFrame();
		}
        yield return null;
	}
}
