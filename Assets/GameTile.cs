using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameTile : MonoBehaviour
{
    public GridLayout namesLayout;
    public TextDisplay codeText;
    public List<string> usernames;
    public TextDisplay namePrefab;
    public GameObject editIndicator;
    public Button deleteButton;
    public Dictionary<string, TextDisplay> names = new Dictionary<string, TextDisplay>();
    public string code;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(new UnityAction(async () => 
        {
            GameNetwork.instance.GameCode = code;
            if (await GameNetwork.instance.JoinGame(code))
            {
                GameObject.FindGameObjectWithTag("JoinMenu").GetComponent<Toast>().Show(400);
                GameObject.FindGameObjectWithTag("HostParticipants").GetComponent<CommonTweens>().FadeAlphaInTolerant(0.4f);
                var gameCodeObj = GameObject.FindGameObjectWithTag("GameCode");
                gameCodeObj.GetComponent<Toast>().Show(400);
            }
        }));

        deleteButton.onClick.AddListener(new UnityAction(() => 
        {
            GameManager.instance.DeleteGame(code);
        }));
    }

    public int NextHighestPowerOfTwo(int x)
    {
        x--;
        x |= x >> 1;
        x |= x >> 2;
        x |= x >> 4;
        x |= x >> 8;
        x |= x >> 16;
        x++;
        return x;
    }


    // Update is called once per frame
    void Update()
    {
        namesLayout.columns = Mathf.Max(NextHighestPowerOfTwo((int)Mathf.Sqrt(usernames.Count)), 2);
        if(GameManager.instance.editGamesList)
        {
            editIndicator.SetActive(true);
        }
        else
        {
            editIndicator.SetActive(false);
        }
    }

    public void AddPlayer(string username)
    {
        if(usernames.Contains(username)) return;
        usernames.Add(username);
        names.Add(username, Instantiate(namePrefab));
        names[username].SetText(username);
        namesLayout.AddItem(names[username].gameObject);
    }

    public string Serialize()
    {
        return string.Join(",", usernames) + ";" + code;
    }

    public void Deserialize(string json)
    {
        string[] parts = json.Split(';');
        if(parts.Length != 2)
        {
            Debug.LogError("Invalid game tile json: " + json);
            Destroy(gameObject);
            return;
        }
        var usernames = parts[0].Split(',').ToList();
        code = parts[1];
        codeText.SetText(code.ToString());

        foreach (var username in usernames)
        {
            AddPlayer(username);
            Debug.Log(username);
        }
    }
}
