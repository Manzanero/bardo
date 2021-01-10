using UnityEngine;

public class ConsoleToGUI : MonoBehaviour
{
    static string myLog = "";
    private string output;
    private string stack;
     
    void OnEnable()
    {
        Application.logMessageReceived += Log;
    }
     
    void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }
     
    public void Log(string logString, string stackTrace, LogType type)
    {
        output = logString;
        stack = stackTrace;
        myLog = output + "\n" + myLog;
        if (myLog.Length > 5000)
        {
            myLog = myLog.Substring(0, 4000);
        }
    }
     
    void OnGUI()
    {
        //if (!Application.isEditor) //Do not display in editor ( or you can use the UNITY_EDITOR macro to also disable the rest)
            myLog = GUI.TextArea(new Rect(0, Screen.height - Screen.height / 3f, Screen.width / 2f,  Screen.height / 3f), myLog);
    }
}