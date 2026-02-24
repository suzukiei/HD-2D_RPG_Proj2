using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Character: MonoBehaviour
{
    private CharacterData characterData;
    public string charactername;
    public Sprite characterIcon;
    public bool enemyCheckFlag;
    public int hp, mp, atk, def, spd;
    public int maxHp, maxMp;
    public int exp, level;
    public GameObject CharacterObj;
    public SkillData[] skills;
    public Vector3 CharacterTransfrom;
    public StatusFlag StatusFlag;
    private GameObject characterDataObj;
    [SerializeField]
    private Animator enemyAnimator;
    
    // バフ管理
    private CharacterBuffManager buffManager;
    
    // ベースステータス（バフ適用前の元の値）
    private int baseAtk;
    private int baseDef;
    private int baseSpd;
    private int baseMaxHp;
    private int baseMaxMp;

    //バフ関連
    //すべての攻撃を全体攻撃にするフラグ
    public bool AllAttack;
    
    public void init(CharacterData _characterData)
    {
        characterData = _characterData;
        charactername = characterData.charactername;
        characterIcon = characterData.characterIcon;
        enemyCheckFlag = characterData.enemyCheckFlag;
        hp = characterData.hp;
        mp = characterData.mp;
        atk = characterData.atk;
        def = characterData.def;
        spd = characterData.spd;
        maxHp = characterData.maxHp;
        maxMp = characterData.maxMp;
        exp = characterData.exp;
        level = characterData.level;
        CharacterObj = this.gameObject;
        characterDataObj= characterData.CharacterObj;
        skills = characterData.skills;
        CharacterTransfrom= characterData.CharacterTransfrom;
        StatusFlag= characterData.StatusFlag;
        if (this.transform.Find("BlockWolfAnimation"))
        enemyAnimator = this.transform.Find("BlockWolfAnimation").GetComponent<Animator>();
        
        // ベースステータスを保存
        baseAtk = atk;
        baseDef = def;
        baseSpd = spd;
        baseMaxHp = maxHp;
        baseMaxMp = maxMp;
        
        // バフマネージャーの初期化
        InitializeBuffManager();
    }
    
    /// <summary>
    /// バフマネージャーの初期化
    /// </summary>
    private void InitializeBuffManager()
    {
        buffManager = GetComponent<CharacterBuffManager>();
        if (buffManager == null)
        {
            buffManager = gameObject.AddComponent<CharacterBuffManager>();
        }
        buffManager.Initialize(this);
    }
    // ダメージ計算
    public int Attack(Character enemy,SkillData skillData)
    {
        // バフ適用後の防御力を使用
        int effectiveDef = enemy.GetEffectiveDefense();
        int damage = Mathf.Max(0, (int)skillData.power - effectiveDef);
        enemy.TakeDamage(damage);
        return damage;
    }

    // ダメージを受ける
    public void TakeDamage(int damage)
    {
        hp = Mathf.Max(0, hp - damage);
    }

    // HP/MP回復
    public void Heal(int amount)
    {
        hp = Mathf.Min(maxHp, hp + amount);
    }
    public void RestoreMp(int amount)
    {
        mp = Mathf.Min(maxMp, mp + amount);
    }

    [Header("レベルアップ設定")]
    public int maxLevel = 99; // 最大レベル
    
    // レベルアップイベント（旧レベルを引数として渡す）
    public System.Action<int> OnLevelUp;
    
    public Animator EnemyAnimator
    {
        get { return enemyAnimator; }
    }
    // 経験値アップ
    public void GainExp(int amount)
    {
        if (level >= maxLevel)
        {
            exp = 0; // 最大レベルに達している場合は経験値を0に
            return;
        }
        
        exp += amount;
        
        // 連続レベルアップに対応
        while (exp >= ExpToLevelUp() && level < maxLevel)
        {
            int expNeeded = ExpToLevelUp();
            exp -= expNeeded;
            LevelUp();
        }
        
        // 最大レベルに達した場合
        if (level >= maxLevel)
        {
            exp = 0;
        }
    }
    
    /// <summary>
    /// 次のレベルアップに必要な経験値を計算
    /// 二次関数的な成長曲線を使用（よりバランスが良い）
    /// </summary>
    private int ExpToLevelUp()
    {
        // オプション1: 二次関数的な成長（推奨）
        return 100 + (level - 1) * (level - 1) * 50;
        
        // オプション2: 指数関数的な成長（コメントアウト）
        // return (int)(100 * Mathf.Pow(1.5f, level - 1));
        
        // オプション3: 線形成長（コメントアウト）
        // return level * 100;
    }
    
    /// <summary>
    /// レベルアップ処理
    /// </summary>
    private void LevelUp()
    {
        int oldLevel = level;
        level++;
        
        // ステータス上昇
        maxHp += 10;
        maxMp += 5;
        atk += 2;
        def += 2;
        spd += 1;
        
        // ベースステータスを更新
        baseAtk = atk;
        baseDef = def;
        baseSpd = spd;
        baseMaxHp = maxHp;
        baseMaxMp = maxMp;
        
        // バフマネージャーにベースステータスの更新を通知
        if (buffManager != null)
        {
            buffManager.UpdateBaseStats(baseAtk, baseDef, baseSpd, baseMaxHp, baseMaxMp);
        }
        
        // HP/MP全回復
        hp = maxHp;
        mp = maxMp;
        
        // レベルアップイベント発火
        OnLevelUp?.Invoke(oldLevel);
        
        Debug.Log($"{charactername} がレベル {oldLevel} から {level} にレベルアップしました！");
    }
    
    /// <summary>
    /// 次のレベルアップに必要な経験値を取得（外部から参照可能）
    /// </summary>
    public int GetExpToNextLevel()
    {
        if (level >= maxLevel) return 0;
        return ExpToLevelUp();
    }
    
    /// <summary>
    /// 現在の経験値の割合を取得（0.0～1.0）
    /// </summary>
    public float GetExpRatio()
    {
        int expNeeded = GetExpToNextLevel();
        if (expNeeded <= 0) return 1f;
        return (float)exp / expNeeded;
    }

    ///<summary>
    ///
    /// CharacterDataを取得
    ///
    ///</summary>
    public CharacterData GetCharacterData()
    {
        CharacterData newCharacterData =new CharacterData();   
        newCharacterData.charactername = charactername;
        newCharacterData.characterIcon = characterIcon;
        newCharacterData.enemyCheckFlag = enemyCheckFlag;
        newCharacterData.hp = hp;
        newCharacterData.mp = mp;
        newCharacterData.atk = atk;
        newCharacterData.def = def;
        newCharacterData.spd = spd;
        newCharacterData.maxHp = maxHp;
        newCharacterData.maxMp = maxMp;
        newCharacterData.exp = exp;
        newCharacterData.level = level;
        newCharacterData.CharacterObj = characterDataObj;
        newCharacterData.skills = skills;
        newCharacterData.CharacterTransfrom = CharacterTransfrom;
        newCharacterData.StatusFlag = StatusFlag;
        return newCharacterData;
    }
    
    /// <summary>
    /// バフ適用後の攻撃力を取得
    /// </summary>
    public int GetEffectiveAttack()
    {
        if (buffManager != null)
        {
            return buffManager.GetEffectiveAttack();
        }
        return atk;
    }
    
    /// <summary>
    /// バフ適用後の防御力を取得
    /// </summary>
    public int GetEffectiveDefense()
    {
        if (buffManager != null)
        {
            return buffManager.GetEffectiveDefense();
        }
        return def;
    }
    
    /// <summary>
    /// バフ適用後の速度を取得
    /// </summary>
    public int GetEffectiveSpeed()
    {
        if (buffManager != null)
        {
            return buffManager.GetEffectiveSpeed();
        }
        return spd;
    }
    
    /// <summary>
    /// バフ適用後の魔法攻撃力を取得
    /// </summary>
    public int GetEffectiveMagicAttack()
    {
        if (buffManager != null)
        {
            return buffManager.GetEffectiveMagicAttack();
        }
        return atk; // 将来的にINTフィールドが追加されたら変更
    }
    
    /// <summary>
    /// バフマネージャーを取得
    /// </summary>
    public CharacterBuffManager GetBuffManager()
    {
        return buffManager;
    }
    
    /// <summary>
    /// バフを適用
    /// </summary>
    public bool ApplyBuff(BuffInstance buffInstance, Character appliedBy)
    {
        if (buffManager != null)
        {
            return buffManager.ApplyBuff(buffInstance, appliedBy);
        }
        return false;
    }
    
    /// <summary>
    /// バフのターン経過処理
    /// </summary>
    public void TickBuffTurn()
    {
        if (buffManager != null)
        {
            buffManager.TickTurn();
        }
    }
}
