using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Party Member")]
public class PartyMemberInfo : ScriptableObject
{
    public string MemberName;
    public int StartingLevel;
    public int BaseHealth;
    public int BaseStr;
    public int BaseInitiative;

    public GameObject MemberBattleVisualPrefab; //What will be displayed in battle Scene
    public GameObject MemberOverWorldVisualPrefab; //What will be displayed in the overworld scene
}
