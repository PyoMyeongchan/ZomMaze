using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;



public class DoorManager : MonoBehaviour
{
    public float openDoor = 180;
    private float closeDoor = 0;
    private float smooth = 2.0f;


    private bool isOpen = false;

    void Update()
    {
        DoorOpen();
    }



    public void ChangeDoorOpen()
    {
        isOpen = !isOpen;
    }

    void DoorOpen()
    {
        if (isOpen)
        {

            // 애니메이션으로도 가능
            // 문열리고 닫히는 사운드 넣기
            Quaternion rotation = Quaternion.Euler(-90, openDoor, 0);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, rotation, smooth * Time.deltaTime);


        }
        else
        {
            Quaternion rotation2 = Quaternion.Euler(-90, closeDoor, 0);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, rotation2, smooth * Time.deltaTime);

        }

    }






}
