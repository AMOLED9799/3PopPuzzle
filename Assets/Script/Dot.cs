using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : Dot_Mom {

    void Start () {
        StartCoroutine(MoveDotToCR());
        StartCoroutine(DestroyDotCo());
    }

    //*****************************************
    void Update () {
             
    }
    //*****************************************


    protected override IEnumerator DestroyDotCo()
    {
        for(; ; )
        {
            // destroyDotTF가 true일 때
            if (destroyDotTF || destroyedBySpecialDotTF || isCheckedByColorPop)
            {
                // 1초 기다린 후 (swipe나 drop하는 경우 움직이는 액션을 기다려준다)
                yield return new WaitForSeconds(0.2f);

                                // 색을 잠시 변하게 하고
                SpriteRenderer sprite = GetComponent<SpriteRenderer>();
                sprite.color = new Color(0.5f, 0.5f, 0.5f);

                // 0.5초 후에
                yield return new WaitForSeconds(0.3f);

                if (Board.board.allDots[column, row] != null)
                {
                    DotManager.dotManager.howManyDotsDestroy--;

                    // board를 초기화시키고, Dot을 destroy시킨다
                    Board.board.allDots[column, row] = null;
                }

                destroyDotTF = false;
                destroyedBySpecialDotTF = false;

                Destroy(this.gameObject);


            }

            yield return null;
        }
    }
}