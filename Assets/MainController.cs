using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    public GameObject arrow;
    public GameObject eyesSprite;
    public GameObject mouthSprite;
    public GameObject headSprite;
    public GameObject dialogueBox;
    public GameObject optionsBox;
    public Sprite[] mouthSprites;
    public Sprite eyesShiftySprite;
    public Sprite eyesGooglySprite;
    public Sprite eyesSurprisedSprite;
    
    [System.Serializable]
    class State {
        public float inputX = 0;
        public float inputY = 0;
        public float inputFire = 0;
        
        public string dialogue = "";
        public double lastLetterTimestamp = 0;
        
        public State Clone() {
            return (State)MemberwiseClone();
        }
    };
    
    const double textCharacterEntryDuration = 0.04;
    const double textCharacterEntryDurationFast = 0.0;
    const int dialogueMaxLineLength = 28;
    
    Text dialogueText;
    Text optionsText;
    State state = new State();
    State previousState = new State();
    
    
    void Start() {
        dialogueText = dialogueBox.GetComponent<Text>();
        optionsText = optionsBox.GetComponent<Text>();
        
        dialogueText.text = "";
        optionsText.text = "";
        arrow.SetActive(false);
        
        state.dialogue = "This is some test text. Whoop dee doo! It should appear promptly. Here is some more text. Wheee! This is a thingy.";
    }

    // Update is called once per frame
    void Update() {
        State holdPreviousState = state.Clone();
        double time = Time.time;
        state.inputX = Input.GetAxis("Horizontal");
        state.inputY = Input.GetAxis("Vertical");
        state.inputFire = Input.GetAxis("Fire1");
        
        if (dialogueText.text.Length < state.dialogue.Length) {
            double nextLetterTimestamp = state.lastLetterTimestamp + ((state.inputFire > 0) ? textCharacterEntryDurationFast : textCharacterEntryDuration);
            
            if (time >= nextLetterTimestamp) {
                char nextLetter = state.dialogue[dialogueText.text.Length];
                
                if (nextLetter == ' ') {
                    int currentLineCount = countLinesInDialogueText(dialogueText.text);
                    int nextLineCount = countLinesInDialogueText(dialogueText.text + ' ' + findNextWordInTextAfterIndex(state.dialogue, dialogueText.text.Length));
                    
                    if (nextLineCount > currentLineCount) {
                        nextLetter = '\n';
                    }
                }
                
                dialogueText.text += nextLetter;
                state.lastLetterTimestamp = time;
            }
        }
        
        previousState = holdPreviousState;
    }
    
    string findNextWordInTextAfterIndex(string text, int index) {
        string result = "";
        
        for(int i = index+1; i < text.Length; ++i) {
            if (text[i] == ' ') {
                break;
            }
            
            result += text[i];
        }
        
        return result;
    }
    
    int countLinesInDialogueText(string text) {
        int lineCount = 0;
        int lineCharCount = 0;
        int positionOfLastSpace = -1;
        
        for(int i = 0; i < text.Length; ++i) {
            lineCharCount += 1;
            
            if (text[i] == ' ') {
                positionOfLastSpace = i;
            }
            
            if (text[i] == '\n') {
                positionOfLastSpace = -1;
                lineCharCount = 0;
                lineCount += 1;
                continue;
            }
            
            if (lineCharCount <= dialogueMaxLineLength) {
                continue;
            }
            
            if (positionOfLastSpace != -1) {
                lineCharCount = i - positionOfLastSpace + 1;
            } else {
                lineCharCount = 1;
            }
            
            positionOfLastSpace = -1;
            lineCount += 1;
        }
        
        return lineCount;
    }
}
