using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SheepAnimation : MonoBehaviour
{
    [Header("速度阈值")]
    [Tooltip("低于此速度不播放任何动画，角色视为静止")]
    public float idleSpeed = 0.05f;

    [Tooltip("低于此速度播放 'Walk' 动画")]
    public float walkSpeed = 2f;

    [Tooltip("高于此速度播放 'Run' 动画")]
    public float runSpeed = 5f;

    [Header("转向阈值")]
    [Tooltip("速度方向与角色Transform.forward的夹角超过此值时，触发转向动画")]
    [Range(0f, 45f)]
    public float turnAngleThreshold = 10f;

    [Header("动画名称")]
    public string walkForwardAnimation = "walk_forward";
    public string walkBackwardAnimation = "walk_backwards";
    public string runForwardAnimation = "run_forward";
    public string turn90LAnimation = "turn_90_L";
    public string turn90RAnimation = "turn_90_R";
    public string trotAnimation = "trot_forward";
    public string sittostandAnimation = "sit_to_stand";
    public string standtositAnimation = "stand_to_sit";

    public string attackAnimation = "attack";
    public string hitReactionAnimation = "hit_reaction";
    public string deathAnimation = "death";
    public string idleAnimation = "idle";

    private Animator _animator;
    private Vector3 _previousPosition;
    private Vector3 _currentVelocity;

    private bool _isInCombat = true;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _previousPosition = transform.position;

        if (_animator == null)
        {
            Debug.LogError("Animator组件丢失！请确保GameObject上挂载了Animator。");
            enabled = false;
        }
    }

    void Update()
    {
        if (_isInCombat)
        {
            return;
        }

        CalculateVelocity();
        UpdateAnimationState();
    }

    /// <summary>
    /// 计算当前帧的速度向量。
    /// </summary>
    private void CalculateVelocity()
    {
        // 计算这一帧的位置变化
        Vector3 displacement = transform.position - _previousPosition;

        // 计算速度 (位移 / 时间)
        _currentVelocity = displacement / Time.deltaTime;

        // 存储当前位置供下一帧使用
        _previousPosition = transform.position;
    }

    /// <summary>
    /// 根据速度和转向角度播放对应的动画。
    /// </summary>
    private void UpdateAnimationState()
    {
        // 忽略垂直速度 (Y轴)
        Vector3 horizontalVelocity = new Vector3(_currentVelocity.x, 0, _currentVelocity.z);
        float speed = horizontalVelocity.magnitude;
        Vector3 forward = transform.forward;

        // --- 静止判断 (速度太低) ---
        if (speed < idleSpeed)
        {
            _animator.Play(idleAnimation);
            return;
        }


        // 计算速度方向与角色朝向的夹角
        //float angle = Vector3.Angle(forward, horizontalVelocity);

        //if (angle > turnAngleThreshold)
        //{
        //    // 使用叉乘判断是左转还是右转
        //    Vector3 cross = Vector3.Cross(forward, horizontalVelocity);

        //    // 如果Y分量 > 0，速度在前方偏右；如果Y分量 < 0，速度在前方偏左
        //    if (cross.y > 0)
        //    {
        //        // 速度在右侧 -> 播放右转动画
        //        _animator.Play(turn90RAnimation);
        //    }
        //    else
        //    {
        //        // 速度在左侧 -> 播放左转动画
        //        _animator.Play(turn90LAnimation);
        //    }
        //    return;
        //}

        // 使用点积判断运动方向是否向前（点积 > 0 表示在前半球）
        float forwardDot = Vector3.Dot(forward, horizontalVelocity.normalized);

        if (forwardDot > 0.5f) // 确保不是完全侧向移动
        {
            if (speed > runSpeed)
            {
                _animator.Play(runForwardAnimation);
            }
            else if (speed > walkSpeed)
            {
                // 处于 Walk 和 Run 速度之间，可以播放 Run 或 Walk，这里选择 Run
                _animator.Play(runForwardAnimation);
            }
            else if (speed >= idleSpeed)
            {
                _animator.Play(walkForwardAnimation);
            }
        }
    }


    public void EnterCombatState()
    {
        _isInCombat = true;
    }

    public void PlayAttack()
    {
        // 使用CrossFadeInFixedTime而不是Play，可以让动画过渡更平滑
        // 0.1f是过渡时间
        _animator.CrossFadeInFixedTime(attackAnimation, 0.1f);
    }

    public void PlayHitReaction()
    {
        _animator.CrossFadeInFixedTime(hitReactionAnimation, 0.1f);
    }

    public void PlayDeath()
    {
        _animator.CrossFadeInFixedTime(deathAnimation, 0.1f);
        Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

    }
    public GameObject deathEffectPrefab;

}