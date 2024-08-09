using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NewTeamColorLookup", menuName = "Team Color Lookup")]
public class TeamColorLookup : ScriptableObject
{
    [SerializeField] private Color[] teamColors;

    public Color GetTeamColor(int teamNum)
    {
        if(teamNum < 0 || teamNum >= teamColors.Length)
        {
            return Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }
        return teamColors[teamNum];
    }
}
