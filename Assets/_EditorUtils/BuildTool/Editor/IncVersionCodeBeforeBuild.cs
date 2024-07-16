using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class IncVersionCodeBeforeBuild : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        switch (report.summary.platform)
        {
            case BuildTarget.iOS:
                PlayerSettings.iOS.buildNumber = incBuildNumber(PlayerSettings.iOS.buildNumber);
                Debug.Log("building ios version: " + PlayerSettings.iOS.buildNumber);
                break;
            case BuildTarget.Android:
                PlayerSettings.Android.bundleVersionCode += 1;
                Debug.Log("building android version: " + PlayerSettings.Android.bundleVersionCode);
                break;
        }
    }

    private string incBuildNumber(string build)
    {
        return int.TryParse(build, out var number) ? (++number).ToString() : "10000";
    }
}