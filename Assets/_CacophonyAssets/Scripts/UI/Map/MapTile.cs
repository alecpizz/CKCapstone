using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author: Trinity
/// Description: A tile of the UI map
/// </summary>
public class MapTile : MonoBehaviour
{
    [SerializeField]
    GameplayScenes Scene;

    Image image;

    List<Vector2Int> directions = new()
    {
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(1, 0)
    };

    Dictionary<Vector2Int, Image> directionalExits = new();

    private void Awake()
    {
        image = GetComponent<Image>();

        int i = 0;
        foreach(Transform t in transform.GetChild(0))
        {
            directionalExits[directions[i]] = t.GetComponent<Image>();
            i++;
        }
    }

    public void SetPositionFromCoordinates(int x, int y, float scale, Vector2 anchorPosition)
    {
        transform.position = new Vector3(transform.position.x + anchorPosition.x + x * scale *2 , transform.position.y + anchorPosition.y + y * scale *2, 0);
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }

    public void SetScene(GameplayScenes scene)
    {
        Scene = scene;
        EnableExits();
    }

    private void EnableExits()
    {
        foreach(KeyValuePair<Vector2Int, Image> entry in directionalExits)
        {
            if (SaveSceneData.Instance.FindSceneDataInDirection(Scene, entry.Key) != null)
                entry.Value.gameObject.SetActive(true);
        }
    }

    public GameplayScenes GetScene()
    {
        return Scene;
    }
}
