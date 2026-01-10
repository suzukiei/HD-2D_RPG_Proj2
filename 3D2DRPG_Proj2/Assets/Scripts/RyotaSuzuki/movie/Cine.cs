using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// ムービー再生時にキャラクターの位置を管理するクラス
/// </summary>
public class Cine : MonoBehaviour
{
    [Header("ムービー設定")]
    [SerializeField, Tooltip("再生するPlayableDirector")] 
    private PlayableDirector playableDirector;
    
    [Header("キャラクター設定")]
    [SerializeField, Tooltip("ムービー中に移動させるキャラクター")] 
    private GameObject targetCharacter;
    
    [SerializeField, Tooltip("ムービー中のキャラクター位置")] 
    private Transform moviePosition;
    
    [SerializeField, Tooltip("ムービー後に元の位置に戻すか")] 
    private bool returnToOriginalPosition = true;
    
    [SerializeField, Tooltip("ムービー中プレイヤー操作を無効化するか")] 
    private bool disablePlayerControl = true;
    
    // 保存用の変数
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isPlaying = false;
    
    // プレイヤーコントローラーの参照
    private MonoBehaviour playerController;
    
    void Start()
    {
        // PlayableDirectorのイベントを登録
        if (playableDirector != null)
        {
            playableDirector.played += OnMovieStarted;
            playableDirector.stopped += OnMovieStopped;
        }
        
        // プレイヤーコントローラーを取得（存在する場合）
        if (targetCharacter != null && disablePlayerControl)
        {
            playerController = targetCharacter.GetComponent<MonoBehaviour>();
        }
    }
    
    /// <summary>
    /// ムービーを開始する
    /// </summary>
    public void PlayMovie()
    {
        if (playableDirector == null)
        {
            Debug.LogWarning("PlayableDirectorが設定されていません");
            return;
        }
        
        if (targetCharacter == null)
        {
            Debug.LogWarning("TargetCharacterが設定されていません");
            return;
        }
        
        // 元の位置を保存
        SaveOriginalTransform();
        
        // ムービー位置に移動
        if (moviePosition != null)
        {
            targetCharacter.transform.position = moviePosition.position;
            targetCharacter.transform.rotation = moviePosition.rotation;
        }
        
        // プレイヤー操作を無効化
        if (disablePlayerControl)
        {
            DisablePlayerControl();
        }
        
        // ムービー再生
        playableDirector.Play();
        isPlaying = true;
        
        Debug.Log("ムービー開始: キャラクターを移動しました");
    }
    
    /// <summary>
    /// ムービー開始時のコールバック
    /// </summary>
    private void OnMovieStarted(PlayableDirector director)
    {
        Debug.Log("ムービー再生中...");
    }
    
    /// <summary>
    /// ムービー終了時のコールバック
    /// </summary>
    private void OnMovieStopped(PlayableDirector director)
    {
        if (!isPlaying) return;
        
        // 元の位置に戻す
        if (returnToOriginalPosition && targetCharacter != null)
        {
            RestoreOriginalTransform();
            Debug.Log("ムービー終了: キャラクターを元の位置に戻しました");
        }
        
        // プレイヤー操作を有効化
        if (disablePlayerControl)
        {
            EnablePlayerControl();
        }
        
        isPlaying = false;
    }
    
    /// <summary>
    /// 元のTransformを保存
    /// </summary>
    private void SaveOriginalTransform()
    {
        originalPosition = targetCharacter.transform.position;
        originalRotation = targetCharacter.transform.rotation;
    }
    
    /// <summary>
    /// 元のTransformを復元
    /// </summary>
    private void RestoreOriginalTransform()
    {
        targetCharacter.transform.position = originalPosition;
        targetCharacter.transform.rotation = originalRotation;
    }
    
    /// <summary>
    /// プレイヤー操作を無効化
    /// </summary>
    private void DisablePlayerControl()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // CharacterControllerがあれば無効化
        var characterController = targetCharacter.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        // Rigidbodyがあれば物理演算を停止
        var rb = targetCharacter.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }
    
    /// <summary>
    /// プレイヤー操作を有効化
    /// </summary>
    private void EnablePlayerControl()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        // CharacterControllerがあれば有効化
        var characterController = targetCharacter.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        // Rigidbodyがあれば物理演算を再開
        var rb = targetCharacter.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }
    
    /// <summary>
    /// 外部から呼び出す用のムービー停止
    /// </summary>
    public void StopMovie()
    {
        if (playableDirector != null && isPlaying)
        {
            playableDirector.Stop();
        }
    }
    
    void OnDestroy()
    {
        // イベントの登録解除
        if (playableDirector != null)
        {
            playableDirector.played -= OnMovieStarted;
            playableDirector.stopped -= OnMovieStopped;
        }
    }
}
