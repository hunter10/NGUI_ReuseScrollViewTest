using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIListItemGroup : MonoBehaviour {

    public UIGrid mGrid;

    private UIListItem mItem_L;
    private UIListItem mItem_R;

    
    public void Awake()
    {
        mItem_L = null;
        mItem_R = null;
    }

    public void AddChild(GameObject prefab)
    {
        GameObject goTemp = NGUITools.AddChild(this.gameObject, prefab);

        if (mItem_L == null)
            mItem_L = goTemp.GetComponent<UIListItem>();
        else
            mItem_R = goTemp.GetComponent<UIListItem>();

        mGrid.AddChild(goTemp.transform);
    }

    // 필요함수 (인덱스가 변경될때 호출됩니다.)
    public void OnChangeItem(int index, int nTotalItemCount)
    {
        if (mItem_L != null)
            mItem_L.OnChangeItem(index * 2);

        // 오른쪽은 이미 만들어져 있다면 재활용시 전체아이템 갯수보다 작다면 보이지 않아야 함.
        int rightIndex = (index * 2) + 1;
        if (rightIndex < nTotalItemCount)
        {
            if (mItem_R != null)
            {
                mItem_R.gameObject.SetActive(true);
                mItem_R.OnChangeItem(rightIndex);
            }
        }
        else
        {
            if (mItem_R != null)
                mItem_R.gameObject.SetActive(false);
        }
    }

    public bool IsFull()
    {
        if (mItem_L != null && mItem_R != null)
            return true;

        return false;
    }
}
