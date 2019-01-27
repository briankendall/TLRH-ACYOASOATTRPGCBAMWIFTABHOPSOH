using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceSynth : MonoBehaviour
{
    public enum Voice {
        Voice1 = 0,
        Voice2 = 1,
        Voice3 = 2,
        Voice4 = 3,
        Voice5 = 4,
        Voice6 = 5,
        Voice7 = 6,
        Voice8 = 7,
        Voice9 = 8,
        Voice10 = 9,
        Voice11 = 10
    };
    
    public enum VoiceStyle {
        Wide,
        Normal,
        Narrow,
        Monotone,
        Low,
        High
    };
    
    class Bucket {
        public Bucket(int inNumStaleValues) {
            freshValues = new List<int>();
            staleValues = new List<int>();
            numStaleValues = inNumStaleValues;
        }
        
        public void FillBucketWithRange(int min, int exclusiveMax) {
            freshValues.Clear();
            staleValues.Clear();
            
            for(int i = min; i < exclusiveMax; ++i) {
                freshValues.Add(i);
            }
        }
        
        public int Take() {
            int index = Random.Range(0, freshValues.Count);
            int value = freshValues[index];
            freshValues.RemoveAt(index);
            staleValues.Add(value);
            
            while(staleValues.Count > numStaleValues) {
                int freshAgain = staleValues[0];
                staleValues.RemoveAt(0);
                freshValues.Add(freshAgain);
            }
            
            return value;
        }
        
        int numStaleValues;
        List<int> freshValues;
        List<int> staleValues;
    };
    
    class VoiceStyleSet {
        public VoiceStyleSet() {
            styles = new Dictionary<VoiceStyle, List<AudioClip> >();
        }
        
        public Dictionary<VoiceStyle, List<AudioClip> > styles;
    };
    
    class VoiceSet {
        public VoiceSet() {
            speeds = new Dictionary<int, VoiceStyleSet>();
        }
        
        public Dictionary<int, VoiceStyleSet> speeds;
    };
    
    static Dictionary<Voice, VoiceSet> voiceSets = null;
    
    public bool talking = false;
    public Voice voice = Voice.Voice1;
    public VoiceStyle style = VoiceStyle.Normal;
    public AudioSource audioSource = null;
    
    const double speed1PieceDuration = 0.2;
    const int numSpeeds = 3;
    const int numPiecesPerStyle = 8;
    double nextPlayTime = 0;
    int lastPieceIndex = -1;
    Bucket pieceBucket;
    
    // Start is called before the first frame update
    void Awake() {
        pieceBucket = new Bucket(2);
        pieceBucket.FillBucketWithRange(0, numPiecesPerStyle);
        
        if (voiceSets == null) {
            loadAllVoiceClips();
        }
    }

    void loadAllVoiceClips() {
        voiceSets = new Dictionary<Voice, VoiceSet>();
        
        foreach(Voice voice in System.Enum.GetValues(typeof(Voice))) {
            if (!voiceSets.ContainsKey(voice)) {
                voiceSets[voice] = new VoiceSet();
            }
            
            for(int speed = 1; speed <= numSpeeds; ++speed) {
                if (!voiceSets[voice].speeds.ContainsKey(speed)) {
                    voiceSets[voice].speeds[speed] = new VoiceStyleSet();
                }
            
                foreach(VoiceStyle style in System.Enum.GetValues(typeof(VoiceStyle))) {
                    if (!voiceSets[voice].speeds[speed].styles.ContainsKey(style)) {
                        voiceSets[voice].speeds[speed].styles[style] = new List<AudioClip>();
                    }
                
                    for(int pieceIndex = 0; pieceIndex < numPiecesPerStyle; ++pieceIndex) {
                        string filename = "voc" + (((int)voice)+1) + "_speed" + speed + "_" + style.ToString().ToLower() + "_" + pieceIndex.ToString("D2");
                        string path = "voices/voc" + (((int)voice)+1) + "/" + filename;
                        AudioClip clip = Resources.Load<AudioClip>(path);
                        
                        if (clip == null) {
                            throw new System.Exception("Failed to load voice clip: " + path);
                        }
                        
                        voiceSets[voice].speeds[speed].styles[style].Add(clip);
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (audioSource == null || !talking) {
            return;
        }
        
        playVoicePieceAndQueueNext();
    }
    
    void playVoicePieceAndQueueNext() {
        double time = Time.realtimeSinceStartup;
        
        if (time >= nextPlayTime) {
            int speed = Random.Range(1, numSpeeds+1);
            int pieceIndex = pieceBucket.Take();
            audioSource.PlayOneShot(voiceSets[voice].speeds[speed].styles[style][pieceIndex]);
            
            lastPieceIndex = pieceIndex;
            nextPlayTime = time + speed1PieceDuration / (speed/2.0 + 0.5);
        }
    }
}
