using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ピースに関する処理
public class ProcessPiece : MonoBehaviour
{
    public int PieceNo; // 各ピースのナンバー

    // シーン初め
    void Start()
    {
        // pieceのNo.を取得
        string num = Regex.Replace (transform.name, @"[^0-9]", "");
        this.PieceNo = int.Parse(num);
    }
}