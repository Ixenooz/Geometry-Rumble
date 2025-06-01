using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour 
{
    [SerializeField] Player player;
    [SerializeField] private Transform spawnedObjectPrefab;

    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
        new MyCustomData {
            _int = 56,
            _bool = true,
        }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); // Doit etre initialisée dès que l'objet est crée et pas dans une Update

    public struct MyCustomData : INetworkSerializable {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }

    public override void OnNetworkSpawn()
    {
        // Debug lorsque la valeur change
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) => {
            Debug.Log(OwnerClientId + ";"  + newValue._int + " ; " + newValue._bool + "; " + newValue.message);
        };
    }


    public float moveSpeed = 3f; // Vitesse de déplacement du joueur
    private Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        // Uniquement si le joueur est l'owner du client
        if (!IsOwner) return;
        
        // Déplacement du joueur
        HandleMovement();

        if (Input.GetKeyDown(KeyCode.T)) {
            /**
            TestServerRpc(new ServerRpcParams());
            TestClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong>{ 1 } } });// Envoi depuis le serveur uniquement aux clients d'id 1
            randomNumber.Value = new MyCustomData {
                _int = 10,
                _bool = false,
                message = "All your base are belong to us"
            };
            **/
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            RequestShootServerRpc();
        }
        
    }

    private void HandleMovement() {
        // Récupération des inputs
        float moveX = 0f;
        float moveY = 0f;

        if (Input.GetKey(KeyCode.W)) // Avancer (haut)
        {
            moveY = 1f;
        }
        if (Input.GetKey(KeyCode.S)) // Reculer (bas)
        {
            moveY = -1f;
        }
        if (Input.GetKey(KeyCode.A)) // Gauche
        {
            moveX = -1f;
        }
        if (Input.GetKey(KeyCode.D)) // Droite
        {
            moveX = 1f;
        }

        // Normalisation du vecteur pour éviter les déplacements plus rapides en diagonale
        Vector2 movement = new Vector2(moveX, moveY).normalized;

        // Application du mouvement au Rigidbody2D
        rb.linearVelocity = movement * moveSpeed;
    }

    [ServerRpc]
    private void TestServerRpc(ServerRpcParams serverRpcParams) {
        Debug.Log("TestServerRpc " + OwnerClientId + "; " + serverRpcParams.Receive.SenderClientId); // Envoyer depuis un client au serveur
    }

    [ServerRpc]
private void RequestShootServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Instancier la balle sur le serveur
        Transform bulletTransform = Instantiate(spawnedObjectPrefab, transform.position, Quaternion.identity);
        NetworkObject bulletNetworkObject = bulletTransform.GetComponent<NetworkObject>();
        bulletNetworkObject.Spawn(true); // Synchroniser la balle avec tous les clients

        // Définir l'ID du client qui a tiré la balle via la NetworkVariable
        BulletNetwork bullet = bulletTransform.GetComponent<BulletNetwork>();
        bullet.shooterClientId.Value = OwnerClientId;
    }

    [ClientRpc]
    private void TestClientRpc(ClientRpcParams clientRpcParams) {
        Debug.Log("TestClientRpc "); // Envoyer depuis le serveur aux clients
    }
}
