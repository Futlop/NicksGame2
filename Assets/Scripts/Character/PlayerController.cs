using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Sprite sprite;
    [SerializeField] string name;

    const float offsetY = 0.3f;

    public event Action OnEncountered;
    public event Action<Collider2D> OnEnterTrainerView;
    private Vector2 input;
    private Character character;
    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        if(!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");


            // Removes diagonal movement
            if(input.x != 0)
            {
                input.y = 0;
            }

            if(input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }

        character.HandleUpdate();

        if(Input.GetKeyDown(KeyCode.Z))
            Interact();
    }

    void Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, offsetY, GameLayers.i.InteractableLayer);
        if(collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    private void OnMoveOver()
    {
        CheckForEncounters();
        CheckIfInTrainerView();
    }

    private void CheckForEncounters()
    {
        if(Physics2D.OverlapCircle(transform.position - new Vector3(0, 0.3f), 0.2f, GameLayers.i.GrassLayer) != null)
        {
            if(UnityEngine.Random.Range(1, 101) <= 10)
            {
                character.Animator.IsMoving = false;
                OnEncountered();
            }
        }
    }

    private void CheckIfInTrainerView()
    {
        var collider = Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.i.FOVLayer);
        if(collider != null)
        {
            character.Animator.IsMoving = false;
            OnEnterTrainerView?.Invoke(collider);
        }
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }
}
