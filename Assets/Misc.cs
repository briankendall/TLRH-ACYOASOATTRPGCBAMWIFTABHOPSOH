using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bucket {
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
