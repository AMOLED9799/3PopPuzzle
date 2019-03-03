using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenSpecialDot : MonoBehaviour {

	public static GenSpecialDot genSpecialDot;

	public GameObject[] horizontalDots;
	public GameObject[] verticalDots;

	void awake() {
		genSpecialDot = this;
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public IEnumerator GenSpecialDotCo(string dotTag, Vector2 genPosition, int direction) {
		// direction _ 1 : vertical, 2 : horizontal
		while(true) {
			if(Board.board.allDots[(int)genPosition.x, (int)genPosition.y] == null) {
				if(direction == 1) {
					int dotNumber = 0;
					for(dotNumber = 0; dotNumber < horizontalDots.Length; dotNumber++) {
						if(horizontalDots[dotNumber].CompareTag(dotTag)){
							break;
						}
					}
					
					GameObject dot = Instantiate(horizontalDots[dotNumber], genPosition, Quaternion.identity);

					dot.name = "horizontal_Dot";

					Board.board.allDots[(int) genPosition.x, (int) genPosition.y] = dot;
					dot.GetComponent<Dot_Mom>().column = (int) genPosition.x;
					dot.GetComponent<Dot_Mom>().row = (int) genPosition.y;

					break;
				}
			}
			yield return new WaitForEndOfFrame();
		}

	}
}
