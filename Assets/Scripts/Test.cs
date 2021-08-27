using UnityEngine;

public class Test : MonoBehaviour
{
    int[] array = new int[4];
    int index = 0;

    private void Awake()
    {
        Debugger.Ins.Enable();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            array[index++] = index + 1;
            Debug.Log(index);
        }
    }
}
