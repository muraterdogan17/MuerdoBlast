using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] [Range(2, 10)] private int rowCount = 6;
    [SerializeField] [Range(2, 10)] private int columnCount = 6; 
    [SerializeField] [Range(2, 6)] private int colorCount = 4;
    [SerializeField] public int conditionA;
    [SerializeField] public int conditionB;
    [SerializeField] public int conditionC;
    [SerializeField] GameObject[] itemPrefabs;
    private List<GameObject>[] items; //board
    Dictionary<Tuple<int, int>, int> cellGroup = new Dictionary<Tuple<int, int>, int>(); //x,y -> group
    Dictionary<int, List<Tuple<int, int>>> groupCells = new Dictionary<int, List<Tuple<int, int>>>(); //group -> list of (x,y)'s

    // Start is called before the first frame update
    void Start()
    {
        initialRandomSpawns();
        findRelatedCells(); 
        getRelatedGroups();
        updateSprites();
        //shuffle();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Create a ray from the camera's position and direction based on the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Convert the 3D ray to a 2D ray
            Vector2 rayOrigin2D = new Vector2(ray.origin.x, ray.origin.y);
            Vector2 rayDirection2D = new Vector2(ray.direction.x, ray.direction.y);

            // Perform the 2D raycast and check if it hits an object
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin2D, rayDirection2D, 100f);

            // Check if the raycast hit an object
            if (hit.collider != null)
            {
                pop(hit.collider.gameObject);
            }
        }

        updateIndexesAndAddItems();
        cellGroup.Clear();
        groupCells.Clear();
        findRelatedCells();
        getRelatedGroups();
        updateSprites();
        shuffle();
    }

    //Generate Board
    private void initialRandomSpawns()
    {
        items = new List<GameObject>[columnCount];
        for (int i = 0; i < columnCount; i++)
        {
            items[i] = new List<GameObject>();
            for (int j = 0; j < rowCount; j++) {
                var item = Instantiate(itemPrefabs[Random.Range(0, colorCount)], new Vector2(i, j), Quaternion.identity);
                item.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                items[i].Insert(j, item);
            }
        }
    }

    /* Update indexes and add new random items if anything is deleted*/
    private void updateIndexesAndAddItems()
    {
        for (int i = 0; i < columnCount; i++)
        {
            var temp = items[i].OrderBy(x => x == null).ThenBy(x => x != null);
            int nullCount = temp.Count(x => x == null);
            items[i] = temp.ToList();
            items[i].RemoveAll(x => x == null);
            for (int k = 0; k < nullCount; k++)
            {
                var newItem = Instantiate(itemPrefabs[Random.Range(0, colorCount)], new Vector2(i, rowCount + k + 0.1f), Quaternion.identity);
                newItem.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                items[i].Add(newItem);
            }
        }
    }

    //Creats dict group -> cells
    private void getRelatedGroups()
    {

        foreach (var item in cellGroup)
        {
            if (!groupCells.ContainsKey(item.Value))
            {
                groupCells[item.Value] = new List<Tuple<int, int>>();
            }
            groupCells[item.Value].Add(item.Key);
        }
    }

    /* for debug purposes */
    private void printItems()
    {
        foreach (var item in items)
        {
            foreach (var go in item)
            {
                Debug.Log(go.name);
            }
        }
    }

    private int getID(int i, int j)
    {
        return items[i][j].GetComponent<Item>().ID;
    }

    private int getRelatedCount(Tuple<int, int> coordinates)
    {
        int thisCellsGroup = cellGroup[coordinates];
        int count = groupCells[thisCellsGroup].Count;
        return count;
    }

    private void updateSprites()
    {
        for (int i = 0; i < columnCount; i++)
        {
            for (int j = 0; j < rowCount; j++)
            {
                var coordinate = new Tuple<int, int>(i, j);
                if (getRelatedCount(coordinate) <= conditionA)
                {
                    items[i][j].GetComponent<Item>().changeState(Item.States.Default);
                }
                else if (getRelatedCount(coordinate) <= conditionB)
                {
                    items[i][j].GetComponent<Item>().changeState(Item.States.A);

                }
                else if (getRelatedCount(coordinate) <= conditionC)
                {
                    items[i][j].GetComponent<Item>().changeState(Item.States.B);
                }
                else
                {
                    items[i][j].GetComponent<Item>().changeState(Item.States.C);
                }
            }
        }
    }

    //Destroy the cells/cubes/boxes
    public void pop(GameObject gameObject)
    {
        if (gameObject.tag == "Item")
        {
            for (int i = 0; i < columnCount; i++)
            {
                for (int j = 0; j < rowCount; j++)
                {
                    if (GameObject.ReferenceEquals(items[i][j], gameObject))
                    {
                        var index = new Tuple<int, int>(i, j);
                        int thisCellsGroup = cellGroup[index];
                        List<Tuple<int, int>> itemsCoordinatesToDelete = groupCells[thisCellsGroup];
                        if(itemsCoordinatesToDelete.Count > 1)
                        {
                            foreach (var cell in itemsCoordinatesToDelete)
                            {
                                Destroy(items[cell.Item1][cell.Item2]);
                            }
                            break;
                        } else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    private void findRelatedCells()
    {
        int group = 0;
        for (int i = 0; i < columnCount; i++)
        {
            for (int j = 0; j < rowCount; j++)
            {
                Tuple<int, int> key = new Tuple<int, int>(i, j);
                if (!cellGroup.ContainsKey(key))
                {
                    var id = getID(i, j);
                    floodFill(i, j, id, group);
                    group++;
                }
            }
        }
    }

    //Algorithm that determines the area connected
    public void floodFill(int i, int j, int id, int group)
    {
        if(i < 0 || i + 1  > columnCount || j < 0 || j + 1 > rowCount)
        {
            return;
        }
        Tuple<int, int> key = new Tuple<int, int>(i, j);
        if (cellGroup.ContainsKey(key))
        {
            return;
        }
        if (id == items[i][j].GetComponent<Item>().ID)
        {
            cellGroup[new Tuple<int, int>(i, j)] = group;
            floodFill(i + 1, j, id, group);
            floodFill(i, j + 1, id, group);
            floodFill(i - 1, j, id, group);
            floodFill(i, j - 1, id, group);
        }
        else
        {
            return;
        }
    }
    // Check all groups to see if they are all single blocks
    private bool needShuffle()
    {
        bool shuf = true;
        foreach (var item in groupCells)
        {
            if(item.Value.Count > 1)
            {
                shuf = false;
            }
        }
        return shuf;
    }

    //Used to relocates items that are shuffled
    private void updatePositions()
    {
        for (int i = 0; i < columnCount; i++)
        {
            for (int j = 0; j < rowCount; j++)
            {
                items[i][j].transform.position = new Vector3(i, j, 0);
            }
        }
    }
    //Simple shuffle to create matches
    private void shuffle()
    {
        
        if (needShuffle()) //gozden gecir
        {
            for (int i = 0; i < columnCount; i++)
            {
                var temp = items[i].OrderBy(x => Random.value);
                items[i] = temp.ToList();
            }
            updatePositions();
        }
    }
}
