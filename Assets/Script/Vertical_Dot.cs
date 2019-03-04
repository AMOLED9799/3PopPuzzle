using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertical_Dot : Dot_Mom {

    void Start()
    {
        StartCoroutine(MoveDotToCR());
        StartCoroutine(DestroyDotCo());
    }

    //*****************************************
    void Update()
    {

    }
    //*****************************************




	protected override IEnumerator DestroyDotCo()
    {
        for (; ; )
        {
            // destroyDotTF가 true일 때
            if (destroyDotTF || destroyedBySpecialDotTF)
            {
                StartCoroutine(VerticalDominoCo());

                // 1초 기다린 후 (swipe나 drop하는 경우 움직이는 액션을 기다려준다)
                yield return new WaitForSeconds(0.2f);

                // 색을 잠시 변하게 하고
                SpriteRenderer sprite = GetComponent<SpriteRenderer>();
                sprite.color = new Color(0.5f, 0.5f, 0.5f);

                while (!dominoDone)
                {
                    yield return new WaitForEndOfFrame();
                }

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

    private IEnumerator VerticalDominoCo()
    {
        for (int _row = 0; _row < Board.board.height; _row++)
        {
            if (row - _row < 0 && row + _row >= Board.board.height)
            {
                Debug.Log("break 실행됨 " + _row.ToString());
                break;
            }
            // null 이 아닌 Dot 중에서
            if (row - _row >= 0 && Board.board.allDots[column, row - _row] != null)
            {
                if (!Board.board.allDots[column, row - _row].GetComponent<Dot_Mom>().destroyDotTF && !Board.board.allDots[column, row - _row].GetComponent<Dot_Mom>().destroyedBySpecialDotTF)
                {
                    Board.board.allDots[column, row - _row].GetComponent<Dot_Mom>().destroyedBySpecialDotTF = true;
                    DotManager.dotManager.howManyDotsDestroy++;
                }
            }

            if (row + _row < Board.board.height && Board.board.allDots[column, row + _row] != null)
            {
                if (!Board.board.allDots[column, row + _row].GetComponent<Dot_Mom>().destroyDotTF && !Board.board.allDots[column, row + _row].GetComponent<Dot_Mom>().destroyedBySpecialDotTF)
                {
                    Board.board.allDots[column, row + _row].GetComponent<Dot_Mom>().destroyedBySpecialDotTF = true;
                    DotManager.dotManager.howManyDotsDestroy++;
                }
            }

            Debug.Log(Board.board.height);
            yield return new WaitForSeconds(0.01f);
            Debug.Log(row.ToString());

        }

        dominoDone = true;
        Debug.Log("For 탈출 ");
    }
}