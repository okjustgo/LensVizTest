using UnityEngine;
using UnityEngine.UI;

public class MenuTap : MonoBehaviour
{
    public string Aesthetic;

    private Dropdown _dropdown;

	// Use this for initialization
	void Start () {
        _dropdown = gameObject.GetComponent<Dropdown>();
        _dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChangedHandler(_dropdown);
        });

    }

    // Update is called once per frame
    void Update () {
	}

    void OnSelect()
    {
        _dropdown.Show();

        Debug.Log("current " + Aesthetic + " is " + _dropdown.value);

        var dropList = gameObject.transform.FindChild("Dropdown List");
        var canvas3 = dropList.GetComponent<Canvas>();
        canvas3.sortingOrder = 0;

        //dropDown.Select();
        //dropDown.OnPointerClick(new PointerEventData(EventSystem.current));

        //GameObject myEventSystem = GameObject.Find("EventSystem");
        //myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    private void DropdownValueChangedHandler(Dropdown target)
    {
        SendMessageUpwards("UpdateAesthetic", Aesthetic);
    }

    public void SetDropdownIndex(int index)
    {
        _dropdown.value = index;
    }
}
