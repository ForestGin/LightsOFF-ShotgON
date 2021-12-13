﻿using System.Collections;
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
    

    public void Initialize(int _id, string _username, Color _color)
    {
        id = _id;
        username = _username;
        color = _color;
        health = maxHealth;
        isReady = false;
    }

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
}
