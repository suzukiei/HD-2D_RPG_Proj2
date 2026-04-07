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

    [Header("カットインエフェクトプレハブ")]
    [SerializeField] private ParticleSystem explosionEffect;  // 爆発エフェクト
    
    [Header("ステータスエフェクトプレハブ")]
    [SerializeField] private ParticleSystem healEffect;       // 回復エフェクト
    [SerializeField] private ParticleSystem buffEffect;       // バフエフェクト
    [SerializeField] private ParticleSystem debuffEffect;     // デバフエフェクト
    [SerializeField] private ParticleSystem poisonEffect;     // 毒エフェクト
    [SerializeField] private ParticleSystem attackUpEffect;   // 攻撃力アップエフェクト
    [SerializeField] private ParticleSystem defenseUpEffect;  // 防御力アップエフェクト

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

    /// <summary>
    /// 回復エフェクトを再生
    /// </summary>
    public void PlayHealEffect(GameObject target, Vector3 offset = default)
    {
        if (target == null || healEffect == null)
        {
            Debug.LogWarning("VFXManager: targetまたはhealEffectがnullです");
            return;
        }

        Vector3 spawnPos = target.transform.position + (offset == default ? effectOffset : offset);
        ParticleSystem ps = Instantiate(healEffect, spawnPos, Quaternion.identity);
        ps.transform.SetParent(target.transform); // ターゲットに追従
        ps.Play();

        float duration = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(ps.gameObject, duration);
    }

    /// <summary>
    /// バフエフェクトを再生
    /// </summary>
    public void PlayBuffEffect(GameObject target, Vector3 offset = default)
    {
        if (target == null || buffEffect == null)
        {
            Debug.LogWarning("VFXManager: targetまたはbuffEffectがnullです");
            return;
        }

        Vector3 spawnPos = target.transform.position + (offset == default ? effectOffset : offset);
        ParticleSystem ps = Instantiate(buffEffect, spawnPos, Quaternion.identity);
        ps.transform.SetParent(target.transform); // ターゲットに追従
        ps.Play();

        float duration = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(ps.gameObject, duration);
    }

    /// <summary>
    /// デバフエフェクトを再生
    /// </summary>
    public void PlayDebuffEffect(GameObject target, Vector3 offset = default)
    {
        if (target == null || debuffEffect == null)
        {
            Debug.LogWarning("VFXManager: targetまたはdebuffEffectがnullです");
            return;
        }

        Vector3 spawnPos = target.transform.position + (offset == default ? effectOffset : offset);
        ParticleSystem ps = Instantiate(debuffEffect, spawnPos, Quaternion.identity);
        ps.transform.SetParent(target.transform); // ターゲットに追従
        ps.Play();

        float duration = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(ps.gameObject, duration);
    }

    /// <summary>
    /// 毒エフェクトを再生
    /// </summary>
    public void PlayPoisonEffect(GameObject target, Vector3 offset = default)
    {
        if (target == null || poisonEffect == null)
        {
            Debug.LogWarning("VFXManager: targetまたはpoisonEffectがnullです");
            return;
        }

        Vector3 spawnPos = target.transform.position + (offset == default ? effectOffset : offset);
        ParticleSystem ps = Instantiate(poisonEffect, spawnPos, Quaternion.identity);
        ps.transform.SetParent(target.transform); // ターゲットに追従
        ps.Play();

        float duration = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(ps.gameObject, duration);
    }

    /// <summary>
    /// 攻撃力アップエフェクトを再生
    /// </summary>
    public void PlayAttackUpEffect(GameObject target, Vector3 offset = default)
    {
        if (target == null || attackUpEffect == null)
        {
            Debug.LogWarning("VFXManager: targetまたはattackUpEffectがnullです");
            return;
        }

        Vector3 spawnPos = target.transform.position + (offset == default ? effectOffset : offset);
        ParticleSystem ps = Instantiate(attackUpEffect, spawnPos, Quaternion.identity);
        ps.transform.SetParent(target.transform);
        ps.Play();

        float duration = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(ps.gameObject, duration);
    }

    /// <summary>
    /// 防御力アップエフェクトを再生
    /// </summary>
    public void PlayDefenseUpEffect(GameObject target, Vector3 offset = default)
    {
        if (target == null || defenseUpEffect == null)
        {
            Debug.LogWarning("VFXManager: targetまたはdefenseUpEffectがnullです");
            return;
        }

        Vector3 spawnPos = target.transform.position + (offset == default ? effectOffset : offset);
        ParticleSystem ps = Instantiate(defenseUpEffect, spawnPos, Quaternion.identity);
        ps.transform.SetParent(target.transform);
        ps.Play();

        float duration = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(ps.gameObject, duration);
    }
}
