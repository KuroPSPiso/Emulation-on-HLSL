using UnityEngine;

public partial class test : MonoBehaviour
{
    void Update()
    {
        this.transform.position += Vector3.up * speed * Time.deltaTime;
    }
}
