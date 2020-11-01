using UnityEngine;

public class DoorLogic : MonoBehaviour
{
    Animator animator;
    [SerializeField] bool isLocked = false;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void OpenDoor()
    {
        animator.SetBool("Open", true);
    }
    public void CloseDoor()
    {
        animator.SetBool("Open", false);
    }
    bool Unlocked(Collider2D collision)
    {
        if (isLocked)
        {
            IPocket pocket = collision.transform.GetComponent<IPocket>();
            bool isOpen = pocket.UseKey();
            isLocked = !isOpen;
            return isOpen;
        }
        return true;
    }
    bool DetectCharacter(Collider2D collision)
    {
        IMovable character = collision.transform.GetComponent<IMovable>();
        if (character != null) return true;
        else return false;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!DetectCharacter(collision)) return;
        if (Unlocked(collision)) OpenDoor();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!DetectCharacter(collision)) return;
        CloseDoor();
    }
}
