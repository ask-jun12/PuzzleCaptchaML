using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// 座標ログをcsv出力
public class WritePosition : MonoBehaviour
{
    GameObject piece; // 可動ピース
    SetPosition SPscr;
    SetRandom SRscr;
    private Vector3 firstPos; // 可動ピースの初期位置
    private Vector3 goalPos; // 可動ピースの目標位置

    // シーン初め
    void Start()
    {
        this.piece = GameObject.Find("Agent");

        this.SPscr = this.GetComponent<SetPosition>();
        this.firstPos = SPscr.firstPos;

        this.SRscr = this.GetComponent<SetRandom>();
        int PieceNo = SRscr.SelectNo;

        this.goalPos = new Vector3(SPscr.piecePosX[PieceNo], SPscr.piecePosY[PieceNo], 0);
    }

    private float timeElapsed;
    public float timeOut; // サンプリング間隔
    public float dis; // 始筆と終筆距離

    // 毎フレーム
    void Update()
    {
        timeElapsed += Time.deltaTime;

        // サンプリング間隔
        if(timeElapsed >= timeOut) {
            StreamWriter swLEyeLog;
            FileInfo fiLEyeLog = new FileInfo("C:/Users/jun12/Desktop/kenkyu/PuzzleCaptchaML/csv/position.csv");
            swLEyeLog = fiLEyeLog.AppendText();
            swLEyeLog.Write(Time.time); swLEyeLog.Write(", ");

            swLEyeLog.Write(piece.transform.position.x); swLEyeLog.Write(", ");

            swLEyeLog.WriteLine(piece.transform.position.y);

            swLEyeLog.Flush();
            swLEyeLog.Close();

            StreamWriter swLEyeLogFirst; // 始め
            float disFirst = (piece.transform.position - firstPos).sqrMagnitude;
            StreamWriter swLEyeLogGoal; // 終わり
            float disGoal = (piece.transform.position - goalPos).sqrMagnitude;
            StreamWriter swLEyeLogMiddle; // 途中

            if(disFirst < dis * dis){ // 始め
                FileInfo fiLEyeLogFirst = new FileInfo("C:/Users/jun12/Desktop/kenkyu/PuzzleCaptchaML/csv/position_first.csv");
                swLEyeLogFirst = fiLEyeLogFirst.AppendText();
                swLEyeLogFirst.Write(Time.time); swLEyeLogFirst.Write(", ");

                swLEyeLogFirst.Write(piece.transform.position.x); swLEyeLogFirst.Write(", ");

                swLEyeLogFirst.WriteLine(piece.transform.position.y);

                swLEyeLogFirst.Flush();
                swLEyeLogFirst.Close();
            }
            else if(disGoal < dis * dis){ // 終わり
                FileInfo fiLEyeLogGoal = new FileInfo("C:/Users/jun12/Desktop/kenkyu/PuzzleCaptchaML/csv/position_goal.csv");
                swLEyeLogGoal = fiLEyeLogGoal.AppendText();
                swLEyeLogGoal.Write(Time.time); swLEyeLogGoal.Write(", ");

                swLEyeLogGoal.Write(piece.transform.position.x); swLEyeLogGoal.Write(", ");

                swLEyeLogGoal.WriteLine(piece.transform.position.y);

                swLEyeLogGoal.Flush();
                swLEyeLogGoal.Close();
            }
            else { // 途中
                FileInfo fiLEyeLogMiddle = new FileInfo("C:/Users/jun12/Desktop/kenkyu/PuzzleCaptchaML/csv/position_middle.csv");
                swLEyeLogMiddle = fiLEyeLogMiddle.AppendText();
                swLEyeLogMiddle.Write(Time.time); swLEyeLogMiddle.Write(", ");

                swLEyeLogMiddle.Write(piece.transform.position.x); swLEyeLogMiddle.Write(", ");

                swLEyeLogMiddle.WriteLine(piece.transform.position.y);

                swLEyeLogMiddle.Flush();
                swLEyeLogMiddle.Close();
            }

            timeElapsed = 0.0f;
        }
    }
}
