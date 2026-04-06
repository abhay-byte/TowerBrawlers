using System;
using UnityEngine;

[Serializable]
public class BuildingCatalogEntry
{
    public string buildingId;
    public string displayName;
    public GameObject prefab;
    public GameObject blueTeamPrefab;
    public GameObject redTeamPrefab;
    public Sprite icon;
    public int snackCost = 50;
    [Range(0f, 1f)] public float sellRefundMultiplier = 0.5f;
    public bool isCastle;
    public bool availableForPlayer = true;
    public bool availableForAI = true;
    public BuildingSlotCategory slotCategory = BuildingSlotCategory.Building;

    public Sprite GetResolvedIcon()
    {
        if (icon != null)
            return icon;

        GameObject resolvedPrefab = prefab != null ? prefab : blueTeamPrefab != null ? blueTeamPrefab : redTeamPrefab;
        if (resolvedPrefab == null)
            return null;

        SpriteRenderer spriteRenderer = resolvedPrefab.GetComponent<SpriteRenderer>();
        return spriteRenderer != null ? spriteRenderer.sprite : null;
    }

    public GameObject ResolvePrefabForTeam(CombatTeam team)
    {
        if (team == CombatTeam.Blue && blueTeamPrefab != null)
            return blueTeamPrefab;

        if (team == CombatTeam.Red && redTeamPrefab != null)
            return redTeamPrefab;

        return prefab;
    }
}
