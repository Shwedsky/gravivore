#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Gravivore.Presentation.Development
{
    public interface IDevelopmentTelemetrySink
    {
        void Append(string jsonLine);
    }

    public sealed class DevelopmentJsonLinesSink : IDevelopmentTelemetrySink
    {
        private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false);
        public DevelopmentJsonLinesSink(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Telemetry path is required.", nameof(path));
            Path = System.IO.Path.GetFullPath(path);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
        }

        public string Path { get; }
        public void Append(string jsonLine) => File.AppendAllText(Path, jsonLine + Environment.NewLine, Utf8WithoutBom);
    }

    [Serializable]
    public sealed class DevelopmentEventEnvelope<TPayload>
    {
        [SerializeField] private int schemaVersion;
        [SerializeField] private string sessionId;
        [SerializeField] private string profileId;
        [SerializeField] private long sequence;
        [SerializeField] private string utc;
        [SerializeField] private string eventName;
        [SerializeField] private TPayload payload;

        public DevelopmentEventEnvelope(int version, Guid session, Guid profile, long order, DateTime timestampUtc, string name, TPayload value)
        {
            schemaVersion = version;
            sessionId = session.ToString("D");
            profileId = profile.ToString("D");
            sequence = order;
            utc = timestampUtc.ToString("O");
            eventName = name;
            payload = value;
        }

        public int SchemaVersion => schemaVersion;
        public string SessionId => sessionId;
        public string ProfileId => profileId;
        public long Sequence => sequence;
        public string Utc => utc;
        public string EventName => eventName;
        public TPayload Payload => payload;
    }

    public sealed class DevelopmentTelemetryRecorder
    {
        public const int CurrentSchemaVersion = 1;
        private readonly IDevelopmentTelemetrySink _sink;
        private readonly Func<DateTime> _utcNow;
        private readonly Action<Exception> _errorReporter;
        private long _sequence;

        public DevelopmentTelemetryRecorder(
            Guid sessionId,
            Guid profileId,
            IDevelopmentTelemetrySink sink,
            Func<DateTime> utcNow = null,
            Action<Exception> errorReporter = null)
        {
            if (sessionId == Guid.Empty) throw new ArgumentException("Session id is required.", nameof(sessionId));
            if (profileId == Guid.Empty) throw new ArgumentException("Profile id is required.", nameof(profileId));
            SessionId = sessionId;
            ProfileId = profileId;
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            _utcNow = utcNow ?? (() => DateTime.UtcNow);
            _errorReporter = errorReporter ?? (exception => Debug.LogWarning($"[DevTelemetry] {exception}"));
        }

        public Guid SessionId { get; }
        public Guid ProfileId { get; }

        public bool TryRecord<TPayload>(string eventName, TPayload payload)
        {
            if (string.IsNullOrWhiteSpace(eventName)) throw new ArgumentException("Event name is required.", nameof(eventName));
            var envelope = new DevelopmentEventEnvelope<TPayload>(
                CurrentSchemaVersion,
                SessionId,
                ProfileId,
                ++_sequence,
                _utcNow(),
                eventName,
                payload);
            try
            {
                _sink.Append(JsonUtility.ToJson(envelope));
                return true;
            }
            catch (Exception exception)
            {
                try { _errorReporter(exception); }
                catch (Exception reporterException) { Debug.LogException(reporterException); }
                return false;
            }
        }

        public static string CreateSessionPath(string persistentDataPath, DateTime startedUtc, Guid sessionId)
        {
            var file = $"session-{startedUtc:yyyyMMdd-HHmmss}-{sessionId:N}.jsonl";
            return System.IO.Path.Combine(persistentDataPath, "dev-analytics", file);
        }
    }

    [Serializable] public sealed class EmptyPayload { }
    [Serializable] public sealed class LifecyclePayload { public bool paused; public LifecyclePayload(bool value) => paused = value; }
    [Serializable] public sealed class StatLevelChangedPayload { public string stat; public int previousLevel; public int newLevel; public StatLevelChangedPayload(string s, int p, int n) { stat = s; previousLevel = p; newLevel = n; } }
    [Serializable] public sealed class AssimilationRewardPayload { public string enemyId; public string stat; public long totalAssimilation; public AssimilationRewardPayload(string enemy, string rewardStat, long total) { enemyId = enemy; stat = rewardStat; totalAssimilation = total; } }
    [Serializable] public sealed class ObjectiveCompletedPayload { public string questId; public string objectiveId; public ObjectiveCompletedPayload(string quest, string objective) { questId = quest; objectiveId = objective; } }
    [Serializable] public sealed class EncounterPayload { public string encounterId; public EncounterPayload(string id) => encounterId = id; }
    [Serializable] public sealed class PlayerDeathPayload { public long totalAssimilation; public int power; public int hull; public int armor; public int flux; public int mobility; public PlayerDeathPayload(long total, int p, int h, int a, int f, int m) { totalAssimilation = total; power = p; hull = h; armor = a; flux = f; mobility = m; } }
    [Serializable] public sealed class OfflineRewardPayload { public long amount; public long pending; public long balance; public OfflineRewardPayload(long value, long pendingValue, long balanceValue) { amount = value; pending = pendingValue; balance = balanceValue; } }
}
#endif
