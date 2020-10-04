using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLogic : MonoBehaviour
{
    Animator animator;

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
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OpenDoor();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        CloseDoor();
    }
}
