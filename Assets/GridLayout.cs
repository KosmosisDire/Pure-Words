using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.StackLayout;

[RequireComponent(typeof(StackLayout))]
[ExecuteAlways]
public class GridLayout : MonoBehaviour
{
    public StackLayout rowPrefab;
    public int columns;
    public bool updateInEditMode = false;
    public List<StackLayout> rows = new List<StackLayout>();
    public List<GameObject> elements = new List<GameObject>();
    public bool autoSetColumnCount = true;

    public void AddItem(GameObject item)
    {
        StackLayout row;
        int firstOpenRow = -1;
        bool placedInSoonestSpotAlready = false;

        for(int i = 0; i < rows.Count; i++)
        {
            if(rows[i] == null)
            {
                rows.RemoveAt(i);
                i--;
            }
        }


        foreach (var r in rows)
        {
            if(r.elements.Contains(item.GetComponent<Element>())) placedInSoonestSpotAlready = true;

            if (r.elements.Count < columns)
            {
                firstOpenRow = rows.IndexOf(r);
                Debug.Log("Found open row: " + firstOpenRow);
                break;
            }
            else
            {
                if(r.elements.Count > columns)
                {
                    Debug.Log("Row " + rows.IndexOf(r) + " is too long, removing elements");
                    r.elements.GetRange(columns, r.elements.Count - columns).ForEach(e => e.transform.SetParent(transform, true));
                    r.elements.RemoveRange(columns, r.elements.Count - columns);
                }
            }
        }

        if(placedInSoonestSpotAlready && rows.Count != 0) 
        {
            Debug.Log("Placed in soonest spot already");
            return;
        }

        if(firstOpenRow != -1)
        {
            row = rows[firstOpenRow];
        }
        else
        {
            row = Instantiate(rowPrefab);
            row.transform.SetParent(transform, true);
            rows.Add(row);
        }

        if(item.GetComponent<Element>() == null)
        {
            item.AddComponent<Element>();
        }

        item.transform.position = row.transform.position;
        item.transform.SetParent(row.transform, true);
        row.elements.Add(item.GetComponent<Element>());

        if(!elements.Contains(item))
        {
            elements.Add(item);
        }
    }

    public void RemoveItem(GameObject item, bool delete = true)
    {
        var element = item.GetComponent<Element>();
        if(element == null) return;
        var row = element.transform.parent.GetComponent<StackLayout>();
        if(row == null) return;
        row.elements.Remove(element);
        elements.Remove(item);
        if(delete)
        {
            Destroy(item);
        }

        UpdateGridOrder();
    }

    public void UpdateGridOrder()
    {
        for(int i = 0; i < elements.Count; i++)
        {
            if(elements[i] == null)
            { 
                elements.RemoveAt(i);
                i--;
                continue;
            }
            AddItem(elements[i]);
        }
    }

    public void ClearGrid()
    {
        foreach(var r in rows)
        {
            for(int i = 0; i < r.elements.Count; i++)
            {
                var e = r.elements[i];
                r.elements.RemoveAt(i);
                i--;
                Destroy(e.gameObject);
            }
        }
    }

    void Update()
    {
        if(updateInEditMode && !Application.isPlaying)
        {
            UpdateGridOrder();
        }

        if(autoSetColumnCount) 
        {
            if(rows.Count > 0)
                columns = Mathf.RoundToInt(rows[0].GetComponent<RectTransform>().rect.width / rows[0].GetComponent<RectTransform>().rect.height);
        }
    }
}
