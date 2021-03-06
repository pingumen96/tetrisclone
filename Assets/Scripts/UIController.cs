﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
    public static UIController instance = null;

    public static readonly float MENU_VOLUME_MULT = 0.6f;

    // video
    public Toggle fullScreenToggle;
    public Dropdown resolutionDropdown;
    public Dropdown qualityDropdown;
    public Camera currentCamera;

    // audio
    public AudioClip[] audioClips;
    public Dropdown musicDropdown;
    public AudioSource playTheme;
    public AudioSource menuTheme;
    public AudioSource soundEffect;
    public Slider musicVolume;
    public Slider soundEffectVolume;


    // other
    public GameObject pauseMenu;
    public GameObject gameOverMenu;

    // HUD
    public Text scoreValue;
    public Text levelValue;
    public Text linesValue;
    public RawImage nextPieceImage;
    public Texture[] textures;

	// Use this for initialization
	void Start () {
        if(instance == null) {
            instance = this;
        } else if(instance != this) {
            Destroy(gameObject);
        }

        fullScreenToggle.isOn = Screen.fullScreen;
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        

        resolutionDropdown.options = new List<Dropdown.OptionData>();
        foreach(Resolution r in Screen.resolutions) {
            resolutionDropdown.options.Add(new Dropdown.OptionData(r.width.ToString() + "x" + r.height.ToString()));
        }
        resolutionDropdown.value = resolutionDropdown.options.Count - 1;
        OnResolutionDropdownChanged();

        musicDropdown.options = new List<Dropdown.OptionData>();
        foreach(AudioClip a in audioClips) {
            musicDropdown.options.Add(new Dropdown.OptionData(a.name));
        }
        musicDropdown.value = 0;
        musicDropdown.RefreshShownValue();
        OnMusicDropdownChanged();

        musicVolume.value = playTheme.volume;
        OnMusicVolumeChanged();


    }

    public void OnResolutionDropdownChanged() {
        string[] res = resolutionDropdown.options[resolutionDropdown.value].text.Split('x');
        Screen.SetResolution(int.Parse(res[0]), int.Parse(res[1]), Screen.fullScreen);
    }

    public void OnQualityDropdownChanged() {
        QualitySettings.SetQualityLevel(qualityDropdown.value);
    }

    public void OnFullScreenToggleClicked() {
        Screen.fullScreen = fullScreenToggle.isOn;
    }

    public void OnMusicDropdownChanged() {
        playTheme.clip = audioClips[musicDropdown.value];
    }

    public void PlayGameMusic() {
        menuTheme.Stop();
        playTheme.Play();
    }
    
    public void SetGameMusicSpeed(float speed) {
        playTheme.pitch = speed;
    }

    public void PlayMenuMusic() {
        playTheme.Stop();
        menuTheme.Play();
    }

    public void PlaySoundEffect() {
        soundEffect.Play();
    }

    public void OnMusicVolumeChanged() {
        playTheme.volume = musicVolume.value;
        menuTheme.volume = musicVolume.value * MENU_VOLUME_MULT;
    }

    public void OnSoundEffectVolumeChanged() {
        soundEffect.volume = soundEffectVolume.value;
    }

    public void ShowPauseMenu(bool active) {
        pauseMenu.SetActive(active);
    }

    public void UpdateHUD(uint score, uint level, uint lines) {
        scoreValue.text = score.ToString();
        levelValue.text = level.ToString();
        linesValue.text = lines.ToString();
    }

    public void SetNextPiece(Piece.TetrominoType tetrominoType) {
        nextPieceImage.texture = textures[(int)tetrominoType];
    }

    public void ShowGameOverMenu(bool active) {
        gameOverMenu.SetActive(active);
    }

    public void AlternativeMode() {
        currentCamera.transform.Rotate(new Vector3(0.0f, 0.0f, 180.0f));
    }
}
