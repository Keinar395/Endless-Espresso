using UnityEngine;
using TMPro; // TextMeshPro kütüphanesi

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float destroyTime = 1.5f; // 1.5 saniye sonra yok olacak
    
    private TextMeshPro textMesh;
    private Color textColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null)
        {
            textColor = textMesh.color;
        }
        
        // Obje doğduktan belli bir süre sonra kendini yok etsin
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // Yazıyı yavaşça yukarı kaydır
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        // Yazının Alpha (Saydamlık) değerini yavaşça sıfıra düşür
        if (textMesh != null)
        {
            textColor.a -= (1f / destroyTime) * Time.deltaTime;
            textMesh.color = textColor;
        }
    }

    // Doğduğu anda bu fonksiyonla içine ne yazacağını söyleyeceğiz
    public void Setup(string amount)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
        textMesh.text = "+$" + amount;
    }
}