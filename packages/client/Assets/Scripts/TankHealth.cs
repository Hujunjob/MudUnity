﻿using System;
using UniRx;
using DefaultNamespace;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using ObservableExtensions = UniRx.ObservableExtensions;

public class TankHealth : MonoBehaviour
{
    public float m_StartingHealth = 100f;
    private float m_CurrentHealth;

    public Slider m_Slider;
    public Image m_FillImage;
    public Color m_FullHealthColor = Color.green;
    public Color m_ZeroHealthColor = Color.red;
    public GameObject m_ExplosionPrefab;
    public GameObject shell;

    // TODO: Get PlayerSync
    private PlayerSync _player;

    private ParticleSystem m_ExplosionParticles;
    private bool m_Dead;
    private CompositeDisposable _disposable = new();


    private void Awake()
    {
        m_ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>();
        m_ExplosionParticles.gameObject.SetActive(false);
        // TODO: Get PlayerSync component
        _player = GetComponent<PlayerSync>();
        // TODO: Subscribe to HealthTable Updates
        ObservableExtensions.Subscribe(HealthTable.OnRecordUpdate().ObserveOnMainThread(),OnHealthChange).AddTo(_disposable);
        // TODO: Subscribe to HealthTable Deletions
        ObservableExtensions.Subscribe(HealthTable.OnRecordDelete().ObserveOnMainThread(),OnPlayerDeath).AddTo(_disposable);
    }


    private void OnEnable()
    {
        m_CurrentHealth = m_StartingHealth;
        m_Dead = false;

        SetHealthUI();
    }

    // TODO: Callback for HealthTable update
    private void OnHealthChange(HealthTableUpdate update)
    {
        if(update.Key!=_player.key){return;}

        var shellPosition = transform.position;
        shellPosition.y += 10;
        Instantiate(shell,shellPosition,Quaternion.LookRotation(Vector3.down));

        var health = update.TypedValue.Item1;
        m_CurrentHealth = (float)health.value;
        SetHealthUI();
    }

    // TODO: Callback for HealthTable deletion
    private void OnPlayerDeath(HealthTableUpdate update)
    {
        if(update.Key!=_player.key){return;}

        if(update.TypedValue.Item1==null){
            OnDeath();
        }
    }

    private void SetHealthUI()
    {
        // Adjust the value and colour of the slider.
        m_Slider.value = m_CurrentHealth;
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
    }

    private void OnDeath()
    {
        m_Dead = true;
        m_ExplosionParticles.transform.position = transform.position;
        m_ExplosionParticles.gameObject.SetActive(true);
        m_ExplosionParticles.Play();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _disposable?.Dispose();
    }
}
