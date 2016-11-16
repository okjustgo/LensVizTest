using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuTap : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnSelect()
    {
        Debug.Log("selected " + this.name);
        var dropDown = gameObject.GetComponent<Dropdown>();
        dropDown.Show();

        var dropList = gameObject.transform.FindChild("Dropdown List");
        var canvas3 = dropList.GetComponent<Canvas>();
        canvas3.sortingOrder = 0;
        //dropDown.Select();
        //dropDown.OnPointerClick(new PointerEventData(EventSystem.current));

        //GameObject myEventSystem = GameObject.Find("EventSystem");
        //myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }
}
