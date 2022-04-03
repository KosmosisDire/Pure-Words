using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityAndroidOpenUrl;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AutoUpdate : MonoBehaviour
{
    //send http request to thecodespace.ddns.net/fileshare/words/v.version on start. 
    //if version is different, download the new version.

    public Slider progressBar;
    public UnityEvent OnNeedUpdate;

    public void Start()
    {
        UpdateApp();
    }

    public async void UpdateApp()
    {
        if(await NeedsNewVersion())
        {
            OnNeedUpdate.Invoke();
            DownloadNewVersion();
        }
    }

    public async Task<bool> NeedsNewVersion()
    {
        string url = "https://thecodespace.ddns.net/fileshare/words/v.txt";
        UnityWebRequest request = new UnityWebRequest(url);
        DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
        request.downloadHandler = dH;
        request.SendWebRequest();
        while (!request.isDone)
        {
            Debug.Log("waiting for response");
            await Task.Delay(100);
        }

        switch (request.result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError(": Error: " + request.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError(": HTTP Error: " + request.error);
                break;
            case UnityWebRequest.Result.Success:
                Debug.Log("local version: " + Application.version + " remote version: " + dH.text);
                if (request.downloadHandler.text != Application.version) return true;
                else return false;
        }

        return false;
    }

    public async void DownloadNewVersion()
    {
        string url = "https://thecodespace.ddns.net/fileshare/words/words.apk";
        UnityWebRequest request = new UnityWebRequest(url);
        DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
        request.downloadHandler = dH;
        request.SendWebRequest();
        while (!request.isDone)
        {
            await Task.Delay(50);
            progressBar.value = request.downloadProgress;
        }

        switch (request.result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError(": Error: " + request.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError(": HTTP Error: " + request.error);
                break;
            case UnityWebRequest.Result.Success:
                byte[] bytes = request.downloadHandler.data;
                string path = Application.temporaryCachePath + "/words.apk";
                System.IO.File.WriteAllBytes(path, bytes);
                Debug.Log("Downloaded new version to: " + path);
                AndroidOpenUrl.OpenFile(path);
                break;
        }

        dH.Dispose();
        request.Dispose();
    }


}
