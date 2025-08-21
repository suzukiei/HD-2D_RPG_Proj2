using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMove : MonoBehaviour
{
    Rigidbody rigidbody;
    [SerializeField]float speed = 10;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = this.GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        float Power = speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.A))
            rigidbody.AddForce(new Vector3(Power, 0));
        if (Input.GetKey(KeyCode.D))
            rigidbody.AddForce(new Vector3(-Power, 0));
        if (Input.GetKey(KeyCode.W))
            rigidbody.AddForce(new Vector3(0,0,Power));
        if (Input.GetKey(KeyCode.S))
            rigidbody.AddForce(new Vector3(0,0,-Power));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            SceneManager.LoadScene("turnTestScene");

        }
    }

}
