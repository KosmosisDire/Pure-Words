using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.StackLayout;

public class GameNetwork : MonoBehaviour
{
    public static GameNetwork instance;
    public Client client;
    public string _gameCode;
    public string GameCode 
    { 
        get
        {
            if(!string.IsNullOrWhiteSpace(gameCodeInput.text))
            { 
                _gameCode = gameCodeInput.text;
                GameCode = _gameCode;
            }
            
            return _gameCode;
        }
        set
        { 
            _gameCode = value;
            gameCodeInput.SetTextWithoutNotify(_gameCode);
            gameCodeText.text = _gameCode;
        }
    }

    [Header("Inputs")]
    public TMP_InputField gameCodeInput;
    public TMP_Text gameCodeText;
    public TMP_InputField usernameText;

    [Header("Waiting Room")]
    public GridLayout playerList;
    public List<string> players = new List<string>();
    public RectTransform onlinePlayerTilePrefab;
    public Toast disconnectToast;
    public bool online = false;
    public string Username { get => usernameText.text; private set => usernameText.SetTextWithoutNotify(value); }

    public void Start()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);

        if(PlayerPrefs.HasKey("Username"))
        {
            Username = PlayerPrefs.GetString("Username");
        }

        usernameText.onValueChanged.AddListener(delegate {
            if(!string.IsNullOrWhiteSpace(usernameText.text))
            {
                PlayerPrefs.SetString("Username", usernameText.text);
            }
        });

        client = new Client(Username);
    }

    public void Reconnect()
    {
        client.DisconnectLocal();
        client = new Client(Username);
        playerList.ClearGrid();
        players.Clear();
    }

    public async void HostNewGame()
    {
        if(!Application.isPlaying) return;

        GameCode = await client.HostGame();
    }

    public async Task<bool> JoinGame(string code = "")
    {
        if(!Application.isPlaying) return false;
        return await client.JoinGame(code == "" ? GameCode : code);
    }

    public async void JoinGameNoTask(string code = "")
    {
        await JoinGame(code);
    }

    public void SendTilebagState(TileBag bag)
    {
        client.SendTilebagToServer(bag, GameCode).Wait();
    }

    public void SendMove(Move move)
    {
        client.SendMove(move, GameCode);
    }

    public void PlayerOnline(string username)
    {
        Debug.Log("Player online: " + username);

        var tile = Instantiate(onlinePlayerTilePrefab);
        tile.GetComponentInChildren<TextDisplay>().SetText(username);
        
        playerList.AddItem(tile.gameObject);
        players.Add(username);

        if(GameManager.instance != null)
        {
            GameManager.instance.AddPlayerToGame(username);
        }
    }

    public void PlayerOffline(string username)
    {
        Debug.Log("Player offline" + username);
    }

    public void OnDisconnected()
    {
        Debug.Log("Disconnected");
        online = false;
        disconnectToast.Show(1000);
        StartCoroutine(LoadSceneDelay(0, 5000));
    }

    //create coroutine to load scene after a delay
    public IEnumerator LoadSceneDelay(int scene, int delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(scene);
    }

    public void OnReconnected()
    {
        disconnectToast.Hide(1);
        online = true;
    }

    public void OnApplicationQuit()
    {
        client.BeginFullDisconnect();
    }
}
