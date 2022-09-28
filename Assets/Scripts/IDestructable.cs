using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDestructable
{
	public int Health { get; }
	public void Hit(int damage);
}
