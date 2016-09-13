using UnityEngine;
using System.Collections;

public class TestCode : MonoBehaviour {

    UIReuseScrollView rsv;

    public uint ItemCount;
    public uint PopIndex;

    void Awake()
    {
        if( null == rsv )
        {
            rsv = gameObject.GetComponent<UIReuseScrollView>();
        }

        PopIndex = 0;
    }
	// Use this for initialization
	void Start () {
        if( null != rsv )
        {
            rsv.Init((int)ItemCount);
        }
	
	}
	
    public void Push()
    {
        if( null != rsv )
        {
            ++ItemCount;
            rsv.PushItem();
        }
        
    }

    public void Pop()
    {
        if (null != rsv)
        {
            if (0 < ItemCount)
            {
                --ItemCount;
                rsv.PopItem();
            }
        }

    }

    public void Refresh()
    {
        if (null != rsv)
        {
            rsv.Refresh((int)ItemCount);
        }
    }
}
