using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    public UnityEngine.UI.Text interactionText;
    public GameObject dialogPanel;
    public Transform canvasParent;

    Camera camera;

    List<TownResidentActor> residents;
    Dictionary<TownResidentActor, UnityEngine.UI.Text> residentNames;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        residents = new List<TownResidentActor>();
        residentNames = new Dictionary<TownResidentActor, Text>();
    }

    public void RegisterResident(TownResidentActor resident)
    {
        residents.Add(resident);
        
        UnityEngine.UI.Text text = Instantiate((GameObject)Resources.Load("ResidentNameTag"), Vector3.zero, Quaternion.identity).GetComponent<UnityEngine.UI.Text>();
        text.gameObject.name = resident.data.name;
        text.text = resident.data.name;
        text.transform.SetParent(canvasParent);

        residentNames.Add(resident, text);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;
            LayerMask mask = LayerMask.GetMask("InteractionTrigger");
            Physics.Raycast(new Ray(camera.transform.position, camera.transform.forward), out hit, mask);

            Debug.Log(hit.collider.gameObject.name);

            if (hit.distance < 5)
            {
                hit.collider.SendMessageUpwards("RecieveInteraction", SendMessageOptions.DontRequireReceiver);
            }
        }
        foreach(KeyValuePair<TownResidentActor, UnityEngine.UI.Text> kvp in residentNames)
        {
            Vector3 screenPt = camera.WorldToScreenPoint(kvp.Key.transform.position + Vector3.up * 2);
            kvp.Value.rectTransform.position = screenPt;
            float opacity = 1f - Mathf.Clamp01((transform.position - kvp.Key.transform.position).sqrMagnitude / 300f);
            kvp.Value.color = new Color(1, 1, 1, screenPt.z > 0f ? opacity : 0f);
        }
    }

    public void successfulInteraction (string response)
    {
        dialogPanel.SetActive(true);
        interactionText.text = response;
    }

    public void endInteraction ()
    {
        dialogPanel.SetActive(false);
    }


}
