using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Horizontal_Dot : Dot_Mom {

	// Use this for initialization
	void Start () {
        StartCoroutine(MoveDotToCR());
        StartCoroutine(DestroyDot());
    }

    // Update is called once per frame
    void Update() {

    }

      protected override IEnumerator DestroyDot()
    {
        for (; ; )
        {
            // destroyDotTF가 true일 때
            if (destroyDotTF || destroyedBySpecialDotTF)
            {
                for (int _column = 0; _column < Board.board.width; _column++)
                {
                    // null 이 아닌 Dot 중에서
                    if (Board.board.allDots[_column, row] == null)
                    {
                        Debug.Log("삐빅");
                    }

                    else
                    {
                        if (!Board.board.allDots[_column, row].GetComponent<Dot_Mom>().destroyDotTF & !Board.board.allDots[_column, row].GetComponent<Dot_Mom>().destroyedBySpecialDotTF)
                        {
                            Board.board.allDots[_column, row].GetComponent<Dot_Mom>().destroyedBySpecialDotTF = true;
                            DotManager.dotManager.howManyDotsDestroy++;
                        }
                    }
                }

                // 1초 기다린 후 (swipe나 drop하는 경우 움직이는 액션을 기다려준다)
                yield return new WaitForSeconds(0.3f);

                // 색을 잠시 변하게 하고
                SpriteRenderer sprite = GetComponent<SpriteRenderer>();
                sprite.color = new Color(0.5f, 0.5f, 0.5f);

                // 0.5초 후에
                yield return new WaitForSeconds(0.5f);

                // board를 초기화시키, Dot을 destroy시킨다
                Board.board.allDots[column, row] = null;
                Destroy(this.gameObject);

                DotManager.dotManager.howManyDotsDestroy--;

                destroyDotTF = false;
                destroyedBySpecialDotTF = false;
            }

            yield return null;
        }
    }

}
