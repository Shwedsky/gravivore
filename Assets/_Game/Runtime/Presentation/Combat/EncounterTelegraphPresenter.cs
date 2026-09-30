using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Encounters;
using UnityEngine;

namespace Gravivore.Presentation.Combat
{
    [DisallowMultipleComponent]
    public sealed class EncounterTelegraphPresenter : MonoBehaviour
    {
        private readonly List<Material> _materials = new List<Material>();
        private MagnetarGuardController _elite;
        private CustodianBossController _boss;
        private BossCompletionState _completion;
        private GameObject _eliteTelegraph;
        private GameObject _bossTelegraph;
        private GameObject _bossCircle;
        private GameObject _bossCone;
        private GameObject _bossLine;
        private GameObject _impact;
        private float _impactRemaining;

        public bool EliteTelegraphVisible => _eliteTelegraph != null && _eliteTelegraph.activeSelf;
        public bool BossTelegraphVisible => _bossTelegraph != null && _bossTelegraph.activeSelf;
        public BossAttackType CurrentBossAttack { get; private set; }

        public void Initialize(
            MagnetarGuardController elite,
            CustodianBossController boss,
            BossCompletionState completion,
            Material litMaterial)
        {
            _elite = elite != null ? elite : throw new ArgumentNullException(nameof(elite));
            _boss = boss != null ? boss : throw new ArgumentNullException(nameof(boss));
            _completion = completion ?? throw new ArgumentNullException(nameof(completion));
            if (litMaterial == null) throw new ArgumentNullException(nameof(litMaterial));
            _eliteTelegraph = CreateIndicator("Elite Shockwave Telegraph", PrimitiveType.Cylinder, new Color(0.95f, 0.65f, 0.12f, 1f), litMaterial);
            _bossCircle = CreateIndicator("Boss Circle Telegraph", PrimitiveType.Cylinder, new Color(0.92f, 0.18f, 0.18f, 1f), litMaterial);
            _bossCone = CreateIndicator("Boss Cone Telegraph", PrimitiveType.Cube, new Color(0.92f, 0.18f, 0.18f, 1f), litMaterial);
            _bossLine = CreateIndicator("Boss Line Telegraph", PrimitiveType.Cube, new Color(0.92f, 0.18f, 0.18f, 1f), litMaterial);
            _impact = CreateIndicator("Boss Damage Impact", PrimitiveType.Cylinder, new Color(1f, 0.9f, 0.3f, 1f), litMaterial);
            _elite.TelegraphStarted += HandleEliteTelegraph;
            _elite.ShockwaveResolved += HandleEliteResolved;
            _elite.ShockwaveCancelled += HandleEliteCancelled;
            _elite.Defeated += HandleEliteDefeated;
            _boss.TelegraphStarted += HandleBossTelegraph;
            _boss.AttackResolved += HandleBossResolved;
            _boss.EncounterReset += HandleBossReset;
            _completion.Defeated += HandleBossDefeated;
        }

        public void Shutdown()
        {
            if (_elite != null)
            {
                _elite.TelegraphStarted -= HandleEliteTelegraph;
                _elite.ShockwaveResolved -= HandleEliteResolved;
                _elite.ShockwaveCancelled -= HandleEliteCancelled;
                _elite.Defeated -= HandleEliteDefeated;
            }

            if (_boss != null)
            {
                _boss.TelegraphStarted -= HandleBossTelegraph;
                _boss.AttackResolved -= HandleBossResolved;
                _boss.EncounterReset -= HandleBossReset;
            }

            if (_completion != null) _completion.Defeated -= HandleBossDefeated;
            _elite = null;
            _boss = null;
            _completion = null;
        }

        private void HandleEliteTelegraph(EliteShockwaveTelegraphEvent telegraph)
        {
            _eliteTelegraph.transform.position = telegraph.Origin + Vector3.up * 0.035f;
            _eliteTelegraph.transform.rotation = Quaternion.identity;
            _eliteTelegraph.transform.localScale = new Vector3(
                telegraph.Radius * 2f,
                0.025f,
                telegraph.Radius * 2f);
            _eliteTelegraph.SetActive(true);
        }

