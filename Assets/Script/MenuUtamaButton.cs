using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUtamaButton : MonoBehaviour
{
    [SerializeField] private string namaSceneMenu = "MainMenu";

    public void KembaliKeMenuUtama()
    {
        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic();

        SceneManager.LoadScene(namaSceneMenu);
    }
}