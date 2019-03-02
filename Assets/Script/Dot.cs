using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : Dot_Mom {

    void Start () {
        StartCoroutine(MoveDotToCR());
        StartCoroutine(DestroyDot());
    }

    //*****************************************
    void Update () {
             
    }
    //*****************************************


    protected override IEnumerator DestroyDot()
    {
        for(; ; )
        {
            // destroyDotTF가 true일 때
            if (destroyDotTF || destroyedBySpecialDotTF)
            {
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