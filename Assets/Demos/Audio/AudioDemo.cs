using RESTClient;
using Azure.StorageServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.Networking;

public class AudioDemo : MonoBehaviour
{

  [Header("Azure Storage Service")]
  [SerializeField]
  private string storageAccount;
  [SerializeField]
  private string accessKey;
  [SerializeField]
  private string container;

  private StorageServiceClient client;
  private BlobService blobService;

  [Header("Audio Demo")]
  public Text label;
  public Button buttonRecord;

  [Header("Audio")]
  public AudioSource audioSource;
  public int recordTime = 2;

  private string mic;
  private const int sampleRate = 16000;
  private AudioClip audioClip;
  private bool isRecording = false;
  private float recordingTime;

  private string localPath;

  // Use this for initialization
  void Start()
  {
    if (string.IsNullOrEmpty(storageAccount) || string.IsNullOrEmpty(accessKey))
    {
      Log.Text(label, "Storage account and access key are required", "Enter storage account and access key in Unity Editor", Log.Level.Error);
    }

    client = StorageServiceClient.Create(storageAccount, accessKey);
    blobService = client.GetBlobService();
  }

  // Update is called once per frame
  void Update()
  {
    if (!isRecording)
    {
      return;
    }
    recordingTime += Time.deltaTime;
    if (recordingTime > recordTime)
    {
      StopRecording();
    }
  }

  public void TappedRecordAudio()
  {
    if (Microphone.devices.Length == 0)
    {
      Log.Text(label, "No microphone found to record audio clip sample with.", "Check microphone is connected and Microphone permissions are enabled in Player Settings.", Log.Level.Error);
      return;
    }
    buttonRecord.GetComponent<Button>().image.color = Color.red;
    mic = Microphone.devices[0];
    label.text = mic;
    audioClip = Microphone.Start(mic, false, recordTime, sampleRate);
    isRecording = true;
  }

  private void StopRecording()
  {
    Microphone.End(mic);
    recordingTime = 0.0f;
    isRecording = false;
    buttonRecord.GetComponent<Button>().image.color = Color.white;
    Log.Text(label, "Recording completed");
    SaveAsWav();
  }

  private void SaveAsWav()
  {
    string filepath;
    byte[] bytes = WavUtility.FromAudioClip(audioClip, out filepath, true);
    Log.Text(label, "Saved .wav file as:" + filepath, "Saved audio bytes:" + bytes.Length + " filepath:" + filepath);
    localPath = filepath;
  }

  public void TappedPlayAudio()
  {
    if (!IsFileReady())
      return;
    if (audioClip == null || audioClip.loadState != AudioDataLoadState.Loaded)
    {
      Log.Text(label, "AudioClip not ready.", "AudioClip is not ready. Saving to:" + localPath, Log.Level.Warning);
      StartCoroutine(PlayAudioFile());
      return;
    }
    audioSource.clip = audioClip;
    audioSource.Play();
    string output = string.Format("Playing {0} secs audio clip with {1} samples, {2} channel(s) and {3} frequency.", audioClip.length, audioClip.samples, audioClip.channels, audioClip.frequency);
    Log.Text(label, "Playing", output);
  }

  private IEnumerator PlayAudioFile()
  {
    string filePath = "file:" + localPath;
    Log.Text(label, "Load: " + filePath, "Load and play sample: " + filePath);

    WWW localFileRequest = new WWW(filePath);
    yield return localFileRequest;
    AudioClip audioClip = localFileRequest.GetAudioClip(false, false, AudioType.WAV);
    audioSource.clip = audioClip;
    audioSource.Play();
  }

  public void TappedSave()
  {
    if (!IsFileReady())
      return;
    PutAudio();
  }

  private void PutAudio()
  {
    byte[] wavBytes = File.ReadAllBytes(localPath);

    string filename = Path.GetFileName(localPath);
    Log.Text(label, "Put audio file: " + filename, "Put audio file path: " + localPath);

    StartCoroutine(blobService.PutAudioBlob(PutAudioCompleted, wavBytes, container, filename, "audio/wav"));
  }

  public void PutAudioCompleted(RestResponse response)
  {
    if (response.IsError)
    {
      Log.Text(label, response.ErrorMessage, "Error putting blob audio:" + response.Content, Log.Level.Error);
      return;
    }
    Log.Text(label, response.Url, "Put audio blob:" + response.Content);
  }

  public void TappedLoad()
  {
    if (!IsFileReady())
      return;
    string filename = Path.GetFileName(localPath);
    string resourcePath = container + '/' + filename;
    Log.Text(label, "Load and play: " + resourcePath);
    StartCoroutine(blobService.GetAudioBlob(GetAudioBlobComplete, resourcePath));
  }

  private void GetAudioBlobComplete(IRestResponse<AudioClip> response)
  {
    if (response.IsError)
    {
      Log.Text(label, "Failed to load audio: " + response.StatusCode, response.ErrorMessage, Log.Level.Error);
    }
    else
    {
      Log.Text(label, "Loaded audio:" + response.Url);
      AudioClip audioClip = response.Data;
      audioSource.clip = audioClip;
      audioSource.Play();
    }
  }

  private bool IsFileReady()
  {
    if (string.IsNullOrEmpty(localPath) && !File.Exists(localPath))
    {
      Log.Text(label, "Tap 'Record audio' button", "Record audio first", Log.Level.Warning);
      return false;
    }
    return true;
  }

  public void TappedNext()
  {
    SceneManager.LoadScene("ImageScene");
  }
}
