using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryController : MonoBehaviour
{
    // These are the GameObjects we need to assign to the player
    public GameObject Roller;
    public GameObject Head;
    public GameObject Neck;
    public GameObject Body;
    public GameObject BodyMesh;
    public GameObject MainCamera;

    public float SmoothingTime = 24.0f;
    public float SmoothingTimeMultiplier = 4.0f;
    public float MaxDistanceFromRoller = 0.65f;

    float BodySmoothingTime;
    float NewSmoothingTime;
    public float Thrust = 100.0f;
    public float MaxVelocity = 20f;
    public float HoverVelocity = 40f;
    public float MaxHoverVelocity = 15f;
    public float HoverEnergy = 100f;
    public float HoverEnergyRegenSpeed = 5.0f;
    public float MaxFlyHeight = 2.0f;
    public float GravityMultiplier = 2.0f;

    // our private variables
    private Rigidbody rb;
    private Vector3 rollerPosOffset;
    private Vector3 headPosOffset;
    private Vector3 neckPosOffset;
    private float distanceToGround;

    private Vector3 launchPoint;
    private bool lockY = false;
    
    // Start is called before the first frame update
    void Start()
    {
        // Get the rigidbody component off of the roller part of the player
        rb = Roller.GetComponent<Rigidbody>();

        if (rb)
        {
            Debug.Log("Rigidbody component found on " + Roller.name + "!");
        }
        else
        {
            Debug.Log("No rigidbody found: please verify that the " + Roller.name + " GameObject has a rigidbody!");
        }

        rollerPosOffset = Body.transform.position - Roller.transform.position;
        neckPosOffset = Neck.transform.position - BodyMesh.transform.position;
        headPosOffset = Head.transform.position - Neck.transform.position;

        BodySmoothingTime = SmoothingTime;

        Collider collider = Roller.GetComponent<Collider>();
        distanceToGround = collider.bounds.extents.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (IsGrounded())
        {
            launchPoint = Roller.transform.position;
        }

        // add a gravity multiplier that only affects the player
        rb.AddForce(new Vector3(0.0f, Physics.gravity.y * GravityMultiplier, 0.0f), ForceMode.Acceleration);

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(h, 0.0f, v);

        rb.AddForce(movement * Thrust);

        if (rb.velocity.magnitude >= MaxVelocity)
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, MaxVelocity);
        }

        float bodyDist = Vector3.Distance(Body.transform.position, Roller.transform.position);

        if (bodyDist > MaxDistanceFromRoller)
        {
            NewSmoothingTime = SmoothingTime * SmoothingTimeMultiplier;
        }
        else
        {
            NewSmoothingTime = SmoothingTime;
        }

        BodySmoothingTime = Mathf.Lerp(BodySmoothingTime, NewSmoothingTime, Time.deltaTime);

        // Hovering/Flying
        if (Input.GetKey(KeyCode.LeftControl))
        {
            rb.AddForce(Vector3.up * HoverVelocity, ForceMode.Force);

            if (rb.velocity.magnitude >= MaxHoverVelocity)
            {
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, MaxHoverVelocity);
            }

            float yDistance = Roller.transform.position.y - launchPoint.y;

            if (yDistance >= MaxFlyHeight)
            {
                rb.constraints = RigidbodyConstraints.FreezePositionY; 
            }

        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            rb.constraints = RigidbodyConstraints.None;
        }

        UpdateBodyAndHeadPosition();
    }

    void UpdateBodyAndHeadPosition()
    {
        // update the Body to the Roller's position
        Body.transform.position = Vector3.Slerp(Body.transform.position, Roller.transform.position + rollerPosOffset, BodySmoothingTime * Time.deltaTime);

        // update the neck's position to the body's MESH, not the BODY itself!
        Neck.transform.position = Vector3.Slerp(Neck.transform.position, BodyMesh.transform.position + neckPosOffset, BodySmoothingTime * 0.99f * Time.deltaTime);

        // update the head's position to the neck
        Head.transform.position = Vector3.Slerp(Head.transform.position, Neck.transform.position + headPosOffset, BodySmoothingTime * 0.98f * Time.deltaTime);
    }

    bool IsGrounded()
    {
        return Physics.Raycast(Roller.transform.position, -Vector3.up, distanceToGround + 0.5f);
    }
}
