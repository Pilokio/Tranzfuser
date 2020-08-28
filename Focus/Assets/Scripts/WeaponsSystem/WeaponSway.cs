using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{

    public float amount;
    public float maxAmount;
    public float smoothAmount;

    private Vector3 initialPosition;

    private PlayerController playerController = new PlayerController();

    // Start is called before the first frame update
    void Start()
    {
        playerController = PlayerManager.Instance.Player.GetComponent<PlayerController>();
        initialPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float movementX = -playerController.GetLookDirection().x * amount;
        float movementY = -playerController.GetLookDirection().y * amount;

        movementX = Mathf.Clamp(movementX, -maxAmount, maxAmount);
        movementY = Mathf.Clamp(movementY, -maxAmount, maxAmount);

        Vector3 finalPosition = new Vector3(movementX, movementY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPosition, Time.deltaTime * smoothAmount);
    }
}
