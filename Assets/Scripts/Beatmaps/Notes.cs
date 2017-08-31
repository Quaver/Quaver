using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This structure contains the information for a single row of notes.
// For now, it's just a binary digit, is there a note there or not. 
// If we want to support multiple note types, we'll need to change this.
public struct Notes
{
    public bool left;
    public bool right;
    public bool up;
    public bool down;
}
