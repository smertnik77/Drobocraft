using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class ReadMeButton : MonoBehaviour, ISelectHandler , IPointerEnterHandler,IPointerExitHandler
{
	// When highlighted with mouse.
	public void OnPointerEnter(PointerEventData eventData)
	{
		// Do something.
		//Debug.Log("<color=red>Event:</color> Completed mouse highlight.");
		transform.Find ("Text").GetComponent<Text> ().color = Color.black;
	}
	// When selected.
	public void OnSelect(BaseEventData eventData)
	{
		// Do something.
		transform.Find ("Text").GetComponent<Text> ().color = Color.black;
	}
	public void OnPointerExit (PointerEventData eventData) 
	{
		//Debug.Log ("The cursor exited the selectable UI element.");
		transform.Find ("Text").GetComponent<Text> ().color = Color.white;
		UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject (null);
	}
}
