using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ピースの位置を配列に格納
public class SetPosition : MonoBehaviour
{
    private Vector3 pos; //ピースのポジション用変数
    public float[] piecePosX = new float[48]; // ピースのX座標
    public float[] piecePosY = new float[48]; // ピースのY座標
    public Vector3 firstPos = new Vector3(-4f, -3.8f, 0); // 可動ピースの初期位置
    public float span; // ピースがワープする距離
    
    // シーン初め
    void Start()
    {
        for ( int i=0; i<48; i++){
            GameObject child = transform.GetChild(i).gameObject;
            // 座標取得
            Transform childTransform = child.transform;
            this.pos = childTransform.localPosition;
            this.piecePosX[i] = pos.x;
            this.piecePosY[i] = pos.y;
        }
    }
}
