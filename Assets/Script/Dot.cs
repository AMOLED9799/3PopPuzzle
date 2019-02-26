using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour {


	private Board board;


    public int column;
	public int row;
    public int nullCount = 0;

    public bool swipeMove = false;

    public Vector2 velocity = Vector2.zero;

    public bool isMatched = false;

	void Start () {
		board = FindObjectOfType<Board> ();
        StartCoroutine(swipeDot());
	}

	//*****************************************
	void Update () {
        	 
	}
	//*****************************************


    public IEnumerator swipeDot()
    {
        for(; ;)
        {
            if (swipeMove)
            {   
                transform.position = Vector2.SmoothDamp(transform.position, new Vector2(column, row), ref velocity, 0.3f, 10f, Time.deltaTime);

                if (Mathf.Abs(transform.position.x - column) < 0.05f && Mathf.Abs(transform.position.y - row) < 0.05f)
                {
                    this.gameObject.transform.position = new Vector2(column, row);
                    swipeMove = false;
                    yield return null;

                }

            }

            yield return new WaitForEndOfFrame();
        }
    }
}