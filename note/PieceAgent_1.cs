using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public Vector3 startPos; // 可動ピースの初期位置(SPscr)
    private float span; // ピースがワープする距離(SPscr)
    bool isFirst; // 1回目のエピソードかどうか
    bool isSecond; // 2回目のエピソードかどうか

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
        this.startPos = SPscr.startPos;
        this.span = SPscr.span;

        // Targetオブジェクトの取得
        this.Target = GameObject.Find("Target");

        // WritePosition.csをオフにする
        this.pieces.GetComponent<WritePosition>().enabled = false;

        this.isFirst = true;
        this.isSecond = false;
    }

    GameObject piece;
    SpriteRenderer component;
    SpriteRenderer componentPiece;
    SetRandom SRscr;
    ProcessPiece PPscr;
    private int SelectNo;
    private Vector3 prevPos;
    private float sTime;
    private float mTime;
    private float gTime;
    private float gVel;

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
                child.transform.position = this.startPos;
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
        this.transform.position = this.startPos;
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

        this.prevPos = this.startPos;

        // 始筆・送筆・終筆の時間・速度を設定
        this.sTime = 0.180f + Random.Range(-0.043f, 0.043f);
        this.mTime = this.sTime + 0.713f + Random.Range(-0.039f, 0.039f);
        this.gTime = this.mTime + 1.903f + Random.Range(-0.243f, 0.243f);
        this.gVel = 11.720f + Random.Range(-1.470f, 1.470f);
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

    private Vector3 nowPos;
    public bool isDrag = false; // ドラッグ中かどうか
    private float timeAcc = 0; // サンプリング時のtimeElapsedを累積
    public float forceMultiplier; // 加える力の係数
    private float previousDistance = 0.0f;

    // 行動, 報酬の受け取り
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        this.timeAcc += (Time.deltaTime/2);
        this.nowPos = this.transform.position; // 座標計測
        this.isDrag = true;

        // 行動設定
        // x, y軸方向に力を加える
        Vector2 controlSignal = Vector2.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.y = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * this.forceMultiplier);

        // 報酬設定
        // ターゲットとの距離・エージェントの速度を計測
        var nowDistance = Vector3.Distance(this.transform.position, Target.transform.position);
        var dis = Mathf.Abs(Vector3.Distance(this.prevPos, this.nowPos));
        var velocity = dis / (this.timeAcc-this.sTime);

        if (this.timeAcc <= this.sTime) // 始筆
        {
            // スタート位置との距離を計測
            var sDis = Vector3.Distance(this.nowPos, this.startPos);
            // 始筆の時間中に移動距離が少ないほど得点
            if (Mathf.Abs(sDis) < 0.5f)
            {AddReward(0.01f);}
        }
        else if ((this.sTime < this.timeAcc) && (this.timeAcc <= this.mTime)) // 送筆
        {
            // 前エピソードとの距離の差によってを得・減点
            AddReward((previousDistance - nowDistance) * 1.0f);
            // 送筆の速度が（平均 ± 標準偏差）に近いほど得点
            AddReward(1.0f / (40.0f + Mathf.Abs(velocity-this.gVel)));
        }
        else if ((this.mTime < this.timeAcc) && (this.timeAcc <= this.gTime)) // 終筆
        {
            // AddReward(0.01f);
            // 前エピソードとの距離の差によってを得・減点
            AddReward((previousDistance - nowDistance) * 1.0f);
            // 終筆の時間中に移動距離が少ないほど得点
            if (nowDistance < 0.5f)
            {AddReward(0.01f);}
            // 終筆の時間の 1s 以内で終了
            if (Mathf.Abs(this.gTime-this.timeAcc) < 1.0f)
            {
                // ターゲットに到達すれば得点
                if (nowDistance < this.span)
                {
                    AddReward(30.0f);
                    Debug.LogError("success");
                    EpisodeReset();
                    EndEpisode();
                }
            }
        }
        else
        {
            // 前エピソードとの距離の差によってを得・減点
            AddReward((previousDistance - nowDistance) * 0.1f);
            // ターゲットに到達すれば得点
            if (nowDistance < this.span)
            {
                AddReward(0.1f);
                Debug.LogError("success");
                EpisodeReset();
                EndEpisode();
            }
            // 制限時間を超えたら減点
            // if (this.timeAcc > (this.gTime + 5.00f))
            // {
            // }
            AddReward(-3.0f);
            Debug.LogWarning("miss");
            EpisodeReset();
            EndEpisode();
        }

        // パズルの範囲外で減点・終了
        if ((this.transform.position.x < -5f) || (this.transform.position.x > 5f))
        {
            AddReward(-3.0f);
            Debug.LogWarning("miss");
            EpisodeReset();
            EndEpisode();
        }
        else if ((this.transform.position.y < -4f) || (this.transform.position.y > 4f))
        {
            AddReward(-3.0f);
            Debug.LogWarning("miss");
            EpisodeReset();
            EndEpisode();
        }

        this.previousDistance = nowDistance;
        this.prevPos = this.nowPos;
    }

    // エピソード終わり
    public void EpisodeReset()
    {
        this.timeAcc = 0;
        this.isDrag = false;
        // 可動ピースのSpriteを元に戻す
        // SpriteRendererのSpriteをNoneに変更.
        var componentNone = componentPiece.sprite;
        this.componentPiece.sprite = component.sprite;
        this.component.sprite = componentNone;
        this.pieces.GetComponent<SetRandom>().Start();

        // 1回目のエピソードではcsv出力しない
        if (this.isFirst == true)
        {
            this.isFirst = false;
            this.isSecond = true;
            this.pieces.GetComponent<WritePosition>().enabled = true;
        }
        // 2回目のエピソードの場合のみCSV出力する
        else if (this.isSecond == true)
        {
            this.pieces.GetComponent<WritePosition>().enabled = false;
            this.isSecond = false;
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