using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SheepAnimation : MonoBehaviour
{
    [Header("动画参数名称")]
    [Tooltip("Animator中代表速度的Float参数的名称")]
    public string speedParameterName = "Speed"; // 假设你的Animator里有一个叫"Speed"的浮点参数

    [Header("动画名称 (战斗用)")]
    public string attackAnimation = "attack";
    public string hitReactionAnimation = "hit_reaction";
    public string deathAnimation = "death";
    public string idleAnimation = "idle";
    public GameObject deathEffectPrefab;

    private Animator _animator;
    private bool _isInCombat = false; // ✨ 注意：默认值应该是false
    private int _speedParamID;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        // 将字符串参数名转换为ID，效率更高
        _speedParamID = Animator.StringToHash(speedParameterName);

        if (_animator == null)
        {
            Debug.LogError("Animator组件丢失！", this.gameObject);
            enabled = false;
        }
    }

    void Update()
    {
        if (_isInCombat)
        {
            return;
        }
    }

    public void UpdateMovementAnimation(float currentSpeed)
    {
        // 如果在战斗中，则忽略所有移动动画的更新
        if (_isInCombat) return;

        // 直接将传入的速度值设置给Animator
        _animator.SetFloat(_speedParamID, currentSpeed);
    }


    // --- 公共接口 (由外部脚本如TurnBasedManager调用) ---

    /// <summary>
    /// 命令单位进入战斗状态。
    /// 这会停止移动动画的更新，并播放待机动画。
    /// </summary>
    public void EnterCombatState()
    {
        _isInCombat = true;
        // 立即播放待机动画，为战斗做准备
        _animator.Play(idleAnimation);
    }

    /// <summary>
    /// 命令单位退出战斗状态（如果未来需要的话）。
    /// </summary>
    public void ExitCombatState()
    {
        _isInCombat = false;
    }

    /// <summary>
    /// 播放攻击动画。
    /// </summary>
    public void PlayAttack()
    {
        if (!_isInCombat) return;
        _animator.CrossFadeInFixedTime(attackAnimation, 0.1f);
    }

    /// <summary>
    /// 播放受击动画。
    /// </summary>
    public void PlayHitReaction()
    {
        if (!_isInCombat) return;
        _animator.CrossFadeInFixedTime(hitReactionAnimation, 0.1f);
    }

    /// <summary>
    /// 播放死亡动画，并生成死亡特效。
    /// </summary>
    public void PlayDeath()
    {
        if (!_isInCombat) return;
        _animator.CrossFadeInFixedTime(deathAnimation, 0.1f);
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}