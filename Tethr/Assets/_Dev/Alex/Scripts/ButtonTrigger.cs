using UnityEngine.UI;
using UnityEngine;

public class ButtonTrigger : MonoBehaviour
{
    Button button;

    private void Start()
    {
        button = this.GetComponent<Button>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collided with " + collision.gameObject.name);
        button.onClick.Invoke();
    }
}
