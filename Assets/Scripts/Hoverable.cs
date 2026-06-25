using UnityEngine;

public class Hoverable : MonoBehaviour
{
	public bool mouseIsOver = false;
    
    void OnMouseEnter() { mouseIsOver = true; }

	void OnMouseExit() { mouseIsOver = false; }
}