using System.IO;
using UnityEngine;

public class UserDatabase : MonoBehaviour
{
    public static UserDatabase Instance;

    private string filePath;
    private UserList userList;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            filePath = Path.Combine(Application.dataPath, "users.json");
            Debug.Log(filePath);
            LoadUsers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadUsers()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            userList = JsonUtility.FromJson<UserList>(json);
        }
        else
        {
            userList = new UserList();
            SaveUsers();
        }
    }

    public bool Login(string username, string password)
    {
        foreach (UserData user in userList.users)
        {
            if (user.username == username && user.password == password)
            {
                return true;
            }
        }

        return false;
    }

    public void SaveUsers()
    {
        string json = JsonUtility.ToJson(userList, true);
        File.WriteAllText(filePath, json);
    }

    public UserList GetUsers()
    {
        return userList;
    }

    public string GetFilePath()
    {
        return filePath;
    }
}