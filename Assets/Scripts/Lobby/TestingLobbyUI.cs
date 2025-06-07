using UnityEngine;
using UnityEngine.UI;

public class TestingLobbyUI : MonoBehaviour
{
    [SerializeField] private Button createGameBtn;
    [SerializeField] private Button joinGameBtn;

    private void Awake()
    {
        createGameBtn.onClick.AddListener(() => {
            
        });

        joinGameBtn.onClick.AddListener(() => {
            
        });
    }
}
