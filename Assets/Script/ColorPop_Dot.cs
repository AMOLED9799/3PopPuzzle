using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPop_Dot : Dot_Mom {

	// Use this for initialization
	void Start () {
        StartCoroutine(MoveDotToCR());
        StartCoroutine(DestroyDotCo());
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    protected override IEnumerator DestroyDotCo()
    {
        for (; ; )
        {
            // ColorPop의 Skill은 Match 단계에서 마무리함.

            // destroyDotTF가 true일 때
            if (destroyDotTF || destroyedBySpecialDotTF)
            {

                // 1초 기다린 후 (swipe나 drop하는 경우 움직이는 액션을 기다려준다)
                yield return new WaitForSeconds(0.2f);

                // 색을 잠시 변하게 하고
                SpriteRenderer sprite = GetComponent<SpriteRenderer>();
                sprite.color = new Color(0.5f, 0.5f, 0.5f);

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

    public void ColorPop(GameObject swipedDot)
    {
        Debug.Log("ColorPopCoroutine Executed");

        foreach(GameObject dot in Board.board.allDots)
        {
            // swipe한 Dot과 같은 tag를 가진 Dot을 모두 Check한다
            if(swipedDot.CompareTag(dot.tag))
            {
                dot.GetComponent<Dot_Mom>().isCheckedByColorPop = true;
                DotManager.dotManager.howManyDotsMatched++;
            }
        }

        this.isCheckedByColorPop = true;

        return;
    }

}
