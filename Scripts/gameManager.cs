using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using System.Linq;
using System.Text;

public class gameManager : MonoBehaviour
{
    public TextAsset defaultLvl;
    public levelData currentLvl;
    public Vector2 axis;

    private static string key = "kb3TCgxZCT37HxLYH5u0KMj7lafWOfzgfklOu81nQ0tNJFzgMNH8EqT3WWltdEZO";

    void Awake(){

        this.tag = "gameManager";

        GameObject[] objs = GameObject.FindGameObjectsWithTag("gameManager");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);

        levelTemp.currentLvl = JsonConvert.DeserializeObject<levelData>(defaultLvl.text);
        levelTemp.levelPlaying = JsonConvert.DeserializeObject<levelData>(defaultLvl.text);

        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;

        //SceneManager.sceneLoaded -= OnSceneLoaded;
        //SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /*public string encrypt(string input){
        aes.GenerateIV();
        byte[] textBytes = Encoding.UTF8.GetBytes(input);
        using (var encryptor = aes.CreateEncryptor())
        using (var memoryStream = new MemoryStream())
        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        {

            cryptoStream.Write(textBytes, 0, textBytes.Length);

            cryptoStream.FlushFinalBlock();

            byte[] cipherText = memoryStream.ToArray();

            return Encoding.UTF8.GetString(cipherText);

        }
    }*/

    public string encrypt(string input)
    {
        char[] chars = input.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] ^= key[i % key.Length];
        }
        return new string(chars);
    }

    /*public string decrypt(string input){
        aes.GenerateIV();

        byte[] cipherTextBytes = Encoding.UTF8.GetBytes(input);

        using (var decryptor = aes.CreateDecryptor())
        using (var memoryStream = new MemoryStream(cipherTextBytes))
        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
        {

            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            int bytesRead = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

            return Encoding.UTF8.GetString(plainTextBytes, 0, bytesRead);

        }
    }*/

    public string decrypt(string input)
    {
        return encrypt(input);
    }

    void Update(){
        axis.x = Input.GetAxis("Horizontal");
        axis.y = Input.GetAxis("Vertical");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach(SpriteRenderer sr in GameObject.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)){
            Debug.Log(sr.gameObject);
            sr.gameObject.AddComponent(typeof(renderCheck));
        }
    }
}
