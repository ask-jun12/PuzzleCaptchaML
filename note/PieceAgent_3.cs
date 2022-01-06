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
        this.pieces.GetComponent<WriteCsv>().enabled = false;

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
    private float prevTime;

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

        // 前ステップの位置と時間
        this.prevPos = this.startPos;
        this.prevTime = this.sTime;

        // スタート位置とTarget位置の距離を計測
        var dis = Mathf.Abs(Vector3.Distance(this.startPos, this.Target.transform.position));
        // 始筆・送筆・終筆の時間・速度を設定
        if (dis <= 3.712)
        {
            this.sTime = 0.14f + Random.Range(-0.02f, 0.02f);
            this.mTime = this.sTime + 0.27f + Random.Range(-0.05f, 0.05f);
            this.gTime = this.mTime + 0.93f + Random.Range(-0.2f, 0.2f);
            this.gVel = 5.986f + Random.Range(-0.5f, 0.5f);
        }
        else if ((3.712 < dis) && (dis <= 6.872))
        {
            this.sTime = 0.21f + Random.Range(-0.014f, 0.014f);
            this.mTime = this.sTime + 0.403f + Random.Range(-0.046f, 0.046f);
            this.gTime = this.mTime + 1.372f + Random.Range(-0.255f, 0.255f);
            this.gVel = 10.636f + Random.Range(-0.12f, 0.12f);
        }
        else if ((6.872 < dis) && (dis <= 9.788))
        {
            this.sTime = 0.185f + Random.Range(-0.025f, 0.025f);
            this.mTime = this.sTime + 0.465f + Random.Range(-0.055f, 0.055f);
            this.gTime = this.mTime + 1.425f + Random.Range(-0.155f, 0.155f);
            this.gVel = 14.823f + Random.Range(-1.637f, 1.637f);
        }
        else if (9.788 < dis)
        {
            this.sTime = 0.2f + Random.Range(-0.02f, 0.02f);
            this.mTime = this.sTime + 0.54f + Random.Range(-0.05f, 0.05f);
            this.gTime = this.mTime + 1.68f + Random.Range(-0.2f, 0.2f);
            this.gVel = 18.727f + Random.Range(-0.5f, 0.5f);
        }
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

    // private Vector3 nowPos;
    public bool isDrag = false; // ドラッグ中かどうか
    public float timeAcc = 0; // サンプリング時のtimeElapsedを累積
    public float forceMultiplier; // 加える力の係数
    private float previousDistance = 0.0f;
    private float prevelocity = 0.0f;
    public float check;

    // 行動, 報酬の受け取り
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        this.timeAcc += Time.deltaTime;
        this.isDrag = true;
        // if ((this.sTime < this.timeAcc) && (this.timeAcc <= this.mTime))
        // {this.forceMultiplier = 40.0f;}
        // else
        // {this.forceMultiplier = 10.0f;}

        // 行動設定
        Vector2 controlSignal = Vector2.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.y = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * this.forceMultiplier); // x, y軸方向に力を加える

        // 報酬設定
        // ターゲットとの距離を計測
        var nowDistance = Vector3.Distance(this.transform.position, Target.transform.position);
        // エージェントが動いた距離, エージェントの速度を計測
        // var velocity = (Vector3.Distance(this.prevPos, this.transform.position)) / (this.timeAcc-this.prevTime);

        // 始筆間 0.191s 10ステップ
        if (this.timeAcc <= this.sTime)
        {
            // スタート位置との距離を計測
            // 始筆の時間中に移動距離が少ないほど得点
            if (Vector3.Distance(this.transform.position, this.startPos) < 0.05f)
            {AddReward(0.5f); Debug.Log("sarea");}
        }
        // 送筆間 0.421s 21ステップ
        else if ((this.sTime < this.timeAcc) && (this.timeAcc <= this.mTime))
        {
            // 前エピソードとの距離の差によってを得・減点
            AddReward((previousDistance - nowDistance) * 1.0f);
            // エージェントが動いた距離, エージェントの速度を計測
            var velocity = (Vector3.Distance(this.prevPos, this.transform.position)) / (this.timeAcc-this.prevTime);
            this.check = velocity;
            // 送筆の速度が（平均 ± 標準偏差）に近いほど得点
            AddReward((prevelocity - velocity) * 1.0f);
            // Debug.Log(prevelocity - velocity);
            if (this.timeAcc < (this.mTime - 0.1f))
            {
                AddReward(1.0f / (20.0f + Mathf.Abs(velocity-this.gVel)));
                if (Mathf.Abs(velocity-this.gVel) < 1.0f)
                {AddReward(1.0f); Debug.Log("debug 1");}
            }
            else
            {
                var vel = (Vector3.Distance(this.transform.position, this.startPos)) / (this.timeAcc-this.sTime);
                if (Mathf.Abs(vel-this.gVel) < 1.0f)
                {AddReward(10.0f); Debug.Log("debug 2");}
            }
            // ゴール位置の範囲内にいれば得点
            if (nowDistance < 0.5f)
            {AddReward(10.0f); Debug.Log("debug 3");}
            this.prevelocity = velocity;
        }
        // 終筆間 1.334s 67ステップ
        else if ((this.mTime < this.timeAcc) && (this.timeAcc <= this.gTime))
        {
            // 前エピソードとの距離の差によってを得・減点
            // AddReward((previousDistance - nowDistance) * 0.1f);
            // 終筆の時間中に移動距離が少ないほど得点
            if (nowDistance < 0.05f)
            {AddReward(0.1f); Debug.Log("garea");}
        }
        // 終筆後 3s 経過
        else if (this.timeAcc > (this.gTime + 3.0f))
        {
            // 制限時間を超えたら減点
            AddReward(-5.0f);
            Debug.LogWarning("time out miss");
            EpisodeReset();
            EndEpisode();
        }
        // その他
        else{
            AddReward(-0.01f);
            // 前エピソードとの距離の差によってを得・減点
            AddReward((previousDistance - nowDistance) * 1.0f);
            // ターゲットに到達すれば得点
            if (nowDistance < this.span)
            {
                if (Mathf.Abs(this.gTime-this.timeAcc) < 0.1f)
                {AddReward(30.0f); Debug.LogError("1st success");}
                else if (Mathf.Abs(this.gTime-this.timeAcc) < 0.5f)
                {AddReward(20.0f); Debug.LogError("2nd success");}
                else if (Mathf.Abs(this.gTime-this.timeAcc) < 1.0f)
                {AddReward(10.0f); Debug.LogError("3rd success");}
                else{AddReward(3.0f); Debug.LogError("normal success");}
                EpisodeReset();
                EndEpisode();
            }
            else{}
        }

        // パズルの範囲外で減点・終了
        if ((this.transform.position.x < -5f) || (this.transform.position.x > 5f))
        {
            AddReward(-5.0f);
            Debug.LogWarning("range out miss");
            EpisodeReset();
            EndEpisode();
        }
        else if ((this.transform.position.y < -4f) || (this.transform.position.y > 5f))
        {
            AddReward(-5.0f);
            Debug.LogWarning("range out miss");
            EpisodeReset();
            EndEpisode();
        }

        // 前ステップ用変数に保存
        this.previousDistance = nowDistance;
        this.prevPos = this.transform.position;
        this.prevTime = this.timeAcc;
    }

    // エピソード終わり
    public void EpisodeReset()
    {
        this.isDrag = false;
        this.timeAcc = 0;
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
            this.pieces.GetComponent<WriteCsv>().enabled = true;
        }
        // 2回目のエピソードの場合のみCSV出力する
        else if (this.isSecond == true)
        {
            this.pieces.GetComponent<WriteCsv>().enabled = false;
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