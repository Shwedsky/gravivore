using System;
using System.IO;
using Gravivore.Core.Time;
using UnityEngine;

namespace Gravivore.Persistence.Profile
{
    public interface ISaveDiagnostics
    {
        void Warning(string message, Exception exception = null);
        void Error(string message, Exception exception);
    }

    public sealed class UnitySaveDiagnostics : ISaveDiagnostics
    {
        public void Warning(string message, Exception exception = null)
        {
            Debug.LogWarning(exception == null ? $"[Save] {message}" : $"[Save] {message}\n{exception}");
        }

        public void Error(string message, Exception exception)
        {
            Debug.LogError($"[Save] {message}\n{exception}");
        }
    }

    public readonly struct ProfileLoadResult
    {
        public ProfileLoadResult(SaveRootDto save, bool wasCreated, bool recoveredFromBackup, bool wasMigrated)
        {
            Save = save ?? throw new ArgumentNullException(nameof(save));
            WasCreated = wasCreated;
            RecoveredFromBackup = recoveredFromBackup;
            WasMigrated = wasMigrated;
        }

        public SaveRootDto Save { get; }
        public bool WasCreated { get; }
        public bool RecoveredFromBackup { get; }
        public bool WasMigrated { get; }
    }

    public interface IProfileRepository
    {
        ProfileLoadResult LoadOrCreate(Func<SaveRootDto> freshFactory, Action<SaveRootDto> validate);
        void Save(SaveRootDto save, Func<SaveRootDto> freshFactory, Action<SaveRootDto> validate);
    }

    public interface IProfileFileSystem
    {
        void CreateDirectory(string path);
        bool FileExists(string path);
        string ReadAllText(string path);
        void WriteAllText(string path, string contents);
        void CopyFile(string source, string destination, bool overwrite);
        void MoveFile(string source, string destination);
        void DeleteFile(string path);
        void ReplaceFile(string source, string destination);
    }

    public sealed class SystemProfileFileSystem : IProfileFileSystem
    {
        public void CreateDirectory(string path) => Directory.CreateDirectory(path);
        public bool FileExists(string path) => File.Exists(path);
        public string ReadAllText(string path) => File.ReadAllText(path);
        public void WriteAllText(string path, string contents) => File.WriteAllText(path, contents);
        public void CopyFile(string source, string destination, bool overwrite) => File.Copy(source, destination, overwrite);
        public void MoveFile(string source, string destination) => File.Move(source, destination);
        public void DeleteFile(string path) => File.Delete(path);
        public void ReplaceFile(string source, string destination) => File.Replace(source, destination, null);
    }

    public sealed class JsonProfileRepository : IProfileRepository
    {
        private const string MainFileName = "profile.json";
        private const string BackupFileName = "profile.backup.json";
        private const string TempFileName = "profile.temp.json";

        private readonly string _directory;
        private readonly ISaveSerializer _serializer;
        private readonly SaveMigrationPipeline _migrations;
        private readonly ITimeProvider _time;
        private readonly ISaveDiagnostics _diagnostics;
        private readonly IProfileFileSystem _fileSystem;

        public JsonProfileRepository(
            string directory,
            ISaveSerializer serializer,
            SaveMigrationPipeline migrations,
            ITimeProvider time,
            ISaveDiagnostics diagnostics,
            IProfileFileSystem fileSystem = null)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("Profile directory is required.", nameof(directory));
            _directory = Path.GetFullPath(directory);
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _migrations = migrations ?? throw new ArgumentNullException(nameof(migrations));
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            _fileSystem = fileSystem ?? new SystemProfileFileSystem();
        }

        public string MainPath => Path.Combine(_directory, MainFileName);
        public string BackupPath => Path.Combine(_directory, BackupFileName);
        public string TempPath => Path.Combine(_directory, TempFileName);

        public ProfileLoadResult LoadOrCreate(Func<SaveRootDto> freshFactory, Action<SaveRootDto> validate)
        {
            if (freshFactory == null) throw new ArgumentNullException(nameof(freshFactory));
            if (validate == null) throw new ArgumentNullException(nameof(validate));
            _fileSystem.CreateDirectory(_directory);
            var defaults = freshFactory();

            if (TryReadValid(MainPath, defaults, validate, out var main, out var mainMigrated, out var mainError))
            {
                if (mainMigrated) Save(main, freshFactory, validate);
                return new ProfileLoadResult(main, false, false, mainMigrated);
            }

            if (_fileSystem.FileExists(MainPath)) _diagnostics.Warning("Main profile is invalid; trying backup.", mainError);
            if (TryReadValid(BackupPath, defaults, validate, out var backup, out var backupMigrated, out var backupError))
            {
                if (_fileSystem.FileExists(MainPath)) PreserveCorruptCopy(MainPath);
                WriteMainFromValidatedSave(backup, defaults, validate);
                _diagnostics.Warning("Recovered profile from backup.");
                return new ProfileLoadResult(backup, false, true, backupMigrated);
            }

            if (_fileSystem.FileExists(BackupPath)) _diagnostics.Warning("Backup profile is invalid; creating a fresh profile.", backupError);
            if (_fileSystem.FileExists(MainPath)) PreserveCorrupt(MainPath);
            if (_fileSystem.FileExists(BackupPath)) PreserveCorrupt(BackupPath);
            var fresh = freshFactory();
            validate(fresh);
            Save(fresh, freshFactory, validate);
            return new ProfileLoadResult(fresh, true, false, false);
        }

