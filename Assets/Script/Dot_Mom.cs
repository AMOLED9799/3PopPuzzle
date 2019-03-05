using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot_Mom : MonoBehaviour
{
    public int column;
    public int row;
    public int nullCount = 0;

    public bool swipeDotTF = false;
    public bool destroyDotTF = false;
    public bool dropDotTF = false;
    public bool firstSet = false;
    public bool refillDotTF = false;

    public bool destroyedBySpecialDotTF = false;

    private bool stateChanged = false;
    public bool dot2Drop = false;
    public bool isMatched = false;
    public bool isCheckedByColorPop = false;

    public bool isCheckedForSpecial = false;
    protected bool dominoDone = false;

    public Vector2 velocity = Vector2.zero;


    void Start()
    {
    }

    //*****************************************
    void Update()
    {

    }
    //*****************************************


    public IEnumerator MoveDotToCR()
    {
        for (; ; )
        {
            if (swipeDotTF || dropDotTF || refillDotTF)
            {
                // 목표지점으로 smoothDamp로 이동시킨다
                transform.position = Vector2.SmoothDamp(transform.position, new Vector2(column, row), ref velocity, 0.1f, 10f, Time.deltaTime);

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

    protected virtual IEnumerator DestroyDotCo()
    {
        yield return null;
    }
}