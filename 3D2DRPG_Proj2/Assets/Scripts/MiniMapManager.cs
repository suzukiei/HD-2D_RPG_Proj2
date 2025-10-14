using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class MiniMapManager : MonoBehaviour
{
    [SerializeField, Header("Player")]
    private GameObject playerIcon;
    [SerializeField, Header("Enemy")]
    private List<GameObject> enemyIcon;
    [SerializeField, Header("Camera")]
    private GameObject cameratest;
    [SerializeField, Header("ターゲットポジション")]
    private Transform target;
    //private Vector3 targetPos;
    [SerializeField, Header("ターゲットセットフラグ")]
    private bool targetSetFlag = false;
    // Start is called before the first frame update

    [SerializeField, Header("Canvas")]
    private Canvas canvas;
    [SerializeField, Header("UI")]
    private GameObject image;

    //経路表示用
    [SerializeField,Header("更新間隔 (秒)")] private float updateInterval = 0.2f;
    private LineRenderer lineRenderer;
    private NavMeshPath navPath;
    private float timer;
    void Start()
    {
        targetSetFlag = false;
        //最初にプレイヤーアイコンを生成
        var objImage = Instantiate(image, this.transform.position, Quaternion.identity);
        objImage.transform.parent = playerIcon.transform;
        objImage.transform.localPosition = Vector3.zero;
        //カメラを生成
        var objCamera = Instantiate(cameratest, this.transform.position, Quaternion.identity);
        objCamera.transform.parent = playerIcon.transform ;
        //カメラをプレイヤーの位置にセット
        objCamera.transform.localPosition = new Vector3(0,10,0);
        objCamera.transform.localRotation = Quaternion.Euler(90, 0, 0);
        //cameratest.transform.position = new Vector3(playerIcon.transform.position.x, cameratest.transform.position.y, playerIcon.transform.position.z);

        //エネミーを追加
        //enemyIcon = new List<GameObject>();
        //追加したエネミーにアイコンを追加
        for (int i = 0; i < enemyIcon.Count; i++)
        {
            var objImage2 = Instantiate(image, this.transform.position, Quaternion.identity);
            enemyIcon[i].transform.parent = objImage2.transform;
            objImage2.transform.localPosition = Vector3.zero;
        }
        //canvas = this.GetComponent<Canvas>();
        //canvas.enabled = true;
        lineInitialization();
    }
    private void lineInitialization()
    {
        // 安全に初期化
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer がこのオブジェクトに付いていません。");
        }
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 3.0f;
        lineRenderer.endWidth = 3.0f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.cyan;
        lineRenderer.endColor = Color.cyan;
        navPath = new NavMeshPath();
    }


    // Update is called once per frame
    void Update()
    {
        //経路表示の更新
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdatePath();
        }
        if (targetSetFlag)
        {
            Vector3 dir = target.position - cameratest.transform.position;
            dir.y = 0;
            cameratest.transform.position += dir.normalized * Time.deltaTime * 10;
            if (dir.magnitude < 0.5f)
            {
                targetSetFlag = false;
            }
        }
    }
    public void SetTargetPos(Vector3 _targetPos)
    {
        target.position = new Vector3(_targetPos.x, _targetPos.y, _targetPos.z);
        targetSetFlag = true;
    }
    //エネミーアイコンを削除
    public void EnemyIconDelete(int index)
    {
        if (enemyIcon.Count > index)
        {
            Destroy(enemyIcon[index]);
            enemyIcon.RemoveAt(index);
        }
    }
    //エネミーアイコンを追加
    public void EnemyIconAdd(GameObject obj)
    {
        enemyIcon.Add(obj);
        var objImage = Instantiate(image, this.transform.position, Quaternion.identity);
        enemyIcon[enemyIcon.Count - 1].transform.parent = objImage.transform;
        objImage.transform.localPosition = Vector3.zero;
    }
    void UpdatePath()
    {
        if (playerIcon == null || target == null)
        {
            Debug.LogWarning("Player または Target が設定されていません。");
            return;
        }

        if (navPath == null)
            navPath = new NavMeshPath();

        // 経路計算
        bool success = NavMesh.CalculatePath(playerIcon.transform.position, target.position, NavMesh.AllAreas, navPath);

        if (success && navPath.corners.Length > 1)
        {
            lineRenderer.positionCount = navPath.corners.Length;
            lineRenderer.SetPositions(navPath.corners);
        }
        else
        {
            lineRenderer.positionCount = 0;
            //
            Debug.Log("経路が見つかりません。NavMeshの上にいますか？");
        }
    }

    //ターゲットまでの道のりを表示

}
