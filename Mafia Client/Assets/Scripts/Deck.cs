using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Deck", menuName = "Scriptable Objects/Deck")]
public class Deck : ScriptableObject
{
    public Role[] roles;
}
