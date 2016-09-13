using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(TestCode))]
public class TestCodeInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        
        if (GUILayout.Button("Push"))
        {
            TestCode tc = target as TestCode;
            if (null != tc)
            {
                tc.Push();
            }
        }

        if (GUILayout.Button("Pop"))
        {
            TestCode tc = target as TestCode;
            if (null != tc)
            {
                tc.Pop();
            }
        }

        if (GUILayout.Button("Refresh"))
        {
            TestCode tc = target as TestCode;
            if (null != tc)
            {
                tc.Refresh();
            }
        }

        /*
        if (GUILayout.Button("PopIndex"))
        {
            TestCode tc = target as TestCode;
            if (null != tc)
            {
                tc.Pop_Index();
            }
        }*/
    }
}
