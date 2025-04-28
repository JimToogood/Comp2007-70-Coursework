using UnityEngine;
using System;
using System.IO;

public class SaveDataManager : MonoBehaviour {
    private PlayerController player;
    private string filePath;

    private void Start() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        // Save file = gameDict/Assets/saveData.json
        filePath = Path.Combine(Application.dataPath, "saveData.json");

        Load();
    }

    private void OnApplicationQuit(){
        Save();
    }

    private void Save() {
        string[] saveDataContents = player.GetAttributes();
        // Combine string list into single string by the line breaks
        string saveDataString = string.Join(Environment.NewLine, saveDataContents);

        File.WriteAllText(filePath, Encode(saveDataString));
        Debug.Log("Successfully written save data to file!");
    }

    private void Load() {
        if (File.Exists(filePath)) {
            Debug.Log("Save data successfully found!");

            string saveDataString = Decode(File.ReadAllText(filePath));
            if (saveDataString != null) {
                // Split the string list into single string by the line breaks
                string[] saveDataContents = saveDataString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                player.SetAttributes(saveDataContents);
            } else {
                // If try catch caught an error in Decode
                Debug.Log("Defaulting attributes...");
                player.SetAttributes();
            }
        } else {
            // If saveData.json file does not exist
            Debug.Log("No save data found! Creating new...");
            
            player.SetAttributes();
        }
    }

    private string Encode(string text) {
        byte[] textBytes = System.Text.Encoding.ASCII.GetBytes(text);
        return Convert.ToBase64String(textBytes);
    }

    private string Decode(string text) {
        try {
            byte[] decodedBytes = Convert.FromBase64String(text);
            return System.Text.Encoding.ASCII.GetString(decodedBytes);
        } catch (Exception error) {
            Debug.Log("Error trying to decode save file: " + error.Message);
            return null;
        }
    }
}
