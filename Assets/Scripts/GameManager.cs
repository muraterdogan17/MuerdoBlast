using MiscUtil.Xml.Linq.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Item;

public class GameManager : MonoBehaviour
{
    [SerializeField] [Range(2, 10)] private int rowCount = 6;
    [SerializeField] [Range(2, 10)] private int columnCount = 6; 
    [SerializeField] [Range(2, 6)] private int colorCount = 4;
    [SerializeField] public int conditionA;
    [SerializeField] public int conditionB;
    [SerializeField] public int conditionC;
    [SerializeField] GameObject[] itemPrefabs;
    private List<GameObject>[] items;
    private List<GameObject>[] relatedCells;

    // Start is called before the first frame update
    void Start()
    {
        initialRandomSpawns();
    }

    // Update is called once per frame
    void Update()
    {
        updateAndAddItems();
    }

    //Generate Board
    private void initialRandomSpawns()
    {
        items = new List<GameObject>[columnCount];
        for (int i = 0; i < columnCount; i++)
        {
            items[i] = new List<GameObject>();
            for (int j = 0; j < rowCount; j++) {
                var item = Instantiate(itemPrefabs[UnityEngine.Random.Range(0, colorCount)], new Vector2(i, j), Quaternion.identity);
                item.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                items[i].Insert(j, item);
            }
        }
    }

    /* Update indexes and add new random items if anything is deleted*/
    private void updateAndAddItems()
    {
        for (int i = 0; i < columnCount; i++)
        {
            var temp = items[i].OrderBy(x => x == null).ThenBy(x => x != null);
            int nullCount = temp.Count(x => x == null);
            items[i] = temp.ToList();
            items[i].RemoveAll(x => x == null);
            for (int k = 0; k < nullCount; k++)
            {
                var newItem = Instantiate(itemPrefabs[UnityEngine.Random.Range(0, colorCount)], new Vector2(i, rowCount + k), Quaternion.identity);
                newItem.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                items[i].Add(newItem);
            }
        }
    }

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
    private void findRelatedCells()
    {
        Point[] visited = new Point[rowCount*columnCount];
        for (int i = 0; i < columnCount; i++)
        {
            for (int j = 0; j < rowCount; j++)
            {
                Point point = new Point(i,j);
                var val = getID(i,j);
                if (visited.Contains(point)) continue;
            }
        }
        /*
        foreach (var item in RelatedNeighbors.Values)
        {
            if (item.Count <= conditionA)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    item[i].GetComponent<Item>().type = Item.States.Default;
                }
            }
            else if (item.Count > conditionA && item.Count <= conditionB)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    item[i].GetComponent<Item>().type = Item.States.Rocket;
                }
            }
            else if (item.Count > conditionB && item.Count <= conditionC)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    item[i].GetComponent<Item>().type = Item.States.Bomb;
                }
            }
            else if (item.Count >= conditionC)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    item[i].GetComponent<Item>().type = Item.States.Discoball;
                }
            }

        }
        */
    }

    //Shuffle
    private void shuffle()
    {
        if(items == null)
        {

        }
    }


}
