using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI messageText;


    public void Register()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (username == "" || password == "")
        {
            messageText.text = "Vui lòng nhập đầy đủ thông tin!";
            return;
        }

        UserList users = UserDatabase.Instance.GetUsers();

        // Kiểm tra tài khoản đã tồn tại
        if (users.users.Any(u => u.username == username))
        {
            messageText.text = "Tên đăng nhập đã tồn tại!";
            return;
        }

        // Tạo tài khoản mới
        UserData newUser = new UserData
        {
            username = username,
            password = password,
            coin = 0,
            level = 1
        };

        users.users.Add(newUser);

        UserDatabase.Instance.SaveUsers();

        messageText.text = "Đăng ký thành công!";
    }
    public void Login()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (username == "" || password == "")
        {
            messageText.text = "Vui lòng nhập đầy đủ thông tin!";
            return;
        }

        UserList users = UserDatabase.Instance.GetUsers();

        foreach (UserData user in users.users)
        {
            if (user.username == username && user.password == password)
            {
                messageText.text = "Đăng nhập thành công!";

                // Chuyển sang Menu
                SceneManager.LoadScene("LV1");
                if (user.username == username && user.password == password)
                {
                    PlayerPrefs.SetString("CurrentUser", user.username);
                    SceneManager.LoadScene("LV1");
                }

                return;
            }
        }

        messageText.text = "Sai tài khoản hoặc mật khẩu!";
    }
    public void PlayGuest()
    {
        PlayerPrefs.SetString("CurrentUser", "Guest");
        SceneManager.LoadScene("LV1");
    }
}
