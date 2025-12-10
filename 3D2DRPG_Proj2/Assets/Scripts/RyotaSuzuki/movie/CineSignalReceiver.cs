using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Timeline Signalを受信してキャラクター位置を管理するクラス
/// Timeline上のSignal Emitterと組み合わせて使用
/// </summary>
public class CineSignalReceiver : MonoBehaviour
{
    [System.Serializable]
    public class CharacterTransformData
    {
        public GameObject character;
        public Transform targetPosition;
        [HideInInspector] public Vector3 savedPosition;
        [HideInInspector] public Quaternion savedRotation;
    }
    
    [Header("キャラクター設定")]
    [SerializeField, Tooltip("管理するキャラクター")]
    private List<CharacterTransformData> characters = new List<CharacterTransformData>();
    
    [Header("制御設定")]
    [SerializeField, Tooltip("操作を無効化するか")]
    private bool disableControl = true;
    
    /// <summary>
    /// キャラクターをムービー位置に移動（Timeline Signalから呼び出し）
    /// </summary>
    public void MoveCharactersToMoviePosition()
    {
        foreach (var data in characters)
        {
            if (data.character == null) continue;
            
            // 元の位置を保存
            data.savedPosition = data.character.transform.position;
            data.savedRotation = data.character.transform.rotation;
            
            // ムービー位置に移動
            if (data.targetPosition != null)
            {
                data.character.transform.position = data.targetPosition.position;
                data.character.transform.rotation = data.targetPosition.rotation;
            }
            
            // 操作を無効化
            if (disableControl)
            {
                SetControlEnabled(data.character, false);
            }
            
            Debug.Log($"{data.character.name} をムービー位置に移動しました");
        }
    }
    
    /// <summary>
    /// キャラクターを元の位置に戻す（Timeline Signalから呼び出し）
    /// </summary>
    public void RestoreCharactersToOriginalPosition()
    {
        foreach (var data in characters)
        {
            if (data.character == null) continue;
            
            // 元の位置に戻す
            data.character.transform.position = data.savedPosition;
            data.character.transform.rotation = data.savedRotation;
            
            // 操作を有効化
            if (disableControl)
            {
                SetControlEnabled(data.character, true);
            }
            
            Debug.Log($"{data.character.name} を元の位置に戻しました");
        }
    }
    
    /// <summary>
    /// 特定のキャラクターだけを移動
    /// </summary>
    public void MoveCharacter(int index)
    {
        if (index < 0 || index >= characters.Count) return;
        
        var data = characters[index];
        if (data.character == null) return;
        
        data.savedPosition = data.character.transform.position;
        data.savedRotation = data.character.transform.rotation;
        
        if (data.targetPosition != null)
        {
            data.character.transform.position = data.targetPosition.position;
            data.character.transform.rotation = data.targetPosition.rotation;
        }
        
        if (disableControl)
        {
            SetControlEnabled(data.character, false);
        }
    }
    
    /// <summary>
    /// 特定のキャラクターだけを元に戻す
    /// </summary>
    public void RestoreCharacter(int index)
    {
        if (index < 0 || index >= characters.Count) return;
        
        var data = characters[index];
        if (data.character == null) return;
        
        data.character.transform.position = data.savedPosition;
        data.character.transform.rotation = data.savedRotation;
        
        if (disableControl)
        {
            SetControlEnabled(data.character, true);
        }
    }
    
    /// <summary>
    /// キャラクターの操作を有効/無効化
    /// </summary>
    private void SetControlEnabled(GameObject character, bool enabled)
    {
        // PlayerControllerなどのスクリプトを無効化
        var playerController = character.GetComponent<MonoBehaviour>();
        if (playerController != null)
        {
            playerController.enabled = enabled;
        }
        
        // CharacterController
        var characterController = character.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = enabled;
        }
        
        // Rigidbody
        var rb = character.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = !enabled;
        }
    }
    
    /// <summary>
    /// すべてのキャラクター操作を無効化
    /// </summary>
    public void DisableAllControl()
    {
        foreach (var data in characters)
        {
            if (data.character != null)
            {
                SetControlEnabled(data.character, false);
            }
        }
    }
    
    /// <summary>
    /// すべてのキャラクター操作を有効化
    /// </summary>
    public void EnableAllControl()
    {
        foreach (var data in characters)
        {
            if (data.character != null)
            {
                SetControlEnabled(data.character, true);
            }
        }
    }
}

