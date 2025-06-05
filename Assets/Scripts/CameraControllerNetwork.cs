using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;

public class CameraControllerNetwork : NetworkBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera; // Reference to the Cinemachine camera
    [SerializeField] private AudioListener audioListener; // Reference to the AudioListener component

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            audioListener.enabled = true; // Enable AudioListener for the local player
            cinemachineCamera.Priority = 100; // Set the priority of the Cinemachine camera for the local player
        }
        else
        {
            cinemachineCamera.Priority = 0; // Disable camera for non-local players
        }
    }
}
