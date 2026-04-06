using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CloudFieldGenerator : MonoBehaviour
{
    private enum SpawnMode
    {
        Individual,
        Groups
    }

    [System.Serializable]
    private class CloudMover
    {
        public Transform transform;
        public float speed;
        public bool isGroup;
    }

    [Header("Assets")]
    [SerializeField] private List<Sprite> cloudSprites = new List<Sprite>();

    [Header("Area")]
    [SerializeField] private Vector2 areaSize = new Vector2(30f, 10f);
    [SerializeField] private Vector2 areaOffset = new Vector2(0f, 10f);

    [Header("Spawn")]
    [SerializeField] private SpawnMode spawnMode = SpawnMode.Groups;
    [SerializeField] private int cloudCount = 8;
    [SerializeField] private int groupCount = 3;
    [SerializeField] private Vector2Int cloudsPerGroup = new Vector2Int(2, 4);
    [SerializeField] private Vector2 groupSpread = new Vector2(4f, 1.5f);
    [SerializeField] private Vector2 scaleRange = new Vector2(0.8f, 1.35f);

    [Header("Wind")]
    [SerializeField] private Vector2 windDirection = Vector2.right;
    [SerializeField] private Vector2 speedRange = new Vector2(0.2f, 0.8f);

    [Header("Rendering")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int baseSortingOrder = 10000;
    [SerializeField] private bool randomizeSortingOrder = true;
    [SerializeField] private Color tint = Color.white;

    private readonly List<CloudMover> movers = new List<CloudMover>();
    private Transform generatedRoot;

    private void Awake()
    {
        if (!Application.isPlaying)
            return;

        EnsureGeneratedRoot();
    }

    private void OnEnable()
    {
        if (!Application.isPlaying)
            return;

        EnsureGeneratedRoot();
        RebuildIfEmpty();
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        MoveClouds(Time.deltaTime <= 0f ? 0.016f : Time.deltaTime);
    }

    private void OnValidate()
    {
        areaSize.x = Mathf.Max(1f, areaSize.x);
        areaSize.y = Mathf.Max(1f, areaSize.y);
        cloudCount = Mathf.Max(1, cloudCount);
        groupCount = Mathf.Max(1, groupCount);
        cloudsPerGroup.x = Mathf.Max(1, cloudsPerGroup.x);
        cloudsPerGroup.y = Mathf.Max(cloudsPerGroup.x, cloudsPerGroup.y);
        scaleRange.x = Mathf.Max(0.1f, scaleRange.x);
        scaleRange.y = Mathf.Max(scaleRange.x, scaleRange.y);
        speedRange.x = Mathf.Max(0f, speedRange.x);
        speedRange.y = Mathf.Max(speedRange.x, speedRange.y);
    }

    [ContextMenu("Load Default Cloud Sprites")]
    public void LoadDefaultCloudSprites()
    {
#if UNITY_EDITOR
        cloudSprites.Clear();
        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Tiny Swords/Terrain/Decorations/Clouds" });
        foreach (string guid in spriteGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null && !cloudSprites.Contains(sprite))
                cloudSprites.Add(sprite);
        }

        cloudSprites.Sort((left, right) => string.CompareOrdinal(left.name, right.name));
        EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Generate Clouds")]
    public void GenerateClouds()
    {
        if (!Application.isPlaying)
            return;

        EnsureGeneratedRoot();
        ClearGeneratedClouds();

        if (cloudSprites.Count == 0)
            LoadDefaultCloudSprites();

        if (cloudSprites.Count == 0)
            return;

        if (spawnMode == SpawnMode.Groups)
            GenerateGroups();
        else
            GenerateIndividuals();
    }

    [ContextMenu("Clear Generated Clouds")]
    public void ClearGeneratedClouds()
    {
        movers.Clear();

        if (generatedRoot == null)
            return;

        for (int i = generatedRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = generatedRoot.GetChild(i);

            Destroy(child.gameObject);
        }
    }

    private void RebuildIfEmpty()
    {
        EnsureGeneratedRoot();

        if (generatedRoot == null || generatedRoot.childCount > 0)
            return;

        GenerateClouds();
    }

    private void EnsureGeneratedRoot()
    {
        if (generatedRoot != null)
            return;

        Transform existing = transform.Find("_GeneratedClouds");
        if (existing != null)
        {
            generatedRoot = existing;
            CacheMovers();
            return;
        }

        GameObject root = new GameObject("_GeneratedClouds");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = Vector3.zero;
        generatedRoot = root.transform;
    }

    private void GenerateIndividuals()
    {
        for (int i = 0; i < cloudCount; i++)
            CreateCloud(i, generatedRoot, RandomPointInArea(), RandomSortingOrder(i));
    }

    private void GenerateGroups()
    {
        for (int groupIndex = 0; groupIndex < groupCount; groupIndex++)
        {
            GameObject groupObject = new GameObject($"CloudGroup_{groupIndex + 1}");
            groupObject.transform.SetParent(generatedRoot, false);
            groupObject.transform.localPosition = RandomPointInArea();

            float speed = Random.Range(speedRange.x, speedRange.y);
            movers.Add(new CloudMover
            {
                transform = groupObject.transform,
                speed = speed,
                isGroup = true
            });

            int count = Random.Range(cloudsPerGroup.x, cloudsPerGroup.y + 1);
            for (int cloudIndex = 0; cloudIndex < count; cloudIndex++)
            {
                Vector3 localOffset = new Vector3(
                    Random.Range(-groupSpread.x, groupSpread.x),
                    Random.Range(-groupSpread.y, groupSpread.y),
                    0f);

                CreateCloud(cloudIndex, groupObject.transform, localOffset, RandomSortingOrder(groupIndex + cloudIndex));
            }
        }
    }

    private void CreateCloud(int index, Transform parent, Vector3 localPosition, int sortingOrder)
    {
        GameObject cloudObject = new GameObject($"Cloud_{index + 1}");
        cloudObject.transform.SetParent(parent, false);
        cloudObject.transform.localPosition = localPosition;

        float scale = Random.Range(scaleRange.x, scaleRange.y);
        cloudObject.transform.localScale = Vector3.one * scale;

        SpriteRenderer spriteRenderer = cloudObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = cloudSprites[Random.Range(0, cloudSprites.Count)];
        spriteRenderer.color = tint;
        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = sortingOrder;
        spriteRenderer.spriteSortPoint = SpriteSortPoint.Center;

        if (spawnMode == SpawnMode.Individual)
        {
            movers.Add(new CloudMover
            {
                transform = cloudObject.transform,
                speed = Random.Range(speedRange.x, speedRange.y)
            });
        }
    }

    private void CacheMovers()
    {
        movers.Clear();

        if (generatedRoot == null)
            return;

        if (spawnMode == SpawnMode.Groups)
        {
            for (int i = 0; i < generatedRoot.childCount; i++)
            {
                Transform child = generatedRoot.GetChild(i);
                movers.Add(new CloudMover
                {
                    transform = child,
                    speed = Mathf.Lerp(speedRange.x, speedRange.y, Mathf.InverseLerp(0, Mathf.Max(1, generatedRoot.childCount - 1), i)),
                    isGroup = true
                });
            }
            return;
        }

        SpriteRenderer[] renderers = generatedRoot.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in renderers)
        {
            movers.Add(new CloudMover
            {
                transform = renderer.transform,
                speed = Random.Range(speedRange.x, speedRange.y),
                isGroup = false
            });
        }
    }

    private void MoveClouds(float deltaTime)
    {
        if (movers.Count == 0)
            CacheMovers();

        Vector2 direction = windDirection.sqrMagnitude > 0.0001f ? windDirection.normalized : Vector2.right;
        Vector2 halfSize = areaSize * 0.5f;
        Vector2 center = (Vector2)transform.position + areaOffset;

        foreach (CloudMover mover in movers)
        {
            if (mover == null || mover.transform == null)
                continue;

            Vector3 position = mover.transform.position;
            position += (Vector3)(direction * mover.speed * deltaTime);
            mover.transform.position = position;

            if (IsOutOfBounds(position, center, halfSize))
                RecycleCloud(mover, direction, center, halfSize);
        }
    }

    private bool IsOutOfBounds(Vector3 position, Vector2 center, Vector2 halfSize)
    {
        Vector2 relative = (Vector2)position - center;
        return relative.x > halfSize.x || relative.x < -halfSize.x || relative.y > halfSize.y || relative.y < -halfSize.y;
    }

    private void RecycleCloud(CloudMover mover, Vector2 direction, Vector2 center, Vector2 halfSize)
    {
        Vector2 spawnPosition = center;

        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
        {
            spawnPosition.x = direction.x >= 0f ? center.x - halfSize.x : center.x + halfSize.x;
            spawnPosition.y = Random.Range(center.y - halfSize.y, center.y + halfSize.y);
        }
        else
        {
            spawnPosition.y = direction.y >= 0f ? center.y - halfSize.y : center.y + halfSize.y;
            spawnPosition.x = Random.Range(center.x - halfSize.x, center.x + halfSize.x);
        }

        mover.transform.position = new Vector3(spawnPosition.x, spawnPosition.y, mover.transform.position.z);

        if (mover.isGroup)
        {
            float speed = Random.Range(speedRange.x, speedRange.y);
            mover.speed = speed;

            for (int i = 0; i < mover.transform.childCount; i++)
            {
                Transform child = mover.transform.GetChild(i);
                child.localPosition = new Vector3(
                    Random.Range(-groupSpread.x, groupSpread.x),
                    Random.Range(-groupSpread.y, groupSpread.y),
                    0f);
                child.localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);

                SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
                if (renderer != null && cloudSprites.Count > 0)
                {
                    renderer.sprite = cloudSprites[Random.Range(0, cloudSprites.Count)];
                    renderer.sortingOrder = RandomSortingOrder(i);
                    renderer.color = tint;
                }
            }

            return;
        }

        mover.speed = Random.Range(speedRange.x, speedRange.y);

        SpriteRenderer spriteRenderer = mover.transform.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && cloudSprites.Count > 0)
        {
            spriteRenderer.sprite = cloudSprites[Random.Range(0, cloudSprites.Count)];
            spriteRenderer.sortingOrder = RandomSortingOrder(0);
            spriteRenderer.color = tint;
        }

        mover.transform.localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);
    }

    private Vector3 RandomPointInArea()
    {
        return new Vector3(
            Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f) + areaOffset.x,
            Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f) + areaOffset.y,
            0f);
    }

    private int RandomSortingOrder(int index)
    {
        if (!randomizeSortingOrder)
            return baseSortingOrder;

        return baseSortingOrder + index + Random.Range(0, 20);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 1f, 0.15f);
        Vector3 center = transform.position + (Vector3)areaOffset;
        Gizmos.DrawCube(center, new Vector3(areaSize.x, areaSize.y, 0f));

        Gizmos.color = new Color(1f, 1f, 1f, 0.45f);
        Gizmos.DrawWireCube(center, new Vector3(areaSize.x, areaSize.y, 0f));

        Gizmos.color = new Color(0.7f, 0.9f, 1f, 0.8f);
        Vector3 arrowDirection = (Vector3)(windDirection.sqrMagnitude > 0.0001f ? windDirection.normalized : Vector2.right) * 2f;
        Gizmos.DrawLine(center, center + arrowDirection);
        Gizmos.DrawSphere(center + arrowDirection, 0.15f);
    }
}
