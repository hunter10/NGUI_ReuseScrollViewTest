//----------------------------------------------
// NGUI : Custom Control - ReuseScrollView 
// Ver. 1.0.0
// Copyright © 2016 Steve 
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIReuseScrollView : UIScrollView
{
    // Reuse ScrollView
    protected GameObject mHead;         // ReuseScrollView의 Bounds값을 설정하기위해 필요 - 머리 (overflow체크에도 사용) 
    protected GameObject mTail;         // ReuseScrollView의 Bounds값을 설정하기위해 필요 - 꼬리 (overflow체크에도 사용) 
    protected List<GameObject> mList;   // ReuseScrollView의 Body에 해당하는 부분. (화면 출력에 필요한 최소한의 GameObject를 만들어 준다.) - 재사용 게임오브젝트 리스트
    public GameObject mTemplateItem;    // ReuseScrollView의 리스트 아이템의 모양.
    public GameObject mTemplateItemGroup;   // ReuseScrollView의 리스트 그룹 아이템의 모양.
    public Vector2 mItemSize;           // ReuseScrollView의 리스트 아이템의 크기. (가로=x, 세로=y)
    Vector3 mStartPos;                  // ReuseScrollView의 Bounds값의 시작위치.

    int mRelativeIndex = 0;             // 인덱스의 상대값 (0을 기준으로 증가한다.)  
    int mCurIndex = 0;                  // 위치에 따른 인덱스 값.
    int mPrvIndex = 0;                  // 이동에 따른 이전 인덱스 값을 저장한다. 
    int mMaxIndex = 0;                  // 최대 인덱스 값.
    int mItemCount = 0;                 // 아이템의 실제 갯수.
    int mGroupItemCount = 0;            // 그룹아이템의 갯수.
    bool mInit = false;                 

    protected bool bReuseCalculatedBounds = false;  // ReuseScrollView는 ScrollView과 다른 타이밍에 Bounds값을 체크하기 위해 필요하다. 
    private bool m_bUseGroup = false;

    // 기존의 ScrollView의 bounds값과 동일하다. 단지 업데이트 위치를 바꾸기 위해서 bReuseCalculatedBounds변수를 추가하여 사용한다.
    public override Bounds bounds
    {
        get
        {
            if (!mCalculatedBounds && !bReuseCalculatedBounds)
            {
                mCalculatedBounds = true;
                bReuseCalculatedBounds = true;
                mTrans = transform;
                mBounds = NGUIMath.CalculateRelativeWidgetBounds(mTrans, mTrans);
            }
            return mBounds;
        }
    }

    // 처음올 ReuseScrollView의 아이템을 생성할때 사용. (List아이템을 삭제하여 새로 만들어 쓴다.) 
    public void Init(int iCount, bool bUseGroup = false)
    {
        m_bUseGroup = bUseGroup;

        mItemCount = iCount;
        if(bUseGroup)
        {
            mGroupItemCount = (int)iCount / 2;
            mGroupItemCount += 1;
        }
        DestroyBody();

        if (bUseGroup)
            SetTemplate(mGroupItemCount, bUseGroup);
        else
            SetTemplate(iCount);

        mStartPos = mTrans.localPosition;

        if(bUseGroup)
            CreateBody(mGroupItemCount, bUseGroup);
        else
            CreateBody(iCount);

        MovePosition(m_bUseGroup);
        mInit = true;
    }

    // 재설정 (List아이템을 재사용한다.)
    public void Refresh(int iCount, bool bUseGroup = false)
    {
        mItemCount = iCount;
        if (bUseGroup)
        {
            mGroupItemCount = (int)iCount / 2;
            mGroupItemCount += 1;
            SetTemplate(mGroupItemCount, bUseGroup);    // Head, Tail 생성 및 설정
            ResizeBody(mGroupItemCount);      // Body 생성
        }
        else
        {
            SetTemplate(iCount);    // Head, Tail 생성 및 설정
            ResizeBody(iCount);      // Body 생성
        }

        Rebound();
        MovePosition(bUseGroup);
    }

    /*
    public void Refresh( int iCount)
    {
        mItemCount = iCount;
        DestroyBody();
        SetTemplate(iCount);    // Head, Tail 생성 및 설정
        CreateBody(iCount);      // Body 생성
        MovePosition();
    }*/

   
    // Bounds 값을 재설정한다. 
    private void Rebound()
    {
        if (null == mHead || null == mTail) return;
        mHead.SetActive(true);
        mTail.SetActive(true);
        mCalculatedBounds = false;
        bReuseCalculatedBounds = false;
        RestrictWithinBounds(true); //Bounds값 재설정하여 ScrollView의 값을 변경
        UpdateScrollbars();         //Scrollbar의 값을 재설정
        mHead.SetActive(false);
        mTail.SetActive(false);
    }

    // 설정된 아이템을 이용하여 머리와 꼬리위치를 구한다.
    private void SetTemplate(int count, bool bUseGroup = false)
    {
        if(bUseGroup)
            if (null == mTemplateItemGroup) return;
        else
            if (null == mTemplateItem) return;

        if (mHead == null)
        {
            if (bUseGroup)
                mHead = NGUITools.AddChild(gameObject, mTemplateItemGroup);
            else
                mHead = NGUITools.AddChild(gameObject, mTemplateItem);

            mHead.name = "Head";
        }

        if (mTail == null)
        {
            if (bUseGroup)
                mTail = NGUITools.AddChild(gameObject, mTemplateItemGroup);
            else
                mTail = NGUITools.AddChild(gameObject, mTemplateItem);

            mTail.name = "Tail";
        }

        // 값이 없다면 아이템의 크기를 Prefab의 크기로 변환
        if( Vector2.zero == mItemSize )
        {
            Transform tf;
            if (bUseGroup)
                tf = mTemplateItemGroup.transform;
            else
                tf = mTemplateItem.transform;
            Bounds b = NGUIMath.CalculateRelativeWidgetBounds(tf, tf);
            mItemSize.x = b.size.x;
            mItemSize.y = b.size.y;
        }

        float CellWidth = mItemSize.x;
        float CellHeight = mItemSize.y;

        float firstX = panel.baseClipRegion.x - ((panel.baseClipRegion.z - CellWidth) * 0.5f);
        float firstY = panel.baseClipRegion.y + ((panel.baseClipRegion.w - CellHeight + panel.clipSoftness.y) * 0.5f);
        int iTailPos = (0 < count ? count -1 : 0);
        if (movement == Movement.Vertical)
        {
            mHead.transform.localPosition = new Vector3(0, firstY, 0);  //처음위치
            mTail.transform.localPosition = new Vector3(0, firstY - CellHeight * iTailPos, 0); //끝위치
        }
        else
        {
            mHead.transform.localPosition = new Vector3(firstX, 0, 0);  //처음위치
            mTail.transform.localPosition = new Vector3(firstX + CellWidth * iTailPos, 0, 0); //끝위치
        }

        Rebound();
    }

    // 화면에 출력될 아이템을 생성한다. 
    private void CreateBody(int iCount, bool bUseGroup = false)
    {
        if(bUseGroup)
            if (null == mTemplateItemGroup) return;
        else
            if (null == mTemplateItem) return;

        float CellWidth = mItemSize.x;
        float CellHeight = mItemSize.y;

        float firstX = panel.baseClipRegion.x - ((panel.baseClipRegion.z - CellWidth) * 0.5f);
        float firstY = panel.baseClipRegion.y + ((panel.baseClipRegion.w - CellHeight + panel.clipSoftness.y) * 0.5f);
        if (null == mList)
        {
            mList = new List<GameObject>();
        }

        if (movement == Movement.Vertical)
        {
            float fViewport = panel.baseClipRegion.w;
            int iMaxItem = (int)(fViewport / CellHeight);
            int iRemainder = iCount - iMaxItem;
            mMaxIndex = iRemainder;

            int iBuildCount = (iRemainder < 3 ? iCount : iMaxItem + 2);

            if (bUseGroup)
            {
                int tempTotalItemCount = 0;
                // bUseGroupd일때는 iBuildCount는 생성할 그룹갯수
                for (int i = 0; i < iBuildCount; ++i)
                {
                    // 2. 그룹아이템 하나 생성
                    GameObject goTempGroup = NGUITools.AddChild(gameObject, mTemplateItemGroup);

                    for (int j = 0; j < 2; j++)
                    {
                        if (tempTotalItemCount < mItemCount)
                        {
                            tempTotalItemCount++;
                            goTempGroup.GetComponent<UIListItemGroup>().AddChild(mTemplateItem);
                        }
                    }

                    Vector3 vPos = mHead.transform.localPosition;
                    vPos.y = vPos.y - (CellHeight * i);
                    goTempGroup.transform.localPosition = vPos;

                    // 다중 배열일때 조심.
                    mList.Add(goTempGroup);
                }
            }
            else
            {
                for (int i = 0; i < iBuildCount; ++i)
                {
                    GameObject goTemp = NGUITools.AddChild(gameObject, mTemplateItem);

                    Vector3 vPos = mHead.transform.localPosition;
                    vPos.y = vPos.y - (CellHeight * i);
                    goTemp.transform.localPosition = vPos;

                    mList.Add(goTemp);
                }
            }
        }
        else
        {
            float fViewport = panel.baseClipRegion.z;
            int iMaxItem = (int)(fViewport / CellWidth);
            int iRemainder = iCount - iMaxItem;
            mMaxIndex = iRemainder;

            int iBuildCount = (iRemainder < 3 ? iCount : iMaxItem + 2);

            for (int i = 0; i < iBuildCount; ++i)
            {
                GameObject goTemp = NGUITools.AddChild(gameObject, mTemplateItem);

                Vector3 vPos = mHead.transform.localPosition;
                vPos.x = vPos.x + (CellWidth * i);
                goTemp.transform.localPosition = vPos;

                mList.Add(goTemp);
            }
        }
    }

    // CreateBody는 객체를 생성만하지만 ResizeBody는 크기에 맞게 생성 및 삭제, 재사용한다.
    private void ResizeBody( int iCount, bool bUseGroup = false)
    {
        if(bUseGroup)
            if (null == mTemplateItemGroup) return;
        else
            if (null == mTemplateItem) return;

        float CellWidth = mItemSize.x;
        float CellHeight = mItemSize.y;

        float firstX = panel.baseClipRegion.x - ((panel.baseClipRegion.z - CellWidth) * 0.5f);
        float firstY = panel.baseClipRegion.y + ((panel.baseClipRegion.w - CellHeight + panel.clipSoftness.y) * 0.5f);
        if (null == mList)
        {
            mList = new List<GameObject>();
        }

        if (movement == Movement.Vertical)
        {
            float fViewport = panel.baseClipRegion.w;
            int iMaxItem = (int)(fViewport / CellHeight);
            int iRemainder = iCount - iMaxItem;
            mMaxIndex = iRemainder;

            int iBuildCount = (iRemainder < 3 ? iCount : iMaxItem + 2);

            int iFor = (mList.Count <= iBuildCount ? iBuildCount : mList.Count);
            for (int i = 0; i < iFor; ++i)
            {
                // 생성개수
                if(  i < iBuildCount )
                {
                    // Reposition
                    if ( i < mList.Count && null != mList[i]) 
                    {
                        Vector3 vPos = mHead.transform.localPosition;
                        vPos.y = vPos.y - (CellHeight * i);
                        mList[i].transform.localPosition = vPos;
                    }
                    // Add Item
                    else
                    {
                        GameObject goTemp = NGUITools.AddChild(gameObject, mTemplateItem);
                        Vector3 vPos = mHead.transform.localPosition;
                        vPos.y = vPos.y - (CellHeight * i);
                        goTemp.transform.localPosition = vPos;
                        mList.Add(goTemp);
                    }
                }
                // 초과
                else 
                {
                    if(null != mList[i])
                    {
                        GameObject.DestroyImmediate(mList[i]);
                        mList.RemoveAt(i);
                        --i;
                        --iFor;
                    }
                }
            }
        }
        else
        {
            float fViewport = panel.baseClipRegion.z;
            int iMaxItem = (int)(fViewport / CellWidth);
            int iRemainder = iCount - iMaxItem;
            mMaxIndex = iRemainder;

            int iBuildCount = (iRemainder < 3 ? iCount : iMaxItem + 2);
            int iFor = (mList.Count <= iBuildCount ? iBuildCount : mList.Count);
            for (int i = 0; i < iFor; ++i)
            {
                // 생성개수
                if (i < iBuildCount)
                {
                    // Reposition
                    if ( i < mList.Count && null != mList[i])
                    {
                        Vector3 vPos = mHead.transform.localPosition;
                        vPos.x = vPos.x + (CellWidth * i);
                        mList[i].transform.localPosition = vPos;
                    }
                    // Add Item
                    else
                    {
                        GameObject goTemp = NGUITools.AddChild(gameObject, mTemplateItem);
                        Vector3 vPos = mHead.transform.localPosition;
                        vPos.x = vPos.x + (CellWidth * i);
                        goTemp.transform.localPosition = vPos;
                        mList.Add(goTemp);
                    }
                }
                // 초과
                else
                {
                    if (null != mList[i])
                    {
                        GameObject.DestroyImmediate(mList[i]);
                        mList.RemoveAt(i);
                        --i;
                        --iFor;
                    }
                }
            }
            /*
            for (int i = 0; i < iBuildCount; ++i)
            {
                GameObject goTemp = NGUITools.AddChild(gameObject, mTemplateItem);

                Vector3 vPos = mHead.transform.localPosition;
                vPos.x = vPos.x + (CellWidth * i);
                goTemp.transform.localPosition = vPos;

                mList.Add(goTemp);
            }*/
        }
    }

    // 리스트를 초기화 한다.
    void DestroyBody()
    {
        if (null == mList)
            return;
        for (int iCnt = 0; iCnt < mList.Count; ++iCnt)
        {
            GameObject.DestroyImmediate(mList[iCnt]);
        }
        mList.Clear();
    }

    // 이동 인덱스값을 업데이트 한다. (시작위치 - 이동위치) / 아이템 크기 
    void UpdateIndex()
    {
        if (movement == Movement.Vertical)
        {
            float fMove = mStartPos.y - mTrans.localPosition.y;
            mCurIndex = (int)(fMove / mItemSize.y);
            mRelativeIndex = 0 - mCurIndex;

        }
        else
        {
            float fMove = mStartPos.x - mTrans.localPosition.x;
            mCurIndex = (int)(fMove / mItemSize.x);
            mRelativeIndex = mCurIndex;
        }
    }


    void Update()
    {
        UpdateIndex();
        if (mPrvIndex != mCurIndex)
        {
            mPrvIndex = mCurIndex;
            MovePosition(m_bUseGroup);
        }
    }

    // 현재의 인덱스 값에 맞춰서 화면에 보이는 인덱스의 위치를 조정한다.
    private void MovePosition(bool bUseGroup)
    {
        if (null == mList) return;
        if (null == mHead) return;

        Vector3 vHeadPos = mHead.transform.localPosition;

        int iCount = mList.Count;   // 컨테이너 크기
        int iCurIndex = mRelativeIndex;
        int iStart = iCurIndex;     // 표시 시작 위치
        int iEnd = iStart + iCount; // 표시 끝 위치

        int iConStart = 0 < iCurIndex ? iStart % iCount : 0;

        if (movement == Movement.Vertical)
        {
            for (int iPos = iStart; iPos < iEnd; ++iPos)
            {
                int i = 0 < iPos ? iPos % iCount : 0;
                if (null == mList[i]) { continue; }
                Vector3 vPos = mList[i].transform.localPosition;
                vPos.y = vHeadPos.y - (iPos * mItemSize.y);

                if (OverflowCheck(vPos))
                    continue;

                mList[i].transform.localPosition = vPos;

                if (bUseGroup)
                {
                    // 리스트 아이템에 변경된 인덱스값을 보내준다. (iPos는 리스트의 아이템 실제 인덱스 값)
                    UIListItemGroup item = mList[i].GetComponent<UIListItemGroup>();
                    if (null != item)
                        item.OnChangeItem(iPos);
                }
                else
                {
                    // 리스트 아이템에 변경된 인덱스값을 보내준다. (iPos는 리스트의 아이템 실제 인덱스 값)
                    UIListItem item = mList[i].GetComponent<UIListItem>();
                    if (null != item)
                        item.OnChangeItem(iPos);
                }
            }
        }
        else
        {
            for (int iPos = iStart; iPos < iEnd; ++iPos)
            {
                int i = 0 < iPos ? iPos % iCount : 0;
                if (null == mList[i]) { continue; }
                Vector3 vPos = mList[i].transform.localPosition;
                vPos.x = vHeadPos.x + (iPos * mItemSize.x);

                if (OverflowCheck(vPos))
                    continue;

                mList[i].transform.localPosition = vPos;

                if (bUseGroup)
                {
                    // 리스트 아이템에 변경된 인덱스값을 보내준다. (iPos는 리스트의 아이템 실제 인덱스 값)
                    UIListItemGroup item = mList[i].GetComponent<UIListItemGroup>();
                    if (null != item)
                        item.OnChangeItem(iPos);
                }
                else
                {
                    // 리스트 아이템에 변경된 인덱스값을 보내준다. (iPos는 리스트의 아이템 실제 인덱스 값)
                    UIListItem item = mList[i].GetComponent<UIListItem>();
                    if (null != item)
                        item.OnChangeItem(iPos);
                }
            }
        }
    }

 
    // 포지션값이 머리와 꼬리의 값을 넘으면 false 값을 리턴 (스프링 컨트롤이 있기에 확이하여야 한다.)
    bool OverflowCheck( Vector3 vPos )
    {
        if (null == mHead || null == mTail)
            return false;
        if (mHead.transform.localPosition.x > vPos.x || mHead.transform.localPosition.y < vPos.y)
            return true;
        if (mTail.transform.localPosition.x < vPos.x || mTail.transform.localPosition.y > vPos.y)
            return true;
        return false;
    }

    // 아이템 추가
    public void PushItem()
    {
        if( null != mList && mInit )
        {
            Refresh(mItemCount+1);
        }
        else
        {
            Init(mItemCount+1);
        }
    }

    //아이템 삭제
    public void PopItem()
    {
        if (0 >= mItemCount)
            return;

        if (null != mList && mInit)
        {
            Refresh(mItemCount - 1);
        }
        else
        {
            Init(mItemCount - 1);
        }
    }


}


