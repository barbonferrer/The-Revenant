using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] GameObject avatar;
    [SerializeField] GameObject ghost;
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    bool canSwitch = true;
    bool isGhost = false;
    private float horizontalInput;
    private Rigidbody2D bodyAvatar;
    private BoxCollider2D boxColliderAvatar;
    private Animator animAvatar;
    private Rigidbody2D bodyGhost;
    private BoxCollider2D boxColliderGhost;
    private Animator animGhost;

    private Rigidbody2D cBody;
    private BoxCollider2D cBoxCollider;
    private Animator cAnim;

    private float wallJumpCooldown;

    private void Awake()
    {
        //Grab references for rigidbody and animator from object
        bodyAvatar = avatar.GetComponent<Rigidbody2D>();
        animAvatar = avatar.GetComponent<Animator>();
        boxColliderAvatar = avatar.GetComponent<BoxCollider2D>();
        bodyGhost = ghost.GetComponent<Rigidbody2D>();
        animGhost = ghost.GetComponent<Animator>();
        boxColliderGhost = ghost.GetComponent<BoxCollider2D>();

        cBody = bodyAvatar;
        cBoxCollider = boxColliderAvatar;
        cAnim = animAvatar;
    }
    void Update()
    {
        //Check whether the player is releasing the left alt key
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            canSwitch = true;
        }

        //Read whether left alt is pressed and change the value of isGhost accordingly
        if (canSwitch && Input.GetKey(KeyCode.LeftAlt))
        {
            isGhost = !isGhost;
            Debug.Log("isGhost " + isGhost);
            if (!isGhost)
            {
                cBody = bodyAvatar;
                cBoxCollider = boxColliderAvatar;
                cAnim = animAvatar;
            }
            else
            {
                cBody = bodyGhost;
                cBoxCollider = boxColliderGhost;
                cAnim = animGhost;
            }
            canSwitch = false;
        }

        if (!isGhost)
        {
            characterController(avatar.transform);
        }
        else
        {
            characterController(ghost.transform);
        }
    }

    void characterController(Transform character)
    {

        //read the inputs and make character.transform.position change accordingly
        horizontalInput = Input.GetAxis("Horizontal");
        //Flip player when moving left-right
        if (horizontalInput > 0.01f)
            character.localScale = Vector3.one;
        else if (horizontalInput < -0.01f)
            character.localScale = new Vector3(-1, 1, 1);

        //Set animator parameters
        cAnim.SetBool("Running", horizontalInput != 0);
        cAnim.SetBool("Grounded", isGrounded());

        //Wall jump logic
        if (wallJumpCooldown > 0.2f)
        {
            cBody.velocity = new Vector2(horizontalInput * speed, cBody.velocity.y);
            if (onWall() && !isGrounded())
            {
                cBody.gravityScale = 0;
                cBody.velocity = Vector2.zero;
            }
            else
                cBody.gravityScale = 3;

            if (Input.GetKey(KeyCode.Space))
            {
                Jump(character);
            }
        }
        else
            wallJumpCooldown += Time.deltaTime;
    }

    bool canSwitchCharacter()
    {
        return true;
    }

    private void Jump(Transform character)
    {
        if (isGrounded())
        {
            cBody.velocity = new Vector2(cBody.velocity.x, jumpPower);
            cAnim.SetTrigger("Jump");
        }
        else if (onWall() && !isGrounded())
        {
            if (horizontalInput == 0)
            {
                cBody.velocity = new Vector2(-Mathf.Sign(character.localScale.x) * 10, 0);
                character.localScale = new Vector3(-Mathf.Sign(character.localScale.x), character.localScale.y, character.localScale.z);
            }
            else
                cBody.velocity = new Vector2(-Mathf.Sign(character.localScale.x) * 3, 6);

            wallJumpCooldown = 0;
        }
    }

    private bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(cBoxCollider.bounds.center, cBoxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }
    private bool onWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(cBoxCollider.bounds.center, cBoxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }
    public bool canAttack()
    {
        return horizontalInput == 0 && isGrounded() && !onWall();
    }
}
