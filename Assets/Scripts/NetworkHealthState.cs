using Unity.Netcode;
using UnityEngine;

public class NetworkHealthState : NetworkBehaviour
{
    public NetworkVariable<int> HealthPoint = new NetworkVariable<int>(); // Serveur uniquement doit changer la valeur
    public NetworkVariable<int> MaxHealthPoint = new NetworkVariable<int>(); // Valeur maximale de la santé
    public NetworkVariable<bool> IsDead = new NetworkVariable<bool>(false); // Indique si l'entité est morte


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            MaxHealthPoint.Value = 100; // Initialiser la santé maximale à 100
            HealthPoint.Value = MaxHealthPoint.Value; // Initialiser la santé actuelle à la santé maximale
        }
    }

}