        public void Save(SaveRootDto save, Func<SaveRootDto> freshFactory, Action<SaveRootDto> validate)
        {
            if (save == null) throw new ArgumentNullException(nameof(save));
            if (freshFactory == null) throw new ArgumentNullException(nameof(freshFactory));
            if (validate == null) throw new ArgumentNullException(nameof(validate));
            _fileSystem.CreateDirectory(_directory);
            validate(save);
            _fileSystem.WriteAllText(TempPath, _serializer.Serialize(save));
            if (!TryReadValid(TempPath, freshFactory(), validate, out _, out _, out var tempError))
            {
                throw new InvalidDataException("Temporary profile failed read-back validation.", tempError);
            }

            if (_fileSystem.FileExists(MainPath))
            {
                var backupCandidate = BackupPath + ".candidate";
                _fileSystem.CopyFile(MainPath, backupCandidate, true);
                if (!TryReadValid(backupCandidate, freshFactory(), validate, out _, out _, out var backupError))
                {
                    throw new InvalidDataException("Existing main profile is not valid enough to become backup.", backupError);
                }

                ReplacePortable(backupCandidate, BackupPath);
            }

            ReplacePortable(TempPath, MainPath);
        }

        private bool TryReadValid(
            string path,
            SaveRootDto defaults,
            Action<SaveRootDto> validate,
            out SaveRootDto save,
            out bool migrated,
            out Exception error)
        {
            save = null;
            migrated = false;
            error = null;
            if (!_fileSystem.FileExists(path)) return false;
            var json = _fileSystem.ReadAllText(path);
            try
            {
                var result = _migrations.MigrateToCurrent(json, defaults);
                validate(result.Save);
                save = result.Save;
                migrated = result.WasMigrated;
                return true;
            }
            catch (Exception exception) when (IsInvalidProfile(exception))
            {
                error = exception;
                return false;
            }
        }

        private void WriteMainFromValidatedSave(
            SaveRootDto save,
            SaveRootDto defaults,
            Action<SaveRootDto> validate)
        {
            _fileSystem.WriteAllText(TempPath, _serializer.Serialize(save));
            if (!TryReadValid(TempPath, defaults, validate, out _, out _, out var error))
            {
                throw new InvalidDataException("Recovered profile failed temporary validation.", error);
            }

            ReplacePortable(TempPath, MainPath);
        }

        private void PreserveCorrupt(string path)
        {
            _fileSystem.MoveFile(path, GetCorruptDestination(path));
        }

        private void PreserveCorruptCopy(string path)
        {
            var destination = GetCorruptDestination(path);
            _fileSystem.CopyFile(path, destination, false);
        }

        private string GetCorruptDestination(string path)
        {
            var timestamp = _time.UtcNow.ToString("yyyyMMddTHHmmssfffZ");
            var baseName = Path.GetFileNameWithoutExtension(path);
            var destination = Path.Combine(_directory, $"{baseName}.corrupt.{timestamp}.json");
            var suffix = 1;
            while (_fileSystem.FileExists(destination))
            {
                destination = Path.Combine(_directory, $"{baseName}.corrupt.{timestamp}.{suffix++}.json");
            }

            return destination;
        }

        private void ReplacePortable(string source, string destination)
        {
            if (!_fileSystem.FileExists(destination))
            {
                _fileSystem.MoveFile(source, destination);
                return;
            }

            try
            {
                _fileSystem.ReplaceFile(source, destination);
            }
            catch (PlatformNotSupportedException)
            {
                ReplaceFallback(source, destination);
            }
            catch (IOException)
            {
                ReplaceFallback(source, destination);
            }
        }

        // Android filesystems may not support File.Replace. The backup is committed first,
        // so an interruption during this fallback still leaves a validated recovery file.
        private void ReplaceFallback(string source, string destination)
        {
            var previous = destination + ".previous";
            if (_fileSystem.FileExists(previous)) _fileSystem.DeleteFile(previous);
            _fileSystem.MoveFile(destination, previous);
            try
            {
                _fileSystem.MoveFile(source, destination);
                _fileSystem.DeleteFile(previous);
            }
            catch
            {
                if (!_fileSystem.FileExists(destination) && _fileSystem.FileExists(previous))
                {
                    _fileSystem.MoveFile(previous, destination);
                }
                throw;
            }
        }

        private static bool IsInvalidProfile(Exception exception)
        {
            return exception is ArgumentException ||
                   exception is InvalidOperationException ||
                   exception is NotSupportedException ||
                   exception is FormatException ||
                   exception is OverflowException;
        }
    }
}
