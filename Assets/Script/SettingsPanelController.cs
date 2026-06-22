using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [Header("--- PANEL ---")]
    [SerializeField] private GameObject panelPengaturan;

    [Header("--- SLIDER ---")]
    [SerializeField] private Slider sliderMusik;
    [SerializeField] private Slider sliderSFX;

    void OnEnable()
    {
        if (AudioManager.Instance == null) return;
        sliderMusik.SetValueWithoutNotify(AudioManager.Instance.GetMusicVolume());
        sliderSFX.SetValueWithoutNotify(AudioManager.Instance.GetSFXVolume());
    }

    public void BukaPengaturan()  => panelPengaturan.SetActive(true);
    public void TutupPengaturan() => panelPengaturan.SetActive(false);

    public void OnMusikDiubah(float nilai) => AudioManager.Instance.SetMusicVolume(nilai);
    public void OnSFXDiubah(float nilai)   => AudioManager.Instance.SetSFXVolume(nilai);
}