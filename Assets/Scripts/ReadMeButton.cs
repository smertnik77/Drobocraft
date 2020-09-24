using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class ReadMeButton : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	public void OnPointerEnter(PointerEventData eventData)
	{
		transform.Find ("Text").GetComponent<Text> ().color = Color.black;
	}
	public void OnPointerExit (PointerEventData eventData) 
	{
		transform.Find ("Text").GetComponent<Text> ().color = Color.white;
		var pointer = new PointerEventData(EventSystem.current);
		ExecuteEvents.Execute(this.gameObject, pointer, ExecuteEvents.pointerUpHandler);
	}
	public void OnPointerDown(PointerEventData eventData)
	{
		transform.Find("Text").GetComponent<Text>().color = Color.black;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		EventSystem.current.SetSelectedGameObject(null);
	}
}
