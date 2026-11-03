using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenuButton : MonoBehaviour
{
    public string menuSceneName = "SampleScene";
    public bool stopFusion = true;

    public void OnClickBackToMenu()
    {
        if (stopFusion)
        {
            var holder = NetworkRunnerHolder.Instance;
            var runner = holder != null ? holder.Runner : null;
            if (runner != null) runner.Shutdown();
        }
        SceneManager.LoadScene(menuSceneName);
    }
}
