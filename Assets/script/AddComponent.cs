using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// コンポーネントの追加
public class AddComponent : MonoBehaviour
{
    // シーン初め
    void Start()
    {
        for (int i=0; i<48; i++){
            GameObject child = this.transform.GetChild(i).gameObject;
            // BoxCollider2Dコンポーネントを追加
            child.gameObject.AddComponent<BoxCollider2D>();
            BoxCollider2D collider_component = child.gameObject.GetComponent<BoxCollider2D>();
            collider_component.isTrigger = true; // isTriggerをオン

            // Rigidbody2Dコンポーネントを追加
            child.gameObject.AddComponent<Rigidbody2D>();
            Rigidbody2D rigidbody_component = child.gameObject.GetComponent<Rigidbody2D>();
            rigidbody_component.bodyType = RigidbodyType2D.Kinematic; // bodytypeをkinematicに
        }
    }
}