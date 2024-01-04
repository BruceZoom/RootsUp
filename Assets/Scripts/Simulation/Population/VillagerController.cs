using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class VillagerController : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody2D _rb2d;

    public delegate void VillagerActionCallback();

    [SerializeField]
    private float _walkSpeed = 1f;

    private VillagerActionCallback _deathCallback = null;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        //_rb2d = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Walk to given position.
    /// </summary>
    public void WalkTo(float x, VillagerActionCallback arrivalCallback=null)
    {
        _animator.SetBool("walking", true);
        var displacement = x - transform.position.x;
        transform.localScale = new Vector3(displacement > 0 ? 1 : -1, 1, 1);
        transform.DOMoveX(x, Mathf.Abs(displacement) / _walkSpeed)
                 .SetEase(Ease.Linear)
                 .OnComplete(() => {
                     _animator.SetBool("walking", false);
                     arrivalCallback?.Invoke();
                 });
    }

    /// <summary>
    /// Walk to the tree and get water.
    /// </summary>
    public void GoGetWater(float getWaterTime, VillagerActionCallback finishCallback =null)
    {
        var targetX = Random.Range(SimulationManager.Instance.TreeInteractRange.x, SimulationManager.Instance.TreeInteractRange.y);
        WalkTo(targetX, () => {
            _animator.SetBool("operating", true);
            StartCoroutine(GetWater(getWaterTime, finishCallback));
        });
    }

    private IEnumerator GetWater(float getWaterTime, VillagerActionCallback finishCallback = null)
    {
        yield return new WaitForSeconds(getWaterTime);

        _animator.SetBool("operating", false);
        finishCallback?.Invoke();
    }

    /// <summary>
    /// Go home.
    /// </summary>
    public void GoHome(float x, VillagerActionCallback arrivalCallback = null)
    {
        WalkTo(x, arrivalCallback);
    }

    /// <summary>
    /// Idle state, randomly walk around.
    /// </summary>
    public void StartIdle()
    {
        // TODO:
    }

    public void StopIdle()
    {
        // TODO:
    }

    /// <summary>
    /// Die.
    /// </summary>
    public void Die(VillagerActionCallback finishCallback = null)
    {
        _animator.SetBool("dead", true);
        _deathCallback = finishCallback;
    }

    public void Dead()
    {
        _deathCallback?.Invoke();
        _deathCallback = null;
    }

    public void Revive()
    {
        _animator.SetBool("dead", false);
    }
}
