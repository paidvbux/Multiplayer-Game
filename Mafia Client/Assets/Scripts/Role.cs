using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Role", menuName = "Scriptable Objects/Role")]
public class Role : ScriptableObject 
{
    public string name;
    public Sprite displayImage;
    public string description;
    public bool isBad;
}
