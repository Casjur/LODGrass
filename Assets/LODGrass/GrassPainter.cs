using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Grass))]
public class GrassPainter : Editor
{
    //public GrassData GrassToPaint;
    public int GrassToPaintIndex { get; }


}
