using UnityEngine;
using System;
using System.IO;

public class SaveDataManager : MonoBehaviour {
    // static read only used to ensure single-time allocation of encryption key
    private static readonly byte[] encryptionKey = System.Text.Encoding.UTF8.GetBytes("oY5bF*'j1sZ,&6iU");
    private int keyLength;

    private PlayerController player;
    private string filePath;

    private void Start() {
        keyLength = encryptionKey.Length;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        filePath = Path.Combine(Application.dataPath, "saveData.json");     // Save file = gameDict/Assets/saveData.json

        Load();
    }

    private void OnApplicationQuit(){
        Save();
    }

    public void Save() {
        string[] saveDataContents = player.GetAttributes();
        // Combine string array into a single string by the line breaks
        string saveDataString = string.Join(Environment.NewLine, saveDataContents);

        File.WriteAllText(filePath, Encrypt(saveDataString));
        Debug.Log("Successfully written save data to file!");
    }

    private void Load() {
        if (File.Exists(filePath)) {
            Debug.Log("Save data successfully found!");

            string saveDataString = Decrypt(File.ReadAllText(filePath));
            if (saveDataString != null) {
                // Split string array into a single string by the line breaks
                string[] saveDataContents = saveDataString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                player.SetAttributes(saveDataContents);
            } else {
                // If try catch caught an error in Decrypt, set attributes to default
                Debug.Log("Defaulting attributes...");
                player.SetAttributes();
            }
        } else {
            // If saveData.json file does not exist
            Debug.Log("No save data found! Creating new...");
            
            player.SetAttributes();
        }
    }

    // Encryption is done using a multi-character key XOR method, simple whilst still being secure
    private string Encrypt(string text) {
        // Convert the string to an array of bytes
        byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text);

        for (int i = 0; i < textBytes.Length; i++) {
            // Raise each text byte to the power of the corresponding byte of the encryption key
            // "% keyLength" is used to wrap around to the start of the key if the key is shorter than the text (num of bytes)
            textBytes[i] ^= encryptionKey[i % keyLength];
        }
        
        // Convert array of bytes back to a string
        return Convert.ToBase64String(textBytes);
    }

    private string Decrypt(string text) {
        try {
            // Convert the string to an array of bytes
            byte[] textBytes = Convert.FromBase64String(text);

            for (int i = 0; i < textBytes.Length; i++) {
                // Raise each text byte to the power of the corresponding byte of the encryption key
                // "% keyLength" is used to wrap around to the start of the key if the key is shorter than the text (num of bytes)
                textBytes[i] ^= encryptionKey[i % keyLength];
            }

            // Convert array of bytes back to a string
            return System.Text.Encoding.UTF8.GetString(textBytes);
        } catch (Exception error) {
            // If the save file has been edited or corrupted, return null
            Debug.Log("Error trying to decrypt save file: " + error.Message);
            return null;
        }
    }
}
