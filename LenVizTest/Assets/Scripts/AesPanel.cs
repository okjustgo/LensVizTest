using System;
using UnityEngine;
using UnityEngine.UI;

public class AesPanel : MonoBehaviour {
    public GameObject Graph = null;

    private bool _dropdownsValuesSet = false;

    private Dropdown xDropdown;
    private Dropdown yDropdown;
    private Dropdown zDropdown;
    private Dropdown colorDropdown;

    void Toggle()
    {
        var canvas = this.gameObject.transform.FindChild("Canvas");
        canvas.gameObject.SetActive(!canvas.gameObject.activeInHierarchy);
    }

    // Use this for initialization
    void Start ()
    {
        xDropdown = GetComponentInChildren<Transform>().Find("Canvas/Values/X Drop").gameObject.GetComponent<Dropdown>();
        yDropdown = GetComponentInChildren<Transform>().Find("Canvas/Values/Y Drop").gameObject.GetComponent<Dropdown>();
        zDropdown = GetComponentInChildren<Transform>().Find("Canvas/Values/Z Drop").gameObject.GetComponent<Dropdown>();
        colorDropdown = GetComponentInChildren<Transform>().Find("Canvas/Values/Color Drop").gameObject.GetComponent<Dropdown>();

        xDropdown.ClearOptions();
        yDropdown.ClearOptions();
        zDropdown.ClearOptions();
        colorDropdown.ClearOptions();
    }
	
	// Update is called once per frame
	void Update ()
	{
	    if (Graph != null)
	    {
            var hgd = Graph.GetComponent<Graph>().hgd;
            if (!_dropdownsValuesSet && Graph != null && hgd != null)
            {
                xDropdown.AddOptions(hgd.ColumnNames);
                yDropdown.AddOptions(hgd.ColumnNames);
                zDropdown.AddOptions(hgd.ColumnNames);
                colorDropdown.AddOptions(hgd.ColumnNames);

                xDropdown.value = hgd.ColumnNames.IndexOf(hgd.Aesthetics["x"]);
                yDropdown.value = hgd.ColumnNames.IndexOf(hgd.Aesthetics["y"]);
                zDropdown.value = hgd.ColumnNames.IndexOf(hgd.Aesthetics["z"]);
                colorDropdown.value = hgd.ColumnNames.IndexOf(hgd.Aesthetics["color"]);

                _dropdownsValuesSet = true;
            }
        }
	}

    void UpdateAesthetic(string aesthetic)
    {
        var hgd = Graph.GetComponent<Graph>().hgd;
        var newValue = string.Empty;
        switch (aesthetic)
        {
            case "x":
                newValue = hgd.ColumnNames[xDropdown.value];
                break;
            case "y":
                newValue = hgd.ColumnNames[yDropdown.value];
                break;
            case "z":
                newValue = hgd.ColumnNames[zDropdown.value];
                break;
            case "color":
                newValue = hgd.ColumnNames[colorDropdown.value];
                break;
            default:
                throw new ArgumentException(string.Format("Unsupported aesthetic '{0}'", aesthetic));
        }

        hgd.SetAesthetic(aesthetic, newValue);
        Graph.GetComponent<Graph>().shouldRender = true;
    }

    // Update is called once per frame
    void Awake()
    {

    }
}