        private void HandleEliteResolved(EliteShockwaveResolvedEvent resolved)
        {
            HideEliteTelegraph();
        }

        private void HandleEliteCancelled(EliteShockwaveCancelledEvent cancelled) => HideEliteTelegraph();

        private void HandleEliteDefeated(MagnetarGuardDefeatedEvent defeated) => HideEliteTelegraph();

        private void HideEliteTelegraph()
        {
            if (_eliteTelegraph != null) _eliteTelegraph.SetActive(false);
        }

        private void HandleBossTelegraph(BossTelegraphEvent telegraph)
        {
            CurrentBossAttack = telegraph.Attack;
            HideBossTelegraphs();
            switch (telegraph.Attack)
            {
                case BossAttackType.CirclePulse:
                    _bossTelegraph = _bossCircle;
                    PositionBossIndicator(telegraph.Origin, Quaternion.identity);
                    _bossTelegraph.transform.localScale = new Vector3(
                        telegraph.Range * 2f,
                        0.025f,
                        telegraph.Range * 2f);
                    break;
                case BossAttackType.ConeSweep:
                    _bossTelegraph = _bossCone;
                    var coneWidth = Mathf.Tan(telegraph.HalfAngleDegrees * Mathf.Deg2Rad) * telegraph.Range * 2f;
                    PositionBossIndicator(
                        telegraph.Origin + telegraph.Direction * (telegraph.Range * 0.5f),
                        Quaternion.LookRotation(telegraph.Direction, Vector3.up));
                    _bossTelegraph.transform.localScale = new Vector3(coneWidth, 0.04f, telegraph.Range);
                    break;
                case BossAttackType.LineCharge:
                    _bossTelegraph = _bossLine;
                    PositionBossIndicator(
                        telegraph.Origin + telegraph.Direction * (telegraph.Range * 0.5f),
                        Quaternion.LookRotation(telegraph.Direction, Vector3.up));
                    _bossTelegraph.transform.localScale = new Vector3(telegraph.Width, 0.04f, telegraph.Range);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _bossTelegraph.SetActive(true);
        }

        private void HandleBossResolved(BossAttackResolvedEvent resolved)
        {
            _bossTelegraph.SetActive(false);
            _impact.transform.position = _boss.transform.position + Vector3.up * 0.05f;
            _impact.transform.localScale = new Vector3(1.2f, 0.03f, 1.2f);
            _impact.SetActive(true);
            _impactRemaining = 0.15f;
        }

        private void HandleBossReset(BossEncounterResetEvent reset) => HideBossPresentation();

        private void HandleBossDefeated(BossDefeatedEvent defeated) => HideBossPresentation();

        private void HideBossPresentation()
        {
            HideBossTelegraphs();
            if (_impact != null) _impact.SetActive(false);
            _impactRemaining = 0f;
        }

        private void Update()
        {
            if (_impactRemaining <= 0f) return;
            _impactRemaining -= Time.deltaTime;
            if (_impactRemaining <= 0f) _impact.SetActive(false);
        }

        private GameObject CreateIndicator(string name, PrimitiveType type, Color color, Material litMaterial)
        {
            var indicator = GameObject.CreatePrimitive(type);
            indicator.name = name;
            indicator.transform.SetParent(transform, false);
            var collider = indicator.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
            var material = new Material(litMaterial) { color = color, hideFlags = HideFlags.HideAndDontSave };
            _materials.Add(material);
            indicator.GetComponent<Renderer>().sharedMaterial = material;

            indicator.SetActive(false);
            return indicator;
        }

        private void PositionBossIndicator(Vector3 position, Quaternion rotation)
        {
            _bossTelegraph.transform.position = position + Vector3.up * 0.04f;
            _bossTelegraph.transform.rotation = rotation;
        }

        private void HideBossTelegraphs()
        {
            if (_bossCircle != null) _bossCircle.SetActive(false);
            if (_bossCone != null) _bossCone.SetActive(false);
            if (_bossLine != null) _bossLine.SetActive(false);
        }

        private void OnDestroy()
        {
            Shutdown();
            for (var i = 0; i < _materials.Count; i++)
            {
                if (_materials[i] != null) Destroy(_materials[i]);
            }
        }
    }
}
