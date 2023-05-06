using UnityEngine;

public class Item : MonoBehaviour
{

    public int ID;

    public enum States
    {
        Default,
        Rocket,
        Bomb,
        Tornado
    }

    void OnMouseDown()
    {
        if(true)
        {
            Destroy(gameObject);
        }
    }

    private States state;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void changeState(States newState)
    {
        state = newState;
        spriteRenderer.sprite = sprites[(int)state];
    }
    public bool popable { get; set; } //Will be replaced

}
