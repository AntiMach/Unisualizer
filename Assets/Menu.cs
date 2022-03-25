using SFB;
using UnityEngine;
using UnityEngine.EventSystems;
using NAudio.Wave;

public class Menu : MonoBehaviour {
    private GameObject MenuUI;
    private Display volume;

    public static AudioSource source;
    
    void Start ()
    {
        source = GetComponent<AudioSource>();
        volume = GameObject.Find("Volume").GetComponent<Display>();
        MenuUI = GameObject.Find("Menu");
    }
	
	void Update ()
    {
        // Update menu's position
        float position = (Input.mousePosition.x < 350) ? 0 : -400;
        Vector2 anchoredPos = ((RectTransform)MenuUI.transform).anchoredPosition;
        anchoredPos.x += (position - anchoredPos.x) / 10;
        ((RectTransform)MenuUI.transform).anchoredPosition = anchoredPos;

        // Check for keybinds
        if (Input.GetKeyDown("escape"))
            OnExit();
        if (Input.GetKeyDown("s"))
            OnStop();
        if (Input.GetKeyDown("space"))
        {
            if (source.isPlaying)
                OnPause();
            else
                OnPlay();
        }
    }

    public void OnVolumeChanged()
    {
        if (source != null && volume != null)
            source.volume = volume.value / 100f;
    }

    public void OpenDialog()
    {
        string[] pathList = StandaloneFileBrowser.OpenFilePanel("Import song", "", "mp3", false);
        if (pathList.Length == 0)
            return;

        string path = pathList[0];
        if (path.Length == 0)
            return;

        AudioFileReader data = new AudioFileReader(path);
        float[] audioData = new float[data.Length];
        data.Read(audioData, 0, (int)data.Length);

        source.clip = AudioClip.Create("importedAudio", (int)data.Length, data.WaveFormat.Channels, data.WaveFormat.SampleRate, false);
        source.clip.SetData(audioData, 0);

        source.Play();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnPlay()
    {
        if (source != null)
            source.Play();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnPause()
    {
        if (source != null)
            source.Pause();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnStop()
    {
        if (source != null)
            source.Stop();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnExit()
    {
        Application.Quit();
    }
}
