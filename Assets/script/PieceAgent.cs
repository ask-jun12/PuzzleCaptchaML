using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

// エージェント
public class PieceAgent : Agent
{
    GameObject pieces;
    GameObject Target;
    Rigidbody2D rBody;
    SetPosition SPscr;
    private float[] piecePosX = new float[48];
    private float[] piecePosY = new float[48];
    public Vector3 firstPos; // 可動ピースの初期位置(SPscr)
    bool isFirst; // 1回目のエピソードかどうか

    // シーン初め
    void Start()
    {
        // Rigidbody2Dコンポーネントの取得
        this.rBody = GetComponent<Rigidbody2D>();

        // SetPosition.csから配列PiecePosX・Yを取得
        this.pieces = GameObject.Find("Pieces");
        this.SPscr = pieces.GetComponent<SetPosition>();
        this.piecePosX = SPscr.piecePosX;
        this.piecePosY = SPscr.piecePosY;
        this.firstPos = SPscr.firstPos;

        // Targetオブジェクトの取得
        this.Target = GameObject.Find("Target");

        this.isFirst = true;
    }

    GameObject piece;
    SpriteRenderer component;
    SpriteRenderer componentPiece;
    SetRandom SRscr;
    ProcessPiece PPscr;
    private int SelectNo;

    // エピソード初め
    public override void OnEpisodeBegin()
    {
        // SetRandomScr.csから変数SelectNoを取得
        this.SRscr = pieces.GetComponent<SetRandom>();
        this.SelectNo = SRscr.SelectNo;

        // piecesの子要素取得.
        for (int i=0; i<48; i++){
            GameObject child = pieces.transform.GetChild(i).gameObject;
            this.PPscr = child.GetComponent<ProcessPiece>();
            // PieceNoの取得.
            int PieceNo = PPscr.PieceNo;
            if (PieceNo == this.SelectNo)
            {
                child.transform.position = firstPos;
            }
            else
            {
                child.transform.position = new Vector3(this.piecePosX[PieceNo], this.piecePosY[PieceNo], 0);
            }
        }

        // エージェントの位置と速度を初期化する
        // this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        // 位置を可動ピースの初期位置に設定
        this.transform.position = firstPos;
        // Targetの位置を設定
        this.Target.transform.position = new Vector3(this.piecePosX[SelectNo], this.piecePosY[SelectNo], 0);

        // SpriteRendererのSpriteに可動ピースのSpriteを代入
        // 可動ピースのSpriteをNoneに変更.
        this.component = GetComponent<SpriteRenderer>();
        this.piece = GameObject.Find($"piece_{SelectNo}");
        this.componentPiece = piece.GetComponent<SpriteRenderer>();
        var componentNone = component.sprite;
        this.component.sprite = componentPiece.sprite;
        this.componentPiece.sprite = componentNone;
    }

    // 環境情報の取得
    public override void CollectObservations(VectorSensor sensor)
    {
        // ターゲットとエージェントの位置
        sensor.AddObservation(Target.transform.position);
        sensor.AddObservation(this.transform.position);

        // エージェントの速度
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.y);
    }

    public float forceMultiplier = 100f; // 加える力の係数
    public float previousDistance = 0;

    // 行動, 報酬の受け取り
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // 行動, size = 2
        // x, y軸方向に力を加える
        Vector2 controlSignal = Vector2.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.y = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * forceMultiplier);

        // 報酬
        // ターゲットとの距離を測る
        var nowDistance = Vector3.Distance(this.transform.position, Target.transform.position);
        // 前回より近づいた分報酬
        AddReward((previousDistance - nowDistance) * 0.1f);
        this.previousDistance = nowDistance;

        //時間の経過とともにお叱り
        AddReward(-0.01f);

        // ターゲットに到達したとき
        if (nowDistance < 1f)
        {
            // 報酬を受け取りエピソードを終了する
            AddReward(10.0f);
            Debug.LogError("success");
            EpisodeReset();
            EndEpisode();
        }
        // 範囲外でエピソードを終了
        else if ((this.transform.position.x < -5f) || (this.transform.position.x > 5f))
        {
            // エピソードを終了する
            AddReward(-3.0f);
            Debug.LogWarning("miss");
            EpisodeReset();
            EndEpisode();
        }
        else if ((this.transform.position.y < -4f) || (this.transform.position.y > 4f))
        {
            // エピソードを終了する
            AddReward(-3.0f);
            Debug.LogWarning("miss");
            EpisodeReset();
            EndEpisode();
        }
    }

    // エピソード終わり
    public void EpisodeReset()
    {
        // 可動ピースのSpriteを元に戻す
        // SpriteRendererのSpriteをNoneに変更.
        var componentNone = componentPiece.sprite;
        this.componentPiece.sprite = component.sprite;
        this.component.sprite = componentNone;
        this.pieces.GetComponent<SetRandom>().Start();

        // 最初のエピソードのみCSV出力
        if (isFirst == true)
        {
            GameObject writeposition = GameObject.Find("Pieces");
            writeposition.GetComponent<WritePosition>().enabled = false;
            this.isFirst = false;
        }
    }

    // Heuristicモード
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}