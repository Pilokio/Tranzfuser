using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    public Dropdown resoloutionDropdown;

    Resolution[] resolutions;

    private void Start()
    {
       resolutions = Screen.resolutions;

        resoloutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResoloutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.width &&
                resolutions[i].height == Screen.height)
            {
                currentResoloutionIndex = i;
            }
        }

        resoloutionDropdown.AddOptions(options);
        resoloutionDropdown.value = currentResoloutionIndex;
        resoloutionDropdown.RefreshShownValue();
    }

    public void SetResoloution(int resoloutionIndex)
    {
        Resolution resoloution = resolutions[resoloutionIndex];
        Screen.SetResolution(resoloution.width, resoloution.height, Screen.fullScreen);
    }

    public void SetVolume (float volume)
    {
        //Mathf.Log10(volume) * 20
        audioMixer.SetFloat("volume", volume);
    }

    public void SetQuality (int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullScreen (bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

}
