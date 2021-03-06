using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Elevator : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 0.5f;
    [SerializeField] private float speedSmooth = 0.3f;
    [SerializeField] private float elevatorError = 0.001f;
    [SerializeField] private float innerDoorOpenDelay = 0.1f;
    [SerializeField] private float closeDelay = 10f;
    float closeTimer;

    [SerializeField] private int CurFloor = 1;
    [SerializeField] private Door[] floorDoors;
    [SerializeField] private Transform[] floorTransforms;
    [SerializeField] private Door innerDoor;

    bool working;
    bool moving;

    Vector3 moveTarget;
    Vector3 velocity;

    private void Start() {
        transform.position = floorTransforms[CurFloor].position;
        closeTimer = closeDelay;
    }

    private void Update() {
        if (moving)
            transform.position = Vector3.SmoothDamp(transform.position, moveTarget, ref velocity, speedSmooth, maxSpeed);
        if (floorDoors[CurFloor].isOpen && !working)
            closeTimer -= Time.deltaTime;
        if (closeTimer <= 0)
            StartCoroutine(CloseDoors());
    }

    public void MoveToFloor(int floor) {
        if (!working)
            StartCoroutine(MoveToFloorRoutine(floor));
    }

    public void IncrementFloor() {
        if (!working)
            StartCoroutine(MoveToFloorRoutine((CurFloor + 1) % floorDoors.Length));
    }

    IEnumerator MoveToFloorRoutine(int floor) {
        moveTarget = floorTransforms[floor].position;
        working = true;
        if (CurFloor == floor) {
            floorDoors[floor].Open();
            yield return new WaitForSeconds(innerDoorOpenDelay);
            innerDoor.Open();
        } else {
            if (innerDoor.isOpen) {
                innerDoor.Close();
                yield return new WaitForSeconds(innerDoorOpenDelay / 2);
                if (floorDoors[CurFloor].isOpen) {
                    floorDoors[CurFloor].Close();
                    yield return new WaitForSeconds(innerDoorOpenDelay);
                }
            }
            moving = true;
            moveTarget = floorTransforms[floor].position;
            yield return new WaitUntil(() => Vector3.Distance(transform.position, moveTarget) < elevatorError);
            moving = false;
            floorDoors[floor].Open();
            yield return new WaitForSeconds(innerDoorOpenDelay);
            innerDoor.Open();
            CurFloor = floor;
        }
        working = false;
    }

    IEnumerator CloseDoors() {;
        closeTimer = closeDelay;
        working = true;
        if (floorDoors[CurFloor].isOpen)
            floorDoors[CurFloor].Close();
        if (innerDoor.isOpen)
            innerDoor.Close();
        yield return new WaitForSeconds(innerDoorOpenDelay);
        working = false;
    }
}