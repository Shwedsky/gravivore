using System;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.Quests;
using Gravivore.Gameplay.World;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.Quests
{
    public enum QuestTrackerGuidanceMode
    {
        Objective = 0,
        Assimilation = 1,
        Complete = 2
    }

    public readonly struct QuestTrackerGuidance
    {
        public QuestTrackerGuidance(
            QuestTrackerGuidanceMode mode,
            string text,
            QuestObjective? markerObjective)
        {
            Mode = mode;
            Text = text;
            MarkerObjective = markerObjective;
        }

        public QuestTrackerGuidanceMode Mode { get; }
        public string Text { get; }
        public QuestObjective? MarkerObjective { get; }
    }

    public static class QuestTrackerGuidanceResolver
    {
        public static QuestTrackerGuidance Resolve(
            QuestService quests,
            ProgressionState progression,
            EliteGateRequirement eliteRequirement)
        {
            if (quests == null) throw new ArgumentNullException(nameof(quests));
            if (progression == null) throw new ArgumentNullException(nameof(progression));
            if (eliteRequirement == null) throw new ArgumentNullException(nameof(eliteRequirement));

            var active = quests.ActiveObjective;
            if (!active.HasValue)
            {
                return new QuestTrackerGuidance(
                    QuestTrackerGuidanceMode.Complete,
                    "Primary sequence complete",
                    null);
            }

            var objective = active.Value;
            if (objective.Type == QuestObjectiveType.EliteDefeated &&
                AreRequiredIntroObjectivesCompleted(quests.State, eliteRequirement) &&
                progression.TotalAssimilationScore < eliteRequirement.MinimumAssimilationScore)
            {
                return new QuestTrackerGuidance(
                    QuestTrackerGuidanceMode.Assimilation,
                    $"Assimilation {progression.TotalAssimilationScore}/{eliteRequirement.MinimumAssimilationScore}",
                    null);
            }

            var progress = quests.GetProgress(objective.Id);
            var text = progress.Required > 1
                ? $"{objective.Title} {progress.Progress}/{progress.Required}"
                : objective.Title;
            if (quests.State.ExpandedObjectivesUnlocked && IsRequiredIntroObjective(objective.Id, eliteRequirement))
            {
                text += $" | Intro {CountCompletedIntroObjectives(quests.State, eliteRequirement)}/{eliteRequirement.RequiredObjectiveCount}";
            }

            return new QuestTrackerGuidance(QuestTrackerGuidanceMode.Objective, text, objective);
        }

        private static bool AreRequiredIntroObjectivesCompleted(
            QuestState quests,
            EliteGateRequirement requirement)
        {
            return CountCompletedIntroObjectives(quests, requirement) == requirement.RequiredObjectiveCount;
        }

        private static int CountCompletedIntroObjectives(QuestState quests, EliteGateRequirement requirement)
        {
            var completed = 0;
            for (var i = 0; i < requirement.RequiredObjectiveCount; i++)
            {
                if (quests.IsObjectiveCompleted(requirement.GetRequiredObjectiveId(i))) completed++;
            }

            return completed;
        }

        private static bool IsRequiredIntroObjective(string objectiveId, EliteGateRequirement requirement)
        {
            for (var i = 0; i < requirement.RequiredObjectiveCount; i++)
            {
                if (string.Equals(objectiveId, requirement.GetRequiredObjectiveId(i), StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }

    [DisallowMultipleComponent]
    public sealed class QuestTrackerPresenter : MonoBehaviour
    {
        private QuestService _quests;
        private AssimilationProgressionService _progression;
        private EliteGateRequirement _eliteRequirement;
        private Chapter01WorldConfiguration _world;
        private Vector3 _eliteTargetPosition;
        private Text _trackerText;
        private Text _feedbackText;
        private Transform _marker;
        private Material _markerMaterial;
        private float _feedbackSecondsRemaining;

        public string CurrentTrackerText => _trackerText != null ? _trackerText.text : string.Empty;
        public bool MarkerActive => _marker != null && _marker.gameObject.activeSelf;
        public Vector3 MarkerPosition => _marker != null ? _marker.position : default;

        public void Initialize(
            QuestService quests,
            AssimilationProgressionService progression,
            EliteGateRequirement eliteRequirement,
            Chapter01WorldConfiguration world,
            Vector3 eliteTargetPosition,
            RectTransform hudRoot,
            Func<Color, Material> materialFactory)
        {
            if (quests == null) throw new ArgumentNullException(nameof(quests));
            if (progression == null) throw new ArgumentNullException(nameof(progression));
            if (eliteRequirement == null) throw new ArgumentNullException(nameof(eliteRequirement));
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (hudRoot == null) throw new ArgumentNullException(nameof(hudRoot));
            if (materialFactory == null) throw new ArgumentNullException(nameof(materialFactory));

            _quests = quests;
            _progression = progression;
            _eliteRequirement = eliteRequirement;
            _world = world;
            _eliteTargetPosition = eliteTargetPosition;

            CreateUi(hudRoot);
            CreateMarker(materialFactory);
            _quests.ObjectiveProgressed += HandleQuestChanged;
            _quests.ObjectiveCompleted += HandleQuestChanged;
            _quests.AssimilationFeedback += HandleAssimilationFeedback;
            _progression.RewardGranted += HandleRewardGranted;
            ApplyState();
        }

        public void ApplyState()
        {
            var guidance = QuestTrackerGuidanceResolver.Resolve(
                _quests,
                _progression.State,
                _eliteRequirement);
            _trackerText.text = guidance.Text;
            if (!guidance.MarkerObjective.HasValue)
            {
                SetMarker(false, default);
                return;
            }

            SetMarkerTarget(guidance.MarkerObjective.Value);
        }

        public void Shutdown()
        {
            if (_quests == null) return;
            _quests.ObjectiveProgressed -= HandleQuestChanged;
            _quests.ObjectiveCompleted -= HandleQuestChanged;
            _quests.AssimilationFeedback -= HandleAssimilationFeedback;
            _progression.RewardGranted -= HandleRewardGranted;
            _quests = null;
            _progression = null;
            _eliteRequirement = null;
        }

        private void Update()
        {
            if (_feedbackText == null || _feedbackSecondsRemaining <= 0f) return;
            _feedbackSecondsRemaining -= Time.deltaTime;
            if (_feedbackSecondsRemaining <= 0f) _feedbackText.gameObject.SetActive(false);
            if (_marker != null && _marker.gameObject.activeSelf)
            {
                _marker.Rotate(Vector3.up, 80f * Time.deltaTime, Space.World);
            }
        }

        private void HandleQuestChanged(QuestObjectiveProgressedEvent _) => ApplyState();
        private void HandleQuestChanged(QuestObjectiveCompletedEvent _) => ApplyState();
        private void HandleRewardGranted(CoreRewardGrantedEvent _) => ApplyState();

        private void HandleAssimilationFeedback(QuestAssimilationFeedbackEvent feedback)
        {
            _feedbackText.text = $"+{feedback.Reward.GrantedExperience:0.#} {feedback.Reward.Stat}";
            _feedbackText.gameObject.SetActive(true);
            _feedbackSecondsRemaining = 2f;
        }

        private void SetMarkerTarget(QuestObjective objective)
        {
            switch (objective.TargetType)
            {
                case QuestTargetType.FarmingZone:
                    if (TryGetZone(objective.TargetId, out var zone))
                    {
                        SetMarker(true, zone.LandmarkPosition + Vector3.up * 3.4f);
                        return;
                    }
                    break;
                case QuestTargetType.Elite:
                    SetMarker(true, _eliteTargetPosition + Vector3.up * 3.4f);
                    return;
                case QuestTargetType.BossArena:
                    SetMarker(true, _world.BossArenaCenter + Vector3.up * 3.4f);
                    return;
            }

            SetMarker(false, default);
        }

        private bool TryGetZone(string zoneId, out WorldZoneConfiguration zone)
        {
            for (var i = 0; i < _world.ZoneCount; i++)
            {
                var candidate = _world.GetZone(i);
                if (string.Equals(candidate.Id, zoneId, StringComparison.Ordinal))
                {
                    zone = candidate;
                    return true;
                }
            }

            zone = default;
            return false;
        }

        private void SetMarker(bool active, Vector3 position)
        {
            _marker.gameObject.SetActive(active);
            if (active) _marker.position = position;
        }

        private void CreateUi(RectTransform hudRoot)
        {
            var trackerObject = new GameObject("Quest Tracker", typeof(RectTransform), typeof(Text));
            var trackerTransform = trackerObject.GetComponent<RectTransform>();
            trackerTransform.SetParent(hudRoot, false);
            trackerTransform.anchorMin = new Vector2(0.06f, 0.9f);
            trackerTransform.anchorMax = new Vector2(0.94f, 0.98f);
            trackerTransform.offsetMin = Vector2.zero;
            trackerTransform.offsetMax = Vector2.zero;
            _trackerText = trackerObject.GetComponent<Text>();
            _trackerText.font = GetLegacyRuntimeFont();
            _trackerText.fontSize = 34;
            _trackerText.alignment = TextAnchor.MiddleCenter;
            _trackerText.color = Color.white;

            var feedbackObject = new GameObject("Quest Assimilation Feedback", typeof(RectTransform), typeof(Text));
            var feedbackTransform = feedbackObject.GetComponent<RectTransform>();
            feedbackTransform.SetParent(hudRoot, false);
            feedbackTransform.anchorMin = new Vector2(0.1f, 0.78f);
            feedbackTransform.anchorMax = new Vector2(0.9f, 0.86f);
            feedbackTransform.offsetMin = Vector2.zero;
            feedbackTransform.offsetMax = Vector2.zero;
            _feedbackText = feedbackObject.GetComponent<Text>();
            _feedbackText.font = GetLegacyRuntimeFont();
            _feedbackText.fontSize = 30;
            _feedbackText.alignment = TextAnchor.MiddleCenter;
            _feedbackText.color = new Color(0.65f, 1f, 0.86f, 1f);
            _feedbackText.gameObject.SetActive(false);
        }

        private void CreateMarker(Func<Color, Material> materialFactory)
        {
            var markerObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            markerObject.name = "Quest World Marker";
            markerObject.transform.SetParent(transform, false);
            markerObject.transform.localScale = new Vector3(0.9f, 0.08f, 0.9f);
            var collider = markerObject.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
            _markerMaterial = materialFactory(new Color(0.3f, 0.95f, 1f, 1f));
            if (_markerMaterial != null) markerObject.GetComponent<Renderer>().sharedMaterial = _markerMaterial;
            _marker = markerObject.transform;
            _marker.gameObject.SetActive(false);
        }

        private static Font GetLegacyRuntimeFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null
                ? font
                : throw new InvalidOperationException("Unity built-in font LegacyRuntime.ttf is unavailable.");
        }

        private void OnDestroy()
        {
            Shutdown();
            if (_markerMaterial != null) Destroy(_markerMaterial);
        }
    }
}
