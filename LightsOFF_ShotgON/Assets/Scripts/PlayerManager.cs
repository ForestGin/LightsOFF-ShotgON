using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public Color color;
    public bool isLocal;

    public float health;
    public float maxHealth;

    public int itemCount = 0;

    public MeshRenderer model;

    public bool isReady;
    public bool inGame;

    public MeshRenderer Shield;
    public bool shieldActive;

    //----Movement
    //public float gravity;
    //public float jumpSpeed;
    //public float moveSpeed;
    //public float yVelocity;

    public void Initialize(int _id, string _username, Color _color)
    {
        id = _id;
        username = _username;
        color = _color;
        health = maxHealth;
        isReady = false;

        Shield.enabled = false;
    }

    //public void SetMovement(float _gravity, float _jumpSpeed, float _moveSpeed, float _yVelocity)
    //{
    //    gravity = _gravity;
    //    jumpSpeed = _jumpSpeed;
    //    moveSpeed = _moveSpeed;
    //    yVelocity = _yVelocity;
    //}

    public void SetHealth(float _health)
    {
        health = _health;

        if (health <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        model.enabled = false;
    }

    public void Respawn()
    {
        model.enabled = true;
        SetHealth(maxHealth);
    }

    public void ShieldActive(bool active)
    {
        if (active)
            Shield.enabled = true;
        else
            Shield.enabled = false;
    }

}
