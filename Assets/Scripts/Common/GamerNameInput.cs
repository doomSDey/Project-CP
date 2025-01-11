using UnityEngine;
using TMPro; // Import TextMeshPro namespace
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GamerNameInputTMP : MonoBehaviour
{
    public TMP_InputField nameInputField; // TextMeshPro Input Field
    public Button submitButton;           // Submit Button

    private void Start()
    {
        // Initially disable the submit button
        submitButton.interactable = false;

        // Add listener to InputField to check text changes
        nameInputField.onValueChanged.AddListener(OnInputFieldChanged);

        // Add listener to the button for submission
        submitButton.onClick.AddListener(OnSubmit);
    }

    private void OnInputFieldChanged(string input)
    {
        // Enable the button only if the input field is not empty
        submitButton.interactable = !string.IsNullOrWhiteSpace(input);
    }

    private void OnSubmit()
    {
        string gamerName = nameInputField.text.Trim();

        // Save the gamer name persistently
        PlayerPrefs.SetString("GamerName", gamerName);
        PlayerPrefs.Save();

        // Load Level 1
        SceneManager.LoadScene("Level1"); // Ensure "Level1" matches your scene name
    }

    private void OnDestroy()
    {
        // Remove listeners to avoid memory leaks
        nameInputField.onValueChanged.RemoveListener(OnInputFieldChanged);
        submitButton.onClick.RemoveListener(OnSubmit);
    }
}
