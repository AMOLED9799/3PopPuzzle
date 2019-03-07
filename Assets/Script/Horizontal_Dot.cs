using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Horizontal_Dot : Dot_Mom {

	// Use this for initialization
	void Start () {
        StartCoroutine(MoveDotToCR());
        StartCoroutine(DestroyDotCo());
    }

    // Update is called once per frame
    void Update() {

    }

    protected override IEnumerator DestroyDotCo()
    {
        for (; ; )
        {
            // destroyDotTF가 true일 때
            if (destroyDotTF || destroyedBySpecialDotTF || isCheckedByColorPop)
            {
                StartCoroutine(HorizontalDominoCo());

                // 1초 기다린 후 (swipe나 drop하는 경우 움직이는 액션을 기다려준다)
                yield return new WaitForSeconds(0.2f);

                // 색을 잠시 변하게 하고
                SpriteRenderer sprite = GetComponent<SpriteRenderer>();
                sprite.color = new Color(0.5f, 0.5f, 0.5f);

                while(!dominoDone)
                {
                    yield return new WaitForEndOfFrame();
                }

                if (Board.board.allDots[column, row] != null)
                {
                    DotManager.dotManager.howManyDotsDestroy--;

                    // board를 초기화시키, Dot을 destroy시킨다
                    Board.board.allDots[column, row] = null;
                }
                
                destroyDotTF = false;
                destroyedBySpecialDotTF = false;

                Destroy(this.gameObject);
            }

            yield return null;
        }
    }

    private IEnumerator HorizontalDominoCo() {
        for (int _column = 0; _column < Board.board.width; _column++)
        {
            if (column - _column < 0 && column + _column > Board.board.width)
            {
                break;
            }
            // null 이 아닌 Dot 중에서
            if (column - _column >= 0 && Board.board.allDots[column - _column, row] != null)
            {
                if (!Board.board.allDots[column - _column, row].GetComponent<Dot_Mom>().destroyDotTF & !Board.board.allDots[column - _column, row].GetComponent<Dot_Mom>().destroyedBySpecialDotTF)
                {
                    Board.board.allDots[column - _column, row].GetComponent<Dot_Mom>().destroyedBySpecialDotTF = true;
                    DotManager.dotManager.howManyDotsDestroy++;
                }
            }
            if (_column + column < Board.board.width && Board.board.allDots[column + _column, row] != null)
            {
                if(!Board.board.allDots[_column + column, row].GetComponent<Dot_Mom>().destroyDotTF & !Board.board.allDots[column + _column, row].GetComponent<Dot_Mom>().destroyedBySpecialDotTF)
                {
                    Board.board.allDots[column + _column, row].GetComponent<Dot_Mom>().destroyedBySpecialDotTF = true;
                    DotManager.dotManager.howManyDotsDestroy++;
                }
            }

            yield return new WaitForSeconds(0.01f);
        }

        dominoDone = true;
    }
}
