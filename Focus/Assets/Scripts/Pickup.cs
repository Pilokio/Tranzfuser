using UnityEngine;

public class Pickup : MonoBehaviour
{
    enum PickupType { HP, AMMO };
    [SerializeField] PickupType Type = PickupType.HP;

    [SerializeField] AmmunitionType ammunition = new AmmunitionType(AmmunitionType.AmmoType.Pistol, 0);

    [SerializeField] int HPAmount = 0;

    [SerializeField] float RotationSpeed = 1.0f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * RotationSpeed);
    }



    private void OnTriggerEnter(Collider other)
    {
       
        if (other.transform.tag == "Player")
        {
            Debug.Log("The player is touching the object");

            if (other.gameObject.GetComponent<CharacterStats>() == null)
            {
                Debug.Log("The player does not have the character stats component");
            }
            else
            {
                Debug.Log(transform.name + " has been picked up by the player");

                if (Type == PickupType.AMMO)
                {
                    other.GetComponent<CharacterStats>().AddAmmo(ammunition);
                }
                else if (Type == PickupType.HP)
                {
                    other.GetComponent<CharacterStats>().RestoreHealth(HPAmount);
                }

                Destroy(this.gameObject);
            }
        }
    }
}
