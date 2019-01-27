﻿using System.Collections;
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
    
    enum Face {
        normal,
        googly,
        shifty,
        surprised
    };
    
    [System.Serializable]
    class State {
        public float inputX = 0;
        public float inputY = 0;
        public float inputFire = 0;
        
        public bool waitingForMoreText = false;
        public int currentPageIndex = 0;
        public List<string> dialoguePages = new List<string>();
        public List<string> options = new List<string>();
        public VoiceStyle voiceStyle = VoiceStyle.Normal;
        public double nextLetterTimestamp = 0;
        public double nextMouthMovementTimestamp = 0;
        
        public State Clone() {
            return (State)MemberwiseClone();
        }
    };
    
    class DialogueSection {
        public string text = "";
        public VoiceStyle voiceStyle = VoiceStyle.Normal;
        public Face face;
        public bool lookingDown = false;
        public string[] optionsText;
        public int[] optionsLeadsTo;
        public int id;
    };
    
    const double textCharacterEntryDuration = 0.04;
    const double textCharacterEntryDurationFast = 0.0;
    const int dialogueMaxLineLength = 28;
    const int dialogueMaxVisibleLines = 17;
    const float minMouthMovementDuration = 0.08f;
    const float maxMouthMovementDuration = 0.15f;
    
    Text dialogueText;
    Text optionsText;
    VoiceSynth voiceSynth;
    SpriteRenderer mouthSpriteRenderer;
    SpriteRenderer eyesSpriteRenderer;
    SpriteRenderer headSpriteRenderer;
    State state = new State();
    State previousState = new State();
    Bucket mouthSpriteBucket = new Bucket(1);
    
    void Start() {
        dialogueText = dialogueBox.GetComponent<Text>();
        optionsText = optionsBox.GetComponent<Text>();
        voiceSynth = GetComponent<VoiceSynth>();
        eyesSpriteRenderer = eyesSprite.GetComponent<SpriteRenderer>();
        headSpriteRenderer = headSprite.GetComponent<SpriteRenderer>();
        mouthSpriteRenderer = mouthSprite.GetComponent<SpriteRenderer>();
        mouthSpriteBucket.FillBucketWithRange(-1, 3);
        
        dialogueText.text = "";
        optionsText.text = "";
        arrow.SetActive(false);
        
        DialogueSection section = new DialogueSection();
        section.text = ("This is some test text. Whoop dee doo! It should appear promptly. Here is some more text. Wheee! This is a thingy. " +
                          "This text is going to keep going. Doop dee doo. This text is going to keep going.\n\nDoop dee doo. " +
                          "This text is going to keep going. Doop dee doo. This text is going to keep going. Doop dee doo. " +
                          "This text is going to keep going. Doop dee doo. This text is going to keep going.\n\nDoop dee doo. " +
                          "This text is going to keep going. Doop dee doo. This text is going to keep going. Doop dee doo. ");
        
        section.face = Face.googly;
        section.lookingDown = true;
        section.voiceStyle = VoiceStyle.Narrow;
        section.optionsText = new string[] {"Option 1!", "Another option!", "Huh?"};
        section.optionsLeadsTo = new int[] {0, 0, 0};
        section.id = 0;
        
        
        prepareSection(section);
        
    }

    void prepareSection(DialogueSection section) {
        dialogueText.text = "";
        optionsText.text = "";
        arrow.SetActive(false);
        state.dialoguePages = breakDialogueIntoPages(section.text);
        state.voiceStyle = section.voiceStyle;
        
        headSpriteRenderer.enabled = section.lookingDown;
        
        switch(section.face) {
            case Face.googly:
                eyesSpriteRenderer.sprite = eyesGooglySprite;
                eyesSpriteRenderer.enabled = true;
                break;
            case Face.shifty:
                eyesSpriteRenderer.sprite = eyesShiftySprite;
                eyesSpriteRenderer.enabled = true;
                break;
            case Face.surprised:
                eyesSpriteRenderer.sprite = eyesSurprisedSprite;
                eyesSpriteRenderer.enabled = true;
                break;
            case Face.normal:
                eyesSpriteRenderer.enabled = false;
                break;
        }
        
        state.options.Clear();
        
        foreach(string option in section.optionsText) {
            state.options.Add(option);
        }
    }

    // Update is called once per frame
    void Update() {
        double time = Time.time;
        state.inputX = Input.GetAxis("Horizontal");
        state.inputY = Input.GetAxis("Vertical");
        state.inputFire = Input.GetAxis("Fire1");
        
        string currentDialogue = state.dialoguePages[state.currentPageIndex];
        
        if (dialogueText.text.Length < currentDialogue.Length) {
            voiceSynth.talking = true;
            
            if (time >= state.nextLetterTimestamp) {
                char nextLetter = currentDialogue[dialogueText.text.Length];
                
                dialogueText.text += nextLetter;
                state.nextLetterTimestamp = time + ((state.inputFire > 0) ? textCharacterEntryDurationFast : textCharacterEntryDuration);
            }
            
            if (time >= state.nextMouthMovementTimestamp) {
                pickRandomMouthSprite();
                state.nextMouthMovementTimestamp = time + Random.Range(minMouthMovementDuration, maxMouthMovementDuration);
            }
            
            //if (dialogueText.text.Length >= state.dialogue.Length) {
            //    setupOptions();
            //}
            
            if (dialogueText.text.Length == currentDialogue.Length) {
                if (state.currentPageIndex < state.dialoguePages.Count-1) {
                    dialogueText.text += "\n< Continued... >";
                    state.waitingForMoreText = true;
                } else {
                    finishedSpeakingForSection();
                }
            }
            
        } else {
            voiceSynth.talking = false;
            mouthSpriteRenderer.enabled = false;
            
            if (state.waitingForMoreText && state.inputFire > 0 && previousState.inputFire == 0) {
                state.currentPageIndex += 1;
                dialogueText.text = "";
                state.waitingForMoreText = false;
            }
        }
        
        previousState = state.Clone();
    }
    
    void finishedSpeakingForSection() {
        headSpriteRenderer.enabled = false;
        setupOptions();
    }
    
    void setupOptions() {
        optionsText.text = "";
        
        foreach(string option in state.options) {
            
        }
    }
    
    void pickRandomMouthSprite() {
        int choice = mouthSpriteBucket.Take();
        
        if (choice == -1) {
            mouthSpriteRenderer.enabled = false;
            return;
        }
        
        mouthSpriteRenderer.enabled = true;
        mouthSpriteRenderer.sprite = mouthSprites[choice];
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
    
    List<string> breakDialogueIntoPages(string text) {
        List<string> lines = breakTextIntoLines(text);
        
        for(int i = 0; i < lines.Count; ++i) {
            Debug.Log("line " + i + ": '" + lines[i] + "'");
        }
        
        List<string> result = new List<string>();
        string currentPage = "";
        int lineCount = 0;
        
        for(int i = 0; i < lines.Count; ++i) {
            currentPage += lines[i] + "\n";
            lineCount += 1;
            
            if (lineCount >= dialogueMaxVisibleLines-1 && (i != lines.Count-1)) {
                result.Add(currentPage.Trim());
                currentPage = "";
                lineCount = 0;
            }
        }
        
        if (currentPage.Length > 0) {
            result.Add(currentPage.Trim());
        }
        
        return result;
    }
    
    List<string> breakTextIntoLines(string text) {
        List<string> result = new List<string>();
        string buffer = "";
        string lineBuffer = "";
        
        // ................X
        // asdf asdf asdf asdf^
        // asdfsdf asdfasdff ^
        // asdfsdf asdfasdf\n^
        // asdfsdf asdfasdffasd\n^
        //
        
        for(int i = 0; i < text.Length; ++i) {
            if (text[i] != ' ' && text[i] != '\n') {
                if ((buffer.Length + lineBuffer.Length) >= dialogueMaxLineLength) {
                    result.Add(lineBuffer);
                    lineBuffer = "";
                    buffer = buffer.TrimStart(null);
                }
                
                buffer += text[i];
                continue;
            }
            
            if (text[i] == '\n') {
                lineBuffer += buffer;
                result.Add(lineBuffer);
                buffer = "";
                lineBuffer = "";
                continue;
            }
            
            // current char is a space
            
            if ((buffer.Length + lineBuffer.Length) >= dialogueMaxLineLength) {
                lineBuffer += buffer;
                result.Add(lineBuffer);
                buffer = "";
                lineBuffer = "";
                continue;
            }
            
            lineBuffer += buffer;
            buffer = " ";
        }
        
        lineBuffer += buffer;
        result.Add(lineBuffer);
        
        return result;
    }
}