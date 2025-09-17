using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeController : MonoBehaviour
{
    public Slider volumeSlider;
    public AudioMixer audioMixer;

    void Start()
    {
        // Conecta o evento do slider
        volumeSlider.onValueChanged.AddListener(SetVolume);

        // Garante que inicia no volume máximo (ou o valor salvo)
        volumeSlider.value = 1f;
        SetVolume(volumeSlider.value);
    }

    void SetVolume(float value)
    {
        // Converte 0–1 em -80dB a 0dB
        audioMixer.SetFloat("Master", Mathf.Log10(value) * 20);
    }
}
