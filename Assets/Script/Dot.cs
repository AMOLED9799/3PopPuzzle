using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour {



    public int column;
    public int row;
    public int nullCount = 0;

    public bool swipeDotTF = false;
    public bool destroyDotTF = false;
    public bool dropDotTF = false;
    public bool firstSet = false;
    public bool refillDotTF = false;

    private bool stateChanged = false;
    public bool dot2Drop = false;
    public bool isMatched = false;


    public Vector2 velocity = Vector2.zero;


    void Start () {
        StartCoroutine(MoveDotToCR());
        StartCoroutine(DestroyDot());
    }

    //*****************************************
    void Update () {
             
    }
    //*****************************************


    public IEnumerator MoveDotToCR()
    {
        for(; ;)
        {
            if (swipeDotTF || dropDotTF || refillDotTF)
            {
                // 목표지점으로 smoothDamp로 이동시킨다
                transform.position = Vector2.SmoothDamp(transform.position, new Vector2(column, row), ref velocity, 0.3f, 10f, Time.deltaTime);
                
                // 목표지점에 거의 가까워지면
                if (Mathf.Abs(transform.position.x - column) < 0.05f && Mathf.Abs(transform.position.y - row) < 0.05f)
                {
                    // 목표지점으로 포개어버리고
                    this.gameObject.transform.position = new Vector2(column, row);

                    // 조건을 초기화
                    
                    if (dropDotTF)
                    {
                        DotManager.dotManager.howManyDotsNeedDrop--;
                    }

                    swipeDotTF = false;
                    refillDotTF = false;
                    dropDotTF = false;
                    dot2Drop = false;

                    yield return null;
                }

            }

            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator DestroyDot()
    {
        for(; ; )
        {
            // destroyDotTF가 true일 때
            if (destroyDotTF)
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
            }

            yield return null;
        }
    }
}