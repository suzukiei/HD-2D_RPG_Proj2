using UnityEngine;
using System.Collections;

public class VFXManager : MonoBehaviour
{
    private static VFXManager instance;
    public static VFXManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<VFXManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("VFXManager");
                    instance = obj.AddComponent<VFXManager>();
                }
            }
            return instance;
        }
    }

    [Header("エフェクトプレハブ")]
    [SerializeField] private ParticleSystem explosionEffect;  // 爆発エフェクト

    [Header("エフェクト設定")]
    [SerializeField] private Vector3 effectOffset = Vector3.up;  // 位置オフセット

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 指定オブジェクトの位置に爆発エフェクトを再生
    /// </summary>
    public void PlayExplosion(GameObject target, Vector3 offset = default)
    {
        if (target == null || explosionEffect == null)
        {
            Debug.LogWarning("VFXManager: targetまたはexplosionEffectがnullです");
            return;
        }

        Vector3 spawnPos = target.transform.position + (offset == default ? effectOffset : offset);
        ParticleSystem ps = Instantiate(explosionEffect, spawnPos, Quaternion.identity);
        ps.Play();

        // パーティクル終了後に削除
        float duration = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(ps.gameObject, duration);
    }

    /// <summary>
    /// 汎用VFX再生（任意のParticleSystemを再生）
    /// </summary>
    public void PlayVFX(ParticleSystem vfxPrefab, Vector3 position, Quaternion rotation = default)
    {
        if (vfxPrefab == null) return;

        if (rotation == default) rotation = Quaternion.identity;

        ParticleSystem ps = Instantiate(vfxPrefab, position, rotation);
        ps.Play();

        float duration = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(ps.gameObject, duration);
    }
}