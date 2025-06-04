using UnityEngine;

public class PlayerUI : MonoBehaviour
{

    [SerializeField] RectTransform healthBar;

    void OnEnable()
    {
        GetComponent<NetworkHealthState>().HealthPoint.OnValueChanged += HealthChanged;
    }

    void OnDisable()
    {
        GetComponent<NetworkHealthState>().HealthPoint.OnValueChanged -= HealthChanged;
    }
    
    
    private void HealthChanged(int oldValue, int newValue)
    {
        healthBar.transform.localScale = new Vector3(newValue / 100f, 1f, 1f);

    }
}
