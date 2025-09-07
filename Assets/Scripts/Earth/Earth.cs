using UnityEngine;
using System.Collections;
using DG.Tweening;
using Cinemachine;

/// <summary>
/// Spin the object at a specified speed
/// </summary>
public class Earth : MonoBehaviour, IDamageable
{
	[Header("Spin Settings")]
	[SerializeField] private bool spin;
	[SerializeField] private bool spinParent;
	[SerializeField] private float speed = 10f;
	[SerializeField] private bool clockwise = true;

	[Header("Earth Health")]
	[SerializeField] private int maxHealth;
	private int curHealth;

	[Header("Hit Settings")]
	[SerializeField] private Vector3 punchScale = new Vector3(1.2f, 1.2f, 1.2f);
	[SerializeField] private float punchDuration = 0.3f;

	[SerializeField] public CinemachineImpulseSource impulseSource;


	private float direction = 1f;
	private float directionChangeSpeed = 2f;

	// Update is called once per frame
	void Update() {
		if (direction < 1f) {
			direction += Time.deltaTime / (directionChangeSpeed / 2);
		}

		if (spin) {
			if (clockwise) {
				if (spinParent)
					transform.parent.transform.Rotate(Vector3.up, (speed * direction) * Time.deltaTime);
				else
					transform.Rotate(Vector3.up, (speed * direction) * Time.deltaTime);
			} else {
				if (spinParent)
					transform.parent.transform.Rotate(-Vector3.up, (speed * direction) * Time.deltaTime);
				else
					transform.Rotate(-Vector3.up, (speed * direction) * Time.deltaTime);
			}
		}
	}

    public void OnEarthHit()
    {
		transform.DOPunchScale(punchScale, punchDuration, 3);
    }

    public void TakeDamage(int damage)
    {
		impulseSource.GenerateImpulse();
		curHealth -= damage;

		if (curHealth <= 0)
        {
			//IsDestroyed();
        }
    }

    public void IsDestroyed()
    {
		Destroy(gameObject);
    }
}