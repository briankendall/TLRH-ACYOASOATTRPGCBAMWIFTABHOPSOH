using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainController : MonoBehaviour
{
    public GameObject arrow;
    public GameObject eyesSprite;
    public GameObject mouthSprite;
    public GameObject headSprite;
    public GameObject dialogueBox;
    public GameObject optionsBox;
    public GameObject gameoverSprite;
    public Sprite[] mouthSprites;
    public Sprite eyesShiftySprite;
    public Sprite eyesGooglySprite;
    public Sprite eyesSurprisedSprite;
    public Sprite eyesAngrySprite;
    public AudioSource audioSource;
    public AudioClip selectAudioClip;
    public AudioClip invalidAudioClip;
    public AudioClip submitAudioClip;
    public AudioClip gameOverAudioClip;
    
    enum Face {
        normal,
        googly,
        angry,
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
        public List<int> optionsLeadTo = new List<int>();
        public VoiceStyle voiceStyle = VoiceStyle.Normal;
        public double nextLetterTimestamp = 0;
        public double nextMouthMovementTimestamp = 0;
        public int selectedOption = 0;
        
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
    Vector3 arrowOrigin;
    
    Dictionary<int, DialogueSection> sections;
    
    void Start() {
        dialogueText = dialogueBox.GetComponent<Text>();
        optionsText = optionsBox.GetComponent<Text>();
        voiceSynth = GetComponent<VoiceSynth>();
        eyesSpriteRenderer = eyesSprite.GetComponent<SpriteRenderer>();
        headSpriteRenderer = headSprite.GetComponent<SpriteRenderer>();
        mouthSpriteRenderer = mouthSprite.GetComponent<SpriteRenderer>();
        mouthSpriteBucket.FillBucketWithRange(-1, 3);
        
        arrowOrigin = arrow.transform.localPosition;
        
        dialogueText.text = "";
        optionsText.text = "";
        arrow.SetActive(false);
        
        sections = new Dictionary<int, DialogueSection>();
        int id;
        
        id = 0;
        sections[id] = new DialogueSection();
        sections[id].text = ("Hey there! Welcome to try out my totally awesome amazing incredible really good -- like it's totally going to rock your " +
                             "socks -- kind of a tabletop RPG role playing game? I set the entire thing up myself, including the rules and the system. " +
                             "It's going to go great.");
        sections[id].face = Face.normal;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Normal;
        sections[id].optionsText = new string[] {"You bet!"};
        sections[id].optionsLeadsTo = new int[] {1};
        
        id = 1;
        sections[id] = new DialogueSection();
        sections[id].text = ("Okay, we're going to start off making a character for you. Would you prefer to play a wizard, a mage, or a spell-caster?");
        sections[id].face = Face.normal;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Normal;
        sections[id].optionsText = new string[] {"Wizard", "Mage", "Spell-caster", "Totally OP fighter that has maxed out stats"};
        sections[id].optionsLeadsTo = new int[] {3, 3, 3, 2};
        
        id = 2;
        sections[id] = new DialogueSection();
        sections[id].text = ("Yeah, asshole, you can't play that kind of character. My system doesn't have stats like that.");
        sections[id].face = Face.angry;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Low;
        sections[id].optionsText = new string[] {"OK fine I'll be a mage"};
        sections[id].optionsLeadsTo = new int[] {3};
        
        id = 3;
        sections[id] = new DialogueSection();
        sections[id].text = ("Just a second now, I'm working out the details...\n\n...\n\n...\n\n...\n\n...\n\n...\n\n...\n\n...\n\n...\n\n..." +
                             "\n\n...\n\n...\n\n...\n\n\n\n...\n\n\n\n...\n\n\n\n...\n\n...\n\n...\n\n\n\n...");
        sections[id].face = Face.googly;
        sections[id].lookingDown = true;
        sections[id].voiceStyle = VoiceStyle.Narrow;
        sections[id].optionsText = new string[] {"..."};
        sections[id].optionsLeadsTo = new int[] {4};
        
        id = 4;
        sections[id] = new DialogueSection();
        sections[id].text = ("Okay, I got it! Let's play!");
        sections[id].face = Face.googly;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Wide;
        sections[id].optionsText = new string[] {"Sweet"};
        sections[id].optionsLeadsTo = new int[] {5};
        
        id = 5;
        sections[id] = new DialogueSection();
        sections[id].text = ("You are in a far away distant land trying to find your way back home. To the west of you is a scary looking cave that's " +
                             "probably quite possibly filled with treasure. To the east of you is a path leading to a castle. Which way do you go?");
        sections[id].face = Face.normal;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Normal;
        sections[id].optionsText = new string[] {"West, to the cave", "East, to the castle", "South"};
        sections[id].optionsLeadsTo = new int[] {8, 11, 6};
        
        id = 6;
        sections[id] = new DialogueSection();
        sections[id].text = ("Um okay, I wasn't expecting that, let's see what's to the south...");
        sections[id].face = Face.shifty;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Narrow;
        sections[id].optionsText = new string[] {"..."};
        sections[id].optionsLeadsTo = new int[] {7};
        
        id = 7;
        sections[id] = new DialogueSection();
        sections[id].text = ("Uh oh! You smack directly into a tree! You impact it so hard it breaks your nose, and your neck. Before you can die from " +
                             "your injuries, the enormous crack you made in the tree from your intense impact causes it fall directly on to you, squishing " +
                             "you like a bug. You are dead!");
        sections[id].face = Face.googly;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Wide;
        sections[id].optionsText = new string[] {"Duhhh, uh what?"};
        sections[id].optionsLeadsTo = new int[] {-1};
        
        id = 8;
        sections[id] = new DialogueSection();
        sections[id].text = ("You enter into the cave, and it's very dark and you can't see. Hold on, I have to check what's in the cave...");
        sections[id].face = Face.shifty;
        sections[id].lookingDown = true;
        sections[id].voiceStyle = VoiceStyle.Low;
        sections[id].optionsText = new string[] {"Sure, take your time"};
        sections[id].optionsLeadsTo = new int[] {9};
        
        id = 9;
        sections[id] = new DialogueSection();
        sections[id].text = ("Wait, did I say it was a cave? I meant it was a giant ravine!");
        sections[id].face = Face.surprised;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Wide;
        sections[id].optionsText = new string[] {"The hell?"};
        sections[id].optionsLeadsTo = new int[] {10};
        
        id = 10;
        sections[id] = new DialogueSection();
        sections[id].text = ("You fall into the pit! And fall for a good while! Then you hit the bottom!\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nDid I mention you die?");
        sections[id].face = Face.googly;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Normal;
        sections[id].optionsText = new string[] {"Duhhhh?"};
        sections[id].optionsLeadsTo = new int[] {-1};
        
        id = 11;
        sections[id] = new DialogueSection();
        sections[id].text = ("You start making your way to the castle. It's a long road, but you're making good time.");
        sections[id].face = Face.normal;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Normal;
        sections[id].optionsText = new string[] {"Okay."};
        sections[id].optionsLeadsTo = new int[] {12};
        
        id = 12;
        sections[id] = new DialogueSection();
        sections[id].text = ("Suddenly, you encounter a monster! Because, you know, roads to castles are always covered in monsters and stuff.\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" +
                             "It's some sort of water slime thing, quite possibly a water slime. You get yourself ready for combat!");
        sections[id].face = Face.surprised;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Wide;
        sections[id].optionsText = new string[] {"Exciting?"};
        sections[id].optionsLeadsTo = new int[] {13};
        
        id = 13;
        sections[id] = new DialogueSection();
        sections[id].text = ("Okay, which action do you want to take?");
        sections[id].face = Face.normal;
        sections[id].lookingDown = true;
        sections[id].voiceStyle = VoiceStyle.Normal;
        sections[id].optionsText = new string[] {"I cast magic missle", "I punch it in the face", "I run away", "I ignore it completely and keep going"};
        sections[id].optionsLeadsTo = new int[] {14, 15, 18, 20};
        
        id = 14;
        sections[id] = new DialogueSection();
        sections[id].text = ("Oh no! It turns out magic missle is a heat seeking magic missile! And since water slimes don't have any body heat at all, " +
                             "the only thing for it to home in on is you! It immediately reverses direction and smacks into you. Your body explodes " + 
                             "spectacularly, littering your magically charred glittery remains all over the countryside. The water slime regards this bizarre " +
                             "occurence in silence.");
        sections[id].face = Face.googly;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Wide;
        sections[id].optionsText = new string[] {"Why do you do this to me?"};
        sections[id].optionsLeadsTo = new int[] {-1};
        
        id = 15;
        sections[id] = new DialogueSection();
        sections[id].text = ("Even though the slime barely moves at all, surprisingly, you miss! What do you do now?");
        sections[id].face = Face.normal;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Narrow;
        sections[id].optionsText = new string[] {"I try to punch it again", "I cast magic missle", "I run away", "I ignore the slime and continue"};
        sections[id].optionsLeadsTo = new int[] {16, 14, 18, 20};
        
        id = 16;
        sections[id] = new DialogueSection();
        sections[id].text = ("Hmmm, you miss again! Wow! Now what?");
        sections[id].face = Face.surprised;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Wide;
        sections[id].optionsText = new string[] {"I try to punch yet again", "I cast magic missle", "I run away", "I ignore the slime and continue"};
        sections[id].optionsLeadsTo = new int[] {17, 14, 18, 20};
        
        id = 17;
        sections[id] = new DialogueSection();
        sections[id].text = ("Okay, this time you hit the immobile, nearly-impossible to miss slime...\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" +
                             "Your fist just passes through it. So, um, now what?");
        sections[id].face = Face.surprised;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Normal;
        sections[id].optionsText = new string[] {"I cast magic missle", "I run away", "I ignore the slime and continue"};
        sections[id].optionsLeadsTo = new int[] {14, 18, 20};
        
        id = 18;
        sections[id] = new DialogueSection();
        sections[id].text = ("You run like a frightened little baby, your arms embarassingly and comically flailing behind you...");
        sections[id].face = Face.googly;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Normal;
        sections[id].optionsText = new string[] {"..."};
        sections[id].optionsLeadsTo = new int[] {19};
        
        id = 19;
        sections[id] = new DialogueSection();
        sections[id].text = ("...And you smack directly into a tree! Like all of the other trees in this part of the countryside, they are " +
                             "outrageously flimsy and weak but still incredibly heavy. The crack you create in its trunk causes it to fall over " +
                             "directly onto you, killing you instantly. Many legends will be told in the futuretimes to the amusement of many an " +
                             "inn patron telling of your compellingly dumb adventure that ended in self-inflicted death after encountering a single slime.");
        sections[id].face = Face.googly;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Normal;
        sections[id].optionsText = new string[] {"You suck. This is dumb."};
        sections[id].optionsLeadsTo = new int[] {-1};
        
        id = 20;
        sections[id] = new DialogueSection();
        sections[id].text = ("Good idea! Water slimes are totally docile and harmless. You continue walking towards the castle...");
        sections[id].face = Face.googly;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Normal;
        sections[id].optionsText = new string[] {".... Uh huh...."};
        sections[id].optionsLeadsTo = new int[] {21};
        
        id = 21;
        sections[id] = new DialogueSection();
        sections[id].text = ("After many minutes, you reach the castle.\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nIt turns out it's just a large painting " +
                             "of a castle, there to liven up to view.");
        sections[id].face = Face.normal;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Normal;
        sections[id].optionsText = new string[] {"?!?"};
        sections[id].optionsLeadsTo = new int[] {22};
        
        id = 22;
        sections[id] = new DialogueSection();
        sections[id].text = ("Hold on, there's something I need to look up for this exact situation...");
        sections[id].face = Face.normal;
        sections[id].lookingDown = true;
        sections[id].voiceStyle = VoiceStyle.Normal;
        sections[id].optionsText = new string[] {"Uh huh..."};
        sections[id].optionsLeadsTo = new int[] {23};
        
        id = 23;
        sections[id] = new DialogueSection();
        sections[id].text = ("Ah yes, your head explodes.");
        sections[id].face = Face.normal;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Wide;
        sections[id].optionsText = new string[] {"..."};
        sections[id].optionsLeadsTo = new int[] {24};
        
        id = 24;
        sections[id] = new DialogueSection();
        sections[id].text = ("...");
        sections[id].face = Face.googly;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Normal;
        sections[id].optionsText = new string[] {"..."};
        sections[id].optionsLeadsTo = new int[] {25};
        
        id = 25;
        sections[id] = new DialogueSection();
        sections[id].text = ("...");
        sections[id].face = Face.googly;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Normal;
        sections[id].optionsText = new string[] {"..."};
        sections[id].optionsLeadsTo = new int[] {26};
        
        id = 26;
        sections[id] = new DialogueSection();
        sections[id].text = ("...");
        sections[id].face = Face.googly;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Normal;
        sections[id].optionsText = new string[] {"..."};
        sections[id].optionsLeadsTo = new int[] {27};
        
        id = 27;
        sections[id] = new DialogueSection();
        sections[id].text = ("You've died.");
        sections[id].face = Face.googly;
        sections[id].lookingDown = false;
        sections[id].voiceStyle = VoiceStyle.Wide;
        sections[id].optionsText = new string[] {("Yeah, no kidding. This is an incredibly dumb game and you should feel bad for making it. It's " +
                                                  "no fun at all and there's no way to win. Seriously, what the hell? Good god man what sort of " +
                                                  "person goes through the effort of making something like this? I really just don't get it. It's " + 
                                                  "not even that funny, and it's really just the same tired joke over and over again.")};
        sections[id].optionsLeadsTo = new int[] {-1};
        
        prepareSection(sections[0]);
    }

    void prepareSection(DialogueSection section) {
        dialogueText.text = "";
        optionsText.text = "";
        arrow.SetActive(false);
        state.dialoguePages = breakDialogueIntoPages(section.text);
        state.currentPageIndex = 0;
        state.voiceStyle = section.voiceStyle;
        voiceSynth.style = section.voiceStyle;
        
        headSpriteRenderer.enabled = section.lookingDown;
        
        switch(section.face) {
            case Face.googly:
                eyesSpriteRenderer.sprite = eyesGooglySprite;
                eyesSpriteRenderer.enabled = true;
                break;
            case Face.angry:
                eyesSpriteRenderer.sprite = eyesAngrySprite;
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
        state.optionsLeadTo.Clear();
        
        foreach(string option in section.optionsText) {
            state.options.Add(option);
        }
        
        foreach(int id in section.optionsLeadsTo) {
            state.optionsLeadTo.Add(id);
        }
    }

    // Update is called once per frame
    void Update() {
        double time = Time.time;
        state.inputX = Input.GetAxis("Horizontal");
        state.inputY = Input.GetAxis("Vertical");
        state.inputFire = Input.GetAxis("Fire1");
        
        if (gameoverSprite.activeSelf) {
            if (Input.GetAxis("Fire2") > 0) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            return;
        }
        
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
            
            if (state.waitingForMoreText) {
                if (state.inputFire > 0 && previousState.inputFire == 0) {
                    state.currentPageIndex += 1;
                    dialogueText.text = "";
                    state.waitingForMoreText = false;
                }
            } else {
                handleOptions();
            }
        }
        
        previousState = state.Clone();
    }
    
    void gameover() {
        audioSource.PlayOneShot(gameOverAudioClip);
        gameoverSprite.SetActive(true);
    }
    
    void handleOptions() {
        if (state.inputY < 0 && previousState.inputY >= 0) {
            if (state.selectedOption < state.options.Count-1) {
                audioSource.PlayOneShot(selectAudioClip);
                state.selectedOption += 1;
            } else {
                audioSource.PlayOneShot(invalidAudioClip);
            }
        }
        
        if (state.inputY > 0 && previousState.inputY <= 0) {
            if (state.selectedOption > 0) {
                audioSource.PlayOneShot(selectAudioClip);
                state.selectedOption -= 1;
            } else {
                audioSource.PlayOneShot(invalidAudioClip);
            }
        }
        
        arrow.transform.localPosition = new Vector3(arrowOrigin.x,
                                                    arrowOrigin.y + (-1.162f / 3f)*state.selectedOption,
                                                    arrowOrigin.z);
        
        if (state.inputFire > 0 && previousState.inputFire <= 0) {
            audioSource.PlayOneShot(submitAudioClip);
            
            if (state.optionsLeadTo[state.selectedOption] == -1) {
                gameover();
            } else {
                prepareSection(sections[state.optionsLeadTo[state.selectedOption]]);
            }
        }
    }
    
    void finishedSpeakingForSection() {
        //headSpriteRenderer.enabled = false;
        setupOptions();
    }
    
    void setupOptions() {
        optionsText.text = "";
        
        foreach(string option in state.options) {
            optionsText.text += option + "\n";
        }
        
        arrow.transform.localPosition = arrowOrigin;
        arrow.SetActive(true);
        state.selectedOption = 0;
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
