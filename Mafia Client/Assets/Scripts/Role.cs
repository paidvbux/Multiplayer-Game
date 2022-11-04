using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Role", menuName = "Scriptable Objects/Role")]
public class Role : ScriptableObject 
{
    /// <summary>
    /// Holds role information
    /// </summary>

    public string roleName;
    public Sprite displayImage;
    [TextArea(1, 4)] public string description;
    public bool isBad;
    public enum RoleType { Wolf, Special, Good }
    public RoleType type;

    public enum SelectionType { None, Wolf, Witch, Detective, Guard, Knight }
    public SelectionType selectionType;
}
