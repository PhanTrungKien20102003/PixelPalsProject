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

    public bool IsShowing {  get; private set; } //stop the NPC moving around when the dialog is showing

    public IEnumerator ShowDialogText(string text, bool waitForInput = true)
    {
        IsShowing = true;
        dialogBox.SetActive(true);
        
        yield return TypeDialog(text);
        
        //this will make the function wait until the player presses the Z key
        if (waitForInput)
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
        }
        
        dialogBox.SetActive(false);
        IsShowing = false;
    }
    public IEnumerator ShowDialog(Dialog dialog)
    {                                            
        /* When reach the end of Update() function, the "Space" key will still be pressed
        --> Because doing all this in the same frame in which user try to interact with the NPC*/

        yield return new WaitForEndOfFrame(); /* Wait for one frame before executing all the logic inside ShowDialog()
                                                 --> "Space" key won't be pressed anymore */

        OnShowDialog?.Invoke();
        IsShowing = true;
        dialogBox.SetActive(true);

        foreach (var line in dialog.Lines)
        {
            yield return TypeDialog(line);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
        }
        
        dialogBox.SetActive(false);
        IsShowing = false;
        OnCloseDialog?.Invoke();


    }

    public void HandleUpdate()
    {
        
    }

    public IEnumerator TypeDialog(string line)
    {
        dialogText.text = "";
        foreach (var letter in line.ToCharArray()) //loop through each letters and add them one by one
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond); //after adding each letter, I want to wait a little bit
        }
    }
}
