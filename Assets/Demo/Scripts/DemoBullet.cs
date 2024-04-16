using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoBullet : MonoBehaviour
{
	public float Lifetime;
	public float Speed;
	public Vector3 Direction;

	private Rigidbody _rb;
	private float _lifeTimer;

	public void Awake()
	{
		_rb = GetComponent<Rigidbody>();
	}

	public void Update()
	{
		_rb.velocity = Direction * Speed;
		_lifeTimer += Time.deltaTime;

		if(_lifeTimer >= Lifetime)
		{
			Destroy(gameObject);
		}
	}
}
