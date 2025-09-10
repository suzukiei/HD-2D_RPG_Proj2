using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField,Header("歩きの速度"),Range(0,10)]
    private float Speed;

    [SerializeField, Header("ダッシュの速度"), Range(1, 5)]
    private float DashSpeed;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.LeftShift))
                {
                this.transform.Translate(Speed * DashSpeed *Time.deltaTime, 0, 0);
                }
            this.transform.Translate(Speed*Time.deltaTime, 0, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                this.transform.Translate(-(Speed * DashSpeed * Time.deltaTime), 0, 0);
            }
            this.transform.Translate(-(Speed * Time.deltaTime), 0, 0);
        }
        if (Input.GetKey(KeyCode.W))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                this.transform.Translate(0, 0, Speed * DashSpeed * Time.deltaTime);
            }
            this.transform.Translate(0, 0, Speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                this.transform.Translate(0, 0, -(Speed * DashSpeed * Time.deltaTime));
            }
            this.transform.Translate(0, 0, -(Speed * Time.deltaTime));
        }
    }
}
