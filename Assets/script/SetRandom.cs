using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 0~47の整数値をランダムに生成
public class SetRandom : MonoBehaviour
{
    public int SelectNo;
    public int fixNo; // 評価用にNoを固定
    // シーン初め
    public void Start()
    {
        this.SelectNo = Random.Range(0, 48);
        // this.SelectNo = fixNo;
    }
}
