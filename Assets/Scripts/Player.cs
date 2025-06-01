using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] int HP;
    [SerializeField] string pseudo;

    // Fonction pour prendre des dégats
    public void takeDamage(int damage)
    {
        this.HP -= damage;
    }

    // Fonction pour récupérer des HP
    public void gainHP(int gain)
    {
        this.HP += gain;
    }
}
