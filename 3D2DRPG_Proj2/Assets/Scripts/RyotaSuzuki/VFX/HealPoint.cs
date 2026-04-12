using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class HealPoint : MonoBehaviour
{
    [SerializeField] ParticleSystem particle;
    [SerializeField, Header("プレイヤーのデータ")]
    public List<CharacterData> players;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        foreach(var player in players)
        {
            player.hp = player.maxHp;
            player.mp = player.maxMp;
        }
        particle.Stop();
    }
}
