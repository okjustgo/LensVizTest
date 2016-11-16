using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GazeDetect : MonoBehaviour
{

    public bool isAnchor;
    public bool isMenu;

    // Use this for initialization
    void Start()
    {

    }
    void OnGazeEnter()
    {
        //Debug.Log("GazeEnter Me " + this.name);
        var select = gameObject.GetComponent<Selectable>();
        if (select != null)
        {
            select.Select();
        }
        else
        {
            Debug.Log("Null select object on " + this.name);
        }

        if (isAnchor)
        {
            var subMenu = gameObject.transform.FindChild("SubMenu");
            if (subMenu != null)
            {
                subMenu.localScale = Vector3.one;
                var collider = gameObject.GetComponent<BoxCollider>();
                collider.center = new Vector3(5f, -5f, 0f);
                collider.size = new Vector3(20f, 20f, 0.75f);
            }
        }

    }

    void OnGazeLeave()
    {
        //Debug.Log("GazeLeave Me " + this.name);
        GameObject myEventSystem = GameObject.Find("EventSystem");
        myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);

        if (!isMenu) {
            return;
        }

        if (!isAnchor)
        {
            this.transform.parent.parent.SendMessage("OnGazeLeave");
        }
        else
        {
            var gazeOrigin = Camera.main.transform.position;
            var gazeDirection = Camera.main.transform.forward;
            var gazeRotation = Camera.main.transform.rotation;

            RaycastHit[] hits;
            hits = Physics.RaycastAll(gazeOrigin, gazeDirection, 50.0f);

            bool foundChart = false;
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.transform.name == this.name)
                {
                    foundChart = true;
                }
            }

            //if (foundChart)
            //{
            //    Debug.Log("Found Chart " + this.name);
            //}
            //else
            //{
            //    Debug.Log("Not Found Chart" + this.name);
            //}

            if (!foundChart)
            {
                var subMenu = gameObject.transform.FindChild("SubMenu");
                if (subMenu != null)
                {
                    subMenu.localScale = Vector3.zero;
                    var collider = gameObject.GetComponent<BoxCollider>();
                    collider.center = new Vector3(0f, 0f, 0f);
                    collider.size = new Vector3(10f, 10f, 0.75f);
                }
            }
        }
    }


    // Update is called once per frame
    void Update()
    {

    }
}
