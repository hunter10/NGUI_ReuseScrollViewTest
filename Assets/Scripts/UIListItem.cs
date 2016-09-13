using UnityEngine;
using System.Collections;

public class UIListItem : MonoBehaviour
{

    UILabel mLbName = null;

    // 예제코드
    void Awake()
    {
        if( null == mLbName )
        {
            mLbName = gameObject.GetComponentInChildren<UILabel>();
        }
    }

    // 필요함수 (인덱스가 변경될때 호출됩니다.)
    public void OnChangeItem( int index )
    {
        if( null != mLbName )
        {
            mLbName.text = index.ToString();
        }
    }


	void Start ()
    {
	
	}
	
	void Update ()
    {
	
	}
}
