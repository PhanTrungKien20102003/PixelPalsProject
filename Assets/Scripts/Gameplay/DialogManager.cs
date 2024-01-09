using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] GameObject dialogBox;
    [SerializeField] TextMeshProUGUI dialogText;
    [SerializeField] int lettersPerSecond;

    public event Action OnShowDialog;
    public event Action OnCloseDialog;

    //use singleton pattern
    public static DialogManager Instance {  get; private set; } //reference this from any class we want
    private void Awake()
    {
        Instance = this;
    }

    Dialog dialog; //show dialog in a global object
    int currentLine = 0; //variable to store the current line of the dialog
    bool isTyping; //check for the user press "Space" key while the first dialog is still running

    public bool IsShowing {  get; private set; } //stop the NPC moving around when the dialog is showing

    public IEnumerator ShowDialog(Dialog dialog)
    {
        /* When reach the end of Update() function, the "Space" key will still be pressed
        --> Because doing all this in the same frame in which user try to interact with the NPC*/

        yield return new WaitForEndOfFrame(); /* Wait for one frame before executing all the logic inside ShowDialog()
                                                 --> "Space" key won't be pressed anymore */

        OnShowDialog?.Invoke();

        IsShowing = true;
        this.dialog = dialog;
        dialogBox.SetActive(true);
        StartCoroutine(TypeDialog(dialog.Lines[0])); //show the first line of the dialog
    }

    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTyping) //when player press "Space" key, do all the code below if isTyping = false
        {
            currentLine++; //increment current line that available
            if (currentLine < dialog.Lines.Count) 
            {
                StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
            }
            else
            {
                currentLine = 0; //next time start a dialog, starts from zero
                IsShowing = false;
                dialogBox.SetActive(false);
                OnCloseDialog?.Invoke();
            }
        }
    }

    public IEnumerator TypeDialog(string line)
    {
        isTyping = true;
        dialogText.text = "";
        foreach (var letter in line.ToCharArray()) //loop through each letters and add them one by one
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond); //after adding each letter, I want to wait a little bit
        }
        isTyping = false;
    }
}
